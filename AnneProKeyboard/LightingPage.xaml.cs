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
    public partial class LightingPage : Page, IContentPage
    {
        private Color SelectedColour;
        private Boolean matchingButtonColour = false;

        public KeyboardProfileItem editingProfile = null;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        public LightingPage()
        {
            this.InitializeComponent();
            SelectedColour = colourPicker.Color;
        }

        public void ChangeSelectedProfile(KeyboardProfileItem profile)
        {
            this.editingProfile = profile;
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
            Color default_colour = Color.FromArgb(255, 94, 97, 102);
            if (ColourWASD(profile))
            {
                SetButtonColour(multi_button, multi_colour);
            }
            else
            {
                SetButtonColour(multi_button, default_colour);
            }

            //check IJKL keys
            if (ColourIJKL(profile))
            {
                multi_colour = ConvertIntToColour(profile.KeyboardColours[22]); //I key idx
                multi_button = (this.FindName("IJKLKeys") as Button);
                SetButtonColour(multi_button, multi_colour);
            }
            else
            {
                SetButtonColour(multi_button, default_colour);
            }

            //check Modifier Keys
            if (ColourModifiers(profile))
            {
                multi_colour = ConvertIntToColour(profile.KeyboardColours[14]); //Tab key idx
                multi_button = (this.FindName("ModifierKeys") as Button);
                SetButtonColour(multi_button, multi_colour);
            }
            else
            {
                SetButtonColour(multi_button, default_colour);
            }

            //check num row
            if (ColourNumRow(profile))
            {
                multi_colour = ConvertIntToColour(profile.KeyboardColours[1]); //1 key idx
                multi_button = (this.FindName("NumKeys") as Button);
                SetButtonColour(multi_button, multi_colour);
            }
            else
            {
                SetButtonColour(multi_button, default_colour);
            }

            //set all buttons colour to default colour. Easy to see if all buttons are the same colour
            multi_button = (this.FindName("AllKeys") as Button);
            SetButtonColour(multi_button, default_colour);
        }

        private Boolean ColourWASD(KeyboardProfileItem profile)
        {
            return (
                profile.KeyboardColours[16] == profile.KeyboardColours[29] &&
                profile.KeyboardColours[29] == profile.KeyboardColours[30] &&
                profile.KeyboardColours[30] == profile.KeyboardColours[31]
            );
        }

        private Boolean ColourIJKL(KeyboardProfileItem profile)
        {
            return (
                profile.KeyboardColours[22] == profile.KeyboardColours[35] &&
                profile.KeyboardColours[35] == profile.KeyboardColours[36] &&
                profile.KeyboardColours[36] == profile.KeyboardColours[37]
            );
        }

        private Boolean ColourNumRow(KeyboardProfileItem profile)
        {
            int colour = profile.KeyboardColours[1];
            for (int i = 2; i < 11; i++)
            {
                if (profile.KeyboardColours[i] != colour)
                    return false;
            }
            return true;
        }

        private Boolean ColourModifiers(KeyboardProfileItem profile)
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
        
        private void SetButtonColour(Button button)
        {
            SetButtonColour(button, this.SelectedColour);
        }

        private void SetButtonColour(Button button, Color colour)
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
                int buttonIndex = Int32.Parse(button.Name.Remove(0, 14));
                int colourInt = this.editingProfile.KeyboardColours[buttonIndex];
                this.SelectedColour = colour;
                colourPicker.Color = colour;
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
                SolidColorBrush buttonBackground = button.Background as SolidColorBrush;
                colourPicker.Color = buttonBackground.Color;
                this.SelectedColour = colourPicker.Color;
                //enable multikey buttons
                EnableMultiButtons();
            }
            else
            {
                Button button = (Button)sender;
                SetButtonColour(button);

                int button_index = Int32.Parse(button.Name.Remove(0, 14));

                this.editingProfile.KeyboardColours[button_index] = ConvertColourToInt(this.SelectedColour);
            }
        }

        private void KeyboardAllButton_Click(object sender, RoutedEventArgs e)
        {

            Button button = (Button)sender;
            SetButtonColour(button);

            for (int i = 0; i < 70; i++)
            {
                if (i == 40 || i == 53 || i == 54 || i == 59 || i == 60 || i == 62 || i == 63 || i == 64 || i == 65)
                {
                    continue;
                }
                this.editingProfile.KeyboardColours[i] = ConvertColourToInt(this.SelectedColour);
                string s = "keyboardButton" + i;
                button = (this.FindName(s) as Button);
                SetButtonColour(button);
            }
            //set Multi selection button colors
            SetButtonColour(this.FindName("WASDKeys") as Button);
            SetButtonColour(this.FindName("IJKLKeys") as Button);
            SetButtonColour(this.FindName("NumKeys") as Button);
            SetButtonColour(this.FindName("AllKeys") as Button);
            SetButtonColour(this.FindName("ModifierKeys") as Button);
        }

        private void KeyboardWASDButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SetButtonColour(button);

            int[] modifiers = new int[4] { 16, 29, 30, 31 };
            foreach (int i in modifiers)
            {
                this.editingProfile.KeyboardColours[i] = ConvertColourToInt(this.SelectedColour);

                string s = "keyboardButton" + i;
                button = (this.FindName(s) as Button);
                SetButtonColour(button);
            }
        }

        private void KeyboardIJKLButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SetButtonColour(button);

            int[] modifiers = new int[4] { 22, 35, 36, 37 };
            foreach (int i in modifiers)
            {
                this.editingProfile.KeyboardColours[i] = ConvertColourToInt(this.SelectedColour);

                string s = "keyboardButton" + i;
                button = (this.FindName(s) as Button);
                SetButtonColour(button);
            }
        }

        private void KeyboardNumRowButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SetButtonColour(button);

            for (int i = 1; i < 11; i++) //num: 1-10 -=: 11-12
            {
                this.editingProfile.KeyboardColours[i] = ConvertColourToInt(this.SelectedColour);
                string s = "keyboardButton" + i;
                button = (this.FindName(s) as Button);
                SetButtonColour(button);
            }
        }

        private void KeyboardModifiersButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SetButtonColour(button);

            int[] modifiers = new int[14] { 14, 28, 42, 56, 57, 58, 66, 67, 68, 69, 55, 41, 13, 27 }; //Tab->LCtrl->RCtrl->Bkspc: 14,28,42,56,57,58,66,67,68,69,55,41,13,27
            foreach (int i in modifiers)
            {
                this.editingProfile.KeyboardColours[i] = ConvertColourToInt(this.SelectedColour);
                string s = "keyboardButton" + i;
                button = (this.FindName(s) as Button);
                SetButtonColour(button);
            }
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
                DisableMultiButtons();
            }
            else
            {
                matchingButtonColour = false;
                button.Content = "Match a Colour";
                //enable multikey buttons
                EnableMultiButtons();
            }
        }

        private void EnableMultiButtons()
        {
            (this.FindName("WASDKeys") as Button).IsEnabled = true;
            (this.FindName("IJKLKeys") as Button).IsEnabled = true;
            (this.FindName("NumKeys") as Button).IsEnabled = true;
            (this.FindName("AllKeys") as Button).IsEnabled = true;
            (this.FindName("ModifierKeys") as Button).IsEnabled = true;
        }

        private void DisableMultiButtons()
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

        private void ColourPicker_ColorChanged(Windows.UI.Xaml.Controls.ColorPicker sender, ColorChangedEventArgs args)
        {
            this.SelectedColour = sender.Color;
        }
    }
}
