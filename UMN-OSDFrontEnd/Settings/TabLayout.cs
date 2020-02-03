using System.Collections.Generic;
using Newtonsoft.Json;

namespace UMN_OSDFrontEnd.Settings
{
    internal class TabLayout
    {
        [JsonProperty(Required = Required.Always)]
        public string GroupTitle { get; set; }
        public string ItemType { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public bool Dynamic { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public ManualCheckBoxes ManualCheckBoxOptions { get; set; }
        public ManualDropDown ManualDropDownOptions { get; set; }
        public ManualTextBlock ManualTextBlock { get; set; }
        public TSVariableCheckBoxes TSVariableCheckBoxOptions { get; set; }
        public TSVariableDropDownGroup TSVariableDropDownOptions { get; set; }
        public TSVariableTextBlock TSVariableTextBlock { get; set; }

        public bool ShouldSerializeManualCheckBoxes()
        {
            return (ItemType == "checkBoxGroup" && !Dynamic);
        }

        public bool ShouldSerializeManualDropDownOptions()
        {
            return (ItemType == "dropDownGroup" && !Dynamic);
        }

        public bool ShouldSerializeManualTextBlock()
        {
            return (ItemType == "textBlock" && !Dynamic);
        }

        public bool ShouldSerializeTSVariableCheckBoxes()
        {
            return (ItemType == "checkBoxGroup" && Dynamic);
        }

        public bool ShouldSerializeTSVariableDropDownOptions()
        {
            return (ItemType == "dropDownGroup" && Dynamic);
        }

        public bool ShouldSerializeTSVariableTextBlock()
        {
            return (ItemType == "textBlock" && Dynamic);
        }
    }
}
