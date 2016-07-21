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

using Windows.Storage.Streams;

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
        private const string WRITE_GATT_GUID = "f000ffc2-0451-4000-b000-000000000000";

        public MainPage()
        {
            this.InitializeComponent();

            FindKeyboard();
        }

        private async void FindKeyboard()
        {
            string deviceSelectorInfo = BluetoothLEDevice.GetDeviceSelectorFromPairingState(true);
            DeviceInformationCollection deviceInfoCollection = await DeviceInformation.FindAllAsync(deviceSelectorInfo, null);

            foreach (DeviceInformation deviceInfo in deviceInfoCollection)
            {
                if (deviceInfo.Name.Contains("ANNE"))
                {
                    ConnectToKeyboard(deviceInfo);
                    break;
                }
            }

            deviceSelectorInfo = BluetoothDevice.GetDeviceSelectorFromPairingState(true);
            deviceInfoCollection = await DeviceInformation.FindAllAsync(deviceSelectorInfo, null);

            foreach (DeviceInformation deviceInfo in deviceInfoCollection)
            {
                if (deviceInfo.Name.Contains("ANNE"))
                {
                    ConnectToKeyboard(deviceInfo);
                    break;
                }
            }
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
            if (btAdv.Advertisement.LocalName.Contains("ANNE"))
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                {
                    var device = await BluetoothLEDevice.FromBluetoothAddressAsync(btAdv.BluetoothAddress);
                    var result = await device.DeviceInformation.Pairing.PairAsync(DevicePairingProtectionLevel.None);
                });
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

                var service = keyboard.GetGattService(new Guid(OAD_GUID));

                if (service == null)
                {
                    return;
                }

                var write_gatt = service.GetCharacteristics(new Guid(WRITE_GATT_GUID))[0];

                if (write_gatt == null)
                {
                    return;
                }

                Random Random = new Random();
                List<int> colours = new List<int>();

                for (int i = 0; i < 70; i++)
                {
                    colours.Add(Random.Next(0, 0xFFFFFF));
                }

                byte[] meta_data = { 0x09, 0xD7, 0x03 }; //  { 0x09, 0x02, 0x01 };
                byte[] send_data = GenerateKeyboardBLEData(colours);//{ 0x03 };

                KeyboardWriter keyboard_writer = new KeyboardWriter(Dispatcher, write_gatt, meta_data, send_data);

                keyboard_writer.WriteToKeyboard();
                int z = 0;
                z += 1;

                /*  var writer = new DataWriter();
                  byte[] test_bytes = { 0x09, 0x02, 0x01, 0x01}; // this will set the keyboard to red
                  writer.WriteBytes(test_bytes);
                  var res = await write_gatt.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);

                  if (res == GattCommunicationStatus.Success)
                  {
                      Debug.WriteLine("Wrote some data! " );
                  }
                  else
                  {
                      Debug.WriteLine("Failed to write some data!");
                  }*/




            }
            catch
            {
            }
        }

        private async void DeviceAdded(DeviceWatcher watcher, DeviceInformation device)
        {
            if (device.Name.Contains("ANNE"))
            {
                ConnectToKeyboard(device);
            }
        }

        private byte[] GenerateKeyboardBLEData(List<Int32> colours)
        {
            byte[] bluetooth_data = new byte[214];

            for (int i = 0; i < 70; i++)
            {
                int j = 0;
                if (!(i == 40 || i == 53 || i == 54 || i == 59 || i == 60 || i == 62 || i == 63 || i == 64 || i == 65))
                {
                    int colour = colours[i];
                    byte green = (byte)((65280 & colour) >> 8);
                    byte blue = (byte)(255 & colour);
                    bluetooth_data[(i * 3) + 4] = (byte)((16711680 & colour) >> 16);
                    bluetooth_data[((i * 3) + 4) + 1] = green;
                    bluetooth_data[((i * 3) + 4) + 2] = blue;
                    j++;
                }
            }

            int checksum = CRC16.CalculateChecksum(bluetooth_data, 4, 210);
            Debug.WriteLine("CHECKSUM " + checksum);

            byte[] checksum_data = BitConverter.GetBytes(checksum);
            Array.Reverse(checksum_data);

            for (int i = 0; i < 4; i++)
            {
                bluetooth_data[i] = checksum_data[i];
            }

            return bluetooth_data;
        }

        private void DeviceUpdated(DeviceWatcher watcher, DeviceInformationUpdate update)
        {
            //Debug.WriteLine($"Device updated: {update.Id}");
        }

        private void startButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.SetupBluetooth();
        }
    }
}
