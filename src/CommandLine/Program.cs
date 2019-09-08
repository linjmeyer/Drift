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
                foreach(var step in action.Steps)
                {
                    step.PreviousContexts.AddRange(previousContexts);
                    var runResult = step.Run();
                    var evalResult = step.EvaluateBool();
                    previousContexts.Add(step);
                }
            }
        }
    }
}
