using System.Collections.Generic;

namespace UMN_OSDFrontEnd.Settings
{
    /// <summary>
    /// Contains all the information possible for every tab setting in the AppSettings.json file.
    /// </summary>
    class Tab
    {
        public string TabName { get; set; }
        public string TabDisplayName { get; set; }
        public string TabType { get; set; }
        public bool Enabled { get; set; } = true;
        public int RuleLessThan { get; set; }
        public int RuleGreaterThan { get; set; }
        public bool RuleGreaterLessThanEnabled { get; set; }
        public string RuleStartsWith { get; set; }
        public bool RuleStartsWithEnabled { get; set; }
        public string RuleEndsWith { get; set; }
        public bool RuleEndsWithEnabled { get; set; }
        public List<PreFlightCheck> PreFlightChecks { get; set; }
        public bool DomainUsersOnly { get; set; }
        public string UserDomainPrefix { get; set; }
        public List<BindLocations> BindLocations { get; set; }
        public List<TabLayout> TabLayout { get; set; }
    }
}
