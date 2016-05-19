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
    class Client
    {
        private Socket clientSocket;
        private byte[] data = new byte[1024];

        public Client(Socket s)
        {
            clientSocket = s;
            // thread for each client
            Thread thread = new Thread(ReceiveMessage);
            thread.Start();
        }

        private void ReceiveMessage()
        {
            while (true)
            {
                // client disconnect
                if (clientSocket.Poll(-1, SelectMode.SelectRead))
                {
                    int nRead = clientSocket.Receive(data);
                    if (nRead == 0)
                    {
                        clientSocket.Close();
                        Console.WriteLine("Disconnect a Client...");
                        break;
                    }
                    else
                    {
                        // receive message
                        int length = clientSocket.Receive(data);
                        string message = Encoding.UTF8.GetString(data, 0, length);

                        // boardcast message
                        Program.BroadcastMessage(message);
                        //Console.WriteLine(message);
                    }
                }
            }
        }

        public void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            clientSocket.Send(data);
        }

        public bool Connected()
        {
            return clientSocket.Connected;
        }
    }
}
