using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Drift.Steps;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Drift
{
    public class DriftClient
    {
        public static IKubernetes Kubernetes;
        public static ILogger<DriftClient> Logger;

        private DriftConfig _config;
        private DriftClientConfig _clientConfig = new DriftClientConfig();
        private List<string> _actionNames = new List<string>();

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
            DriftClient.Kubernetes = new Kubernetes(config);
        }

        private void SetupDriftConfig()
        {
            // Load config (not actions, steps, etc. but config options)
            var fileContents = File.ReadAllText(_clientConfig.DriftConfigPath);
            _config = JsonConvert.DeserializeObject<DriftConfig>(fileContents);
            // Setup logging if passed in
            Logger = _clientConfig.Logger;
        }


        /// <summary>
        /// Gets each action as an action which can be invoked individually
        /// </summary>
        /// <returns></returns>
        public DriftAction[] GetActions()
        {
            // Setup k8s, then build services
            SetupKubernetesClient();
            // Load drift config
            SetupDriftConfig();
            // Check names for issues
            ValidateActionNames();

            return _config.Actions;
        }

        /// <summary>
        /// Returns the number of Actions present in the config without running or preparing to run them.
        /// </summary>
        /// <returns></returns>
        public string[] GetActionNames()
        {
            SetupDriftConfig();
            ValidateActionNames();

            return _actionNames.ToArray();
        }

        /// <summary>
        /// Syncronously runs all actions and steps
        /// </summary>
        [DisplayName("Drift Run")]
        public void Run()
        {
            var runners = GetActions();
            foreach(var action in runners)
            {
                action.Run(Logger);
            }
        }

        /// <summary>
        /// Runs a particular action
        /// </summary>
        /// <param name="index"></param>
        [DisplayName("Drift Run: {0}")]
        public void Run(string name)
        {
            var actions = GetActions();
            var action = actions.FirstOrDefault(a => a.Name == name);
            if(action == null)
            {
                throw new InvalidOperationException($"The action {name} does not exist");
            }
            action.Run(Logger);
        }

        private void ValidateActionNames()
        {
            // Get all names
            _actionNames.AddRange(_config.Actions.Select(a => a.Name));
            // Find any null or empty
            var nullNames = _actionNames.Where(n => string.IsNullOrWhiteSpace(n)).ToList();
            if(nullNames.Count > 0)
            {
                throw new InvalidOperationException($"Null or empty name found, all actions need a unique Name");
            }
            // Find any duplicates
            var duplicates = _actionNames
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(k => k.Key)
                .ToList();
            if(duplicates.Count > 0)
            {
                var firstDuplicate = duplicates.First();
                throw new InvalidOperationException($"Duplicate action names: {firstDuplicate}.  Action names must be unique");
            }
        }
    }
}