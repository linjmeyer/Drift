using System.IO;
using Microsoft.Extensions.Logging;

namespace Drift
{
    public class DriftClientConfig 
    {
        public string DriftConfigPath { get; set; } = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}drift.json";
        public string KubernetesContext { get; set; } = null; // Current context used in kubeconfig file by default
        public string KubeConfigPath { get; set; } = null; // Default kubeconfig file for OS used by default
        public ILogger<DriftClient> Logger { get; set; } = null;
    }
}