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
        private static string myIP = "10.32.93.177";
        private static int myProt = 8885;

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
            Socket tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse(myIP);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, myProt);
            tcpServer.Bind(ipEndPoint);

            Console.WriteLine("Server Start....");
            tcpServer.Listen(10);

            while (true)
            {
                Socket clientSocket = tcpServer.Accept();
                Client client = new Client(clientSocket);
                Console.WriteLine("Connect a Client...");
                clientlists.Add(client);
            }
        }
    }

}
