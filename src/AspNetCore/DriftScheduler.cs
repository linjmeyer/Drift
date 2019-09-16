using Hangfire;
using Drift;
using System.ComponentModel;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace Drift.AspNetCore
{
    /// <summary>
    /// Gets all Jobs from the Drift config and schedules the Jobs seperately within Hangfire
    /// </summary>
    public class DriftScheduler
    {
        private IBackgroundJobClient _backgroundJobs;
        private DriftClientConfig _driftConfig;
        private ILogger<DriftScheduler> _logger;

        public DriftScheduler(IBackgroundJobClient backgroundJobs, DriftClientConfig driftConfig, ILogger<DriftScheduler> logger)
        {
            _backgroundJobs = backgroundJobs;
            _driftConfig = driftConfig;
            _logger = logger;
        }

        /// <summary>
        /// Schedules all current Jobs within Hanfire as seperate jobs
        /// </summary>
        public void Schedule()
        {
            _logger.LogInformation("Loading drift jobs");
            // Recreate client using this config and get job names
            var jobNames = new DriftClient(_driftConfig).GetJobNames();
            _logger.LogInformation($"Found total of {jobNames.Length} jobs to queue");
            foreach(var jobName in jobNames)
            {
                // Create separate queue'd job for each name
                _backgroundJobs.Enqueue(() => StartJob(jobName, _driftConfig, null));
                _logger.LogInformation($"Queued job: {jobName}");
            }
        }

        /// <summary>
        /// Adds a Drift Job to the Hangfire job queue and injects the HangfireConsoleLogger
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="config"></param>
        /// <param name="hangfireContext"></param>
        [DisplayName("Drift Job: {0}")]
        public static void StartJob(string jobName, DriftClientConfig config, PerformContext hangfireContext)
        {
            config.Logger = new HangfireConsoleLogger<DriftClient>(hangfireContext, config.Logger);
            var drift = new DriftClient(config);
            drift.Run(jobName);
        }
    }
}