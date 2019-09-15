using System;
using System.IO;
using CSScriptLib;

namespace Drift.Steps
{
    /// <summary>
    /// Extension methods for IDriftStep interface objects.
    /// Useful for creating shared methods for steps that are not exposed to users via context
    /// </summary>
    internal static class DriftStepExtensions
    {
        internal static T RunUserScript<T>(this IDriftStep step, string typeString = null)
        {
            // Get file contents if set
            // Set file contents to Script property
            // (this means scriptFile contents override script if both are set by user)
            var scriptFile = GetUserScriptFile(step);
            if (scriptFile != null)
            {
                step.Script = scriptFile;
            }

            if (string.IsNullOrWhiteSpace(step.Script))
            {
                return default(T);
            }

            if (step.ScriptingLanguage == ScriptingLanguage.Csharp)
            {
                return step.RunUserScriptCSharp<T>(typeString);
            }
            // Else javascript
            return step.RunUserScriptJavascript<T>();
        }

        private static T RunUserScriptCSharp<T>(this IDriftStep step, string typeString)
        {
            var allCode = $@"
            public {(typeString ?? typeof(T).ToString())} UsersMethod(dynamic context)
            {{
                {step.Script}
            }}";
            dynamic script = CSScript.Evaluator.LoadMethod(allCode);
            return (T)script.UsersMethod((dynamic)step);
        }

        private static T RunUserScriptJavascript<T>(this IDriftStep step)
        {
            var engine = new Jint.Engine();
            engine.SetValue("context", step);
            var allCode = $@"
            function UsersMethod() {{ 
                {step.Script}
            }};
            UsersMethod();
            ";

            var rawResult = engine.Execute(allCode).GetCompletionValue().ToObject();
            return (T) rawResult;
        }

        private static string GetUserScriptFile(IDriftStep step)
        {
            if (!string.IsNullOrWhiteSpace(step.ScriptFile))
            {
                return File.ReadAllText(step.ScriptFile);
            }

            return null;
        }
    }
}