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
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AnneProKeyboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<KeyboardProfileItem> _keyboardProfiles = new ObservableCollection<KeyboardProfileItem>();
        public ObservableCollection<KeyboardProfileItem> KeyboardProfiles
        {
            get { return _keyboardProfiles; }
            set { }
        }
        public KeyboardProfileItem EditingProfile;
        public KeyboardProfileItem RenamingProfile;

        private DeviceWatcher BluetoothDeviceWatcher;
        private DeviceWatcher AllDevicesWatcher;

        private readonly Guid OAD_GUID = new Guid("f000ffc0-0451-4000-b000-000000000000");
        private readonly Guid WRITE_GATT_GUID = new Guid("f000ffc2-0451-4000-b000-000000000000");
        private readonly Guid READ_GATT_GUID = new Guid("f000ffc1-0451-4000-b000-000000000000");

        private GattCharacteristic WriteGatt;
        private GattCharacteristic ReadGatt;
        private DeviceInformation KeyboardDeviceInformation;

        //private ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

        private AboutPage aboutPage;
        private LayoutPage layoutPage;
        private LightingPage lightingPage;

        private Boolean IsContentPage;

        public MainPage()
        {
            this.InitializeComponent();
            initPages();
            this._frame.Content = new LayoutPage();
            IsContentPage = true;
            FindKeyboard();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
            lightingNavItem.Icon = new FontIcon { Glyph = "\uE706" };
            Color systemAccentColor = (Color)App.Current.Resources["SystemAccentColor"];
            LoadProfiles();
            this._keyboardProfiles.CollectionChanged += KeyboardProfiles_CollectionChanged;
            Application.Current.Suspending += new SuspendingEventHandler(App_Suspending);
        }

        private void initPages()
        {
            layoutPage = new LayoutPage();
            aboutPage = new AboutPage();
            lightingPage = new LightingPage();
        }

        private void KeyboardProfiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            KeyboardProfileItem profile = (e.ClickedItem as KeyboardProfileItem);
            (this._frame.Content as IContentPage).ChangeSelectedProfile(profile);

        }

        private void ProfileAddButton_Click(object sender, RoutedEventArgs e)
        {
            this.CreateNewKeyboardProfile();
            (this._frame.Content as IContentPage).ChangeSelectedProfile(_keyboardProfiles[_keyboardProfiles.Count - 1]);
            this.ProfilesCombo.SelectedIndex = this.ProfilesCombo.Items.Count - 1;
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
            SaveProfiles();
        }

        private void ProfileDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FrameworkElement parent = (FrameworkElement)button.Parent;
            TextBox textbox = (TextBox)parent.FindName("ProfileNameTextbox");
            KeyboardProfileItem selected_profile = this._keyboardProfiles[(int)button.Tag];

            //// always make sure that the keyboard profiles list has 1 element in it
            if (this._keyboardProfiles.Count != 1)
            {
                int curr = this._keyboardProfiles.IndexOf(selected_profile);
                if(curr - 1 == -1)
                {
                    ProfilesCombo.SelectedItem = this._keyboardProfiles[++curr];
                } else
                {
                    ProfilesCombo.SelectedItem = this._keyboardProfiles[--curr];
                }
                this._keyboardProfiles.Remove(selected_profile);
            }
            //ensure not deleting the selected one or vice versa
            //delete
            //// Change the chosen profile to the first element

            this.SaveProfiles();
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
            }
            catch (UnauthorizedAccessException)
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
                    ObservableCollection<KeyboardProfileItem> saved_profiles = (ObservableCollection<KeyboardProfileItem>)serialiser.ReadObject(inStream.AsStreamForRead());

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

            (this._frame.Content as IContentPage).ChangeSelectedProfile(this._keyboardProfiles[0]);
        }

        private void CreateNewKeyboardProfile()
        {
            KeyboardProfileItem profile_item = new KeyboardProfileItem(this._keyboardProfiles.Count, "Profile " + (this._keyboardProfiles.Count + 1));
            this._keyboardProfiles.Add(profile_item);
        }

        private Color ConvertIntToColour(int coloured_int)
        {
            int red = (coloured_int >> 16) & 0xff;
            int green = (coloured_int >> 8) & 0xff;
            int blue = (coloured_int >> 0) & 0xff;

            return Color.FromArgb(255, (byte)red, (byte)green, (byte)blue);
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

        private void SetupBluetooth()
        {
            if (this.BluetoothDeviceWatcher == null)
            {
                BluetoothDeviceWatcher = DeviceInformation.CreateWatcher("System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"", null, DeviceInformationKind.AssociationEndpoint);

                try
                {
                    BluetoothDeviceWatcher.Added += BluetoothDeviceAdded;
                    BluetoothDeviceWatcher.Updated += BluetoothDeviceUpdated;
                    BluetoothDeviceWatcher.Start();
                }
                catch
                {
                }
            }

            if (AllDevicesWatcher == null)
            {
                AllDevicesWatcher = DeviceInformation.CreateWatcher(DeviceClass.All);

                try
                {
                    AllDevicesWatcher.Updated += HIDDeviceUpdated;
                    AllDevicesWatcher.Start();
                }
                catch
                {
                }
            }
        }

        private async void HIDDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            DeviceInformation device_info = await DeviceInformation.CreateFromIdAsync(args.Id);

            if(device_info == null)
            {
                return;
            }

            // Do not let the background task starve, check if we are paired then connect to the keyboard
            if (device_info.Name.Contains("ANNE"))
            {
                if (device_info.IsEnabled)
                {
                    FindKeyboard();
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        KeyboardDeviceInformation = null;
                        connectionStatusLabel.Text = "Not Connected";
                        connectionStatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                        ProfileSyncButton.IsEnabled = true;
                    });
                }
            }
        }

        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            SaveProfiles();

            if (this.BluetoothDeviceWatcher != null)
            {
                BluetoothDeviceWatcher.Added -= BluetoothDeviceAdded;
                BluetoothDeviceWatcher.Updated -= BluetoothDeviceUpdated;

                BluetoothDeviceWatcher.Stop();

                this.BluetoothDeviceWatcher = null;
            }

            if (this.AllDevicesWatcher != null)
            {
                AllDevicesWatcher.Updated -= HIDDeviceUpdated;

                AllDevicesWatcher.Stop();

                this.AllDevicesWatcher = null;
            }
        }

        private void App_Resuming(object sender, object e)
        {
            this.SetupBluetooth();
        }

        private async void BluetoothDeviceAdded(DeviceWatcher watcher, DeviceInformation device)
        {
            if (device.Name.Contains("ANNE"))
            {
                if (device.Pairing.IsPaired)
                {
                    ConnectToKeyboard(device);
                }
                else if (device.Pairing.CanPair)
                {
                    var result = await device.Pairing.PairAsync();

                    if (result.Status == DevicePairingResultStatus.Paired)
                    {
                        ConnectToKeyboard(device);
                    }
                }
            }
        }

        private void BluetoothDeviceUpdated(DeviceWatcher watcher, DeviceInformationUpdate device)
        {
            // Need this function for Bluetooth LE device watcher, otherwise it won't detect anything
        }

        private async void ConnectToKeyboard(DeviceInformation device)
        {
            try
            {
                if (this.KeyboardDeviceInformation != null)
                {
                    return;
                }

                var keyboard = await BluetoothLEDevice.FromIdAsync(device.Id);

                if (keyboard == null)
                {
                    return;
                }

                if (keyboard.ConnectionStatus != BluetoothConnectionStatus.Connected)
                {
                    return;
                }

                var service = keyboard.GetGattService(OAD_GUID);

                if (service == null)
                {
                    return;
                }

                var write_gatt = service.GetCharacteristics(WRITE_GATT_GUID)[0];
                var read_gatt = service.GetCharacteristics(READ_GATT_GUID)[0];

                if (write_gatt == null || read_gatt == null)
                {
                    return;
                }

                this.WriteGatt = write_gatt;
                this.ReadGatt = read_gatt;
                this.KeyboardDeviceInformation = device;

                await this.ReadGatt.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                this.ReadGatt.ValueChanged += ReadGatt_ValueChanged;

                // Sync up the profile data
                this.RequestKeyboardSync();
            }
            // We should actually catch errors here...
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void ReadGatt_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var data = args.CharacteristicValue.ToArray();

            //TODO: handle the retrieved data
        }

        private void RequestKeyboardSync()
        {
            // expect firmware version and mac address
            byte[] device_id_meta_data = { 0x02, 0x01, 0x01 };

            KeyboardWriter keyboard_writer = new KeyboardWriter(this.WriteGatt, device_id_meta_data, null);
            keyboard_writer.WriteToKeyboard();

            keyboard_writer.OnWriteFinished += (object_s, events) =>
            {
                FinishSync();
            };
        }

        private async void FinishSync()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                this.connectionStatusLabel.Text = "Connected";
                this.connectionStatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                ProfileSyncButton.IsEnabled = true;
            });
        }

        public void SyncProfile(KeyboardProfileItem EditingProfile)
        {
            EditingProfile.SyncProfile(this.WriteGatt);
        }

        private static Color lightenDarkenColor(Color color, double correctionFactor)
        {
            double red = (255 - color.R) * correctionFactor + color.R;
            double green = (255 - color.G) * correctionFactor + color.G;
            double blue = (255 - color.B) * correctionFactor + color.B;
            return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }

        private void KeyboardSyncButton_Click(object sender, RoutedEventArgs e)
        {
            if(_frame.Content.GetType() == typeof(LayoutPage))
            {
                LayoutSyncButton(_frame.Content as LayoutPage);
            }

            else if (_frame.Content.GetType() == typeof(LightingPage))
            {
                LightingSyncButton(_frame.Content as LightingPage);
            }
        }

        private void LightingSyncButton(LightingPage child)
        {
            if (!child.editingProfile.ValidateKeyboardKeys())
            {
                this.SyncStatus.Text = "Fn or Anne keys were not found in the Standard or Fn layouts";
                return;
            }

            try
            {
                SaveProfiles();
            }
            catch (UnauthorizedAccessException)
            {
                SyncStatus.Text = "UnAuthorizedAccessException: Unable to access file. ";
            }

            ProfileSyncButton.IsEnabled = false;

            this.SyncProfile(child.editingProfile);
            child.editingProfile.SyncStatusNotify += async (object_s, events) =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.SyncStatus.Text = (string)object_s;

                    this.ProfileSyncButton.IsEnabled = true;
                });
            };
        }

        private void LayoutSyncButton(LayoutPage child)
        {
            if (!child.editingProfile.ValidateKeyboardKeys())
            {
                this.SyncStatus.Text = "Fn or Anne keys were not found in the Standard or Fn layouts";
                return;
            }

            try
            {
                SaveProfiles();
            }
            catch (UnauthorizedAccessException)
            {
                SyncStatus.Text = "UnAuthorizedAccessException: Unable to access file. ";
            }
            ProfileSyncButton.IsEnabled = false;

            this.SyncProfile(child.editingProfile);
            child.editingProfile.SyncStatusNotify += async (object_s, events) =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.SyncStatus.Text = (string)object_s;

                    this.ProfileSyncButton.IsEnabled = true;
                });
            };
        }

        private void AnneNav_Loaded(object sender, RoutedEventArgs e)
        {
            // set the initial SelectedItem 
            foreach (NavigationViewItemBase item in AnneNav.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "layout")
                {
                    AnneNav.SelectedItem = item;
                    break;
                }
            }
        }

        private void AnneNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                IsContentPage = false;
                sender.Header = "About";
                this._frame.Content = aboutPage;
            }
            else
            {
                IsContentPage = true;
                NavigationViewItem item = args.SelectedItem as NavigationViewItem;

                switch (item.Tag)
                {
                    case "layout":
                        _frame.Content = layoutPage;
                        break;

                    case "lighting":
                        _frame.Content = lightingPage;
                        break;
                }
            }
        }

        private void AnneNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                IsContentPage = false;
                sender.Header = "About";
                _frame.Content = aboutPage;
            }
            else
            {
                IsContentPage = true;
                switch (args.InvokedItem)
                {
                    case "Layout":
                        sender.Header = "Layout";
                        _frame.Content = layoutPage;
                        break;

                    case "Lighting":
                        sender.Header = "Lighting";
                        _frame.Content = lightingPage;
                        break;
                }
            }
        }

        private void ProfilesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SaveProfiles();
            } catch
            {
                this.SyncStatus.Text = "Profiles failed to save";
            }
            lightingPage.ChangeSelectedProfile((KeyboardProfileItem)ProfilesCombo.SelectedItem);
            layoutPage.ChangeSelectedProfile((KeyboardProfileItem)ProfilesCombo.SelectedItem);
        }

        private void ProfilesCombo_Loaded(object sender, RoutedEventArgs e)
        {
            if(_keyboardProfiles.Count > 0)
            {
                ProfilesCombo.SelectedIndex = 0;
            }
        }
    }
}
