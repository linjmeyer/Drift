using System.Collections.Generic;
using System.Dynamic;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Drift.Steps
{
    public abstract class AbstractDriftStep : IDriftStep
    {
        protected IKubernetes _k8s;
        protected ILogger _logger { get; set; }

        public string Type { get; set; }
        public string Script { get; set; }
        public string ScriptFile { get; set; }
        public ScriptingLanguage ScriptingLanguage { get; set; } = ScriptingLanguage.JavaScript;
        public List<IDriftStep> PreviousContexts { get; set; } = new List<IDriftStep>();
        public dynamic Bag { get; set; } = new ExpandoObject();
        public abstract bool Run();
        public abstract void Load();

        public void Configure(IKubernetes k8s, ILogger logger)
        {
            _k8s = k8s;
            _logger = logger;
        }

        public IDriftStep GetPreviousContext(int index = 0)
        {
            if (PreviousContexts != null && PreviousContexts.Count > index)
            {
                _logger?.LogDebug($"{nameof(GetPreviousContext)} for '{index}' succesfully");
                return PreviousContexts[index];
            }
            // default return null
            _logger?.LogDebug($"{nameof(GetPreviousContext)} for '{index}' called but no matching context found");
            return null;
        }

        public void Log(string message, params object[] vars)
        {
            _logger?.LogInformation(message, vars);
        }
    }
}