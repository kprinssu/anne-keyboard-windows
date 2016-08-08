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
    }
}
