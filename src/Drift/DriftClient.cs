using System;
using System.Collections.Generic;
using System.IO;
using Drift.Steps;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Drift
{
    public class DriftClient
    {
        internal static ServiceProvider Services;
        public static Kubernetes _k8s; //ToDo: Make private non-static
        private DriftConfig _config;
        private ServiceCollection _serviceCollection = new ServiceCollection();
        private DriftClientConfig _clientConfig = new DriftClientConfig();

        public DriftClient(Action<DriftClientConfig> config = null)
        {
            config?.Invoke(_clientConfig);
        }

        private void SetupServices()
        {
            _serviceCollection.AddSingleton<IKubernetes>(_k8s);
            if(_clientConfig.Logger != null) 
            {
                _serviceCollection.AddSingleton<ILogger>(_clientConfig.Logger);
                _serviceCollection.AddSingleton<ILogger<DriftClient>>(_clientConfig.Logger);
            }
            DriftClient.Services = _serviceCollection.BuildServiceProvider();
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
            var fileContents = File.ReadAllText(_clientConfig.DriftConfigPath);
            _config = JsonConvert.DeserializeObject<DriftConfig>(fileContents);
        }

        public void Run()
        {
            // Setup k8s, then build services
            SetupKubernetesClient();
            SetupServices();
            // Load drift config
            SetupDriftConfig();
            // Begin
            RunActions();
            // Dispose of services
            Services.Dispose();
        }

        private void RunActions()
        {
            var logger = Services.GetRequiredService<ILogger<DriftClient>>();
            foreach (var action in _config.Actions)
            {
                logger.LogInformation($"Found new action with {action.Steps.Length} steps");
                var actionResult = RunSteps(action, logger);
                logger.LogInformation($"Action ran to completion with result: {actionResult}");
            }
        }

        private bool RunSteps(DriftAction action, ILogger<DriftClient> logger)
        {
            var previousContexts = new List<IDriftStep>();
            dynamic previousBag = null;
            for (var i = 0; i < action.Steps.Length; i++)
            {
                logger.LogInformation($"Starting step: {i}");
                var step = action.Steps[i];
                // Save previous contexts to this step
                step.PreviousContexts.AddRange(previousContexts);
                // Save previous Bag (user data store)
                if (previousBag != null)
                {
                    step.Bag = previousBag;
                    logger.LogDebug($"Passing bag from previous step to step {i}.  Contents: {step.Bag}");
                }
                
                // Run the step and get the result
                var runResult = step.Run();
                logger.LogInformation($"Result of {step.Type}: {runResult} {(runResult ? "Will run user eval" : "Will not run user eval")}");
                if(!runResult) return false; // Stop loop if this step is false
                
                // Run the user eval and get the result
                var evalResult = step.RunUserEval<bool?>(typeString: "bool?");
                if(evalResult.HasValue) 
                {
                    logger.LogInformation($"Result of {step.Type} User Eval: {evalResult}");
                    if(!evalResult.Value)
                    {
                        return false; // user returned false, stop action
                    }
                }
                else
                {
                    logger.LogInformation($"No user eval found");
                }

                // save this step and bag for next step
                previousContexts.Add(step);
                previousBag = step.Bag;
            }

            return true; // If everything finished the steps all ran successfully 
        }
    }
}