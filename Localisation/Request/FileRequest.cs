using Newtonsoft.Json;

namespace Localisation.Request
{
    internal class FileRequest
    {
        [JsonProperty("format", Required = Required.Always)]
        public string Format { get; set; }
        [JsonProperty("original_filenames", Required = Required.Always)]
        public bool OriginalFilenames { get; set; }
    }
}
