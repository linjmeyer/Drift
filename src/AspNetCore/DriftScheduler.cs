using Hangfire;
using Drift;
using System.ComponentModel;

namespace Drift.AspNetCore
{
    /// <summary>
    /// Gets all Jobs from the Drift config and schedules the Jobs seperately within Hangfire
    /// </summary>
    public class DriftScheduler
    {
        private IBackgroundJobClient _backgroundJobs;
        private DriftClient _drift;

        public DriftScheduler(IBackgroundJobClient backgroundJobs, DriftClient drift)
        {
            _backgroundJobs = backgroundJobs;
            _drift = drift;
        }

        /// <summary>
        /// Schedules all current Jobs within Hanfire as seperate jobs
        /// </summary>
        [DisplayName("Drift Scheduler")]
        public void Schedule()
        {
            var jobNames = _drift.GetJobNames();
            foreach(var jobName in jobNames)
            {
                _backgroundJobs.Enqueue<DriftClient>(drift => drift.Run(jobName));
            }
        }
    }
}