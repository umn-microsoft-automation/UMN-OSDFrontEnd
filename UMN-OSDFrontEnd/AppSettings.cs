namespace UMN_OSDFrontEnd
{
    /// <summary>
    /// The root class for all settings contained in the AppSettings.json file.
    /// </summary>
    class AppSettings
    {
        public AppSettingsTab[] Tabs { get; set; }
        public int LogoWidth { get; set; }
        public int LogoHeight { get; set; }
        public string LogoSource { get; set; }
        public string WebServiceURI { get; set; }
        public string WebServiceKey { get; set; }
        public AppSettingsSoftwareSection[] SoftwareSections { get; set; }
    }
}
