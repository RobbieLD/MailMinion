using Newtonsoft.Json;

namespace MailMinion
{
    public class Config
    {
        [JsonProperty(PropertyName = "OutputPath")]
        public string OutputPath { get; set; }
        public string InputPath { get; set; }
        public string IgnoreListPath { get; set; }
    }
}
