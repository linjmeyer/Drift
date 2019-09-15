namespace Drift.Steps
{
    /// <summary>
    /// A step that can be used for running user defined scripts.  It does not do anything else.
    /// </summary>
    public class EvalStep : AbstractDriftStep
    {
        public override void Load()
        {
        }

        public override bool Run() => true;
    }
}