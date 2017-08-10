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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AnneProKeyboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class LightingPage : Page
    {
        private ObservableCollection<KeyboardProfileItem> _keyboardProfiles = new ObservableCollection<KeyboardProfileItem>();
        public KeyboardProfileItem EditingProfile;
        public KeyboardProfileItem RenamingProfile;

        private Color SelectedColour;
        private Boolean matchingButtonColour = false;

        public ObservableCollection<KeyboardProfileItem> KeyboardProfiles
        {
            get { return _keyboardProfiles; }
            set { }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        public LightingPage()
        {
            this.InitializeComponent();
            LoadProfiles();
            SelectedColour = colourPicker.SelectedColor.Color;
            this._keyboardProfiles.CollectionChanged += KeyboardProfiles_CollectionChanged;
        }

        public async void SaveProfiles()
        {
            MemoryStream memory_stream = new MemoryStream();
            DataContractSerializer serialiser = new DataContractSerializer(typeof(ObservableCollection<KeyboardProfileItem>));
            serialiser.WriteObject(memory_stream, this._keyboardProfiles);

            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("KeyboardProfilesData", CreationCollisionOption.ReplaceExisting);
            using (Stream file_stream = await file.OpenStreamForWriteAsync())
            {
                memory_stream.Seek(0, SeekOrigin.Begin);
                await memory_stream.CopyToAsync(file_stream);
                await file_stream.FlushAsync();
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
                    ObservableCollection<KeyboardProfileItem> saved_profiles = (ObservableCollection<KeyboardProfileItem>)serialiser.ReadObject(inStream.AsStreamForRead());

                    foreach (KeyboardProfileItem profile in saved_profiles)
                    {
                        this._keyboardProfiles.Add(profile);
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
            this.EditingProfile = profile;
            //chosenProfileName.Title = profile.Label;

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

                }

                string s = "keyboardButton" + i;
                Button button = (this.FindName(s) as Button);

                // NOTE: last 4 keys (RALT, FN, Anne, Ctrl) are identify as 66,67,68,69
                if (button == null)
                {
                    continue;
                }

                int coloured_int = profile.KeyboardColours[i];

                Color colour = ConvertIntToColour(coloured_int);

                button.BorderBrush = new SolidColorBrush(colour);
                button.BorderThickness = new Thickness(1);
                button.Background = new SolidColorBrush(colour);
            }

            //colour the multi selection buttons
            //check the WASD keys. If all same, color WASD button
            Color multi_colour = ConvertIntToColour(profile.KeyboardColours[16]); //W key idx
            Button multi_button = (this.FindName("WASDKeys") as Button);
            Color default_colour = Color.FromArgb(255,94,97,102);
            if (colour_wasd(profile))
            {
                setButtonColour(multi_button, multi_colour);
            }
            else
            {
                setButtonColour(multi_button, default_colour);
            }

            //check IJKL keys
            if (colour_ijkl(profile))
            {
                multi_colour = ConvertIntToColour(profile.KeyboardColours[22]); //I key idx
                multi_button = (this.FindName("IJKLKeys") as Button);
                setButtonColour(multi_button, multi_colour);
            }
            else
            {
                setButtonColour(multi_button, default_colour);
            }

            //check Modifier Keys
            if (colour_modifiers(profile))
            {
                multi_colour = ConvertIntToColour(profile.KeyboardColours[14]); //Tab key idx
                multi_button = (this.FindName("ModifierKeys") as Button);
                setButtonColour(multi_button, multi_colour);
            }
            else
            {
                setButtonColour(multi_button, default_colour);
            }

            //check num row
            if (colour_num_row(profile))
            {
                multi_colour = ConvertIntToColour(profile.KeyboardColours[1]); //1 key idx
                multi_button = (this.FindName("NumKeys") as Button);
                setButtonColour(multi_button, multi_colour);
            }
            else
            {
                setButtonColour(multi_button, default_colour);
            }

            //set all buttons colour to default colour. Easy to see if all buttons are the same colour
            multi_button = (this.FindName("AllKeys") as Button);
            setButtonColour(multi_button, default_colour);
        }

        private Boolean colour_wasd(KeyboardProfileItem profile)
        {
            return (
                profile.KeyboardColours[16] == profile.KeyboardColours[29] &&
                profile.KeyboardColours[29] == profile.KeyboardColours[30] &&
                profile.KeyboardColours[30] == profile.KeyboardColours[31]
            );
        }

        private Boolean colour_ijkl(KeyboardProfileItem profile)
        {
            return (
                profile.KeyboardColours[22] == profile.KeyboardColours[35] &&
                profile.KeyboardColours[35] == profile.KeyboardColours[36] &&
                profile.KeyboardColours[36] == profile.KeyboardColours[37]
            );
        }

        private Boolean colour_num_row(KeyboardProfileItem profile)
        {
            int colour = profile.KeyboardColours[1];
            for (int i = 2; i < 11; i++)
            {
                if (profile.KeyboardColours[i] != colour)
                    return false;
            }
            return true;
        }

        private Boolean colour_modifiers(KeyboardProfileItem profile)
        {
            int[] modifiers = new int[14] { 14, 28, 42, 56, 57, 58, 66, 67, 68, 69, 55, 41, 13, 27 }; //Tab->LCtrl->RCtrl->Bkspc: 14,28,42,56,57,58,66,67,68,69,55,41,13,27

            int colour = profile.KeyboardColours[14];
            foreach (int i in modifiers)
            {
                if (profile.KeyboardColours[i] != colour)
                    return false;
            }
            return true;
        }

        private void KeyboardProfiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            KeyboardProfileItem profile = (e.ClickedItem as KeyboardProfileItem);
            ChangeSelectedProfile(profile);
        }

        private void setButtonColour(Button button)
        {
            setButtonColour(button, this.SelectedColour);
        }

        private void setButtonColour(Button button, Color colour)
        {
            if (!matchingButtonColour)
            {
                button.BorderBrush = new SolidColorBrush(colour);
                button.BorderThickness = new Thickness(1);
                button.Background = new SolidColorBrush(colour);
                if (Brightness(colour) > 200)
                {
                    button.Foreground = new SolidColorBrush(Color.FromArgb(255, 75, 75, 75));
                }
                else
                {
                    button.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
            }
            else if (button.Name.Length > 14)
            {
                //Never gets hit, methinks
                int button_index = Int32.Parse(button.Name.Remove(0, 14));
                int colour_int = this.EditingProfile.KeyboardColours[button_index];
                this.SelectedColour = colour;
                colourPicker.SelectedColor = new SolidColorBrush(colour);
                matchingButtonColour = false;
            }
        }

        private int Brightness(Color c)
        {
            return (int)Math.Sqrt(
               c.R * c.R * .241 +
               c.G * c.G * .691 +
               c.B * c.B * .068);
        }

        private void KeyboardColourButton_Click(object sender, RoutedEventArgs e)
        {
            if (matchingButtonColour)
            {
                (this.FindName("ColourTool") as Button).Content = "Match a Colour";
                matchingButtonColour = false;
                Button button = (Button)sender;
                int btn_int = Int32.Parse(button.Name.Remove(0, 14));
                btn_int = this.EditingProfile.KeyboardColours[btn_int];
                colourPicker.PreviousSelectedColor = colourPicker.SelectedColor;
                colourPicker.SelectedColor = new SolidColorBrush(ConvertIntToColour(btn_int));
                this.SelectedColour = ConvertIntToColour(btn_int);
                //enable multikey buttons
                enableMultiButtons();
            }
            else
            {
                Button button = (Button)sender;
                setButtonColour(button);

                int button_index = Int32.Parse(button.Name.Remove(0, 14));

                this.EditingProfile.KeyboardColours[button_index] = ConvertColourToInt(this.SelectedColour);

                //this may be resource intensive, but it's the only way to gurantee that profiles get saved
                this.SaveProfiles();
            }
        }

        private void KeyboardAllButton_Click(object sender, RoutedEventArgs e)
        {

            Button button = (Button)sender;
            setButtonColour(button);

            for (int i = 0; i < 70; i++)
            {
                if (i == 40 || i == 53 || i == 54 || i == 59 || i == 60 || i == 62 || i == 63 || i == 64 || i == 65)
                {
                    continue;
                }
                this.EditingProfile.KeyboardColours[i] = ConvertColourToInt(this.SelectedColour);
                string s = "keyboardButton" + i;
                button = (this.FindName(s) as Button);
                setButtonColour(button);
            }
            //set Multi selection button colors
            setButtonColour(this.FindName("WASDKeys") as Button);
            setButtonColour(this.FindName("IJKLKeys") as Button);
            setButtonColour(this.FindName("NumKeys") as Button);
            setButtonColour(this.FindName("AllKeys") as Button);
            setButtonColour(this.FindName("ModifierKeys") as Button);

            //this may be resource intensive, but it's the only way to gurantee that profiles get saved
            this.SaveProfiles();
        }

        private void KeyboardWASDButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            setButtonColour(button);

            int[] modifiers = new int[4] { 16, 29, 30, 31 };
            foreach (int i in modifiers)
            {
                this.EditingProfile.KeyboardColours[i] = ConvertColourToInt(this.SelectedColour);

                string s = "keyboardButton" + i;
                button = (this.FindName(s) as Button);
                setButtonColour(button);
            }

            //this may be resource intensive, but it's the only way to gurantee that profiles get saved
            this.SaveProfiles();
        }

        private void KeyboardIJKLButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            setButtonColour(button);

            int[] modifiers = new int[4] { 22, 35, 36, 37 };
            foreach (int i in modifiers)
            {
                this.EditingProfile.KeyboardColours[i] = ConvertColourToInt(this.SelectedColour);

                string s = "keyboardButton" + i;
                button = (this.FindName(s) as Button);
                setButtonColour(button);
            }

            //this may be resource intensive, but it's the only way to gurantee that profiles get saved
            this.SaveProfiles();
        }

        private void KeyboardNumRowButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            setButtonColour(button);

            for (int i = 1; i < 11; i++) //num: 1-10 -=: 11-12
            {
                this.EditingProfile.KeyboardColours[i] = ConvertColourToInt(this.SelectedColour);
                string s = "keyboardButton" + i;
                button = (this.FindName(s) as Button);
                setButtonColour(button);
            }

            //this may be resource intensive, but it's the only way to gurantee that profiles get saved
            this.SaveProfiles();
        }

        private void KeyboardModifiersButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            setButtonColour(button);

            int[] modifiers = new int[14] { 14, 28, 42, 56, 57, 58, 66, 67, 68, 69, 55, 41, 13, 27 }; //Tab->LCtrl->RCtrl->Bkspc: 14,28,42,56,57,58,66,67,68,69,55,41,13,27
            foreach (int i in modifiers)
            {
                this.EditingProfile.KeyboardColours[i] = ConvertColourToInt(this.SelectedColour);
                string s = "keyboardButton" + i;
                button = (this.FindName(s) as Button);
                setButtonColour(button);
            }

            //this may be resource intensive, but it's the only way to gurantee that profiles get saved
            this.SaveProfiles();
        }

        private void KeyboardColourPickerButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Button wasd = this.FindName("WASDKeys") as Button;

            if (!matchingButtonColour)
            {
                matchingButtonColour = true;
                button.Content = "Cancel";
                //Disable multikey buttons
                disableMultiButtons();
            }
            else
            {
                matchingButtonColour = false;
                button.Content = "Match a Colour";
                //enable multikey buttons
                enableMultiButtons();
            }
        }

        private void enableMultiButtons()
        {
            (this.FindName("WASDKeys") as Button).IsEnabled = true;
            (this.FindName("IJKLKeys") as Button).IsEnabled = true;
            (this.FindName("NumKeys") as Button).IsEnabled = true;
            (this.FindName("AllKeys") as Button).IsEnabled = true;
            (this.FindName("ModifierKeys") as Button).IsEnabled = true;
        }

        private void disableMultiButtons()
        {
            (this.FindName("WASDKeys") as Button).IsEnabled = false;
            (this.FindName("IJKLKeys") as Button).IsEnabled = false;
            (this.FindName("NumKeys") as Button).IsEnabled = false;
            (this.FindName("AllKeys") as Button).IsEnabled = false;
            (this.FindName("ModifierKeys") as Button).IsEnabled = false;
        }


        private int ConvertColourToInt(Color colour)
        {
            int colour_int = colour.R;
            colour_int = (colour_int << 8) + colour.G;
            colour_int = (colour_int << 8) + colour.B;

            return colour_int;
        }

        private Color ConvertIntToColour(int coloured_int)
        {
            int red = (coloured_int >> 16) & 0xff;
            int green = (coloured_int >> 8) & 0xff;
            int blue = (coloured_int >> 0) & 0xff;

            return Color.FromArgb(255, (byte)red, (byte)green, (byte)blue);
        }

        private void ProfileNameChangedEvent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox profileName = (sender as TextBox);
            // update Views and KeyboardProfileItem class
            if (this.EditingProfile == RenamingProfile)
            {
                //chosenProfileName.Title = profileName.Text;
            }

            if (this.RenamingProfile != null)
            {
                this.RenamingProfile.Label = profileName.Text;
            }
        }

        private void ProfileAddButton_Click(object sender, RoutedEventArgs e)
        {
            this.CreateNewKeyboardProfile();
            this.SaveProfiles();
            this.ChangeSelectedProfile(_keyboardProfiles[_keyboardProfiles.Count - 1]);
            this.LightingProfilesCombo.SelectedIndex = this.LightingProfilesCombo.Items.Count - 1;
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

            // always make sure that the keyboard profiles list has 1 element in it
            if (this._keyboardProfiles.Count == 0)
            {
                this.CreateNewKeyboardProfile();
            }

            // Change the chosen profile to the first element
            ChangeSelectedProfile(this._keyboardProfiles[0]);
            LightingProfilesCombo.SelectedIndex = 0;

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
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
            {
                // Really inefficient, we should consider re-implementing this later
                for (int i = 0; i < this._keyboardProfiles.Count; i++)
                {
                    KeyboardProfileItem profile = this._keyboardProfiles[i];
                    profile.ID = i;
                }
            }
        }

        private void LightingProfilesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LightingProfilesCombo == null) return;
            var combo = (ComboBox)sender;
            var item = (KeyboardProfileItem)combo.SelectedItem;
            if (item == null)
            {
                this.CreateNewKeyboardProfile();
            }
            ChangeSelectedProfile(_keyboardProfiles[0]);
        }

        private void LightingProfilesCombo_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LightingProfilesCombo.SelectedIndex = 0;
            } catch
            {

            }
        }

        private void colourPicker_SelectedColorChanged(object sender, EventArgs e)
        {
            this.SelectedColour = colourPicker.SelectedColor.Color;
        }
    }
}
