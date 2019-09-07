using System;

public interface IDriftStep
{
    string Type { get; set; }
    bool Run();
}

/// <summary>
/// A placeholder class to allow serailization/deserialization from JSON.  
/// Call ResolveType to get the true Object type before use.
/// </summary>
public class GenericDriftStep : IDriftStep
{
    public string Type { get; set; }

    public bool Run()
    {
        throw new NotImplementedException($"{nameof(GenericDriftStep)} should not be used.  Use a specific step instead");
    }
}