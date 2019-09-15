using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Drift.Steps
{
    public enum ScriptingLanguage
    {
        Csharp,
        JavaScript
    }

    public interface IDriftStep
    {
        string Type { get; set; }
        string Script { get; set; }
        string ScriptFile { get; set; }
        ScriptingLanguage ScriptingLanguage { get; set; }
        List<IDriftStep> PreviousContexts { get; set; }
        dynamic Bag { get; set; }
        bool Run();
        void Load();
        void Log(string message, params object[] vars);
    }
}