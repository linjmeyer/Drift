using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using k8s;

namespace Drift.Steps
{
    public class DeletePodsStep : AbstractDriftStep
    {
        [Required]
        public string[] Names { get; set; }

        public string NamesFromBag { get; set; }

        [Required]
        public string Namespace { get; set; }

        public override void Load()
        {
            if(!string.IsNullOrWhiteSpace(NamesFromBag))
            {
                var dictBag = (IDictionary<string, object>) Bag;
                Names = (string[]) dictBag[NamesFromBag];
            }
        }

        public override bool Run()
        {
            //ToDo: comments
            foreach(var name in Names)
            {
                _k8s.DeleteNamespacedPod(name, Namespace);
            }

            return true;
        }
    }
}