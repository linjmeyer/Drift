using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Drift.Steps;
using k8s;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Drift.Helpers;

namespace Drift
{
    public class DriftClient
    {
        private IKubernetes _k8s;
        private ILogger<DriftClient> _logger;

        private DriftConfig _config;
        private DriftClientConfig _clientConfig = new DriftClientConfig();
        private List<string> _jobNames = new List<string>();

        public DriftClient(DriftClientConfig config)
        {
            _clientConfig = config;
        }

        public DriftClient(Action<DriftClientConfig> config = null)
        {
            config?.Invoke(_clientConfig);
        }

        private void SetupKubernetesClient()
        {
            var config = KubernetesClientConfiguration
                .BuildConfigFromConfigFile(kubeconfigPath: _clientConfig.KubeConfigPath,
                    currentContext: _clientConfig.KubernetesContext);
            _k8s = new Kubernetes(config);
        }

        private void SetupDriftConfig()
        {
            string fileContents;
            // Load config (not jobs, steps, etc. but config options)
            if(_clientConfig.DriftConfigPath.EndsWith(".yaml") || _clientConfig.DriftConfigPath.EndsWith(".yml"))
            {
                // Convert yaml to json
                fileContents = ReadConfigYamlToJson(_clientConfig.DriftConfigPath);
            }
            else 
            {
                // Read json config
                fileContents = ReadConfigJson(_clientConfig.DriftConfigPath);
            }
            // Serialize json to object
            _config = JsonConvert.DeserializeObject<DriftConfig>(fileContents);
            // Setup logging if passed in
            _logger = _clientConfig.Logger;
        }

        private string ReadConfigJson(string path)
        {
            return File.ReadAllText(path);
        }

        private string ReadConfigYamlToJson(string path)
        {
            var yamlReader = new StringReader(File.ReadAllText(path));
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(yamlReader);
            var serializer = new YamlDotNet.Serialization.SerializerBuilder()
                .JsonCompatible()
                .Build();
            return serializer.Serialize(yamlObject);
        }


        /// <summary>
        /// Returns all jobs loaded by the Drift client config
        /// </summary>
        /// <returns></returns>
        public DriftJob[] GetJobs()
        {
            // Setup k8s, then build services
            SetupKubernetesClient();
            // Load drift config
            SetupDriftConfig();
            // Check names for issues
            ValidateJobNames();

            return _config.Jobs;
        }

        /// <summary>
        /// Returns the unique name of all jobs
        /// </summary>
        /// <returns></returns>
        public string[] GetJobNames()
        {
            SetupDriftConfig();
            ValidateJobNames();

            return _jobNames.ToArray();
        }

        /// <summary>
        /// Syncronously runs all jobs and steps
        /// </summary>
        [DisplayName("Drift Run")]
        public void Run()
        {
            var jobs = GetJobs();
            foreach(var job in jobs)
            {
                Run(job);
            }
        }

        /// <summary>
        /// Runs the job based on the name passed 
        /// </summary>
        /// <param name="index"></param>
        [DisplayName("Drift Run: {0}")]
        public void Run(string name)
        {
            var jobs = GetJobs();
            var job = jobs.FirstOrDefault(a => a.Name == name);
            if(job == null)
            {
                throw new InvalidOperationException($"The job {name} does not exist");
            }
            Run(job);
        }

        private bool Run(DriftJob job)
        {
            var previousContexts = new List<IDriftStep>();
            dynamic previousBag = null;
            for (var i = 0; i < job.Steps.Length; i++)
            {
                _logger?.LogInformation($"Starting step: {i}");
                var step = job.Steps[i];
                // Configure the step (think of it like DI.. but ghetto)
                step.Configure(_k8s, _logger);
                // Save previous contexts to this step
                step.PreviousContexts.AddRange(previousContexts);
                // Save previous Bag (user data store)
                if (previousBag != null)
                {
                    step.Bag = previousBag;
                    _logger?.LogDebug($"Passing bag from previous step to step {i}.  Contents: {Extensions.ToLogString(step.Bag)}");
                }
                
                _logger?.LogDebug("Starting load step");
                step.Load();

                // Run the step and get the result
                var runResult = step.Run();
                _logger?.LogInformation($"Result of {step.Type}: {runResult} {(runResult ? "Will run user script" : "Will not run user script")}");
                if(!runResult) return false; // Stop loop if this step is false
                
                // Run the user script and get the result
                var scriptResult = step.RunUserScript<bool?>(typeString: "bool?");
                if(scriptResult.HasValue) 
                {
                    _logger?.LogInformation($"Result of {step.Type} User Script: {scriptResult}");
                    if(!scriptResult.Value)
                    {
                        return false; // user returned false, stop job
                    }
                }
                else
                {
                    _logger?.LogInformation($"No user script found");
                }

                // save this step and bag for next step
                previousContexts.Add(step);
                previousBag = step.Bag;
            }

            return true; // If everything finished the steps all ran successfully 
        }

        private void ValidateJobNames()
        {
            // Get all names
            _jobNames.AddRange(_config.Jobs.Select(a => a.Name));
            // Find any null or empty
            var nullNames = _jobNames.Where(n => string.IsNullOrWhiteSpace(n)).ToList();
            if(nullNames.Count > 0)
            {
                throw new InvalidOperationException($"Null or empty name found, all jobs need a unique Name");
            }
            // Find any duplicates
            var duplicates = _jobNames
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(k => k.Key)
                .ToList();
            if(duplicates.Count > 0)
            {
                var firstDuplicate = duplicates.First();
                throw new InvalidOperationException($"Duplicate job names: {firstDuplicate}.  Jobs names must be unique");
            }
        }
    }
}