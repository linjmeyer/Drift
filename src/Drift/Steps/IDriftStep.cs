using System.Collections.Generic;

namespace Drift.Steps
{
    public interface IDriftStep
    {
        string Type { get; set; }
        string Evaluate { get; set; }
        string EvaluateFile { get; set; }
        List<IDriftStep> PreviousContexts { get; set; }
        dynamic Bag { get; set; }
        bool Run();
    }
}