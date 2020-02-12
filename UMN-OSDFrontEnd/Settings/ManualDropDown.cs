using Newtonsoft.Json;
using System.Collections.Generic;

namespace UMN_OSDFrontEnd.Settings
{
    class ManualDropDown
    {
        [JsonProperty(Required = Required.Always)]
        public List<string> Items { get; set; }
        public string DefaultValue { get; set; }
        public string DefaultValueType { get; set; }
        public string SetTSVariable { get; set; }
    }
}
