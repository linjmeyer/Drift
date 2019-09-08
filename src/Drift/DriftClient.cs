using System.Collections.Generic;
using System.IO;
using Drift.Steps;
using k8s;
using Newtonsoft.Json;

namespace Drift
{
    public class DriftClient
    {
        public static Kubernetes _k8s; //ToDo: Make private non-static
        private DriftConfig _config;

        public DriftClient()
        {
        }

        public void SetupKubernetesClient(string currentContext = null, string kubeConfigPath = null)
        {
            var config = KubernetesClientConfiguration
                .BuildConfigFromConfigFile(kubeconfigPath: kubeConfigPath,
                    currentContext: currentContext);
            _k8s = new Kubernetes(config);
        }

        public void SetupDriftConfig(string driftConfigPath = null)
        {
            if (string.IsNullOrWhiteSpace(driftConfigPath))
            {
                driftConfigPath = $"{Directory.GetCurrentDirectory()}{Path.PathSeparator}drift.json";
            }

            var fileContents = File.ReadAllText(driftConfigPath);
            _config = JsonConvert.DeserializeObject<DriftConfig>(fileContents);
        }

        public void Run()
        {
            if (_k8s == null) SetupKubernetesClient();
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