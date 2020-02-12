using Newtonsoft.Json;

namespace UMN_OSDFrontEnd.Settings
{
    class TSVariableCheckBoxes
    {
        [JsonProperty(Required = Required.Always)]
        public string TSVariable { get; set; }
        public string Delimiter { get; set; }
    }
}
