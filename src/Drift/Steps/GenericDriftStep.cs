using System;

namespace Drift.Steps
{
    /// <summary>
    /// A placeholder class to allow serailization/deserialization from JSON.
    /// Once deserialized check the Type property to determine the correct concrete class to use
    /// </summary>
    internal class TempInternalDriftStep : AbstractDriftStep
    {
        private string _notImplemented = $"{nameof(TempInternalDriftStep)} should not be used.  Use a specific step instead";

        public override void Load()
        {
            throw new NotImplementedException(_notImplemented);
        }

        public override bool Run()
        {
            throw new NotImplementedException(_notImplemented);
        }
    }
}