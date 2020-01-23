using MahApps.Metro.Controls;
using Mono.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UMN_OSDFrontEnd.Settings;

namespace UMN_OSDFrontEnd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private AppSettings Settings;
        private int ComputerNameGreaterThan;
        private int ComputerNameLessThan;
        private string ComputerNameStartsWith;
        private string ComputerNameEndsWith;
        private bool FormEntryComplete = false;
        private bool PreFlightPass = true;
        private MessageBoxResult ProfileDeleteConfirm = new MessageBoxResult();
        private List<string> ProfilesForDeletion = new List<string>();
        ConfigMgrWebService WebService;
        private string AppSettingsJson;
        private List<FrontEndTab> CustomTabs = new List<FrontEndTab>();

        // CommandLine Arguments
        private bool Development = false;

        // Import the DeleteProfile function from userenv.dll
        [DllImport("userenv.dll", CharSet = CharSet.Unicode, ExactSpelling = false, SetLastError = true)]
        public static extern bool DeleteProfile(string lpSidString, string lpProfilePath, string lpComputerName);

        /// <summary>
        /// Main function for the form.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles all setup tasks as the form is loaded.
        /// </summary>
        /// <param name="sender">Contains the object of the control or object that generated the event.</param>
        /// <param name="e">Contains all the event data.</param>
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Parse Application Launch Switches
            string[] CmdArgs = Environment.GetCommandLineArgs();
            OptionSet Options = new OptionSet() {
                { "d|dev|development", v=>Development = true },
                { "s=|settings=", (string v) => AppSettingsJson = v }
            };

            List<string> extra;
            try
            {
                extra = Options.Parse(CmdArgs);
            }
            catch (OptionException Ex)
            {
                MessageBox.Show("Error parsing the arguments: " + Ex.Message);
            }

            string SettingsFile = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), AppSettingsJson));
            Settings = JsonConvert.DeserializeObject<AppSettings>(SettingsFile);

            // Setup WebService
            WebService = new ConfigMgrWebService(Settings.WebServiceURI);

            // Universal Code (Both PE and Windows)
            if (Development)
            {
                MessageBox.Show("Development environment selected, will not work in production.");
            }
            else
            {
                Type EnvironmentType = Type.GetTypeFromProgID("Microsoft.SMS.TSEnvironment");
                dynamic TSEnvironment = Activator.CreateInstance(EnvironmentType);

                if (TSEnvironment.Value["_SMSTSMachineName"] != null)
                {
                    TextBoxComputerName.Text = TSEnvironment.Value["_SMSTSMachineName"];
                }
            }

            // Use logo file
            BitmapImage OverlayImage = new BitmapImage();
            OverlayImage.BeginInit();
            OverlayImage.UriSource = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), Settings.LogoSource));
            OverlayImage.EndInit();
            OverlayLogo.Width = Settings.LogoWidth;
            OverlayLogo.Height = Settings.LogoHeight;
            OverlayLogo.Source = OverlayImage;

            // Setup all the tabs
            foreach (Tab tab in Settings.Tabs)
            {
                // Handle ComputerName Tab Settings
                if (tab.TabName == "TabComputerName")
                {
                    if (!tab.Enabled)
                    {
                        TabControlMainWindow.Items.Remove(TabComputerName);
                    }
                    else
                    {
                        if (!tab.RuleGreaterLessThanEnabled)
                        {
                            GridComputerNameRules.Children.Remove(LabelRuleGreaterThan);
                            GridComputerNameRules.Children.Remove(LabelRuleGreaterThanStatus);
                            GridComputerNameRules.Children.Remove(LabelRuleLessThan);
                            GridComputerNameRules.Children.Remove(LabelRuleLessThanStatus);
                            ButtonComputerNameNext.IsEnabled = true;
                        }
                        else
                        {
                            LabelRuleGreaterThan.Content = "REQ - Length >= " + tab.RuleGreaterThan + ":";
                            LabelRuleLessThan.Content = "REQ - Length <= " + tab.RuleLessThan + ":";
                            ComputerNameGreaterThan = tab.RuleGreaterThan;
                            ComputerNameLessThan = tab.RuleLessThan;
                        }

                        if (!tab.RuleStartsWithEnabled)
                        {
                            GridComputerNameRules.Children.Remove(LabelRuleStartsWith);
                            GridComputerNameRules.Children.Remove(LabelRuleStartsWithStatus);
                        }
                        else
                        {
                            LabelRuleStartsWith.Content = "REQ - Starts With " + tab.RuleStartsWith + ":";
                            ComputerNameStartsWith = tab.RuleStartsWith;
                        }

                        if (!tab.RuleEndsWithEnabled)
                        {
                            GridComputerNameRules.Children.Remove(LabelRuleEndsWith);
                            GridComputerNameRules.Children.Remove(LabelRuleEndsWithStatus);
                        }
                        else
                        {
                            LabelRuleEndsWith.Content = "REQ - Ends With " + tab.RuleEndsWith + ":";
                            ComputerNameEndsWith = tab.RuleEndsWith;
                        }
                    }
                }

                if (tab.TabName == "TabComputerBind")
                {
                    if (!tab.Enabled)
                    {
                        TabControlMainWindow.Items.Remove(TabComputerBind);
                    }
                    else
                    {
                        foreach (BindLocations BindLocation in tab.BindLocations)
                        {
                            TreeViewItem RootOU = new TreeViewItem
                            {
                                Header = BindLocation.RootName,
                                IsExpanded = false,
                                Focusable = false
                            };

                            TreeViewComputerBind.Items.Add(RootOU);

                            try
                            {
                                ADOrganizationalUnit[] organizationalUnits = WebService.GetADOrganizationalUnits(Settings.WebServiceKey, BindLocation.OU);
                                foreach (ADOrganizationalUnit ou in organizationalUnits)
                                {
                                    TreeViewItem subTreeItem = new TreeViewItem
                                    {
                                        Header = ou.Name,
                                        IsExpanded = false,
                                        Tag = ou.DistinguishedName
                                    };

                                    RootOU.Items.Add(subTreeItem);

                                    if (ou.HasChildren)
                                    {
                                        subTreeItem.Focusable = false;
                                        AddChildADNodes(ou, subTreeItem);
                                    }
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Error on AD entry: " + BindLocation.OU);
                            }
                        }
                    }
                }

                // Handle Pre Flight Checks Tab Settings
                if (tab.TabName == "TabPreFlight")
                {
                    if (!tab.Enabled)
                    {
                        TabControlMainWindow.Items.Remove(TabPreFlight);
                    }
                    else
                    {
                        foreach (PreFlightCheck preFlightCheck in tab.PreFlightChecks)
                        {
                            bool CheckPass;
                            PreFlightCheckers preFlightCheckers = new PreFlightCheckers();

                            RowDefinition newRow = new RowDefinition();
                            newRow.Height = GridLength.Auto;
                            GridPreFlightChecks.RowDefinitions.Add(newRow);

                            Label newLabelDescription = new Label();
                            newLabelDescription.Content = preFlightCheck.CheckDescription;
                            GridPreFlightChecks.Children.Add(newLabelDescription);
                            Grid.SetRow(newLabelDescription, GridPreFlightChecks.RowDefinitions.Count - 1);
                            Grid.SetColumn(newLabelDescription, 0);

                            switch (preFlightCheck.CheckType)
                            {
                                case "offlineFilesDetected":
                                    if (preFlightCheck.CheckPassState == preFlightCheckers.OfflineFilesDetected())
                                    {
                                        CheckPass = true;
                                    }
                                    else
                                    {
                                        CheckPass = false;
                                    }
                                    break;
                                case "physicalDiskCount":
                                    if (preFlightCheckers.PhysicalDiskCount(preFlightCheck.DiskCheckLimit))
                                    {
                                        CheckPass = true;
                                    }
                                    else
                                    {
                                        CheckPass = false;
                                    }
                                    break;
                                case "ethernetConnected":
                                    if (preFlightCheckers.EthernetNetworkConnectionDetected())
                                    {
                                        CheckPass = true;
                                    }
                                    else
                                    {
                                        CheckPass = false;
                                    }
                                    break;
                                case "networkConnectivityCheck":
                                    if (preFlightCheckers.TestNetworkConnectivity(preFlightCheck.NetworkAddress))
                                    {
                                        CheckPass = true;
                                    }
                                    else
                                    {
                                        CheckPass = false;
                                    }
                                    break;
                                case "64bitOS":
                                    if (Environment.Is64BitOperatingSystem)
                                    {
                                        CheckPass = true;
                                    }
                                    else
                                    {
                                        CheckPass = false;
                                    }
                                    break;
                                default:
                                    CheckPass = false;
                                    break;
                            }

                            if (preFlightCheck.Required && !CheckPass)
                            {
                                PreFlightPass = false;
                            }

                            Label newLabelStatus = new Label();

                            if (CheckPass)
                            {
                                newLabelStatus.Content = "Pass";
                                newLabelStatus.Foreground = (Brush)Application.Current.Resources["ValidItemBrush"];
                            }
                            else
                            {
                                newLabelStatus.Content = "Fail";
                                newLabelStatus.Foreground = (Brush)Application.Current.Resources["InvalidItemBrush"];
                            }

                            GridPreFlightChecks.Children.Add(newLabelStatus);
                            Grid.SetRow(newLabelStatus, GridPreFlightChecks.RowDefinitions.Count - 1);
                            Grid.SetColumn(newLabelStatus, 1);
                        }
                    }
                }
                if (tab.TabName == "TabUserProfiles")
                {
                    if (!tab.Enabled)
                    {
                        TabControlMainWindow.Items.Remove(TabUserProfiles);
                    }
                    else
                    {
                        SelectQuery Win32UserProfile = new SelectQuery("SELECT * FROM Win32_UserProfile WHERE Loaded != True");
                        ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Win32UserProfile);

                        SecurityIdentifier CurrentUser = null;
                        try
                        {
                            CurrentUser = WindowsIdentity.GetCurrent().User;
                        }
                        catch
                        {
                            MessageBox.Show("Error getting current user.");
                        }

                        foreach (ManagementObject Profile in Searcher.Get())
                        {
                            try
                            {
                                string UserProfileName = new SecurityIdentifier(Profile["SID"].ToString()).Translate(typeof(NTAccount)).ToString();
                                string UserProfileSid = Profile["SID"].ToString();

                                if (CurrentUser.Value != UserProfileSid.ToUpper())
                                {
                                    if (tab.DomainUsersOnly)
                                    {
                                        if (UserProfileName.StartsWith(tab.UserDomainPrefix))
                                        {
                                            ListBoxUserProfiles.Items.Add(UserProfileName);
                                        }
                                    }
                                    else
                                    {
                                        ListBoxUserProfiles.Items.Add(UserProfileName);
                                    }
                                }
                            }
                            catch (IdentityNotMappedException)
                            {
                                string userProfileName = Profile["LocalPath"].ToString();
                                string userProfileSid = Profile["SID"].ToString();
                                ListBoxUserProfiles.Items.Add(userProfileName);
                            }
                        }
                    }
                }

                if (tab.TabName == "TabBackupOptions")
                {
                    if (!tab.Enabled)
                    {
                        TabControlMainWindow.Items.Remove(TabBackupOptions);
                    }
                }

                if (tab.TabName == "TabApplications")
                {
                    if (!tab.Enabled)
                    {
                        TabControlMainWindow.Items.Remove(TabApplications);
                    }
                    else
                    {
                        foreach (SoftwareSection SoftwareSection in Settings.SoftwareSections)
                        {
                            TreeViewItem SectionHeader = new TreeViewItem
                            {
                                Header = SoftwareSection.SoftwareSectionName,
                                IsExpanded = true
                            };

                            foreach (SoftwareSubCategory SoftwareCategory in SoftwareSection.SubCategories)
                            {
                                TreeViewItem CategoryHeader = new TreeViewItem
                                {
                                    Header = SoftwareCategory.CategoryName,
                                    IsExpanded = true
                                };

                                CMApplication[] CategoryApps = WebService.GetCMApplicationByCategory(Settings.WebServiceKey, SoftwareCategory.CategorySCCM);

                                foreach (CMApplication Application in CategoryApps)
                                {
                                    TreeViewItem App = new TreeViewItem
                                    {
                                        Header = new CheckBox
                                        {
                                            Content = Application.ApplicationName
                                        }
                                    };

                                    CategoryHeader.Items.Add(App);
                                }

                                SectionHeader.Items.Add(CategoryHeader);
                            }

                            TreeViewApplications.Items.Add(SectionHeader);
                        }
                    }
                }

                if (tab.TabType == "CustomTab")
                {
                    if (tab.Enabled)
                    {
                        FrontEndTab tabItem = new FrontEndTab(tab, Development);
                        tabItem.NextButton.Click += NextButtonHandler;

                        tabItem.ProcessTabLayout(tab.TabLayout);

                        TabControlMainWindow.Items.Insert(TabControlMainWindow.Items.IndexOf(TabFinish), tabItem);
                        CustomTabs.Add(tabItem);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adOU"></param>
        /// <param name="tree"></param>
        private void AddChildADNodes(ADOrganizationalUnit adOU, TreeViewItem tree)
        {
            ADOrganizationalUnit[] organizationalUnits = WebService.GetADOrganizationalUnits(Settings.WebServiceKey, adOU.DistinguishedName.Replace("LDAP://", ""));

            foreach (ADOrganizationalUnit ou in organizationalUnits)
            {
                TreeViewItem newTreeItem = new TreeViewItem
                {
                    Header = ou.Name,
                    IsExpanded = false,
                    Tag = ou.DistinguishedName
                };

                tree.Items.Add(newTreeItem);

                if (ou.HasChildren)
                {
                    newTreeItem.Focusable = false;
                    AddChildADNodes(ou, newTreeItem);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            // Update computer name text box if it's been filled in (needs to be here to work properly).
            TextBoxComputerName_TextChanged(sender, null);
        }

        /// <summary>
        /// Handles all clean up as the form closes.
        /// </summary>
        /// <param name="sender">Contains the object of the control or object that generated the event.</param>
        /// <param name="e">Contains all the event data.</param>
        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If the form hasn't been fully filled out, exit not successful for task sequence error handling.
            if (!FormEntryComplete)
            {
                Environment.ExitCode = 22;
            }
        }

        /// <summary>
        /// Handles all next buttons on the main form.
        /// </summary>
        /// <param name="sender">Contains the object of the control or object that generated the event.</param>
        /// <param name="e">Contains all the event data.</param>
        private void NextButtonHandler(object sender, RoutedEventArgs e)
        {
            TabItem CurrentTab = TabControlMainWindow.Items[TabControlMainWindow.SelectedIndex] as TabItem;
            bool NextTab = true;

            if (CurrentTab.Name == "TabPreFlight")
            {
                if (!PreFlightPass)
                {
                    MessageBox.Show("Required pre flight checks not passing.");
                    NextTab = false;
                }
            }

            if (NextTab)
            {
                TabControlMainWindow.SelectedIndex++;
                (TabControlMainWindow.Items[TabControlMainWindow.SelectedIndex] as TabItem).IsEnabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Contains the object of the control or object that generated the event.</param>
        /// <param name="e">Contains all the event data.</param>
        private void CompleteButtonHandler(object sender, RoutedEventArgs e)
        {
            if (!Development)
            {
                Type environmentType = Type.GetTypeFromProgID("Microsoft.SMS.TSEnvironment");
                dynamic tsEnvironment = Activator.CreateInstance(environmentType);

                foreach (Tab tab in Settings.Tabs)
                {
                    if (tab.TabName == "TabComputerName" && tab.Enabled)
                    {
                        // Here is where we set the computer name based on text input and if that tab is enabled
                        tsEnvironment.Value["OSDComputerName"] = TextBoxComputerName.Text;
                    }

                    if (tab.TabName == "TabComputerBind" && tab.Enabled)
                    {
                        // Set computer bind location
                        if ((TreeViewItem)TreeViewComputerBind.SelectedItem != null)
                        {
                            tsEnvironment.Value["OSDOULocation"] = ((TreeViewItem)TreeViewComputerBind.SelectedItem).Tag.ToString();
                        }
                    }

                    if (tab.TabName == "TabBackupOptions" && tab.Enabled)
                    {
                        // Here is where we enable WIM backups if it's checked and the tab is enabled
                        if (WIMBackup.IsChecked.Value)
                        {
                            tsEnvironment.Value["OSDWIMBackup"] = "True";
                        }
                        else
                        {
                            tsEnvironment.Value["OSDWIMBackup"] = "False";
                        }

                        // Here is where we enable USMT backups if it's checked and the tab is enabled
                        if (USMTBackup.IsChecked.Value)
                        {
                            tsEnvironment.Value["OSDUSMTBackup"] = "True";
                        }
                        else
                        {
                            tsEnvironment.Value["OSDUSMTBackup"] = "False";
                        }
                    }

                    if (tab.TabName == "TabUserProfiles" && tab.Enabled)
                    {
                        DeleteUserProfiles(ProfilesForDeletion);
                    }

                    if (tab.TabName == "TabApplications" && tab.Enabled)
                    {
                        List<string> AppsToInstall = FindCheckedNodes(TreeViewApplications.Items);
                        int counter = 1;
                        foreach (string app in AppsToInstall)
                        {
                            string appCount = "APP" + counter.ToString("D2");
                            tsEnvironment.Value[appCount] = app;
                            counter++;
                        }
                    }
                }

                foreach (FrontEndTab frontEndTab in CustomTabs)
                {
                    List<string> checkedCheckBoxes = frontEndTab.GetCheckedBoxesTSVariables();
                    List<string> uncheckedCheckBoxes = frontEndTab.GetUncheckedBoxesTSVariables();
                    Dictionary<string, string> comboBoxTSVariables = frontEndTab.GetDropDownValues();

                    if (checkedCheckBoxes != null)
                    {
                        foreach (string checkBoxTSVariable in checkedCheckBoxes)
                        {
                            tsEnvironment.Value[checkBoxTSVariable] = "true";
                        }
                    }

                    if (uncheckedCheckBoxes != null)
                    {
                        foreach (string checkBoxTSVariable in uncheckedCheckBoxes)
                        {
                            tsEnvironment.Value[checkBoxTSVariable] = "false";
                        }
                    }

                    if (comboBoxTSVariables != null)
                    {
                        foreach (KeyValuePair<string, string> comboBoxSelection in comboBoxTSVariables)
                        {
                            tsEnvironment.Value[comboBoxSelection.Key] = comboBoxSelection.Value;
                        }
                    }
                }
            }
            else
            {
                foreach (Tab tab in Settings.Tabs)
                {
                    if (tab.TabName == "TabUserProfiles" && tab.Enabled)
                    {
                        DeleteUserProfiles(ProfilesForDeletion);
                    }

                    if (tab.TabName == "TabComputerBind" && tab.Enabled)
                    {
                        if ((TreeViewItem)TreeViewComputerBind.SelectedItem != null)
                        {
                            MessageBox.Show(((TreeViewItem)TreeViewComputerBind.SelectedItem).Tag.ToString());
                        }
                    }
                }

                foreach (FrontEndTab frontEndTab in CustomTabs)
                {
                    List<string> checkedCheckBoxes = frontEndTab.GetCheckedBoxesTSVariables();
                    List<string> uncheckedCheckBoxes = frontEndTab.GetUncheckedBoxesTSVariables();
                    Dictionary<string, string> comboBoxTSVariables = frontEndTab.GetDropDownValues();

                    if (checkedCheckBoxes != null)
                    {
                        foreach (string checkBoxTSVariable in checkedCheckBoxes)
                        {
                            MessageBox.Show(checkBoxTSVariable + " set to true");
                        }
                    }

                    if (uncheckedCheckBoxes != null)
                    {
                        foreach (string checkBoxTSVariable in uncheckedCheckBoxes)
                        {
                            MessageBox.Show(checkBoxTSVariable + " set to false");
                        }
                    }

                    if (comboBoxTSVariables != null)
                    {
                        foreach (KeyValuePair<string, string> comboBoxSelection in comboBoxTSVariables)
                        {
                            MessageBox.Show(comboBoxSelection.Key + " set to: " + comboBoxSelection.Value);
                        }
                    }
                }
            }

            FormEntryComplete = true;

            Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Contains the object of the control or object that generated the event.</param>
        /// <param name="e">Contains all the event data.</param>
        private void TextBoxComputerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool ButtonEnableLength = true;
            bool ButtonEnableStartsWith = true;
            bool ButtonEnableEndsWith = true;

            if (!string.IsNullOrEmpty(ComputerNameLessThan.ToString()) || !string.IsNullOrEmpty(ComputerNameGreaterThan.ToString()))
            {
                if (TextBoxComputerName.Text.Length >= ComputerNameGreaterThan)
                {
                    LabelRuleGreaterThanStatus.Foreground = (Brush)Application.Current.Resources["ValidItemBrush"];
                    LabelRuleGreaterThanStatus.Content = "True";
                }
                else
                {
                    LabelRuleGreaterThanStatus.Foreground = (Brush)Application.Current.Resources["InvalidItemBrush"];
                    LabelRuleGreaterThanStatus.Content = "False";
                }

                if (TextBoxComputerName.Text.Length <= ComputerNameLessThan)
                {
                    LabelRuleLessThanStatus.Foreground = (Brush)Application.Current.Resources["ValidItemBrush"];
                    LabelRuleLessThanStatus.Content = "True";
                }
                else
                {
                    LabelRuleLessThanStatus.Foreground = (Brush)Application.Current.Resources["InvalidItemBrush"];
                    LabelRuleLessThanStatus.Content = "False";
                }

                if (TextBoxComputerName.Text.Length >= ComputerNameGreaterThan && TextBoxComputerName.Text.Length <= ComputerNameLessThan)
                {
                    ButtonEnableLength = true;
                }
                else
                {
                    ButtonEnableLength = false;
                }
            }

            if (ComputerNameStartsWith != null)
            {
                if (TextBoxComputerName.Text.StartsWith(ComputerNameStartsWith))
                {
                    LabelRuleStartsWithStatus.Foreground = (Brush)Application.Current.Resources["ValidItemBrush"];
                    LabelRuleStartsWithStatus.Content = "True";
                    ButtonEnableStartsWith = true;
                }
                else
                {
                    LabelRuleStartsWithStatus.Foreground = (Brush)Application.Current.Resources["InvalidItemBrush"];
                    LabelRuleStartsWithStatus.Content = "False";
                    ButtonEnableStartsWith = false;
                }
            }

            if (ComputerNameEndsWith != null)
            {
                if (TextBoxComputerName.Text.EndsWith(ComputerNameEndsWith))
                {
                    LabelRuleEndsWithStatus.Foreground = (Brush)Application.Current.Resources["ValidItemBrush"];
                    LabelRuleEndsWithStatus.Content = "True";
                    ButtonEnableEndsWith = true;
                }
                else
                {
                    LabelRuleEndsWithStatus.Foreground = (Brush)Application.Current.Resources["InvalidItemBrush"];
                    LabelRuleEndsWithStatus.Content = "False";
                    ButtonEnableEndsWith = false;
                }
            }

            if (ButtonEnableLength && ButtonEnableStartsWith && ButtonEnableEndsWith)
            {
                ButtonComputerNameNext.IsEnabled = true;
            }
            else
            {
                ButtonComputerNameNext.IsEnabled = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Contains the object of the control or object that generated the event.</param>
        /// <param name="e">Contains all the event data.</param>
        private void SetProfileDeleteHandler(object sender, RoutedEventArgs e)
        {
            StringBuilder ProfileList = new StringBuilder();
            foreach (string Profile in ListBoxUserProfiles.SelectedItems)
            {
                ProfileList.AppendLine(" " + Profile);
                ProfilesForDeletion.Add(Profile);
            }

            ProfileDeleteConfirm = MessageBox.Show("Are you sure you want to delete profiles:\n" + ProfileList, "Delete Confirmation", MessageBoxButton.YesNo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Users"></param>
        private void DeleteUserProfiles(List<string> Users)
        {
            SelectQuery Query = new SelectQuery("SELECT * FROM Win32_UserProfile WHERE Loaded != True");
            ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Query);

            foreach (ManagementObject Profile in Searcher.Get())
            {
                string UserProfileName = "";

                try
                {
                    UserProfileName = new SecurityIdentifier(Profile["SID"].ToString()).Translate(typeof(NTAccount)).ToString();
                }
                catch (IdentityNotMappedException)
                {
                    UserProfileName = Profile["LocalPath"].ToString();
                }

                if (Users.Contains(UserProfileName))
                {
                    if (ProfileDeleteConfirm == MessageBoxResult.Yes)
                    {

                        Profile.Delete();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemCollection"></param>
        /// <returns></returns>
        private List<string> FindCheckedNodes(ItemCollection itemCollection)
        {
            List<string> checkedNodes = new List<string>();

            foreach (TreeViewItem item in itemCollection)
            {
                if (item.HasItems)
                {
                    checkedNodes.AddRange(FindCheckedNodes(item.Items));
                }
                else
                {
                    if (item.Header.GetType() == typeof(CheckBox))
                    {
                        CheckBox cb = (CheckBox)item.Header;
                        if (cb.IsChecked == true)
                        {
                            checkedNodes.Add(cb.Content.ToString());
                        }
                    }
                }
            }

            return checkedNodes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeselectUnwantedOUs(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TreeViewComputerBind.SelectedItem != null)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(("Are you sure you want to bind to this location?\n\n" + ((TreeViewItem)TreeViewComputerBind.SelectedItem).Tag.ToString()), "Confirm OU Bind Location", MessageBoxButton.YesNo);

                if (messageBoxResult == MessageBoxResult.No)
                {
                    TreeViewItem item = TreeViewComputerBind.SelectedItem as TreeViewItem;
                    if (item != null)
                    {
                        TreeViewComputerBind.Focus();
                        item.IsSelected = false;
                    }
                }
            }
        }
    }
}
