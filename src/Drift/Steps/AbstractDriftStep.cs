using System.Collections.Generic;
using System.Dynamic;
using k8s;

namespace Drift.Steps
{
    public abstract class AbstractDriftStep : IDriftStep
    {
        protected Kubernetes _k8s;

        public AbstractDriftStep()
        {
            _k8s = DriftClient._k8s;
        }

        public string Type { get; set; }
        public string Evaluate { get; set; }
        public List<IDriftStep> PreviousContexts { get; set; } = new List<IDriftStep>();
        public dynamic Bag { get; set; } = new ExpandoObject();

        public abstract bool Run();

        public IDriftStep GetPreviousContext(int index = 0)
        {
            if (PreviousContexts != null && PreviousContexts.Count > index)
            {
                return PreviousContexts[index];
            }
            // default return null
            return null;
        }
    }
}