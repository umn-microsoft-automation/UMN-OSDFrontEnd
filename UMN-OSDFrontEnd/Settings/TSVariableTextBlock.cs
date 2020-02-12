using Newtonsoft.Json;

namespace UMN_OSDFrontEnd.Settings
{
    class TSVariableTextBlock
    {
        [JsonProperty(Required = Required.Always)]
        public string TSVariable { get; set; }
    }
}
