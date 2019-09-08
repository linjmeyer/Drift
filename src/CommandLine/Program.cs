using Drift;

namespace CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            var drift = new DriftClient(c => {
                c.DriftConfigPath = "/Users/lin.meyer/Personal/Drift/src/CommandLine/config.jsonc";
            });
            drift.Run();
        }
    }
}
