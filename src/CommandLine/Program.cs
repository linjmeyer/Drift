using System.Collections.Generic;
using System.IO;
using k8s;
using Newtonsoft.Json;

namespace CommandLine
{
    class Program
    {
        public static IKubernetes Client;
        public static DriftConfig Config;

        static void Main(string[] args)
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            var client = Program.Client = new Kubernetes(config);

            GetJsonConfig();
            Run();
        }

        public static void GetJsonConfig()
        {
            var fileContents = File.ReadAllText("/Users/lin.meyer/Personal/Drift/src/CommandLine/config.jsonc");
            Config = JsonConvert.DeserializeObject<DriftConfig>(fileContents);
        }

        public static void Run()
        {
            foreach(var action in Config.Actions)
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
                    var evalResult = step.EvaluateBool();
                    // save this step and bag for next step
                    previousContexts.Add(step);
                    previousBag = step.Bag;
                }
            }
        }
    }
}
