using System;
using System.Collections.Generic;
using System.Dynamic;

public interface IDriftStep
{
    string Type { get; set; }
    string Evaluate { get; set; }
    List<IDriftStep> PreviousContexts { get; set; }
    dynamic Bag { get; set; }
    bool Run();
}

/// <summary>
/// A placeholder class to allow serailization/deserialization from JSON.
/// Once deserialized check the Type property to determine the correct concrete class to use
/// </summary>
public class GenericDriftStep : AbstractDriftStep
{
    private string _notImplemented = $"{nameof(GenericDriftStep)} should not be used.  Use a specific step instead";
    
    public override bool Run()
    {
        throw new NotImplementedException(_notImplemented);
    }
}

public abstract class AbstractDriftStep : IDriftStep
{
    public string Type { get; set; }
    public string Evaluate { get; set; }
    public List<IDriftStep> PreviousContexts { get; set; } = new List<IDriftStep>();
    public dynamic Bag { get; set; } = new ExpandoObject();

    public abstract bool Run();

    public IDriftStep GetPreviousContext(int index = 0)
    {
        if(PreviousContexts != null && PreviousContexts.Count > index) 
        {
            return PreviousContexts[index];
        }
        // default return null
        return null;
    }
}