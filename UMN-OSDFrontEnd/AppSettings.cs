using System.Collections.Generic;

namespace UMN_OSDFrontEnd {
    /// <summary>
    /// The root class for all settings contained in the AppSettings.json file.
    /// </summary>
    class AppSettings {
        public List<AppSettingsTab> Tabs { get; set; }
        public int LogoWidth { get; set; }
        public int LogoHeight { get; set; }
        public string LogoSource { get; set; }
    }
}
