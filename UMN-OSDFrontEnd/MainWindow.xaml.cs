using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Mono.Options;
using Newtonsoft.Json;

namespace UMN_OSDFrontEnd {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        private AppSettings Settings;
        private int ComputerNameGreaterThan;
        private int ComputerNameLessThan;
        private string ComputerNameStartsWith;
        private string ComputerNameEndsWith;
        private bool FormEntryComplete = false;
        private bool PreFlightPass = true;

        // CommandLine Arguments
        private bool Development = false;
        private bool InWinPE = false;

        // Import the DeleteProfile function from userenv.dll
        [DllImport( "userenv.dll", CharSet = CharSet.Unicode, ExactSpelling = false, SetLastError = true )]
        public static extern bool DeleteProfile( string lpSidString, string lpProfilePath, string lpComputerName );

        /// <summary>
        /// Main function for the form.
        /// </summary>
        public MainWindow() {
            InitializeComponent();
        }

        /// <summary>
        /// Handles all setup tasks as the form is loaded.
        /// </summary>
        /// <param name="sender">Contains the object of the control or object that generated the event.</param>
        /// <param name="e">Contains all the event data.</param>
        private void MetroWindow_Loaded( object sender, RoutedEventArgs e ) {
            string SettingsFile = File.ReadAllText( Path.Combine( AppDomain.CurrentDomain.BaseDirectory.ToString(), "AppSettings.json" ) );
            Settings = JsonConvert.DeserializeObject<AppSettings>( SettingsFile );

            // Parse Application Launch Switches
            string[] CmdArgs = Environment.GetCommandLineArgs();
            OptionSet Options = new OptionSet() {
                { "d|dev|development", v=>Development = true },
                { "pe|winpe", v=>InWinPE = true }
            };

            List<string> extra;
            try {
                extra = Options.Parse( CmdArgs );
            } catch(OptionException Ex) {
                MessageBox.Show( "Error parsing the arguments: " + Ex.Message );
            }

            // Universal Code (Both PE and Windows)
            if(Development) {
                MessageBox.Show( "Development environment selected, will not work in production." );
            } else {
                Type EnvironmentType = Type.GetTypeFromProgID( "Microsoft.SMS.TSEnvironment" );
                dynamic TSEnvironment = Activator.CreateInstance( EnvironmentType );

                if(TSEnvironment.Value["_SMSTSMachineName"] != null) {
                    TextBoxComputerName.Text = TSEnvironment.Value["_SMSTSMachineName"];
                } else {
                    TextBoxComputerName.Text = "UMN";
                }
            }

            // Setup all the tabs
            foreach( AppSettingsTab Tab in Settings.Tabs ) {
                // Handle ComputerName Tab Settings
                if( Tab.TabName == "TabComputerName" ) {
                    if( !Tab.Enabled ) {
                        TabControlMainWindow.Items.Remove( TabComputerName );
                    } else {
                        if( !Tab.RuleGreaterLessThanEnabled ) {
                            GridComputerNameRules.Children.Remove( LabelRuleGreaterThan );
                            GridComputerNameRules.Children.Remove( LabelRuleGreaterThanStatus );
                            GridComputerNameRules.Children.Remove( LabelRuleLessThan );
                            GridComputerNameRules.Children.Remove( LabelRuleLessThanStatus );
                            ButtonComputerNameNext.IsEnabled = true;
                        } else {
                            LabelRuleGreaterThan.Content = "REQ - Length >= " + Tab.RuleGreaterThan + ":";
                            LabelRuleLessThan.Content = "REQ - Length <= " + Tab.RuleLessThan + ":";
                            ComputerNameGreaterThan = Tab.RuleGreaterThan;
                            ComputerNameLessThan = Tab.RuleLessThan;
                        }

                        if( !Tab.RuleStartsWithEnabled ) {
                            GridComputerNameRules.Children.Remove( LabelRuleStartsWith );
                            GridComputerNameRules.Children.Remove( LabelRuleStartsWithStatus );
                        } else {
                            LabelRuleStartsWith.Content = "OPT - Starts With " + Tab.RuleStartsWith + ":";
                            ComputerNameStartsWith = Tab.RuleStartsWith;
                        }

                        if( !Tab.RuleEndsWithEnabled ) {
                            GridComputerNameRules.Children.Remove( LabelRuleEndsWith );
                            GridComputerNameRules.Children.Remove( LabelRuleEndsWithStatus );
                        } else {
                            LabelRuleEndsWith.Content = "OPT - Ends With " + Tab.RuleEndsWith + ":";
                            ComputerNameEndsWith = Tab.RuleEndsWith;
                        }
                    }
                }

                // Handle Pre Flight Checks Tab Settings
                if( Tab.TabName == "TabPreFlight" ) {
                    if( !Tab.Enabled ) {
                        TabControlMainWindow.Items.Remove( TabPreFlight );
                    } else {
                        foreach( AppSettingsPreFlight PreFlightCheck in Tab.PreFlightChecks ) {
                            bool CheckPass;
                            PreFlightCheckers preFlightCheckers = new PreFlightCheckers();

                            RowDefinition newRow = new RowDefinition();
                            newRow.Height = GridLength.Auto;
                            GridPreFlightChecks.RowDefinitions.Add( newRow );

                            Label newLabelDescription = new Label();
                            newLabelDescription.Content = PreFlightCheck.CheckDescription;
                            GridPreFlightChecks.Children.Add( newLabelDescription );
                            Grid.SetRow( newLabelDescription, GridPreFlightChecks.RowDefinitions.Count - 1 );
                            Grid.SetColumn( newLabelDescription, 0 );

                            switch( PreFlightCheck.CheckType ) {
                                case "offlineFilesDetected":
                                    if( PreFlightCheck.CheckPassState == preFlightCheckers.OfflineFilesDetected() ) {
                                        CheckPass = true;
                                    } else {
                                        CheckPass = false;
                                    }
                                    break;
                                case "physicalDiskCount":
                                    if( preFlightCheckers.PhysicalDiskCount( PreFlightCheck.DiskCheckLimit ) ) {
                                        CheckPass = true;
                                    } else {
                                        CheckPass = false;
                                    }
                                    break;
                                case "ethernetConnected":
                                    if( preFlightCheckers.EthernetNetworkConnectionDetected() ) {
                                        CheckPass = true;
                                    } else {
                                        CheckPass = false;
                                    }
                                    break;
                                case "networkConnectivityCheck":
                                    if( preFlightCheckers.TestNetworkConnectivity( PreFlightCheck.NetworkAddress ) ) {
                                        CheckPass = true;
                                    } else {
                                        CheckPass = false;
                                    }
                                    break;
                                case "64bitOS":
                                    if( Environment.Is64BitOperatingSystem ) {
                                        CheckPass = true;
                                    } else {
                                        CheckPass = false;
                                    }
                                    break;
                                default:
                                    CheckPass = false;
                                    break;
                            }

                            if( PreFlightCheck.Required && !CheckPass ) {
                                PreFlightPass = false;
                            }

                            Label newLabelStatus = new Label();

                            if( CheckPass ) {
                                newLabelStatus.Content = "Pass";
                                newLabelStatus.Foreground = (Brush)Application.Current.Resources["ValidItemBrush"];
                            } else {
                                newLabelStatus.Content = "Fail";
                                newLabelStatus.Foreground = (Brush)Application.Current.Resources["InvalidItemBrush"];
                            }

                            GridPreFlightChecks.Children.Add( newLabelStatus );
                            Grid.SetRow( newLabelStatus, GridPreFlightChecks.RowDefinitions.Count - 1 );
                            Grid.SetColumn( newLabelStatus, 1 );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles all clean up as the form closes.
        /// </summary>
        /// <param name="sender">Contains the object of the control or object that generated the event.</param>
        /// <param name="e">Contains all the event data.</param>
        private void MetroWindow_Closing( object sender, System.ComponentModel.CancelEventArgs e ) {
            // If the form hasn't been fully filled out, exit not successful for task sequence error handling.
            if( !FormEntryComplete ) {
                Environment.ExitCode = 22;
            }
        }

        /// <summary>
        /// Handles all next buttons on the main form.
        /// </summary>
        /// <param name="sender">Contains the object of the control or object that generated the event.</param>
        /// <param name="e">Contains all the event data.</param>
        private void NextButtonHandler( object sender, RoutedEventArgs e ) {
            TabItem CurrentTab = TabControlMainWindow.Items[TabControlMainWindow.SelectedIndex] as TabItem;
            bool NextTab = true;

            if(CurrentTab.Name == "TabPreFlight") {
                if(!PreFlightPass) {
                    MessageBox.Show( "Required pre flight checks not passing." );
                    NextTab = false;
                }
            }

            if(NextTab) {
                TabControlMainWindow.SelectedIndex++;
                ( TabControlMainWindow.Items[TabControlMainWindow.SelectedIndex] as TabItem ).IsEnabled = true;
            }
        }

        private void CompleteButtonHandler( object sender, RoutedEventArgs e ) {
            if(!Development) {
                Type EnvironmentType = Type.GetTypeFromProgID( "Microsoft.SMS.TSEnvironment" );
                dynamic TSEnvironment = Activator.CreateInstance( EnvironmentType );

                // Set the computer name in the task sequence
                TSEnvironment.Value["OSDComputerName"] = TextBoxComputerName.Text;
            } else {
                if(!InWinPE) {

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
        private void TextBoxComputerName_TextChanged( object sender, TextChangedEventArgs e ) {
            bool ButtonEnableLength = true;
            bool ButtonEnableStartsWith = true;
            bool ButtonEnableEndsWith = true;

            if( !string.IsNullOrEmpty(ComputerNameLessThan.ToString()) || !string.IsNullOrEmpty(ComputerNameGreaterThan.ToString()) ) {
                if(TextBoxComputerName.Text.Length >= ComputerNameGreaterThan) {
                    LabelRuleGreaterThanStatus.Foreground = (Brush)Application.Current.Resources["ValidItemBrush"];
                    LabelRuleGreaterThanStatus.Content = "True";
                } else {
                    LabelRuleGreaterThanStatus.Foreground = (Brush)Application.Current.Resources["InvalidItemBrush"];
                    LabelRuleGreaterThanStatus.Content = "False";
                }

                if( TextBoxComputerName.Text.Length <= ComputerNameLessThan ) {
                    LabelRuleLessThanStatus.Foreground = (Brush)Application.Current.Resources["ValidItemBrush"];
                    LabelRuleLessThanStatus.Content = "True";
                } else {
                    LabelRuleLessThanStatus.Foreground = (Brush)Application.Current.Resources["InvalidItemBrush"];
                    LabelRuleLessThanStatus.Content = "False";
                }

                if( TextBoxComputerName.Text.Length >= ComputerNameGreaterThan && TextBoxComputerName.Text.Length <= ComputerNameLessThan ) {
                    ButtonEnableLength = true;
                } else {
                    ButtonEnableLength = false;
                }
            }

            if(ComputerNameStartsWith != null) {
                if(TextBoxComputerName.Text.StartsWith(ComputerNameStartsWith)) {
                    LabelRuleStartsWithStatus.Foreground = (Brush)Application.Current.Resources["ValidItemBrush"];
                    LabelRuleStartsWithStatus.Content = "True";
                    ButtonEnableStartsWith = true;
                } else {
                    LabelRuleStartsWithStatus.Foreground = (Brush)Application.Current.Resources["InvalidItemBrush"];
                    LabelRuleStartsWithStatus.Content = "False";
                    ButtonEnableStartsWith = false;
                }
            }

            if(ComputerNameEndsWith != null) {
                if(TextBoxComputerName.Text.EndsWith(ComputerNameEndsWith)) {
                    LabelRuleEndsWithStatus.Foreground = (Brush)Application.Current.Resources["ValidItemBrush"];
                    LabelRuleEndsWithStatus.Content = "True";
                    ButtonEnableEndsWith = true;
                } else {
                    LabelRuleEndsWithStatus.Foreground = (Brush)Application.Current.Resources["InvalidItemBrush"];
                    LabelRuleEndsWithStatus.Content = "False";
                    ButtonEnableEndsWith = false;
                }
            }

            if(ButtonEnableLength) {
                ButtonComputerNameNext.IsEnabled = true;
            } else {
                ButtonComputerNameNext.IsEnabled = false;
            }
        }
    }
}
