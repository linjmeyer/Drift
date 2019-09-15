using System;
using System.ComponentModel.DataAnnotations;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Drift.Steps
{
    /// <summary>
    /// A step that gets a pod by name and namespace
    /// </summary>
    public class GetPodStep : AbstractDriftStep
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Namespace { get; set; } = "default";
        public V1Pod Pod { get; private set; }

        public override void Load()
        {
        }

        public override bool Run()
        {
            try
            {
                Pod = _k8s.ReadNamespacedPod(Name, Namespace);
                if (Pod == null) return false;
            }
            catch (HttpOperationException e)
            {
                _logger?.LogDebug($"Exception during step {Type}: ", e);
                return false;
            }
            return true;
        }
    }
}