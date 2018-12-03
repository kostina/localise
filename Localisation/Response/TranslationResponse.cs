using Newtonsoft.Json;

namespace Localisation.Response
{
    internal class TranslationResponse
    {
        [JsonProperty("language_iso", Required = Required.Always)]
        public string LanguageISO { get; set; }

        [JsonProperty("translation", Required = Required.AllowNull)]
        public string Translation { get; set; }
    }
}
