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

        public AbstractDriftStep()
        {
            // ToDo: Use DI somehow?
            _k8s = DriftClient.Kubernetes;
            Logger = DriftClient.Logger;
        }

        public string Type { get; set; }
        public string Script { get; set; }
        public string ScriptFile { get; set; }
        public ScriptingLanguage ScriptingLanguage { get; set; } = ScriptingLanguage.JavaScript;
        public List<IDriftStep> PreviousContexts { get; set; } = new List<IDriftStep>();
        public dynamic Bag { get; set; } = new ExpandoObject();
        public ILogger<DriftClient> Logger { get; set; }
        public abstract bool Run();
        public abstract void Load();

        public IDriftStep GetPreviousContext(int index = 0)
        {
            if (PreviousContexts != null && PreviousContexts.Count > index)
            {
                Logger?.LogDebug($"{nameof(GetPreviousContext)} for '{index}' succesfully");
                return PreviousContexts[index];
            }
            // default return null
            Logger?.LogDebug($"{nameof(GetPreviousContext)} for '{index}' called but no matching context found");
            return null;
        }

        public void Log(string message, params object[] vars)
        {
            Logger?.LogInformation(message, vars);
        }
    }
}