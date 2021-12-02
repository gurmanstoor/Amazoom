using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;

namespace Amazoom
{
    class Server
    {
        private static IPHostEntry host = Dns.GetHostEntry("localhost");
        private static IPAddress ipAddress = host.AddressList[0];
        private static readonly Socket serverSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 11000;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        private static int orderID = 0;

        private static Computer warehouse1 = new Computer();

        static void Main()
        {
            Console.Title = "Server";
            SetupServer();
            Console.ReadLine(); // When we press enter close everything
            CloseAllSockets();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(ipAddress, PORT));
            serverSocket.Listen(1);
            serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private static void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }

            clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Client connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            orderID++;

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Received Text: " + text);

            if(text.ToLower() == "get json")
            {
                string fileName = "../../../testing.json";
                string jsonString = File.ReadAllText(fileName);

                Console.WriteLine("Text is a get json request");
                byte[] data = Encoding.ASCII.GetBytes(jsonString);
                current.Send(data);
                Console.WriteLine("json sent to client");

                //Product[] items = JsonSerializer.Deserialize<Product[]>(jsonString);
            }
            else
            {
                string[] orders = text.Split(";").ToArray();
                int result;
                Product[] products = Computer.ReadInventory();
                List<(Product, int)> orderItems = new List<(Product, int)>();

                for (int i = 0; i < orders.Length; i++)
                {
                    if (int.TryParse(orders[i], out result))
                    {
                        products[result].stock = products[result].stock - 1;

                        if (i == 0)
                        {
                            orderItems.Add((products[i], 1));
                        }
                        else
                        {
                            for (int j = 0; j < orderItems.Count(); j++)
                            {
                                if (orderItems[j].Item1.name == products[result].name)
                                {
                                    orderItems[j] = (orderItems[j].Item1, orderItems[j].Item2 + 1);
                                }
                                else
                                {
                                    orderItems.Add((products[result], 1));
                                }
                            }
                        }
                    }
                }
                Computer.UpdateInventory(products);
                Console.WriteLine("Sending order to warehouse");
                warehouse1.recieveOrder(orderItems, orderID);
            }


            // Always Shutdown before closing
            current.Shutdown(SocketShutdown.Both);
            current.Close();
            clientSockets.Remove(current);
            Console.WriteLine("Client disconnected");
        }
    }
}

