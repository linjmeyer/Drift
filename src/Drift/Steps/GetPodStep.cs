using System;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Drift.Steps
{
    public class GetPodStep : AbstractDriftStep
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public V1Pod Pod { get; private set; }

        public override bool Run()
        {
            try
            {
                Pod = _k8s.ReadNamespacedPod(Name, Namespace);
                if (Pod == null) return false;
            }
            catch (HttpOperationException e)
            {
                Log?.LogDebug($"Exception during step {Type}: ", e);
                return false;
            }
            return true;
        }
    }
}