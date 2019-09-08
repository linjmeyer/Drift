using System;
using k8s;
using k8s.Models;
using Microsoft.Rest;

namespace CommandLine.Steps
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
                Pod = Program.Client.ReadNamespacedPod(Name, Namespace);
                if (Pod == null) return false;
            }
            catch (HttpOperationException e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }
    }
}