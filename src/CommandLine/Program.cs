using Drift;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup logging
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(b => {
                b.AddConsole();
                b.SetMinimumLevel(LogLevel.Debug);
            })
            .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);
            var services = serviceCollection.BuildServiceProvider();
            var logger = services.GetRequiredService<ILogger<DriftClient>>();

            // Create/run client
            var drift = new DriftClient(c => {
                c.DriftConfigPath = "/Users/lin.meyer/Personal/Drift/src/CommandLine/debug_config.jsonc";
                c.Logger = logger;
            });
            drift.Run();

            // Dispose of services, without this logging may not be flushed/displayed
            services.Dispose();
        }
    }
}
