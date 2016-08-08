using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnneProKeyboard
{
    public class KeyboardKey
    {
        public static List<KeyboardKey> AlphabetKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> NumberKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> ModifierKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> PunctuationKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> FunctionKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> SpecialKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> MediaKeys = new List<KeyboardKey>();

        static KeyboardKey()
        {
            // Keys for the Alphabet (a-z, A-Z)
            AlphabetKeys.Add(new KeyboardKey("A", 4));
            AlphabetKeys.Add(new KeyboardKey("B", 5));
            AlphabetKeys.Add(new KeyboardKey("C", 6));
            AlphabetKeys.Add(new KeyboardKey("D", 7));
            AlphabetKeys.Add(new KeyboardKey("E", 8));
            AlphabetKeys.Add(new KeyboardKey("F", 9));
            AlphabetKeys.Add(new KeyboardKey("G", 10));
            AlphabetKeys.Add(new KeyboardKey("H", 11));
            AlphabetKeys.Add(new KeyboardKey("I", 12));
            AlphabetKeys.Add(new KeyboardKey("J", 13));
            AlphabetKeys.Add(new KeyboardKey("K", 14));
            AlphabetKeys.Add(new KeyboardKey("L", 15));
            AlphabetKeys.Add(new KeyboardKey("M", 16));
            AlphabetKeys.Add(new KeyboardKey("N", 17));
            AlphabetKeys.Add(new KeyboardKey("O", 18));
            AlphabetKeys.Add(new KeyboardKey("P", 19));
            AlphabetKeys.Add(new KeyboardKey("Q", 20));
            AlphabetKeys.Add(new KeyboardKey("R", 21));
            AlphabetKeys.Add(new KeyboardKey("S", 22));
            AlphabetKeys.Add(new KeyboardKey("T", 23));
            AlphabetKeys.Add(new KeyboardKey("U", 24));
            AlphabetKeys.Add(new KeyboardKey("V", 25));
            AlphabetKeys.Add(new KeyboardKey("W", 26));
            AlphabetKeys.Add(new KeyboardKey("X", 27));
            AlphabetKeys.Add(new KeyboardKey("Y", 28));
            AlphabetKeys.Add(new KeyboardKey("Z", 29));

            // The row (not the NUMPAD) keyboard keys (0-9)
            NumberKeys.Add(new KeyboardKey("0", 39));
            NumberKeys.Add(new KeyboardKey("1", 30));
            NumberKeys.Add(new KeyboardKey("2", 31));
            NumberKeys.Add(new KeyboardKey("3", 32));
            NumberKeys.Add(new KeyboardKey("4", 33));
            NumberKeys.Add(new KeyboardKey("5", 34));
            NumberKeys.Add(new KeyboardKey("6", 35));
            NumberKeys.Add(new KeyboardKey("7", 36));
            NumberKeys.Add(new KeyboardKey("8", 37));
            NumberKeys.Add(new KeyboardKey("9", 38));

            // Modifier keys (Shift, TAB, ESC, etc.)
            ModifierKeys.Add(new KeyboardKey("Escape", 41));
            ModifierKeys.Add(new KeyboardKey("Tab", 43));
            ModifierKeys.Add(new KeyboardKey("Caps Lock", 57));
            ModifierKeys.Add(new KeyboardKey("Left Shift", 225));
            ModifierKeys.Add(new KeyboardKey("Left Control", 224));
            ModifierKeys.Add(new KeyboardKey("Left Windows", 227));
            ModifierKeys.Add(new KeyboardKey("Right Windows", 231));
            ModifierKeys.Add(new KeyboardKey("Left Command", 227));
            ModifierKeys.Add(new KeyboardKey("Right Command", 231));
            ModifierKeys.Add(new KeyboardKey("Left Option", 226));
            ModifierKeys.Add(new KeyboardKey("Right Option", 230));
            ModifierKeys.Add(new KeyboardKey("Left Alt", 226));
            ModifierKeys.Add(new KeyboardKey("Spacebar", 44));
            ModifierKeys.Add(new KeyboardKey("Right Alt", 230));
            ModifierKeys.Add(new KeyboardKey("Right Control", 228));
            ModifierKeys.Add(new KeyboardKey("Right Shift", 229));
            ModifierKeys.Add(new KeyboardKey("Enter", 40));
            ModifierKeys.Add(new KeyboardKey("Backspace", 42));

            // Punctuation keys (~, \, ", etc)
            PunctuationKeys.Add(new KeyboardKey("`~", 53));
            PunctuationKeys.Add(new KeyboardKey("-_", 45));
            PunctuationKeys.Add(new KeyboardKey("=+", 46));
            PunctuationKeys.Add(new KeyboardKey("[{", 47));
            PunctuationKeys.Add(new KeyboardKey("]}", 48));
            PunctuationKeys.Add(new KeyboardKey("\\|", 49));
            PunctuationKeys.Add(new KeyboardKey(";:", 51));
            PunctuationKeys.Add(new KeyboardKey("'\"", 52));
            PunctuationKeys.Add(new KeyboardKey(",<", 54));
            PunctuationKeys.Add(new KeyboardKey(".>", 55));
            PunctuationKeys.Add(new KeyboardKey("/?", 56));

            // Function keys (F1, F2, etc)
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

            // "Special" Keys (Insert, Home, etc.) also includes the Direction Keys
            // obins devs labelled these keys as the "Fn + x" keys
            SpecialKeys.Add(new KeyboardKey("Print Screen", 70));
            SpecialKeys.Add(new KeyboardKey("Scroll Lock", 71));
            SpecialKeys.Add(new KeyboardKey("Pause", 72));
            SpecialKeys.Add(new KeyboardKey("Insert", 73));
            SpecialKeys.Add(new KeyboardKey("Delete", 76));
            SpecialKeys.Add(new KeyboardKey("Home", 74));
            SpecialKeys.Add(new KeyboardKey("End", 77));
            SpecialKeys.Add(new KeyboardKey("Page Down", 78));
            SpecialKeys.Add(new KeyboardKey("Page Up", 75));
            SpecialKeys.Add(new KeyboardKey("Left", 80));
            SpecialKeys.Add(new KeyboardKey("Up", 82));
            SpecialKeys.Add(new KeyboardKey("Down", 81));
            SpecialKeys.Add(new KeyboardKey("Right", 79));

            // Volume keys (Volume Increase/Decrease, Mute only)
            MediaKeys.Add(new KeyboardKey("Mute", 127));
            MediaKeys.Add(new KeyboardKey("Volume Up", 128));
            MediaKeys.Add(new KeyboardKey("Volume Down", 129));

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
