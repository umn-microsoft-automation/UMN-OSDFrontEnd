using Newtonsoft.Json;
using System.Collections.Generic;

namespace UMN_OSDFrontEnd.Settings
{
    internal class ManualCheckBoxes
    {
        [JsonProperty(Required = Required.Always)]
        public List<FrontEndCheckBox> CheckBoxes { get; set; }
    }
}
