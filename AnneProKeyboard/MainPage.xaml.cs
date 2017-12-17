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
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AnneProKeyboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DeviceWatcher BluetoothDeviceWatcher;
        private DeviceWatcher AllDevicesWatcher;

        private readonly Guid OAD_GUID = new Guid("f000ffc0-0451-4000-b000-000000000000");
        private readonly Guid WRITE_GATT_GUID = new Guid("f000ffc2-0451-4000-b000-000000000000");
        private readonly Guid READ_GATT_GUID = new Guid("f000ffc1-0451-4000-b000-000000000000");

        private GattCharacteristic WriteGatt;
        private GattCharacteristic ReadGatt;
        private DeviceInformation KeyboardDeviceInformation;

        private ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

        private AboutPage aboutPage;
        private LayoutPage layoutPage;
        private LightingPage lightingPage;

        public MainPage()
        {
            this.InitializeComponent();
            initPages();
            _frame.Content = layoutPage;
            // Start up the background thread to find the keyboard
            FindKeyboard();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonInactiveBackgroundColor = Colors.White;
            titleBar.ButtonInactiveForegroundColor = Color.FromArgb(1, 152, 152, 152);
            Window.Current.SetTitleBar(MainTitleBar);
            Window.Current.Activated += Current_Activated;
            Color systemAccentColor = (Color)App.Current.Resources["SystemAccentColor"];
        }

        private void initPages()
        {
            layoutPage = new LayoutPage();
            aboutPage = new AboutPage();
            lightingPage = new LightingPage();
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

        private void hamburgerHover(object sender, RoutedEventArgs e)
        {
            //HamburgerButton.Background = new SolidColorBrush(Colors.Red);
        }

        private static Color lightenDarkenColor(Color color, double correctionFactor)
        {
            double red = (255 - color.R) * correctionFactor + color.R;
            double green = (255 - color.G) * correctionFactor + color.G;
            double blue = (255 - color.B) * correctionFactor + color.B;
            return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }

        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != CoreWindowActivationState.Deactivated)
            {
                MainTitleBar.Background = new SolidColorBrush((Color)this.Resources["SystemAccentColor"]);
                pageHeader.Foreground = new SolidColorBrush(Colors.White);
                HamburgerButton.Background = new SolidColorBrush(Color.FromArgb(51,255,255,255));
                HamburgerButton.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                pageHeader.Foreground = new SolidColorBrush(Color.FromArgb(255, 152, 152, 152));
                MainTitleBar.Background = new SolidColorBrush(Colors.White);
                HamburgerButton.Background = new SolidColorBrush(Colors.White);
                HamburgerButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 152, 152, 152));
            }
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
            if (!child.EditingProfile.ValidateKeyboardKeys())
            {
                //this.SyncStatus.Text = "Fn or Anne keys were not found in the Standard or Fn layouts";
                return;
            }

            try
            {
                child.SaveProfiles();
            }
            catch (UnauthorizedAccessException)
            {
                SyncStatus.Text = "UnAuthorizedAccessException: Unable to access file. ";
            }

            ProfileSyncButton.IsEnabled = false;

            this.SyncProfile(child.EditingProfile);
            child.EditingProfile.SyncStatusNotify += async (object_s, events) =>
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
            if (!child.EditingProfile.ValidateKeyboardKeys())
            {
                this.SyncStatus.Text = "Fn or Anne keys were not found in the Standard or Fn layouts";
                return;
            }

            try
            {
                child.SaveProfiles();
            }
            catch (UnauthorizedAccessException)
            {
                SyncStatus.Text = "UnAuthorizedAccessException: Unable to access file. ";
            }
            ProfileSyncButton.IsEnabled = false;

            this.SyncProfile(child.EditingProfile);
            child.EditingProfile.SyncStatusNotify += async (object_s, events) =>
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.SyncStatus.Text = (string)object_s;

                    this.ProfileSyncButton.IsEnabled = true;
                });
            };
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
        }

        private void LightingNav_Clicked(object sender, RoutedEventArgs e)
        {
            if (!(_frame.Content.GetType() == typeof(LightingPage)))
            {
                _frame.Content = lightingPage;
                lightingPage.LoadProfiles();
                if (connectionStatusLabel.Text == "Connected")
                {
                    ProfileSyncButton.IsEnabled = true;
                }
                pageHeader.Text = "Lighting";
                LightingMenuButton.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            }
        }

        private void LayoutNav_Clicked(object sender, RoutedEventArgs e)
        {
            if (!(_frame.Content.GetType() == typeof(LayoutPage)))
            {
                _frame.Content = layoutPage;
                layoutPage.LoadProfiles();
                if(connectionStatusLabel.Text == "Connected")
                {
                    ProfileSyncButton.IsEnabled = true;
                }
                pageHeader.Text = "Layers";
                LayoutMenuButton.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            }
        }

        private void AboutNav_Clicked(object sender, RoutedEventArgs e)
        {
            if (!(_frame.Content.GetType() == typeof(AboutPage)))
            {
                _frame.Content = aboutPage;
                if (connectionStatusLabel.Text == "Connected")
                {
                    ProfileSyncButton.IsEnabled = false;
                }
                pageHeader.Text = "About";
                LayoutMenuButton.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            }
        }

        private void pageHeader_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
