using Newtonsoft.Json;
using System.Collections.Generic;

namespace Localisation.Response
{
    internal class KeyResponse
    {
        [JsonProperty("key_id", Required = Required.Always)]
        public string KeyId { get; set; }

        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("key_name", Required = Required.Always)]
        public Dictionary<string, string> KeyName { get; set; }

        [JsonProperty("translations", Required = Required.Always)]
        public List<TranslationResponse> Translations { get; set; }
    }
}
