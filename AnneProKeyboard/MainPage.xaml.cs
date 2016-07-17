using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.UI.Core;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AnneProKeyboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private BluetoothLEAdvertisementWatcher Watcher;
        private DeviceWatcher DeviceWatcher;

        private const string OAD_GUID = "f000ffc0-0451-4000-b000-000000000000";

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void StartScanning()
        {
            Watcher.Start();
            DeviceWatcher.Start();
        }

        private void StopScanning()
        {
            Watcher.Stop();
            DeviceWatcher.Stop();
        }


        private void SetupBluetooth()
        {
            Watcher = new BluetoothLEAdvertisementWatcher { ScanningMode = BluetoothLEScanningMode.Active };
            Watcher.Received += DeviceFound;

            DeviceWatcher = DeviceInformation.CreateWatcher();
            DeviceWatcher.Added += DeviceAdded;
            DeviceWatcher.Updated += DeviceUpdated;

            StartScanning();
        }

        private async void DeviceFound(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs btAdv)
        {
            Debug.WriteLine("Found Device: " + btAdv.Advertisement.LocalName);

            if (btAdv.Advertisement.LocalName.Contains("ANNE"))
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                {
                    Debug.WriteLine($"---------------------- {btAdv.Advertisement.LocalName} ----------------------");
                    Debug.WriteLine($"Advertisement Data: {btAdv.Advertisement.ServiceUuids.Count}");
                    var device = await BluetoothLEDevice.FromBluetoothAddressAsync(btAdv.BluetoothAddress);
                    var result = await device.DeviceInformation.Pairing.PairAsync(DevicePairingProtectionLevel.None);
                    Debug.WriteLine($"Pairing Result: {result.Status}");
                    Debug.WriteLine($"Connected Data: {device.GattServices.Count}");
                });
            }
        }

        private async void DeviceAdded(DeviceWatcher watcher, DeviceInformation device)
        {
            if (device.Name.Contains("ANNE"))
            {
                try
                {
                    var keyboard = await BluetoothLEDevice.FromIdAsync(device.Id);

                    var service = keyboard.GetGattService(new Guid(OAD_GUID));

                    foreach(var gatt in service.GetAllCharacteristics())
                    {
                        Debug.WriteLine("UUID: " + gatt.Uuid);
                    }

                    Debug.WriteLine("Opened Service!!");

                    StopScanning();
                }
                catch
                {
                    Debug.WriteLine("Failed to open service.");
                }
            }
        }

        private void DeviceUpdated(DeviceWatcher watcher, DeviceInformationUpdate update)
        {
            Debug.WriteLine($"Device updated: {update.Id}");
        }

        private void startButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.SetupBluetooth();
        }
    }
}
