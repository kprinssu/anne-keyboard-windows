using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace AnneProKeyboard
{
    [DataContract]
    public class KeyboardProfileItem : INotifyPropertyChanged
    {
        private int _ID;
        [DataMember]
        public int ID
        {
            get { return this._ID; }
            set
            {
                this._ID = value;
                OnPropertyChanged("ID");
            }
        }
        private string _label;
        [DataMember]
        public string Label
        {
            get { return this._label; }

            set
            {
                this._label = value;
                OnPropertyChanged("Label");
            }
        }

        public KeyboardProfileItem(int ID, string Label)
        {
            this.ID = ID;
            this.Label = Label;
            
            this.KeyboardColours = new List<int>();

            // We only need 70 values to represent the 61 keys (70 is needed for some reason by the keyboard..)
            for (int i = 0; i < 70; i++)
            {
                this.KeyboardColours.Add(0xFFFFFF); // White by default
            }

            KeyboardKey.InitaliseKeyboardProfile(this);
        }

        [DataMember]
        public List<int> KeyboardColours { get; set; }
        [DataMember]
        public List<KeyboardKey> NormalKeys { get; set; } // the normal keys, WE ***MUST*** ENSURE THAT FN and ANNE keys EXIST!!!
        [DataMember]
        public List<KeyboardKey> FnKeys { get; set; } // represents the Fn + x key combo
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
            var property_handler = PropertyChanged;

            if (property_handler != null)
            {
                property_handler(this, new PropertyChangedEventArgs(property));
            }
        }

        // returns true if the Anne Key and Fn key exists in both Normal keys and Fn keys lists
        public bool ValidateKeyboardKeys()
        {
            // Double-check if the Fn and Anne Pro keys are defined (we are sorta screwed if we do not ensure that the Fn or Anne keys do not exist)
            bool normal_important_keys_exists = SpecialKeyExists(this.NormalKeys);
            bool fn_important_keys_exists = SpecialKeyExists(this.FnKeys);

            return normal_important_keys_exists && fn_important_keys_exists;
        }

        // perform a linear scan for the Anne and Fn keys
        private bool SpecialKeyExists(List<KeyboardKey> keys)
        {
            bool anne_key = false;
            bool fn_key = false;

            foreach (KeyboardKey key in this.NormalKeys)
            {
                if (key.KeyValue == 250)
                {
                    anne_key = true;
                }
                else if (key.KeyValue == 254)
                {
                    fn_key = true;
                }
            }

            return anne_key && fn_key;
        }


        //All of the below logic was ported over from the Android app
        //Credits to devs at obins.net
        public byte[] GenerateKeyboardBacklightData()
        {
            byte[] bluetooth_data = new byte[214];

            for (int i = 0; i < 70; i++)
            {
                int j = 0;
                if (!(i == 40 || i == 53 || i == 54 || i == 59 || i == 60 || i == 62 || i == 63 || i == 64 || i == 65))
                {
                    int colour = this.KeyboardColours[i];
                    byte green = (byte)((65280 & colour) >> 8);
                    byte blue = (byte)(255 & colour);
                    bluetooth_data[(i * 3) + 4] = (byte)((16711680 & colour) >> 16);
                    bluetooth_data[((i * 3) + 4) + 1] = green;
                    bluetooth_data[((i * 3) + 4) + 2] = blue;
                    j++;
                }
            }

            int checksum = CRC16.CalculateChecksum(bluetooth_data, 4, 210);

            byte[] checksum_data = BitConverter.GetBytes(checksum);
            Array.Reverse(checksum_data);
            Array.Copy(checksum_data, 0, bluetooth_data, 0, checksum_data.Length);

            return bluetooth_data;
        }

        //All of the below logic was ported over from the Android app
        //Credits to devs at obins.net
        public byte[] GenerateKeyboardLayoutData()
        {
            byte[] bluetooth_data = new byte[144];

            byte[] standard_keys = this.NormalKeys.Select(key => (byte)key.KeyValue).ToArray();
            byte[] fn_keys = this.FnKeys.Select(key => (byte)key.KeyValue).ToArray();

            byte[] standard_converted_keys = new byte[70];
            byte[] fn_converted_keys = new byte[70];

            // convert from 61 keys to 70 keys
            int j = 0;
            for (int i = 0; i < 70; i++)
            {
                if (!(i == 40 || i == 53 || i == 54 || i == 59 || i == 60 || i == 62 || i == 63 || i == 64 || i == 65))
                {
                    standard_converted_keys[i] = standard_keys[j];
                    fn_converted_keys[j] = fn_keys[j];
                    j++;
                }
            }

            Array.Copy(standard_converted_keys, 0, bluetooth_data, 4, standard_converted_keys.Length);
            Array.Copy(fn_converted_keys, 0, bluetooth_data, 74, fn_converted_keys.Length);

            int checksum = CRC16.CalculateChecksum(bluetooth_data);
            if (checksum < 10)
            {
                checksum += 10;
            }
            byte[] checksum_data = BitConverter.GetBytes(checksum);
            Array.Reverse(checksum_data);
            Array.Copy(checksum_data, 0, bluetooth_data, 0, checksum_data.Length);

            return bluetooth_data;
        }
    }
}
