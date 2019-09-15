using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Drift.AspNetCore
{
    public static class HangfireDriftExtensions
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

        /// <summary>
        /// Adds a Hangfire RecurringJob which reloads the Drift configuration and enqueues each Drift action to run seperately
        /// </summary>
        /// <param name="backgroundJob"></param>
        /// <returns></returns>
        public static IBackgroundJobClient AddDriftActionScheduling(this IBackgroundJobClient backgroundJob)
        {
            // Setup re-occuring
            RecurringJob.AddOrUpdate<DriftScheduler>(
                driftScheduler => driftScheduler.Schedule(),
                Cron.Minutely
            );

            // Fire once now for immediate run
            backgroundJob.Enqueue<DriftScheduler>(
                driftScheduler => driftScheduler.Schedule()
            );
            return backgroundJob;
        }
    }
}