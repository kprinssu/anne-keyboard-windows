using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnneProKeyboard
{
    public class KeyboardKey
    {
        public static List<KeyboardKey> NormalKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> FunctionKeys = new List<KeyboardKey>();
        
        static KeyboardKey()
        {
            NormalKeys.Add(new KeyboardKey("esc", 41));
            NormalKeys.Add(new KeyboardKey("1", 30));
            NormalKeys.Add(new KeyboardKey("2", 31));
            NormalKeys.Add(new KeyboardKey("3", 32));
            NormalKeys.Add(new KeyboardKey("4", 33));
            NormalKeys.Add(new KeyboardKey("5", 34));
            NormalKeys.Add(new KeyboardKey("6", 35));
            NormalKeys.Add(new KeyboardKey("7", 36));
            NormalKeys.Add(new KeyboardKey("8", 37));
            NormalKeys.Add(new KeyboardKey("9", 38));
            NormalKeys.Add(new KeyboardKey("0", 39));
            NormalKeys.Add(new KeyboardKey("-_", 45));
            NormalKeys.Add(new KeyboardKey("=+", 46));
            NormalKeys.Add(new KeyboardKey("bkspace", 42));
            NormalKeys.Add(new KeyboardKey("tab", 43));
            NormalKeys.Add(new KeyboardKey("Q", 20));
            NormalKeys.Add(new KeyboardKey("W", 26));
            NormalKeys.Add(new KeyboardKey("E", 8));
            NormalKeys.Add(new KeyboardKey("R", 21));
            NormalKeys.Add(new KeyboardKey("T", 23));
            NormalKeys.Add(new KeyboardKey("Y", 28));
            NormalKeys.Add(new KeyboardKey("U", 24));
            NormalKeys.Add(new KeyboardKey("I", 12));
            NormalKeys.Add(new KeyboardKey("O", 18));
            NormalKeys.Add(new KeyboardKey("P", 19));
            NormalKeys.Add(new KeyboardKey("[{", 47));
            NormalKeys.Add(new KeyboardKey("]}", 48));
            NormalKeys.Add(new KeyboardKey("\\|", 49));
            NormalKeys.Add(new KeyboardKey("caps", 57));
            NormalKeys.Add(new KeyboardKey("A", 4));
            NormalKeys.Add(new KeyboardKey("S", 22));
            NormalKeys.Add(new KeyboardKey("D", 7));
            NormalKeys.Add(new KeyboardKey("F", 9));
            NormalKeys.Add(new KeyboardKey("G", 10));
            NormalKeys.Add(new KeyboardKey("H", 11));
            NormalKeys.Add(new KeyboardKey("J", 13));
            NormalKeys.Add(new KeyboardKey("K", 14));
            NormalKeys.Add(new KeyboardKey("L", 15));
            NormalKeys.Add(new KeyboardKey(";:", 51));
            NormalKeys.Add(new KeyboardKey("\"", 52));
            NormalKeys.Add(new KeyboardKey("enter", 40));
            NormalKeys.Add(new KeyboardKey("shift", 225));
            NormalKeys.Add(new KeyboardKey("Z", 29));
            NormalKeys.Add(new KeyboardKey("X", 27));
            NormalKeys.Add(new KeyboardKey("C", 6));
            NormalKeys.Add(new KeyboardKey("V", 25));
            NormalKeys.Add(new KeyboardKey("B", 5));
            NormalKeys.Add(new KeyboardKey("N", 17));
            NormalKeys.Add(new KeyboardKey("M", 16));
            NormalKeys.Add(new KeyboardKey(",<", 54));
            NormalKeys.Add(new KeyboardKey(".>", 55));
            NormalKeys.Add(new KeyboardKey("/?", 56));
            NormalKeys.Add(new KeyboardKey("shift", 229));
            NormalKeys.Add(new KeyboardKey("ctrl", 224));
            NormalKeys.Add(new KeyboardKey("win", 227));
            NormalKeys.Add(new KeyboardKey("alt", 226));
            NormalKeys.Add(new KeyboardKey("space", 44));
            NormalKeys.Add(new KeyboardKey("alt", 230));
            NormalKeys.Add(new KeyboardKey("fn", 254));
            NormalKeys.Add(new KeyboardKey("anne", 250));
            NormalKeys.Add(new KeyboardKey("ctrl", 228));

            FunctionKeys.Add(new KeyboardKey("`~", 53));
            FunctionKeys.Add(new KeyboardKey("F1", 58));
            FunctionKeys.Add(new KeyboardKey("F2", 59));
            FunctionKeys.Add(new KeyboardKey("F3", 60));
            FunctionKeys.Add(new KeyboardKey("F4", 61));
            FunctionKeys.Add(new KeyboardKey("F5", 62));
            FunctionKeys.Add(new KeyboardKey("F6", 63));
            FunctionKeys.Add(new KeyboardKey("F7", 64));
            FunctionKeys.Add(new KeyboardKey("F8", 65));
            FunctionKeys.Add(new KeyboardKey("F9", 66));
            FunctionKeys.Add(new KeyboardKey("F10", 67));
            FunctionKeys.Add(new KeyboardKey("F11", 68));
            FunctionKeys.Add(new KeyboardKey("F12", 69));
            FunctionKeys.Add(new KeyboardKey("up", 82));
            FunctionKeys.Add(new KeyboardKey("up", 82));
            FunctionKeys.Add(new KeyboardKey("sc", 71));
            FunctionKeys.Add(new KeyboardKey("pb", 72));
            FunctionKeys.Add(new KeyboardKey("hm", 74));
            FunctionKeys.Add(new KeyboardKey("end", 77));
            FunctionKeys.Add(new KeyboardKey("ps", 70));
            FunctionKeys.Add(new KeyboardKey("lt", 80));
            FunctionKeys.Add(new KeyboardKey("dn", 81));
            FunctionKeys.Add(new KeyboardKey("rt", 79));
            FunctionKeys.Add(new KeyboardKey("lt", 80));
            FunctionKeys.Add(new KeyboardKey("dn", 81));
            FunctionKeys.Add(new KeyboardKey("rt", 79));
            FunctionKeys.Add(new KeyboardKey("pu", 75));
            FunctionKeys.Add(new KeyboardKey("pd", 78));
            FunctionKeys.Add(new KeyboardKey("ins", 73));
            FunctionKeys.Add(new KeyboardKey("del", 76));
            FunctionKeys.Add(new KeyboardKey("lock", 227));
            FunctionKeys.Add(new KeyboardKey("fn", 254));
            FunctionKeys.Add(new KeyboardKey("anne", 250));

        }

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
