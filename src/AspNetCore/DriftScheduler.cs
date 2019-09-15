using Hangfire;
using Drift;
using System.ComponentModel;

namespace Drift.AspNetCore
{
    /// <summary>
    /// Gets all actions from the Drift config and schedules the actions seperately within Hangfire
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
        /// Schedules all current Actions within Hanfire as seperate jobs
        /// </summary>
        [DisplayName("Drift Scheduler")]
        public void Schedule()
        {
            var actionNames = _drift.GetActionNames();
            foreach(var actionName in actionNames)
            {
                _backgroundJobs.Enqueue<DriftClient>(drift => drift.Run(actionName));
            }
        }
    }
}