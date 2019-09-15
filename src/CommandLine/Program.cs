using System;
using System.Collections.Generic;
using System.Linq;
using Drift;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommandLine
{
    class Program
    {
        static void Main(string[] rawArgs)
        {
            // Parse args
            var args = ParseArgs(rawArgs);

            // Setup logging
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(b =>
            {
                b.AddConsole();
                b.SetMinimumLevel(LogLevel.Debug);
            })
            .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);
            var services = serviceCollection.BuildServiceProvider();
            var logger = services.GetRequiredService<ILogger<DriftClient>>();

            // Create/run client
            var drift = new DriftClient(c =>
            {
                c.DriftConfigPath = args.File;
                c.Logger = logger;
            });
            drift.Run();

            // Dispose of services, without this logging may not be flushed/displayed
            services.Dispose();
        }

        private static CommandLineArgs ParseArgs(string[] args)
        {
            var dict = new Dictionary<string, object>();
            foreach (var arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg))
                {
                    continue;
                }

                var splitArg = arg.Split('=');
                string key;
                string value = null;
                if (splitArg.Length == 0)
                {
                    continue; //ToDo: Error later
                }
                key = splitArg[0].TrimStart('-');

                if(splitArg.Length == 2) 
                {
                    value = splitArg[1];
                }
                dict.Add(key,value);
            }

            return GetObject<CommandLineArgs>(dict);
        }

        public static T GetObject<T>(Dictionary<string,object> dict)
        {
            Type type = typeof(T);
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                var property = type.GetProperty(kv.Key.First().ToString().ToUpper() + kv.Key.Substring(1));
                if(property == null)
                {
                    throw new Exception($"{kv.Key} argument does not exist");
                }
                property.SetValue(obj, kv.Value);
            }
            return (T)obj;
        }
    }
}
