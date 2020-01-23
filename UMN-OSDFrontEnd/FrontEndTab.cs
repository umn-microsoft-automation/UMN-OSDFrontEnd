using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using UMN_OSDFrontEnd.Settings;

namespace UMN_OSDFrontEnd
{
    class FrontEndTab : MetroTabItem
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
        private readonly Grid TabGrid;

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

            TabGrid.RowDefinitions.Add(MainRow);
            TabGrid.RowDefinitions.Add(NavRow);

            Grid.SetRow(MainPanel, 0);
            Grid.SetRow(NextButton, 1);
            TabGrid.Children.Add(NextButton);
            TabGrid.Children.Add(MainPanel);

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
            foreach (FrontEndCheckBox frontEndCheckBox in frontEndCheckBoxes)
            {
                frontEndCheckBox.Style = CheckBoxStyle;
                stackPanel.Children.Add(frontEndCheckBox);
            }
        }

        public void AddDropDownGroup(string dropDownName, List<string> dropDownOptions, StackPanel stackPanel)
        {
            ComboBox comboBox = new ComboBox
            {
                Name = dropDownName,
                Style = ComboBoxStyle,
                SelectedIndex = 0
            };

            foreach (string option in dropDownOptions)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem
                {
                    Name = option.Replace(" ", ""),
                    Content = option,
                    Style = ComboBoxItemStyle
                };

                comboBox.Items.Add(comboBoxItem);
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
                        AddTextBlock(tabLayoutItem.TextBlock);
                        break;
                    case "checkBoxGroup":
                        StackPanel checkBoxGroupPanel = AddGroupBox(tabLayoutItem.GroupTitle);
                        AddCheckBoxGroup(tabLayoutItem.ManualCheckBoxes, checkBoxGroupPanel);
                        break;
                    case "dropDownGroup":
                        StackPanel dropDownGroupPanel = AddGroupBox(tabLayoutItem.GroupTitle);
                        List<string> dropDownOptions = new List<string>();

                        if (tabLayoutItem.DropDownOptionsDataType.ToLower() == "string")
                        {
                            dropDownOptions.AddRange(tabLayoutItem.ManualDropDownOptions);
                        }
                        else if (tabLayoutItem.DropDownOptionsDataType.ToLower() == "tsvariable")
                        {
                            if (!Development)
                            {
                                Type EnvironmentType = Type.GetTypeFromProgID("Microsoft.SMS.TSEnvironment");
                                dynamic TSEnvironment = Activator.CreateInstance(EnvironmentType);

                                string TSVariableOptions = TSEnvironment.Value[tabLayoutItem.DropDownOptionsTSVariable];

                                dropDownOptions.AddRange(TSVariableOptions.Split(tabLayoutItem.DropDownOptionsTSVariableDelimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                            }
                            else
                            {
                                string values = "Option1" + tabLayoutItem.DropDownOptionsTSVariableDelimiter + "Option2" + tabLayoutItem.DropDownOptionsTSVariableDelimiter + "Option3";
                                dropDownOptions.AddRange(values.Split(tabLayoutItem.DropDownOptionsTSVariableDelimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                            }
                        }

                        AddDropDownGroup(tabLayoutItem.DropDownOptionsTSVariable, dropDownOptions, dropDownGroupPanel);
                        break;
                    default:
                        break;
                }
            }
        }

        public List<string> GetCheckedBoxesTSVariables()
        {
            List<string> returnList = new List<string>();

            var checkBoxes = this.FindChildren<FrontEndCheckBox>();

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

            var checkBoxes = this.FindChildren<FrontEndCheckBox>();

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

            var comboBoxes = this.FindChildren<ComboBox>();

            foreach (ComboBox comboBox in comboBoxes)
            {
                MessageBox.Show(comboBox.Name + " :: " + comboBox.Text);
            }

            return returnVal;
        }
    }
}
