using System;

namespace Drift.Steps
{
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
}