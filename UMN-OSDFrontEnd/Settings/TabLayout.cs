using System.Collections.Generic;

namespace UMN_OSDFrontEnd.Settings
{
    class TabLayout
    {
        public string TabName { get; set; }
        public string TabDisplayName { get; set; }
        public string ItemType { get; set; }
        public string TextBlock { get; set; }
        public string GroupTitle { get; set; }
        public string CheckBoxOptionsDataType { get; set; }
        public string CheckBoxOptionsTSVariable { get; set; }
        public string CheckBoxOptionsTSVariableDelimiter { get; set; }
        public List<FrontEndCheckBox> ManualCheckBoxes { get; set; }
        public string DropDownOptionsDataType { get; set; }
        public string DropDownOptionsTSVariable { get; set; }
        public string DropDownOptionsTSVariableDelimiter { get; set; }
        public List<string> ManualDropDownOptions { get; set; }
    }
}
