using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SculptorScoketServer
{
    class Program
    {
        private static bool useTcp = false;

        // TCP use
        private static string myIP = "127.0.0.1";
        private static int myProt = 8885;

        // UDP use
        private static Socket udpServer;
        private static List<string> clientIPs;
        private static int recvProt = 8885;
        private static int sendProt = 8886;

        static List<Client> clientlists = new List<Client>();

        public static void BroadcastMessage(string message)
        {
            var NotConnectClient = new List<Client>();
            foreach (var client in clientlists)
            {
                if (client.Connected())
                {
                    client.SendMessage(message);
                }
                else
                {
                    NotConnectClient.Add(client);
                }
            }
            foreach (var temp in NotConnectClient)
            {
                clientlists.Remove(temp);
            }
        }

        static void Main(string[] args)
        {
            myIP = GetLocalIPAddress();

            if (useTcp)
            {
                Socket tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(myIP);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, myProt);
                tcpServer.Bind(ipEndPoint);

                Console.WriteLine("TCP Server Start....");
                tcpServer.Listen(10);

                while (true)
                {
                    Socket clientSocket = tcpServer.Accept();
                    Client client = new Client(clientSocket);
                    Console.WriteLine("Connect a Client...");
                    clientlists.Add(client);
                }
            }
            else
            {
                clientIPs = new List<string>();

                udpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udpServer.Bind(new IPEndPoint(IPAddress.Parse(myIP), recvProt));
                Console.WriteLine("UDP Server Start...");
                Thread t = new Thread(ReciveMsg); // start receive thread
                t.Start();
            }
        }

        static void SendMsg(string message)
        {
            foreach(string cip in clientIPs)
            {
                EndPoint point = new IPEndPoint(IPAddress.Parse(cip), sendProt);
                udpServer.SendTo(Encoding.UTF8.GetBytes(message), point);
                //Console.WriteLine("Send: " + point.ToString() + " " + message);
            }
        }

        static void ReciveMsg()
        {
            while (true)
            {
                EndPoint point = new IPEndPoint(IPAddress.Any, 0); // store the sender ip & port
                byte[] buffer = new byte[1024];
                int length = udpServer.ReceiveFrom(buffer, ref point); // receive data package
                string message = Encoding.UTF8.GetString(buffer, 0, length);

                //Console.WriteLine("Receive: " + point.ToString() + " " + message);

                bool iscontain = false;
                string newIP = point.ToString().Split(':')[0];
                foreach (string cip in clientIPs)
                {
                    if (cip.Equals(newIP))
                    {
                        iscontain = true;
                    }
                }
                if (!iscontain)
                {
                    clientIPs.Add(newIP);
                    Console.WriteLine("Add Client: " + point.ToString());
                }

                if (message != "")
                {
                    SendMsg(message);
                }
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

    }

}
