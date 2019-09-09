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
            // ToDo: Move to json.net 
            _k8s = DriftClient.Services.GetRequiredService<IKubernetes>();
            Log = DriftClient.Services.GetRequiredService<ILogger<DriftClient>>();
        }

        public string Type { get; set; }
        public string Evaluate { get; set; }
        public string EvaluateFile { get; set; }
        public List<IDriftStep> PreviousContexts { get; set; } = new List<IDriftStep>();
        public dynamic Bag { get; set; } = new ExpandoObject();
        public ILogger<DriftClient> Log { get; set; }
        public abstract bool Run();
        public abstract void Load();

        public IDriftStep GetPreviousContext(int index = 0)
        {
            if (PreviousContexts != null && PreviousContexts.Count > index)
            {
                Log?.LogDebug($"{nameof(GetPreviousContext)} for '{index}' succesfully");
                return PreviousContexts[index];
            }
            // default return null
            Log?.LogDebug($"{nameof(GetPreviousContext)} for '{index}' called but no matching context found");
            return null;
        }
    }
}