using System;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace Drift.AspNetCore
{
    /// <summary>
    /// Adds the ability to log to the hangfire dashboard using an ILogger abstraction. 
    /// This allows classes that are aware of .NET Core's ILogger to log to Hangfire without 
    /// a dependency on Hangfire itself
    /// </summary>
    public class HangfireConsoleLogger<T> : ILogger<T>
    {
        private PerformContext _jobContext;
        private ILogger _logger;

        public HangfireConsoleLogger(PerformContext jobContext, ILogger<T> logger)
        {
            _jobContext = jobContext;
            _logger = logger;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // For ILogger inject details that help identify this job, like the Id:
            // Override the eventId to be the hangfire job ID
            eventId = new EventId(eventId.Id, $"hangfire-{_jobContext.BackgroundJob.Id}");
            // Log to ILogger
            _logger?.Log(logLevel, eventId, state, exception, formatter);
            // Log to hangfire
            var message = formatter(state, exception);
            _jobContext.WriteLine(message);
        }

    }
}