using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace Drift.Steps
{
    public class SearchPodsStep : AbstractDriftStep
    {
        [Required]
        public string NameRegex { get; set; }
        [Required]
        public string Namespace { get; set; }
        public V1PodList PodList { get; set; }
        public IList<V1Pod> Matches { get; set; }

        public override void Load()
        {
        }

        public override bool Run()
        {
            PodList = _k8s.ListNamespacedPod(Namespace);
            if(!PodList.Items.Any())
            {
                Log.LogInformation($"Step {Type}: No pods found in namespace {Namespace}");
                return false;
            }
            Matches = PodList.Items.Where(p => Regex.IsMatch(p.Metadata.Name, NameRegex)).ToList();
            if(!Matches.Any())
            {
                Log.LogInformation($"Step {Type}: No matching pods found in namespace {Namespace}, {nameof(NameRegex)} {NameRegex}");
                return false;
            }
            return true;
        }
    }
}