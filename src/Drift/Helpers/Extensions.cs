using Newtonsoft.Json;

namespace Drift.Helpers
{
    public static class Extensions
    {
        public static string ToLogString(this object thing)
        {
            return JsonConvert.SerializeObject(thing);
        }
    }    
}