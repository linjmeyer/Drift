using CSScriptLib;

public static class UserCodeHelpers
{
    public static T Evaluate<T>(this IDriftStep step) 
    {
        if(string.IsNullOrWhiteSpace(step?.Evaluate))
        {
            return default(T);
        }

        var allCode = $@"
        public {typeof(T)} Evaluate(dynamic context)
        {{
            {step.Evaluate}
        }}";
        dynamic script = CSScript.Evaluator.LoadMethod(allCode);
        var dynamicStep = (dynamic) step;
        var prev = dynamicStep.GetPreviousContext();
        return (T) script.Evaluate(dynamicStep);
    }

    public static bool EvaluateBool(this IDriftStep step)
    {
        return Evaluate<bool>(step);
    }
}