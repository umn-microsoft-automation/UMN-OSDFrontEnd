using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using UMN_OSDFrontEnd.Settings;

namespace UMN_OSDFrontEnd
{
    internal class FrontEndTab : MetroTabItem
    {
        // Get MahApps.Metro styles
        private readonly Style NextButtonStyle = Application.Current.Resources["AccentedSquareButtonStyle"] as Style;
        private readonly Style CheckBoxStyle = Application.Current.Resources["MetroCheckBox"] as Style;
        private readonly Style GroupBoxStyle = Application.Current.Resources["MetroGroupBox"] as Style;
        private readonly Style ComboBoxStyle = Application.Current.Resources["MetroComboBox"] as Style;
        private readonly Style ComboBoxItemStyle = Application.Current.Resources["MetroComboBoxItem"] as Style;

        public bool Development { get; set; } = false;
        public Button NextButton;
        public RowDefinition MainRow;
        public RowDefinition NavRow;
        public StackPanel MainPanel;
        public ScrollViewer MainPanelScrollViewer;
        private readonly Grid TabGrid;

        static readonly Random random = new Random();

        public FrontEndTab(Tab tab)
        {
            Name = tab.TabName;
            Header = tab.TabDisplayName;
            IsEnabled = false;

            NextButton = new Button
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5, 5, 5, 5),
                Content = "Next >>",
                Width = 125,
                IsEnabled = true,
                Name = "NextButton",
                Style = NextButtonStyle
            };

            TabGrid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 0)
            };

            MainRow = new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Star)
            };

            NavRow = new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Auto)
            };

            MainPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            MainPanelScrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            MainPanelScrollViewer.Content = MainPanel;

            TabGrid.RowDefinitions.Add(MainRow);
            TabGrid.RowDefinitions.Add(NavRow);

            Grid.SetRow(MainPanelScrollViewer, 0);
            Grid.SetRow(NextButton, 1);
            TabGrid.Children.Add(NextButton);
            TabGrid.Children.Add(MainPanelScrollViewer);

            Content = TabGrid;
        }
        public FrontEndTab(Tab tab, bool development) : this(tab)
        {
            Development = development;
        }

        public StackPanel AddGroupBox(string groupTitle)
        {
            GroupBox groupBox = new GroupBox
            {
                Name = "CustomGroupBox",
                VerticalAlignment = VerticalAlignment.Top,
                Header = groupTitle,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(5, 5, 5, 5),
                Style = GroupBoxStyle
            };

            MainPanel.Children.Add(groupBox);

            StackPanel stackPanel = new StackPanel();
            groupBox.Content = stackPanel;

            return stackPanel;
        }

        public TextBlock AddTextBlock(string text)
        {
            TextBlock textBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5, 5, 5, 5),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Top,
                Text = text
            };

            MainPanel.Children.Add(textBlock);

            return textBlock;
        }

        public void AddCheckBoxGroup(List<FrontEndCheckBox> frontEndCheckBoxes, StackPanel stackPanel)
        {
            _ = frontEndCheckBoxes ?? throw new ArgumentNullException(nameof(frontEndCheckBoxes));

            foreach (FrontEndCheckBox frontEndCheckBox in frontEndCheckBoxes)
            {
                frontEndCheckBox.Style = CheckBoxStyle;
                stackPanel.Children.Add(frontEndCheckBox);
            }
        }

        public void AddDropDownGroup(string tsVariable, List<string> options, StackPanel stackPanel, string defaultSelectedValue = null)
        {
            FrontEndComboBox comboBox = new FrontEndComboBox
            {
                TSVariable = tsVariable,
                Style = ComboBoxStyle,
                SelectedIndex = 0
            };

            foreach (string option in options)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem
                {
                    Content = option,
                    Style = ComboBoxItemStyle
                };

                comboBox.Items.Add(comboBoxItem);
            }

            if (defaultSelectedValue != null)
            {
                int selectionIndex = -1;
                foreach (ComboBoxItem comboBoxItem in comboBox.Items)
                {
                    if (comboBoxItem.Content.ToString() == defaultSelectedValue)
                    {
                        selectionIndex = comboBox.Items.IndexOf(comboBoxItem);
                    }
                }

                if (selectionIndex > -1)
                {
                    comboBox.SelectedIndex = selectionIndex;
                }
                else
                {
                    if(Development)
                    {
                        MessageBox.Show("Didn't find index of " + defaultSelectedValue);
                    }
                }
            }

            stackPanel.Children.Add(comboBox);
        }

        public void ProcessTabLayout(List<TabLayout> tabLayout)
        {
            foreach (TabLayout tabLayoutItem in tabLayout)
            {
                switch (tabLayoutItem.ItemType)
                {
                    case "textBlock":
                        AddTextBlock(tabLayoutItem.ManualTextBlock.Text);
                        break;
                    case "checkBoxGroup":
                        StackPanel checkBoxGroupPanel = AddGroupBox(tabLayoutItem.GroupTitle);
                        List<FrontEndCheckBox> frontEndCheckBoxes = new List<FrontEndCheckBox>();
                        if (!Development)
                        {
                            Type EnvironmentType = Type.GetTypeFromProgID("Microsoft.SMS.TSEnvironment");
                            dynamic TSEnvironment = Activator.CreateInstance(EnvironmentType);

                            if (tabLayoutItem.ManualCheckBoxOptions != null)
                            {
                                frontEndCheckBoxes.AddRange(tabLayoutItem.ManualCheckBoxOptions.CheckBoxes);
                            }
                            else if (tabLayoutItem.TSVariableCheckBoxOptions != null)
                            {
                                string tsVariableOptions = TSEnvironment.Value[tabLayoutItem.TSVariableCheckBoxOptions.TSVariable];

                                foreach (string checkBoxItem in tsVariableOptions.Split(tabLayoutItem.TSVariableCheckBoxOptions.Delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                                {
                                    string tsVariableValue = "false";
                                    bool isChecked;

                                    try
                                    {
                                        tsVariableValue = TSEnvironment.Value[checkBoxItem];
                                    }
                                    catch
                                    {
                                        tsVariableValue = "false";
                                    }

                                    if(tsVariableValue.ToLower() == "true")
                                    {
                                        isChecked = true;
                                    }
                                    else
                                    {
                                        isChecked = false;
                                    }

                                    frontEndCheckBoxes.Add(new FrontEndCheckBox
                                    {
                                        Content = checkBoxItem,
                                        IsChecked = isChecked,
                                        TSVariable = checkBoxItem
                                    });
                                }
                            }
                        }
                        else
                        {
                            if(tabLayoutItem.ManualCheckBoxOptions != null)
                            {
                                frontEndCheckBoxes.AddRange(tabLayoutItem.ManualCheckBoxOptions.CheckBoxes);
                            }
                            else if(tabLayoutItem.TSVariableCheckBoxOptions != null)
                            {
                                string values = "Option-1" + tabLayoutItem.TSVariableCheckBoxOptions.Delimiter + "Option2" + tabLayoutItem.TSVariableCheckBoxOptions.Delimiter + "Option3";
                                foreach (string checkBoxItem in values.Split(tabLayoutItem.TSVariableCheckBoxOptions.Delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                                {
                                    frontEndCheckBoxes.Add(new FrontEndCheckBox
                                    {
                                        Content = checkBoxItem,
                                        IsChecked = false,
                                        TSVariable = checkBoxItem
                                    });
                                }
                            }
                        }

                        AddCheckBoxGroup(frontEndCheckBoxes, checkBoxGroupPanel);
                        break;
                    case "dropDownGroup":
                        StackPanel dropDownGroupPanel = AddGroupBox(tabLayoutItem.GroupTitle);
                        List<string> dropDownOptions = new List<string>();
                        string dropDownDefaultValue = null;
                        string dropDownName = RandomString(16);

                        if (tabLayoutItem.ManualDropDownOptions != null)
                        {
                            dropDownName = tabLayoutItem.ManualDropDownOptions.SetTSVariable;
                            dropDownOptions.AddRange(tabLayoutItem.ManualDropDownOptions.Items);
                        }
                        else if (tabLayoutItem.TSVariableDropDownOptions != null)
                        {
                            if (!Development)
                            {
                                dropDownName = tabLayoutItem.TSVariableDropDownOptions.SetTSVariable;
                                Type EnvironmentType = Type.GetTypeFromProgID("Microsoft.SMS.TSEnvironment");
                                dynamic TSEnvironment = Activator.CreateInstance(EnvironmentType);

                                string tsVariableOptions = TSEnvironment.Value[tabLayoutItem.TSVariableDropDownOptions.TSVariable];
                                if(tabLayoutItem.TSVariableDropDownOptions.DefaultValueType == "tsvariable")
                                {
                                    dropDownDefaultValue = TSEnvironment.Value[tabLayoutItem.TSVariableDropDownOptions.DefaultValue];
                                }
                                else
                                {
                                    dropDownDefaultValue = tabLayoutItem.TSVariableDropDownOptions.DefaultValue;
                                }

                                dropDownOptions.AddRange(tsVariableOptions.Split(tabLayoutItem.TSVariableDropDownOptions.Delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                            }
                            else
                            {
                                dropDownName = tabLayoutItem.TSVariableDropDownOptions.SetTSVariable;
                                dropDownDefaultValue = "Option3";

                                string values = "Option-1" + tabLayoutItem.TSVariableDropDownOptions.Delimiter + "Option2" + tabLayoutItem.TSVariableDropDownOptions.Delimiter + "Option3";
                                dropDownOptions.AddRange(values.Split(tabLayoutItem.TSVariableDropDownOptions.Delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                            }
                        }

                        AddDropDownGroup(dropDownName, dropDownOptions, dropDownGroupPanel, dropDownDefaultValue);
                        break;
                    default:
                        break;
                }
            }
        }

        public List<string> GetCheckedBoxesTSVariables()
        {
            List<string> returnList = new List<string>();

            IEnumerable<FrontEndCheckBox> checkBoxes = this.FindChildren<FrontEndCheckBox>();

            foreach (FrontEndCheckBox frontEndCheckBox in checkBoxes)
            {
                if (frontEndCheckBox.IsChecked == true)
                {
                    returnList.Add(frontEndCheckBox.TSVariable);
                }
            }

            return returnList;
        }

        public List<string> GetUncheckedBoxesTSVariables()
        {
            List<string> returnList = new List<string>();

            IEnumerable<FrontEndCheckBox> checkBoxes = this.FindChildren<FrontEndCheckBox>();

            foreach (FrontEndCheckBox frontEndCheckBox in checkBoxes)
            {
                if (frontEndCheckBox.IsChecked == false)
                {
                    returnList.Add(frontEndCheckBox.TSVariable);
                }
            }

            return returnList;
        }

        public Dictionary<string, string> GetDropDownValues()
        {
            Dictionary<string, string> returnVal = new Dictionary<string, string>();

            IEnumerable<FrontEndComboBox> comboBoxes = this.FindChildren<FrontEndComboBox>();

            foreach (FrontEndComboBox comboBox in comboBoxes)
            {
                returnVal.Add(comboBox.TSVariable, comboBox.Text);
            }

            return returnVal;
        }
        internal string SanitizeName(string name)
        {
            return Regex.Replace(name, @"/^[A-Za-z]+$/", "");
        }

        internal static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
