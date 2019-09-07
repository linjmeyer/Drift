using k8s;
using k8s.Models;

namespace CommandLine.Steps
{
    public class GetPodStep : IDriftStep
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public V1Pod Pod { get; private set; }

        public bool Run()
        {
            Pod = Program.Client.ReadNamespacedPod(Name, Namespace);
            return (Pod != null);
        }
    }
}