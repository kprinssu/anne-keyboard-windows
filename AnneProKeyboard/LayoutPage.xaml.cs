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
    public sealed partial class LayoutPage : Page
    {
        //public MainPage ParentWindow { get; set; }

        private ObservableCollection<KeyboardProfileItem> _keyboardProfiles = new ObservableCollection<KeyboardProfileItem>();
        public KeyboardProfileItem EditingProfile;
        public KeyboardProfileItem RenamingProfile;

        public ObservableCollection<KeyboardProfileItem> KeyboardProfiles
        {
            get { return _keyboardProfiles; }
            set { }
        }

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

            LoadProfiles();

            this._keyboardProfiles.CollectionChanged += KeyboardProfiles_CollectionChanged;
        }

        public async void SaveProfiles()
        {
            MemoryStream memory_stream = new MemoryStream();
            DataContractSerializer serialiser = new DataContractSerializer(typeof(ObservableCollection<KeyboardProfileItem>));
            serialiser.WriteObject(memory_stream, this._keyboardProfiles);
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("KeyboardProfilesData", CreationCollisionOption.ReplaceExisting);
                using (Stream file_stream = await file.OpenStreamForWriteAsync())
                {
                    memory_stream.Seek(0, SeekOrigin.Begin);
                    await memory_stream.CopyToAsync(file_stream);
                    await file_stream.FlushAsync();
                }
            } catch(UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException();
            }
        }

        public async void LoadProfiles()
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("KeyboardProfilesData");
                using (IInputStream inStream = await file.OpenSequentialReadAsync())
                {
                    DataContractSerializer serialiser = new DataContractSerializer(typeof(ObservableCollection<KeyboardProfileItem>));
                    ObservableCollection <KeyboardProfileItem> saved_profiles = (ObservableCollection<KeyboardProfileItem>)serialiser.ReadObject(inStream.AsStreamForRead());

                    foreach (KeyboardProfileItem profile in saved_profiles)
                    {
                        if (!_keyboardProfiles.Contains(profile))
                        {
                            this._keyboardProfiles.Add(profile);
                        }
                    }
                }
            }
            catch
            {
            }

            // UI init code
            if (this._keyboardProfiles.Count == 0)
            {
                this.CreateNewKeyboardProfile();
            }

            ChangeSelectedProfile(this._keyboardProfiles[0]);
        }

		private void CreateNewKeyboardProfile()
        {
            KeyboardProfileItem profile_item = new KeyboardProfileItem(this._keyboardProfiles.Count, "Profile " + (this._keyboardProfiles.Count + 1));
            this._keyboardProfiles.Add(profile_item);
        }

        private void ChangeSelectedProfile(KeyboardProfileItem profile)
        {
            if(profile == null)
            {
                return;
            }
            this.EditingProfile = profile;

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

        //private void KeyboardProfiles_ItemClick(object sender, EventArgs e)
        //{
        //    KeyboardProfileItem profile = ProfilesCombo.SelectedItem as KeyboardProfileItem;
        //    ChangeSelectedProfile(profile);
        //}

        private void ProfileNameChangedEvent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox profileName = (sender as TextBox);
            // update Views and KeyboardProfileItem class
            if(this.EditingProfile == RenamingProfile)
            {
            }
            
            if(this.RenamingProfile != null)
            {
                this.RenamingProfile.Label = profileName.Text;
            }
        }

        private void ProfileAddButton_Click(object sender, RoutedEventArgs e)
        {
            this.CreateNewKeyboardProfile();
            this.SaveProfiles();
            this.ChangeSelectedProfile(_keyboardProfiles[_keyboardProfiles.Count - 1]);
            this.LayoutProfilesCombo.SelectedIndex = this.LayoutProfilesCombo.Items.Count - 1;
        }

        private void ProfileEditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FrameworkElement parent = (FrameworkElement)button.Parent;

            TextBox textbox = (TextBox)parent.FindName("ProfileNameTextbox");
            textbox.IsEnabled = true;
            textbox.Visibility = Visibility.Visible;
            FocusState focus_state = FocusState.Keyboard;
            textbox.Focus(focus_state);

            TextBlock textblock = (TextBlock)parent.FindName("ProfileNameTextblock");
            textblock.Visibility = Visibility.Collapsed;

            this.RenamingProfile = this._keyboardProfiles[(int)button.Tag];
        }

        private void ProfileDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FrameworkElement parent = (FrameworkElement)button.Parent;
            TextBox textbox = (TextBox)parent.FindName("ProfileNameTextbox");
            KeyboardProfileItem selected_profile = this._keyboardProfiles[(int)button.Tag];

            this._keyboardProfiles.Remove(selected_profile);
            //this.LayoutProfilesCombo.Items.Remove(selected_profile);

            // always make sure that the keyboard profiles list has 1 element in it
            if (this._keyboardProfiles.Count == 0)
            {
                this.CreateNewKeyboardProfile();
            }

            // Change the chosen profile to the first element
            ChangeSelectedProfile(this._keyboardProfiles[0]);
            LayoutProfilesCombo.SelectedIndex = 0;
            LayoutProfilesCombo.IsDropDownOpen = false;
            this.SaveProfiles();
        }
        
        private void ProfileNameTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.SaveProfiles();

            TextBox textbox = (TextBox)sender;

            this.RenamingProfile = null;
        }
  
        private void KeyboardProfiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
            {
                // Really inefficient, we should consider re-implementing this later
                for (int i = 0; i < this._keyboardProfiles.Count; i++)
                {
                    KeyboardProfileItem profile = this._keyboardProfiles[i];
                    profile.ID = i;
                }
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
                    this.EditingProfile.FnKeys[index] = KeyboardKey.StringKeyboardKeys[full_label];


                    this.CurrentlyEditingFnKey.Content = label;
                    this.CurrentlyEditingFnKey.Visibility = Visibility.Visible;
                }
                else
                {
                    this.EditingProfile.NormalKeys[index] = KeyboardKey.StringKeyboardKeys[full_label];

                    this.CurrentlyEditingStandardKey.Content = label;
                    this.CurrentlyEditingStandardKey.Visibility = Visibility.Visible;
                }
            }

            this.SaveProfiles();
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

        private void LayoutProfilesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LayoutProfilesCombo == null) return;
            var combo = (ComboBox) sender;
            var item = (KeyboardProfileItem) combo.SelectedItem;
            ChangeSelectedProfile(item);
        }

        private void LayoutProfilesCombo_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LayoutProfilesCombo.SelectedIndex = 0;
            } catch
            {
                //some occassions the load doesn't load correctly
                //dont set any in combobox
            }
        }
    }
}
