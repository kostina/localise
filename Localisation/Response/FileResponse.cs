using Newtonsoft.Json;

namespace Localisation.Response
{
    internal class FileResponse
    {
        [JsonProperty("project_id", Required = Required.Always)]
        public string ProjectId { get; set; }
        [JsonProperty("bundle_url", Required = Required.Always)]
        public string BundleId { get; set; }
    }
}
