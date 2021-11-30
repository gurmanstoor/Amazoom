using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.IO;

namespace Amazoom
{

    public class Client
    {
        public class Item
        {
            public Item(string name, double weight, double price, int id, int shelfId)
            {
                this.name = name;
                this.weight = weight;
                this.price = price;
                this.id = id;
                this.shelfId = shelfId;
            }
            public Item() { }
            public string name { get; set; }
            public double weight { get; set; }
            public double price { get; set; }
            public int id { get; set; }
            public int shelfId { get; set; }
        }

        public class Product : Item
        {
            public Product(string name, double weight, double price, int id, int shelfId, int stock)
            {
                this.name = name;
                this.weight = weight;
                this.price = price;
                this.id = id;
                this.shelfId = shelfId;
                this.stock = stock;
            }
            public Product() { }
            public int stock { get; set; }
        }

        public static void displayStore(List<int> cart)
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|           AMAZOOM STORE          |");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("1: View Products ");
            Console.WriteLine("2: View Cart ");
            Console.WriteLine("3: Exit Store ");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Enter Option: ");
            string line = Console.ReadLine();
            int option;

            while (true)
            {
                if(!int.TryParse(line, out option))
                {
                    Console.WriteLine("Enter an integer value: ");
                    line = Console.ReadLine();
                }
                else if(option > 3 || option < 1)
                {
                    Console.WriteLine("Enter a valid integer (1 to 3): ");
                    line = Console.ReadLine();
                }
                else
                {
                    break;
                }
            }
            if (option == 1)
            {
                viewProducts(cart);
            }
            else if (option == 2)
            {
                viewCart(cart);
            }
            else
            {
                return;
            }
        }
        public static Product[] ReadInventory()
        {
            string fileName = "../../../testing.json";
            string jsonString = File.ReadAllText(fileName);
            //Item[] items = new Item[2];

            Product[] items = JsonSerializer.Deserialize<Product[]>(jsonString);
            return items;
        }

        public static void viewProducts(List<int> cart)
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|          AMAZOOM Products        |");
            Console.WriteLine("------------------------------------");

            Product[] products = ReadInventory();

            foreach (var item in products)
            {
                Console.WriteLine("Product: {0}, Price: {1}, ID: {2}, Stock: {3}", item.name, item.price, item.id, item.stock);
                Console.WriteLine("------------------------------------");
            }
            Console.WriteLine("------------------------------------");
            Console.WriteLine("1: Add item to Cart ");
            Console.WriteLine("2: View Cart ");
            Console.WriteLine("3: Go Back Home ");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Enter Option: ");
            string line = Console.ReadLine();
            int option;

            while (true)
            {
                if (!int.TryParse(line, out option))
                {
                    Console.WriteLine("Enter an integer value: ");
                    line = Console.ReadLine();
                }
                else if (option > 3 || option < 1)
                {
                    Console.WriteLine("Enter a valid integer (1 to 3): ");
                    line = Console.ReadLine();
                }
                else
                {
                    break;
                }
            }
            if (option == 1)
            {
                addCart(cart);
            }
            else if(option == 2){
                viewCart(cart);
            }
            else
            {
                displayStore(cart);
            }     
        }

        public static void viewCart(List<int> cart)
        {
            Product[] products = ReadInventory();
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|         Your AMAZOOM Cart        |");
            Console.WriteLine("------------------------------------");
            foreach (var num in cart)
            {
                Console.WriteLine("Product: {0}, Price: {1}, ID: {2}", products[num].name, products[num].price, products[num].id);
                Console.WriteLine("------------------------------------");
            }
            Console.WriteLine("------------------------------------");
            Console.WriteLine("1: Remove Item ");
            Console.WriteLine("2: Checkout ");
            Console.WriteLine("3: Go Back Home ");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Enter Option: ");
            string line = Console.ReadLine();
            int option;

            while (true)
            {
                if (!int.TryParse(line, out option))
                {
                    Console.WriteLine("Enter an integer value: ");
                    line = Console.ReadLine();
                }
                else if (option > 3 || option < 1)
                {
                    Console.WriteLine("Enter a valid integer (1 to 3): ");
                    line = Console.ReadLine();
                }
                else
                {
                    break;
                }
            }
        }

        public static void addCart(List<int> cart)
        {
            if (cart.Count == 5)
            {
                Console.WriteLine("Cart is full please checkout or remove an item before adding more ");
                viewCart(cart);
            }
            else
            {
                Console.WriteLine("Enter ID of product you would like to add: ");
                int id = Convert.ToInt32(Console.ReadLine());
                Product[] products = ReadInventory();

                if(products[id].stock > 0)
                {
                    cart.Add(id);
                    Console.WriteLine("Item added to cart!");
                }
                else
                {
                    Console.WriteLine("Sorry this item is out of stock. Please check again later. ");
                }
            }
            viewProducts(cart);
        }

        public static void removeCart(List<int> cart)
        {
            if (cart.Count == 0)
            {
                Console.WriteLine("Cart is empty, there is nothing to remove. ");
                displayStore(cart);
            }
            else
            {
                Console.WriteLine("Enter ID of product you would like to remove: ");
                int id = Convert.ToInt32(Console.ReadLine());
                cart.Remove(id);
                Console.WriteLine("Item removed from cart.");
            }
            viewCart(cart);
        }

        public static void checkout(List<int> cart)
        {
            
        }

        public static int Main(String[] args)
        {
            //clientSocket();
            return 0;
        }

        public static void clientSocket(List<int> cart)
        {
            byte[] bytes = new byte[1024];

            try
            {
                // Connect to a Remote server
                // Get Host IP Address that is used to establish a connection
                // In this case, we get one IP address of localhost that is IP : 127.0.0.1
                // If a host has multiple addresses, you will get a list of addresses
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    // Connect to Remote EndPoint
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    string result = string.Join(";", cart);

                    // Encode the data string into a byte array.
                    byte[] msg = Encoding.ASCII.GetBytes(result + ";<EOF>");

                    // Send the data through the socket.
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    // Release the socket.
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void startClient()
        {
            List<int> cart = new List<int>(5);

            while (true)
            {
                displayStore(cart);
                break;
            }

            Console.WriteLine("Goodbye! Please shop at AMAZOOM again!");
        }
    }
}
