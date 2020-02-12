namespace UMN_OSDFrontEnd.Settings
{
    /// <summary>
    /// Contains all possible settings for an entry in the PreFlight checks in the AppSettings.json file.
    /// </summary>
    internal class PreFlightCheck
    {
        public string CheckName { get; set; }
        public string CheckType { get; set; }
        public string CheckDescription { get; set; }
        public bool Required { get; set; }
        public bool CheckPassState { get; set; }
        public string NetworkAddress { get; set; }
        public int DiskCheckLimit { get; set; }
    }
}
