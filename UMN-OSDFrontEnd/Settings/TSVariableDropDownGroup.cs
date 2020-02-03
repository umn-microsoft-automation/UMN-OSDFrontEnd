using Newtonsoft.Json;

namespace UMN_OSDFrontEnd.Settings
{
    class TSVariableDropDownGroup
    {
        [JsonProperty(Required = Required.Always)]
        public string SetTSVariable { get; set; }
        public string TSVariable { get; set; }
        public string Delimiter { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string DefaultValue { get; set; }
        public string DefaultValueType { get; set; }
    }
}
