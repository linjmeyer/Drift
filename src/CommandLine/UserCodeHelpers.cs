using CSScriptLib;

public static class UserCodeHelpers
{
    public static T Evaluate<T>(this IDriftStep step, string userCode) 
    {
        var allCode = $@"
        public {typeof(T)} Evaluate()
        {{
            {userCode}
        }}";
        dynamic script = CSScript.Evaluator.LoadMethod(allCode);
        return (T) script.Evaluate();
    }

    public static bool EvaluateBool(this IDriftStep step, string userCode)
    {
        return Evaluate<bool>(step, userCode);
    }
}