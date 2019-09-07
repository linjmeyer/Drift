using System;
using System.IO;
using System.Linq;
using CSScriptLib;
using k8s;
using k8s.Models;
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
                foreach(var step in action.Steps)
                {
                    var eval = step.Run();
                }
            }
        }
    }
}
