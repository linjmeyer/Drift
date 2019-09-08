using CSScriptLib;

namespace Drift.Steps
{
    /// <summary>
    /// Extension methods for IDriftStep interface objects.
    /// Useful for creating shared methods for steps that are not exposed to users via context
    /// </summary>
    internal static class DriftStepExtensions
    {
        internal static T RunUserEval<T>(this IDriftStep step)
        {
            if (string.IsNullOrWhiteSpace(step?.Evaluate))
            {
                return default(T);
            }

            var allCode = $@"
            public {typeof(T)} UsersMethod(dynamic context)
            {{
                {step.Evaluate}
            }}";
            dynamic script = CSScript.Evaluator.LoadMethod(allCode);
            return (T)script.UsersMethod((dynamic)step);
        }
    }
}