using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Specialized;

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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AnneProKeyboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DeviceWatcher DeviceWatcher;

        private readonly Guid OAD_GUID = new Guid("f000ffc0-0451-4000-b000-000000000000");
        private readonly Guid WRITE_GATT_GUID = new Guid("f000ffc2-0451-4000-b000-000000000000");

        private GattCharacteristic WriteGatt;
        private DeviceInformation KeyboardDeviceInformation;

        private ObservableCollection<KeyboardProfileItem> _keyboardProfiles = new ObservableCollection<KeyboardProfileItem>();
        private KeyboardProfileItem EditingProfile;
        private KeyboardProfileItem RenamingProfile;

        private Color SelectedColour;

        public ObservableCollection<KeyboardProfileItem> KeyboardProfiles
        {
            get { return _keyboardProfiles; }
            set { }
        }

        public MainPage()
        {
            this.InitializeComponent();

            // Start up the background thread to find the keyboard
            FindKeyboard();

            LoadProfiles();
            SelectedColour = colourPicker.SelectedColor;

            this._keyboardProfiles.CollectionChanged += KeyboardProfiles_CollectionChanged;
        }

        private async void FindKeyboard()
        {
            string deviceSelectorInfo = BluetoothLEDevice.GetDeviceSelectorFromPairingState(true);
            DeviceInformationCollection deviceInfoCollection = await DeviceInformation.FindAllAsync(deviceSelectorInfo, null);

            foreach (DeviceInformation device_info in deviceInfoCollection)
            {
                // Do not let the background task starve, check if we are paired then connect to the keyboard
                if (device_info.Name.Contains("ANNE")
                    && device_info.Pairing.IsPaired)
                {
                    ConnectToKeyboard(device_info);

                    break;
                }
            }

            // if the device was never paired start doing the background check
            // Make sure to disable Bluetooth listener
            this.SetupBluetooth();
        }
        
        private async void SaveProfiles()
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

        private async void LoadProfiles()
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("KeyboardProfilesData");
                using (IInputStream inStream = await file.OpenSequentialReadAsync())
                {
                    DataContractSerializer serialiser = new DataContractSerializer(typeof(ObservableCollection<KeyboardProfileItem>));
                    ObservableCollection <KeyboardProfileItem> saved_profiles = (ObservableCollection<KeyboardProfileItem>)serialiser.ReadObject(inStream.AsStreamForRead());

                    foreach(KeyboardProfileItem profile in saved_profiles)
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

        private void StartScanning()
        {
            DeviceWatcher.Start();
        }

        private void SetupBluetooth()
        {
            // quick sanity check
            if(this.DeviceWatcher != null)
            {
                return;
            }

            DeviceWatcher = DeviceInformation.CreateWatcher("System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"", null, DeviceInformationKind.AssociationEndpoint);
            DeviceWatcher.Added += DeviceAdded;
            DeviceWatcher.Updated += DeviceUpdated;

            StartScanning();
        }

        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            if (this.DeviceWatcher != null)
            {
                DeviceWatcher.Added -= DeviceAdded;
                DeviceWatcher.Updated -= DeviceUpdated;

                DeviceWatcher.Stop();
            }
        }

        private void App_Resuming(object sender, object e)
        {
            if (this.DeviceWatcher != null)
            {
                DeviceWatcher.Added += DeviceAdded;
                DeviceWatcher.Updated += DeviceUpdated;

                DeviceWatcher.Start();
            }
        }

        private async void ConnectToKeyboard(DeviceInformation device)
        {
            try
            {
                var keyboard = await BluetoothLEDevice.FromIdAsync(device.Id);

                if (keyboard == null)
                {
                    return;
                }

                if(keyboard.ConnectionStatus != BluetoothConnectionStatus.Connected)
                {
                    return;
                }

                var service = keyboard.GetGattService(OAD_GUID);

                if (service == null)
                {
                    return;
                }

                var write_gatt = service.GetCharacteristics(WRITE_GATT_GUID)[0];

                if (write_gatt == null)
                {
                    return;
                }

                WriteGatt = write_gatt;

                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    this.KeyboardDeviceInformation = device;
                    this.connectionStatusLabel.Text = "Connected";
                    this.connectionStatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                    this.syncButton.IsEnabled = true;
                });
            }
            catch
            {
            }
        }

        private void CreateNewKeyboardProfile()
        {
            KeyboardProfileItem profile_item = new KeyboardProfileItem(this._keyboardProfiles.Count, "Profile " + (this._keyboardProfiles.Count + 1));
            this._keyboardProfiles.Add(profile_item);
        }

        private void ChangeSelectedProfile(KeyboardProfileItem profile)
        {
            this.EditingProfile = profile;
            chosenProfileName.Text = profile.Label;

            // set up the background colours for the keyboard lights
            byte alpha = 255;

            for (int i = 0; i < 70; i++)
            {
                string s = "keyboardButton" + i;
                Button button = (this.FindName(s) as Button);
                
                // NOTE: last 4 keys (RALT, FN, Anne, Ctrl) are identify as 66,67,68,69
                if (button == null)
                {
                    continue;
                }

                int coloured_int = profile.KeyboardColours[i];
                
                int red = (coloured_int >> 16) & 0xff;
                int green = (coloured_int >> 8) & 0xff;
                int blue = (coloured_int >> 0) & 0xff;

                Color colour = Color.FromArgb(alpha, (byte)red, (byte)green, (byte)blue);

                button.BorderBrush = new SolidColorBrush(colour);
                button.BorderThickness = new Thickness(1);
            }
        }

        private void KeyboardProfiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            KeyboardProfileItem profile = (e.ClickedItem as KeyboardProfileItem);
            ChangeSelectedProfile(profile);
        }

        private async void DeviceAdded(DeviceWatcher watcher, DeviceInformation device)
        {
            if(device.Name.Contains("ANNE") && !device.Pairing.IsPaired)
            {
                var result = await device.Pairing.PairAsync();

                if(result.Status == DevicePairingResultStatus.Paired)
                {
                    ConnectToKeyboard(device);
                }
            }
        }
        
        private async void DeviceUpdated(DeviceWatcher watcher, DeviceInformationUpdate device)
        {
            DeviceInformation device_info = await DeviceInformation.CreateFromIdAsync(device.Id);

            // Do not let the background task starve, check if we are paired then connect to the keyboard
            if (device_info.Name.Contains("ANNE"))
            {
                if (device_info.Pairing.IsPaired)
                {
                    FindKeyboard();
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        connectionStatusLabel.Text = "Not Connected";
                        connectionStatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                        syncButton.IsEnabled = false;
                    });
                }
            }
        }

        private void KeyboardColourButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            button.BorderBrush = new SolidColorBrush(this.SelectedColour);
            button.BorderThickness = new Thickness(1);

            int button_index = Int32.Parse(button.Name.Remove(0, 14));

            this.EditingProfile.KeyboardColours[button_index] = ConvertColourToInt(this.SelectedColour);

            //this may be resource intensive, but it's the only way to gurantee that profiles get saved
            this.SaveProfiles();
        }

        private int ConvertColourToInt(Color colour)
        {
            int colour_int = colour.R;
            colour_int = (colour_int << 8) + colour.G;
            colour_int = (colour_int << 8) + colour.B;

            return colour_int;
        }

        private void ProfileNameChangedEvent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox profileName = (sender as TextBox);
            // update Views and KeyboardProfileItem class
            if(this.EditingProfile == RenamingProfile)
            {
                chosenProfileName.Text = profileName.Text;
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

            this.SaveProfiles();
        }

        private void KeyboardSyncButton_Click(object sender, RoutedEventArgs e)
        {
            this.SaveProfiles();

            SendProfilePhase1(this.EditingProfile);
        }

        // send the backlight first data, should cause a waterfall effect on syncing up the profile
        private void SendProfilePhase1(KeyboardProfileItem profile)
        {
            // We need this to identify the type of data we are sending
            byte[] lighting_meta_data = { 0x09, 0xD7, 0x03 };

            // Convert the list of keyboard colours
            byte[] light_data = profile.GenerateKeyboardBacklightData();

            // Send the data to the keyboard
            KeyboardWriter keyboard_writer = new KeyboardWriter(this.Dispatcher, this.WriteGatt, lighting_meta_data, light_data);
            keyboard_writer.WriteToKeyboard();

            keyboard_writer.OnWriteFinished += (object_s, events) => { SendProfilePhase2(profile); }; // we need to do this because of async calls, threading is fun!
        }

        // send the layout data
        private void SendProfilePhase2(KeyboardProfileItem profile)
        {
            if (!profile.ValidateKeyboardKeys())
            {
                // raise an error
                return;
            }

            // We need this to identify the type of data we are sending
            byte[] layout_meta_data = { 0x7, 0x91, 0x02 };

            // Convert the list of keyboard keys
            byte[] layout_data = profile.GenerateKeyboardLayoutData();

            KeyboardWriter keyboard_writer = new KeyboardWriter(this.Dispatcher, this.WriteGatt, layout_meta_data, layout_data);
            keyboard_writer.WriteToKeyboard();
        }

        private void ProfileNameTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.SaveProfiles();

            TextBox textbox = (TextBox)sender;
            textbox.IsEnabled = false;
            textbox.Visibility = Visibility.Collapsed;

            FrameworkElement parent = (FrameworkElement)textbox.Parent;
            TextBlock textblock = (TextBlock)parent.FindName("ProfileNameTextblock");
            textblock.Visibility = Visibility.Visible;

            this.RenamingProfile = null;
        }

        private void colourPicker_colourChanged(object sender, EventArgs e)
        {
            this.SelectedColour = this.colourPicker.SelectedColor;
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
    }
}
