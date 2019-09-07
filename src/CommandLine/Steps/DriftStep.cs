using System;

public interface IDriftStep
{
    string Type { get; set; }
    string Evaluate { get; set; }
    bool Run();
}

/// <summary>
/// A placeholder class to allow serailization/deserialization from JSON.  
/// Call ResolveType to get the true Object type before use.
/// </summary>
public class GenericDriftStep : IDriftStep
{
    private string _notImplemented = $"{nameof(GenericDriftStep)} should not be used.  Use a specific step instead";
    public string Type { get; set; }
    public string Evaluate { get; set; }

    public bool Run()
    {
        throw new NotImplementedException(_notImplemented);
    }
}