namespace UMN_OSDFrontEnd.Settings
{
    /// <summary>
    /// The root class for all settings contained in the AppSettings.json file.
    /// </summary>
    class AppSettings
    {
        public Tab[] Tabs { get; set; }
        public int LogoWidth { get; set; }
        public int LogoHeight { get; set; }
        public string LogoSource { get; set; }
        public string WebServiceURI { get; set; }
        public string WebServiceKey { get; set; }
        public SoftwareSection[] SoftwareSections { get; set; }
    }
}
