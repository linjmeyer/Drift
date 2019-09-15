using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Drift.Steps
{
    public class WebhookStep : AbstractDriftStep
    {
        [Required]
        public string Url { get; set; }

        public object Payload { get ; set; } = new object();

        public HttpResponseMessage Response { get; private set; }

        public override void Load()
        {
        }

        public override bool Run()
        {
            var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json");
            Response = client.PostAsync(Url, content).Result;

            return Response.IsSuccessStatusCode;
        }
    }
}