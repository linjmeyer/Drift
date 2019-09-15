using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Drift.AspNetCore
{
    public static class DriftExtensions
    {
        public static IServiceCollection AddDrift(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddTransient<DriftClient>((services) => {
                // Load "Drift" section from appsettings
                var driftSection = configuration.GetSection("Drift");
                var driftConfig = driftSection.Get<DriftClientConfig>();
                // Pass our logger to the config
                driftConfig.Logger = services.GetService<ILogger<DriftClient>>();
                var drift = new DriftClient(driftConfig);
                return drift;
            });
            return serviceCollection;
        }

        public static IBackgroundJobClient AddDriftJobs(this IBackgroundJobClient backgroundJob)
        {
            // Setup re-occuring
            RecurringJob.AddOrUpdate<DriftClient>(
                drift => drift.Run(),
                Cron.Minutely
            );
            // Fire once now for immediate run
            backgroundJob.Enqueue<DriftClient>(
                drift => drift.Run()
            );
            return backgroundJob;
        }
    }
}