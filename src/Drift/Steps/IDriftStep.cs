using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Drift.Steps
{
    public interface IDriftStep
    {
        string Type { get; set; }
        string Evaluate { get; set; }
        string EvaluateFile { get; set; }
        List<IDriftStep> PreviousContexts { get; set; }
        dynamic Bag { get; set; }
        ILogger<DriftClient> Log { get; set; }
        bool Run();
        void Load();
    }
}