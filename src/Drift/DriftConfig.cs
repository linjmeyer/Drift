using System.Collections.Generic;
using Drift.Steps;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Drift
{
    public class DriftConfig
    {
        public DriftAction[] Actions { get; set; }
    }

    public class DriftAction
    {
        [JsonIgnore]
        public IDriftStep[] Steps { get; set; }

        [JsonProperty(nameof(Steps))]
        public JArray JArraySteps
        {
            get => null;
            set
            {
                ParseActual(value);
            }
        }

        public void ParseActual(JArray raw)
        {
            var steps = new List<IDriftStep>();
            foreach(var item in raw.Children())
            {
                // Serialize generic step to access the Type string set by user
                var genericStep = item.ToObject<GenericDriftStep>();
                // Get real type based on Type value set by user
                var assembly = typeof(GenericDriftStep).Assembly;
                var dotNetTypeString = $"{nameof(Drift)}.{nameof(Drift.Steps)}.{genericStep.Type}Step";
                var dotNetType = assembly.GetType(dotNetTypeString);
                // Serialize to user requested type
                var concreteStep = item.ToObject(dotNetType);
                // Convert back to interface and pass to list
                steps.Add((IDriftStep) concreteStep);
            }
    
            Steps = steps.ToArray();
        }
    }
}