using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Threading;

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

        private static Admin admin = new Admin();
        private static Thread thread;

        static void Main()
        {
            // Start up admin concurrently 
            thread = new Thread(() => admin.startAdmin());
            thread.Start();

            // Setup server
            SetupServer();

            //Join threads before closing the program
            thread.Join();
            //Close all sockets on program closure
            CloseAllSockets();
        }

        /*
         * @return: void
         * Creates a Server socket and begins listening for client connections
         * */
        private static void SetupServer()
        {
            //Setting up server
            serverSocket.Bind(new IPEndPoint(ipAddress, PORT));
            serverSocket.Listen(1);
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        /*
         * @return: void
         * Close all connected client (we do not need to shutdown the server socket as its connections
         * are already closed with the clients).
         */
        private static void CloseAllSockets()
        {
            // loop through each client and close them
            foreach (Socket socket in clientSockets)
            {
                // Shutdown before closing the socket
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            // Finally close the server socket
            serverSocket.Close();
        }

        /*
         * @return: void
         * Accepts a client and if no errors, will begin to get data
         */
        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            // Try Catch block: stop accepting server socket clients
            try
            {
                // End socket acception
                socket = serverSocket.EndAccept(AR);
            }
            // Catch error if socket is disposed
            catch (ObjectDisposedException)
            {
                return;
            }

            // Add client socket to client list
            clientSockets.Add(socket);

            // Begins to async accept data
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            //Console.WriteLine("Client connected, waiting for request...");
            // Accept a new connection
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        /*
         * @return: void
         * Recieves and processes client data. Can either send back JSON data or forward an order to a warehouse
         */
        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;
            int result;
            List<(Product, int)> orderItems = new List<(Product, int)>();
            bool restock = false;
            List<Product> restockProducts = new List<Product>();

            // Try Catch Block: Get number of bytes recieved
            try
            {
                received = current.EndReceive(AR);
            }
            // Catch socket exceptions
            catch (SocketException)
            {
                //Console.WriteLine("Client forcefully disconnected");

                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            // Create recieveing byte buffer
            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);

            // Get string data from client
            string text = Encoding.ASCII.GetString(recBuf);
            //Console.WriteLine("Received Text: " + text);

            // Check if client requested JSON data
            if (text.ToLower() == "get json")
            {
                // Read the JSON file
                string fileName = "../../../catalogue.json";
                string jsonString = File.ReadAllText(fileName);

                //Console.WriteLine("Text is a get json request");

                // Encode the JSON data and send it to the client
                byte[] data = Encoding.ASCII.GetBytes(jsonString);
                current.Send(data);

                //Console.WriteLine("json sent to client");

            }
            // Order from client
            else
            {
                // Increase number of total orders processed
                orderID++;

                // Get the catalogue ID's for the order
                string[] orders = text.Split(";").ToArray();

                // Read the Catalogue JSON file
                Product[] products = Computer.ReadCatalog();

                // Loop through the order and decrement stock
                for (int i = 0; i < orders.Length; i++)
                {
                    // Confirm the order number is an int
                    if (int.TryParse(orders[i], out result))
                    {
                        // Decrement stock of the order product
                        products[result].stock = products[result].stock - 1;

                        // CHeck if the product needs to be restocked
                        if (products[result].stock == 0)
                        {
                            // Initialize restock items
                            restock = true;
                            restockProducts.Add(products[result]);
                        }

                        // Check the first product in the cart
                        if (i == 0)
                        {
                            // Add the product directly to the order Lsit
                            orderItems.Add((products[i], 1));
                        }
                        else
                        {
                            // Loop through the processed order list
                            for (int j = 0; j < orderItems.Count(); j++)
                            {
                                // If the product is already in the list, increment quantity needed
                                if (orderItems[j].Item1.name == products[result].name)
                                {
                                    orderItems[j] = (orderItems[j].Item1, orderItems[j].Item2 + 1);
                                }
                                // If a new product, add it to the list
                                else
                                {
                                    orderItems.Add((products[result], 1));
                                }
                            }
                        }
                    }
                }

                // Update the JSON file with the new stock after the order is fulfilled
                Computer.UpdateCatalog(products);

                // Always Shutdown before closing
                current.Shutdown(SocketShutdown.Both);
                current.Close();
                clientSockets.Remove(current);
                //Console.WriteLine("Client disconnected");

                // Send order to warehouse to be completed
                admin.sendOrder(new Order(orderID, orderItems, ""));
            }
        }
    }
}
