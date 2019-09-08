using System;
using System.Collections.Generic;
using System.IO;
using Drift.Steps;
using k8s;
using Microsoft.Extensions.DependencyInjection;
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

        private void SetupKubernetesClient()
        {
            var config = KubernetesClientConfiguration
                .BuildConfigFromConfigFile(kubeconfigPath: _clientConfig.KubeConfigPath,
                    currentContext: _clientConfig.KubernetesContext);
            _k8s = new Kubernetes(config);
            _serviceCollection.AddSingleton<IKubernetes>(_k8s);
        }

        private void SetupDriftConfig()
        {
            var fileContents = File.ReadAllText(_clientConfig.DriftConfigPath);
            _config = JsonConvert.DeserializeObject<DriftConfig>(fileContents);
        }

        public void Run()
        {
            if (_k8s == null) SetupKubernetesClient();
            DriftClient.Services = _serviceCollection.BuildServiceProvider();
            if (_config == null) SetupDriftConfig();
            foreach(var action in _config.Actions)
            {
                RunSteps(action);
            }
        }

        private void RunSteps(DriftAction action)
        {
            var previousContexts = new List<IDriftStep>();
            dynamic previousBag = null;
            foreach(var step in action.Steps)
            {
                // Save previous contexts to this step
                step.PreviousContexts.AddRange(previousContexts);
                // Save previous Bag (user data store)
                if(previousBag != null)
                {
                    step.Bag = previousBag;
                }
                // Run the internal step and user defined eval
                var runResult = step.Run();
                var evalResult = step.RunUserEval<bool>();
                // save this step and bag for next step
                previousContexts.Add(step);
                previousBag = step.Bag;
            }
        }
    }
}