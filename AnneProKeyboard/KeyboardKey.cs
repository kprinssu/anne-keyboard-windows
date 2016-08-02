using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnneProKeyboard
{
    public class KeyboardKey
    {
        public readonly string KeyLabel;
        public readonly int KeyValue;

        public KeyboardKey(string KeyLabel, int KeyValue)
        {
            this.KeyLabel = KeyLabel;
            this.KeyValue = KeyValue;
        }

        public KeyboardKey(KeyboardKey keyboard_key)
        {
            this.KeyLabel = keyboard_key.KeyLabel;
            this.KeyValue = keyboard_key.KeyValue;
        }
    }
}
