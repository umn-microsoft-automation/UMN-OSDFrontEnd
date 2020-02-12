using Newtonsoft.Json;

namespace UMN_OSDFrontEnd.Settings
{
    class ManualTextBlock
    {
        [JsonProperty(Required = Required.Always)]
        public string Text { get; set; }
    }
}
