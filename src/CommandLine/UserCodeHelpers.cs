using CSScriptLib;

public static class UserCodeHelpers
{
    public static T Evaluate<T>(this IDriftStep step, string userCode) 
    {
        var allCode = $@"
        public {typeof(T)} Evaluate(dynamic context)
        {{
            {userCode}
        }}";
        dynamic script = CSScript.Evaluator.LoadMethod(allCode);
        return (T) script.Evaluate((dynamic) step);
    }

    public static bool EvaluateBool(this IDriftStep step, string userCode)
    {
        return Evaluate<bool>(step, userCode);
    }
}