using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Core;
using Windows.Devices.Bluetooth;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AnneProKeyboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LayoutPage : Page, IContentPage
    {
        //public MainPage ParentWindow { get; set; }

        public KeyboardProfileItem editingProfile;
        public KeyboardProfileItem RenamingProfile;

        public ObservableCollection<String> KeyboardKeyLabels = new ObservableCollection<String>();
        private Dictionary<String, String> KeyboardKeyLabelTranslation = new Dictionary<String, String>();

        private Button CurrentlyEditingStandardKey;
        private Button CurrentlyEditingFnKey;

        public LayoutPage()
        {
            foreach(KeyboardKey key in KeyboardKey.StringKeyboardKeys.Values)
            {
                KeyboardKeyLabels.Add(key.KeyShortLabel);
                KeyboardKeyLabelTranslation[key.KeyShortLabel] = key.KeyLabel;
            }

            this.InitializeComponent();
        }

        public void ChangeSelectedProfile(KeyboardProfileItem profile)
        {
            if(profile == null)
            {
                return;
            }
            this.editingProfile = profile;

            // set up the background colours for the keyboard lights
            for (int i = 0; i < 70; i++)
            {
                // set the standard layout keys
                if (i < 61)
                {
                    string layout_id = "keyboardStandardLayoutButton" + i;
                    Button layout_button = (this.FindName(layout_id) as Button);

                    if (layout_button != null)
                    {
                        layout_button.Content = profile.NormalKeys[i].KeyShortLabel;
                    }

                    string fn_layout_id = "keyboardFNLayoutButton" + i;
                    Button fn_layout_button = (this.FindName(fn_layout_id) as Button);

                    if (fn_layout_id != null)
                    {
                        if (profile.FnKeys[i].KeyShortLabel == "None")
                        {
                            fn_layout_button.Content = "";
                        }
                        else
                        {
                            fn_layout_button.Content = profile.FnKeys[i].KeyShortLabel;
                        }
                    }
                }

                string s = "keyboardButton" + i;
                Button button = (this.FindName(s) as Button);
                
                // NOTE: last 4 keys (RALT, FN, Anne, Ctrl) are identify as 66,67,68,69
                if (button == null)
                {
                    continue;
                }
                int coloured_int = profile.KeyboardColours[i];
            }
        }

        private async void KeyboardLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            if(this.CurrentlyEditingFnKey != null)
            {
                this.CurrentlyEditingFnKey.Visibility = Visibility.Visible;
                this.CurrentlyEditingFnKey = null;
            }

            if(this.CurrentlyEditingStandardKey != null)
            {
                this.CurrentlyEditingStandardKey.Visibility = Visibility.Visible;
                this.CurrentlyEditingStandardKey = null;
            }

            Button button = (Button)sender;
			
            bool fn_mode = button.Name.StartsWith("keyboardFNLayoutButton");

            if(fn_mode)
            {
                this.CurrentlyEditingFnKey = button;
            }
            else
            {
                this.CurrentlyEditingStandardKey = button;
            }

            // Switch parents
            RelativePanel selector_parent = (RelativePanel)keyboardLayoutSelection.Parent;
            selector_parent.Children.Remove(keyboardLayoutSelection);

            //add the combobox to relative panel and place it into the correct position
            RelativePanel parent = (RelativePanel)button.Parent;
            parent.Children.Add(keyboardLayoutSelection);
            var button_idx = parent.Children.IndexOf(button);
            var width = button.Width + 1;
            if (button_idx != 0)
            {
                RelativePanel.SetRightOf(keyboardLayoutSelection, parent.Children[parent.Children.IndexOf(button) - 1]);
            } else
            {
                RelativePanel.SetRightOf(keyboardLayoutSelection, null);
                width += 1;
            }
            if (button_idx != parent.Children.Count - 2) //added combobox, account for it here
            {
                RelativePanel.SetRightOf(parent.Children[parent.Children.IndexOf(button) + 1], keyboardLayoutSelection);
            }
            keyboardLayoutSelection.Width = width; //account for 1px between buttons
            keyboardLayoutSelection.SelectedIndex = this.KeyboardKeyLabels.IndexOf((string)button.Content);
            
            button.Visibility = Visibility.Collapsed;
            
            keyboardLayoutSelection.Visibility = Visibility.Visible;

            await Task.Delay(1);
            
            keyboardLayoutSelection.IsDropDownOpen = true;
        }

        private void KeyboardStandardLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string label = keyboardLayoutSelection.SelectedItem as string;

            if(String.IsNullOrEmpty(label))
            {
                label = KeyboardKey.IntKeyboardKeys[0].KeyShortLabel;
            }

            Button button = (this.CurrentlyEditingStandardKey != null) ? this.CurrentlyEditingStandardKey : this.CurrentlyEditingFnKey;
            int length = (this.CurrentlyEditingStandardKey != null) ? 28 : 22; // special constants 

            int index = -1;

            try
            {
                Int32.TryParse(button.Name.Substring(length), out index);
            }
            catch
            {
            }

            if(index >= 0)
            {
                string full_label = this.KeyboardKeyLabelTranslation[label];

                if(this.CurrentlyEditingFnKey != null)
                {
                    this.editingProfile.FnKeys[index] = KeyboardKey.StringKeyboardKeys[full_label];


                    this.CurrentlyEditingFnKey.Content = label;
                    this.CurrentlyEditingFnKey.Visibility = Visibility.Visible;
                }
                else
                {
                    this.editingProfile.NormalKeys[index] = KeyboardKey.StringKeyboardKeys[full_label];

                    this.CurrentlyEditingStandardKey.Content = label;
                    this.CurrentlyEditingStandardKey.Visibility = Visibility.Visible;
                }
            }
        }

        private void KeyboardStandardLayout_DropDownClosed(object sender, object e)
        {

            if (this.CurrentlyEditingFnKey != null)
            {
                this.CurrentlyEditingFnKey.Visibility = Visibility.Visible;
                RelativePanel parent = (RelativePanel)CurrentlyEditingFnKey.Parent;
                var button_idx = parent.Children.IndexOf(CurrentlyEditingFnKey);
                if (button_idx != 0)
                {
                    RelativePanel.SetRightOf(CurrentlyEditingFnKey, parent.Children[parent.Children.IndexOf(CurrentlyEditingFnKey) - 1]);
                }
                if (button_idx != parent.Children.Count - 2)
                {
                    RelativePanel.SetRightOf(parent.Children[parent.Children.IndexOf(CurrentlyEditingFnKey) + 1], CurrentlyEditingFnKey);
                }
            }

            if (this.CurrentlyEditingStandardKey != null)
            {
                this.CurrentlyEditingStandardKey.Visibility = Visibility.Visible;
                RelativePanel parent = (RelativePanel)CurrentlyEditingStandardKey.Parent;
                var button_idx = parent.Children.IndexOf(CurrentlyEditingStandardKey);
                if (button_idx != 0)
                {
                    RelativePanel.SetRightOf(CurrentlyEditingStandardKey, parent.Children[parent.Children.IndexOf(CurrentlyEditingStandardKey) - 1]);
                }
                if (button_idx != parent.Children.Count - 2)
                {
                    RelativePanel.SetRightOf(parent.Children[parent.Children.IndexOf(CurrentlyEditingStandardKey) + 1], CurrentlyEditingStandardKey);
                }
            }

            this.keyboardLayoutSelection.Visibility = Visibility.Collapsed;
        }
    }
}
