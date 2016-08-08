using System.Collections.Generic;
using System.ComponentModel;

namespace AnneProKeyboard
{
    public class KeyboardProfileItem : INotifyPropertyChanged
    {
        private int _ID;
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
        public string Label
        {
            get { return this._label; }

            set
            {
                this._label = value;
                OnPropertyChanged("Label");
            }
        }
        public List<int> KeyboardColours { get; set; }
        public List<KeyboardKey> NormalKeys { get; set; } // the normal keys, WE ***MUST*** ENSURE THAT FN and ANNE keys EXIST!!!
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
    }
}
