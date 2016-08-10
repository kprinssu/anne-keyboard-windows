using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AnneProKeyboard
{
    [DataContract]
    public class KeyboardKey
    {
        public static List<KeyboardKey> AlphabetKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> NumberKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> ModifierKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> PunctuationKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> FunctionKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> SpecialKeys = new List<KeyboardKey>();
        public static List<KeyboardKey> MediaKeys = new List<KeyboardKey>();

        // use for easy look ups
        public static Dictionary<String, KeyboardKey> StringKeyboardKeys = new Dictionary<string, KeyboardKey>();
        public static Dictionary<Int32, KeyboardKey> IntKeyboardKeys = new Dictionary<Int32, KeyboardKey>();

        static KeyboardKey()
        {
            // "Empty" key, also the unassigned value key
            StringKeyboardKeys[""] = new KeyboardKey("", "None", 0);
            IntKeyboardKeys[0] = StringKeyboardKeys[""];

            // "Anne" key
            StringKeyboardKeys["Anne"] = new KeyboardKey("Anne", "Anne", 250);
            IntKeyboardKeys[250] = StringKeyboardKeys["Anne"];

            // Fn Key
            StringKeyboardKeys["Fn"] = new KeyboardKey("Fn", "Fn", 254);
            IntKeyboardKeys[254] = StringKeyboardKeys["Fn"];

            // Windows Key Lock
            // TODO: verify it works properly

            // Keys for the Alphabet (a-z, A-Z)
            AlphabetKeys.Add(new KeyboardKey("A", "A", 4));
            AlphabetKeys.Add(new KeyboardKey("B", "B", 5));
            AlphabetKeys.Add(new KeyboardKey("C", "C", 6));
            AlphabetKeys.Add(new KeyboardKey("D", "D", 7));
            AlphabetKeys.Add(new KeyboardKey("E", "E", 8));
            AlphabetKeys.Add(new KeyboardKey("F", "F", 9));
            AlphabetKeys.Add(new KeyboardKey("G", "G", 10));
            AlphabetKeys.Add(new KeyboardKey("H", "H", 11));
            AlphabetKeys.Add(new KeyboardKey("I", "I", 12));
            AlphabetKeys.Add(new KeyboardKey("J", "J", 13));
            AlphabetKeys.Add(new KeyboardKey("K", "K", 14));
            AlphabetKeys.Add(new KeyboardKey("L", "L", 15));
            AlphabetKeys.Add(new KeyboardKey("M", "M", 16));
            AlphabetKeys.Add(new KeyboardKey("N", "N", 17));
            AlphabetKeys.Add(new KeyboardKey("O", "O", 18));
            AlphabetKeys.Add(new KeyboardKey("P", "P", 19));
            AlphabetKeys.Add(new KeyboardKey("Q", "Q", 20));
            AlphabetKeys.Add(new KeyboardKey("R", "R", 21));
            AlphabetKeys.Add(new KeyboardKey("S", "S", 22));
            AlphabetKeys.Add(new KeyboardKey("T", "T", 23));
            AlphabetKeys.Add(new KeyboardKey("U", "U", 24));
            AlphabetKeys.Add(new KeyboardKey("V", "V", 25));
            AlphabetKeys.Add(new KeyboardKey("W", "W", 26));
            AlphabetKeys.Add(new KeyboardKey("X", "X", 27));
            AlphabetKeys.Add(new KeyboardKey("Y", "Y", 28));
            AlphabetKeys.Add(new KeyboardKey("Z", "Z", 29));

            InitialiseKeyDictionaries(AlphabetKeys);

            // The row (not the NUMPAD) keyboard keys (0-9)
            NumberKeys.Add(new KeyboardKey("0", "0", 39));
            NumberKeys.Add(new KeyboardKey("1", "1", 30));
            NumberKeys.Add(new KeyboardKey("2", "2", 31));
            NumberKeys.Add(new KeyboardKey("3", "3", 32));
            NumberKeys.Add(new KeyboardKey("4", "4", 33));
            NumberKeys.Add(new KeyboardKey("5", "5", 34));
            NumberKeys.Add(new KeyboardKey("6", "6", 35));
            NumberKeys.Add(new KeyboardKey("7", "7", 36));
            NumberKeys.Add(new KeyboardKey("8", "8", 37));
            NumberKeys.Add(new KeyboardKey("9", "9", 38));

            InitialiseKeyDictionaries(NumberKeys);

            // Modifier keys (Shift, TAB, ESC, etc.)
            ModifierKeys.Add(new KeyboardKey("Escape", "Esc", 41));
            ModifierKeys.Add(new KeyboardKey("Tab", "Tab", 43));
            ModifierKeys.Add(new KeyboardKey("Caps Lock", "Caps", 57));
            ModifierKeys.Add(new KeyboardKey("Left Shift", "L Shft", 225));
            ModifierKeys.Add(new KeyboardKey("Left Control", "L Ctrl", 224));
            ModifierKeys.Add(new KeyboardKey("Left Windows", "L Win", 227));
            ModifierKeys.Add(new KeyboardKey("Right Windows", "R Win", 231));
            ModifierKeys.Add(new KeyboardKey("Left Command", "L Cmd", 227));
            ModifierKeys.Add(new KeyboardKey("Right Command", "R Cmd", 231));
            ModifierKeys.Add(new KeyboardKey("Left Option", "L Opt", 226));
            ModifierKeys.Add(new KeyboardKey("Right Option", "R Opt", 230));
            ModifierKeys.Add(new KeyboardKey("Left Alt", "L Alt", 226));
            ModifierKeys.Add(new KeyboardKey("Spacebar", "Space", 44));
            ModifierKeys.Add(new KeyboardKey("Right Alt", "R Alt", 230));
            ModifierKeys.Add(new KeyboardKey("Right Control", "R Ctrl", 228));
            ModifierKeys.Add(new KeyboardKey("Right Shift", "R Shft", 229));
            ModifierKeys.Add(new KeyboardKey("Enter", "Enter", 40));
            ModifierKeys.Add(new KeyboardKey("Backspace", "BkSpce", 42));

            InitialiseKeyDictionaries(ModifierKeys);

            // Punctuation keys (~, \, ", etc)
            PunctuationKeys.Add(new KeyboardKey("`~", "`~", 53));
            PunctuationKeys.Add(new KeyboardKey("-_", "-_", 45));
            PunctuationKeys.Add(new KeyboardKey("=+", "=+", 46));
            PunctuationKeys.Add(new KeyboardKey("[{", "[{", 47));
            PunctuationKeys.Add(new KeyboardKey("]}", "]}", 48));
            PunctuationKeys.Add(new KeyboardKey("\\|", "\\|", 49));
            PunctuationKeys.Add(new KeyboardKey(";:", ";:", 51));
            PunctuationKeys.Add(new KeyboardKey("'\"", "'\"", 52));
            PunctuationKeys.Add(new KeyboardKey(",<", ",<", 54));
            PunctuationKeys.Add(new KeyboardKey(".>", ".>", 55));
            PunctuationKeys.Add(new KeyboardKey("/?", "/?", 56));

            InitialiseKeyDictionaries(PunctuationKeys);

            // Function keys (F1, F2, etc)
            FunctionKeys.Add(new KeyboardKey("F1", "F1", 58));
            FunctionKeys.Add(new KeyboardKey("F2", "F2", 59));
            FunctionKeys.Add(new KeyboardKey("F3", "F3", 60));
            FunctionKeys.Add(new KeyboardKey("F4", "F4", 61));
            FunctionKeys.Add(new KeyboardKey("F5", "F5", 62));
            FunctionKeys.Add(new KeyboardKey("F6", "F6", 63));
            FunctionKeys.Add(new KeyboardKey("F7", "F7", 64));
            FunctionKeys.Add(new KeyboardKey("F8", "F8", 65));
            FunctionKeys.Add(new KeyboardKey("F9", "F9", 66));
            FunctionKeys.Add(new KeyboardKey("F10", "F10", 67));
            FunctionKeys.Add(new KeyboardKey("F11", "F11", 68));
            FunctionKeys.Add(new KeyboardKey("F12", "F12", 69));

            InitialiseKeyDictionaries(FunctionKeys);

            // "Special" Keys (Insert, Home, etc.) also includes the Direction Keys
            // obins devs labelled these keys as the "Fn + x" keys
            SpecialKeys.Add(new KeyboardKey("Print Screen", "PrtSc", 70));
            SpecialKeys.Add(new KeyboardKey("Scroll Lock", "ScrLk", 71));
            SpecialKeys.Add(new KeyboardKey("Pause", "Pause",  72));
            SpecialKeys.Add(new KeyboardKey("Insert", "Ins", 73));
            SpecialKeys.Add(new KeyboardKey("Delete", "Del", 76));
            SpecialKeys.Add(new KeyboardKey("Home", "Home", 74));
            SpecialKeys.Add(new KeyboardKey("End", "End", 77));
            SpecialKeys.Add(new KeyboardKey("Page Down", "PgDn", 78));
            SpecialKeys.Add(new KeyboardKey("Page Up", "PgUp", 75));
            SpecialKeys.Add(new KeyboardKey("Left", "Left", 80));
            SpecialKeys.Add(new KeyboardKey("Up", "Up", 82));
            SpecialKeys.Add(new KeyboardKey("Down", "Down", 81));
            SpecialKeys.Add(new KeyboardKey("Right", "Right", 79));

            InitialiseKeyDictionaries(SpecialKeys);

            // Volume keys (Volume Increase/Decrease, Mute only)
            MediaKeys.Add(new KeyboardKey("Mute", "Mute", 127));
            MediaKeys.Add(new KeyboardKey("Volume Up", "Vol. Up", 128));
            MediaKeys.Add(new KeyboardKey("Volume Down", "Vol.D.", 129));

            InitialiseKeyDictionaries(MediaKeys);
        }

        [DataMember]
        public readonly string KeyLabel;
        [DataMember]
        public readonly int KeyValue;
        [DataMember]
        public readonly string KeyShortLabel;

        private KeyboardKey(string KeyLabel, string KeyShortLabel, int KeyValue)
        {
            this.KeyLabel = KeyLabel;
            this.KeyValue = KeyValue;
            this.KeyShortLabel = KeyShortLabel;
        }

        private KeyboardKey(KeyboardKey keyboard_key)
        {
            this.KeyLabel = keyboard_key.KeyLabel;
            this.KeyValue = keyboard_key.KeyValue;
            this.KeyShortLabel = keyboard_key.KeyShortLabel;
        }

        private static void InitialiseKeyDictionaries(List<KeyboardKey> keys)
        {
            foreach (KeyboardKey key in keys)
            {
                if(!StringKeyboardKeys.ContainsKey(key.KeyLabel))
                {
                    StringKeyboardKeys.Add(key.KeyLabel, key);
                }

                if (!IntKeyboardKeys.ContainsKey(key.KeyValue))
                {
                    IntKeyboardKeys.Add(key.KeyValue, key);
                }
            }
        }

        public static void InitaliseKeyboardProfile(KeyboardProfileItem profile)
        {
            // This is a straight copy from obins app
            profile.NormalKeys = new List<KeyboardKey>();
            profile.FnKeys = new List<KeyboardKey>();

            // Normal keys
            profile.NormalKeys.Add(IntKeyboardKeys[41]);
            profile.NormalKeys.Add(IntKeyboardKeys[30]);
            profile.NormalKeys.Add(IntKeyboardKeys[31]);
            profile.NormalKeys.Add(IntKeyboardKeys[32]);
            profile.NormalKeys.Add(IntKeyboardKeys[33]);
            profile.NormalKeys.Add(IntKeyboardKeys[34]);
            profile.NormalKeys.Add(IntKeyboardKeys[35]);
            profile.NormalKeys.Add(IntKeyboardKeys[36]);
            profile.NormalKeys.Add(IntKeyboardKeys[37]);
            profile.NormalKeys.Add(IntKeyboardKeys[38]);
            profile.NormalKeys.Add(IntKeyboardKeys[39]);
            profile.NormalKeys.Add(IntKeyboardKeys[45]);
            profile.NormalKeys.Add(IntKeyboardKeys[46]);
            profile.NormalKeys.Add(IntKeyboardKeys[42]);
            profile.NormalKeys.Add(IntKeyboardKeys[43]);
            profile.NormalKeys.Add(IntKeyboardKeys[20]);
            profile.NormalKeys.Add(IntKeyboardKeys[26]);
            profile.NormalKeys.Add(IntKeyboardKeys[8]);
            profile.NormalKeys.Add(IntKeyboardKeys[21]);
            profile.NormalKeys.Add(IntKeyboardKeys[23]);
            profile.NormalKeys.Add(IntKeyboardKeys[28]);
            profile.NormalKeys.Add(IntKeyboardKeys[24]);
            profile.NormalKeys.Add(IntKeyboardKeys[12]);
            profile.NormalKeys.Add(IntKeyboardKeys[18]);
            profile.NormalKeys.Add(IntKeyboardKeys[19]);
            profile.NormalKeys.Add(IntKeyboardKeys[47]);
            profile.NormalKeys.Add(IntKeyboardKeys[48]);
            profile.NormalKeys.Add(IntKeyboardKeys[49]);
            profile.NormalKeys.Add(IntKeyboardKeys[57]);
            profile.NormalKeys.Add(IntKeyboardKeys[4]);
            profile.NormalKeys.Add(IntKeyboardKeys[22]);
            profile.NormalKeys.Add(IntKeyboardKeys[7]);
            profile.NormalKeys.Add(IntKeyboardKeys[9]);
            profile.NormalKeys.Add(IntKeyboardKeys[10]);
            profile.NormalKeys.Add(IntKeyboardKeys[11]);
            profile.NormalKeys.Add(IntKeyboardKeys[13]);
            profile.NormalKeys.Add(IntKeyboardKeys[14]);
            profile.NormalKeys.Add(IntKeyboardKeys[15]);
            profile.NormalKeys.Add(IntKeyboardKeys[51]);
            profile.NormalKeys.Add(IntKeyboardKeys[52]);
            profile.NormalKeys.Add(IntKeyboardKeys[40]);
            profile.NormalKeys.Add(IntKeyboardKeys[225]);
            profile.NormalKeys.Add(IntKeyboardKeys[29]);
            profile.NormalKeys.Add(IntKeyboardKeys[27]);
            profile.NormalKeys.Add(IntKeyboardKeys[6]);
            profile.NormalKeys.Add(IntKeyboardKeys[25]);
            profile.NormalKeys.Add(IntKeyboardKeys[5]);
            profile.NormalKeys.Add(IntKeyboardKeys[17]);
            profile.NormalKeys.Add(IntKeyboardKeys[16]);
            profile.NormalKeys.Add(IntKeyboardKeys[54]);
            profile.NormalKeys.Add(IntKeyboardKeys[55]);
            profile.NormalKeys.Add(IntKeyboardKeys[56]);
            profile.NormalKeys.Add(IntKeyboardKeys[229]);
            profile.NormalKeys.Add(IntKeyboardKeys[224]);
            profile.NormalKeys.Add(IntKeyboardKeys[227]);
            profile.NormalKeys.Add(IntKeyboardKeys[226]);
            profile.NormalKeys.Add(IntKeyboardKeys[44]);
            profile.NormalKeys.Add(IntKeyboardKeys[230]);
            profile.NormalKeys.Add(IntKeyboardKeys[254]);
            profile.NormalKeys.Add(IntKeyboardKeys[250]);
            profile.NormalKeys.Add(IntKeyboardKeys[228]);

            // Function keys (Fn + X)
            profile.FnKeys.Add(IntKeyboardKeys[53]);
            profile.FnKeys.Add(IntKeyboardKeys[58]);
            profile.FnKeys.Add(IntKeyboardKeys[59]);
            profile.FnKeys.Add(IntKeyboardKeys[60]);
            profile.FnKeys.Add(IntKeyboardKeys[61]);
            profile.FnKeys.Add(IntKeyboardKeys[62]);
            profile.FnKeys.Add(IntKeyboardKeys[63]);
            profile.FnKeys.Add(IntKeyboardKeys[64]);
            profile.FnKeys.Add(IntKeyboardKeys[65]);
            profile.FnKeys.Add(IntKeyboardKeys[66]);
            profile.FnKeys.Add(IntKeyboardKeys[67]);
            profile.FnKeys.Add(IntKeyboardKeys[68]);
            profile.FnKeys.Add(IntKeyboardKeys[69]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[82]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[82]);
            profile.FnKeys.Add(IntKeyboardKeys[71]);
            profile.FnKeys.Add(IntKeyboardKeys[72]);
            profile.FnKeys.Add(IntKeyboardKeys[74]);
            profile.FnKeys.Add(IntKeyboardKeys[77]);
            profile.FnKeys.Add(IntKeyboardKeys[70]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[80]);
            profile.FnKeys.Add(IntKeyboardKeys[81]);
            profile.FnKeys.Add(IntKeyboardKeys[79]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[80]);
            profile.FnKeys.Add(IntKeyboardKeys[81]);
            profile.FnKeys.Add(IntKeyboardKeys[79]);
            profile.FnKeys.Add(IntKeyboardKeys[75]);
            profile.FnKeys.Add(IntKeyboardKeys[78]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[73]);
            profile.FnKeys.Add(IntKeyboardKeys[76]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[227]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
            profile.FnKeys.Add(IntKeyboardKeys[254]);
            profile.FnKeys.Add(IntKeyboardKeys[250]);
            profile.FnKeys.Add(IntKeyboardKeys[0]);
        }
    }
}
