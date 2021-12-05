using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Amazoom
{

    public class Client
    {
        // Item class for different things in the warehouses
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

        // Inherited product class for catalogue items
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

        /*
         * @return: void
         * @Param: takes a clients active cart in the form of a list
         * Displays store menu options
         */
        public static void displayStore(List<int> cart)
        {
            // Console output
            Console.Clear();
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


            // Loop until a correct option has been selected
            while (true)
            {
                // Confirm the option is an int
                if (!int.TryParse(line, out option))
                {
                    // Re-prompt user if not an int
                    Console.WriteLine("Enter an integer value: ");
                    line = Console.ReadLine();
                }
                // Confirm option selected is in menu
                else if (option > 3 || option < 1)
                {
                    // Re-prompt user for new int in menu
                    Console.WriteLine("Enter a valid integer (1 to 3): ");
                    line = Console.ReadLine();
                }
                // Break out of the loop once a correct option is selected
                else
                {
                    break;
                }
            }

            // Show store product list
            if (option == 1)
            {
                viewProducts(cart);
            }

            // Show client's cart items
            else if (option == 2)
            {
                viewCart(cart);
            }

            // Exit store
            else
            {
                return;
            }
        }

        /*
         * @return: Product Object Array
         * Requests JSON data from the server to serialize for the product list
         */
        public static Product[] ReadInventory()
        {
            // Intialize variables
            byte[] bytes = new byte[1024];
            string jsonString = "";

            // Try Catch Block: connect to the server
            try
            {
                // Connect to a Remote server
                // Get Host IP Address that is used to establish a connection
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Try Catch Block: Connect the socket to the remote endpoint
                try
                {
                    // Connect to Remote EndPoint
                    sender.Connect(remoteEP);

                    //Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    //string result = string.Join(";", cart);

                    // Encode the data string into a byte array.
                    byte[] msg = Encoding.ASCII.GetBytes("get json");

                    // Send the data through the socket.
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(bytes);

                    // Decode the data
                    jsonString = Encoding.ASCII.GetString(bytes, 0, bytesRec);

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

            // Serialize the final json string
            Product[] items = JsonSerializer.Deserialize<Product[]>(jsonString);

            // return the JSON product list
            return items;
        }

        /*
         * @return: void
         * @Param: takes a clients active cart in the form of a list
         * Displays store inventory and menu options
         */
        public static void viewProducts(List<int> cart)
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|          AMAZOOM Products        |");
            Console.WriteLine("------------------------------------");

            // Read JSON product database
            Product[] products = ReadInventory();

            // Loop through all products
            foreach (var item in products)
            {
                // Display Product information
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

            // Loop until a correct option has been selected
            while (true)
            {
                // Confirm the option is an int
                if (!int.TryParse(line, out option))
                {
                    // Re-prompt user if not an int
                    Console.WriteLine("Enter an integer value: ");
                    line = Console.ReadLine();
                }
                // Confirm option selected is in menu
                else if (option > 3 || option < 1)
                {
                    // Re-prompt user for new int in menu
                    Console.WriteLine("Enter a valid integer (1 to 3): ");
                    line = Console.ReadLine();
                }
                // Break out of the loop once a correct option is selected
                else
                {
                    break;
                }
            }

            // Add item to client's cart
            if (option == 1)
            {
                addCart(cart);
            }

            // View client's cart
            else if (option == 2)
            {
                viewCart(cart);
            }

            // Return to home screen
            else
            {
                displayStore(cart);
            }
        }

        /*
         * @return: void
         * @Param: takes a clients active cart in the form of a list
         * Displays client's active cart and menu options
         */
        public static void viewCart(List<int> cart)
        {
            // Read Product inventory
            Product[] products = ReadInventory();

            Console.Clear();
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|         Your AMAZOOM Cart        |");
            Console.WriteLine("------------------------------------");

            // Loop through the client's cart
            foreach (var num in cart)
            {
                // Display each product in the cart
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

            // Loop until a correct option has been selected
            while (true)
            {
                // Confirm the option is an int
                if (!int.TryParse(line, out option))
                {
                    // Re-prompt user if not an int
                    Console.WriteLine("Enter an integer value: ");
                    line = Console.ReadLine();
                }
                // Confirm option selected is in menu
                else if (option > 3 || option < 1)
                {
                    // Re-prompt user for new int in menu
                    Console.WriteLine("Enter a valid integer (1 to 3): ");
                    line = Console.ReadLine();
                }
                // Break out of the loop once a correct option is selected
                else
                {
                    break;
                }
            }

            // Remove an item from the cart
            if (option == 1)
            {
                removeCart(cart);
            }

            // Checkout the cart
            else if (option == 2)
            {
                // make sure the cart is not empty on checkout
                if (cart.Count == 0)
                {
                    // Display error message and return to the home screen
                    Console.WriteLine("Cart is empty, cannot checkout. ");
                    displayStore(cart);
                }

                // If no errors, checkout the cart
                else
                {
                    checkout(cart);
                }           
            }

            // Return to home screen
            else
            {
                displayStore(cart);
            }
        }

        /*
         * @return: void
         * @Param: takes a clients active cart in the form of a list
         * Adds an item the client chooses to their cart if it is not full
         */
        public static void addCart(List<int> cart)
        {
            // Check if cart is full (5 items)
            if (cart.Count == 5)
            {
                // Display error message and return to cart
                Console.WriteLine("Cart is full please checkout or remove an item before adding more ");
                viewCart(cart);
            }
            // If cart is not full, add the item to the cart if it is in stock
            else
            {
                // Prompt client to provide product id
                Console.WriteLine("Enter ID of product you would like to add: ");
                int id = Convert.ToInt32(Console.ReadLine());

                // Read Product inventory
                Product[] products = ReadInventory();

                // Check if product is in stock
                if (products[id].stock > 0)
                {
                    // Add item to the cart if in stock
                    cart.Add(id);
                    Console.WriteLine("Product added to cart!");
                }

                // Not in stock
                else
                {
                    // display error message
                    Console.WriteLine("Sorry this item is out of stock. Please check again later. ");
                }
            }

            // Return to the poroducts screen
            viewProducts(cart);
        }

        /*
         * @return: void
         * @Param: takes a clients active cart in the form of a list
         * Removes an item from the clients cart if cart has item to remove
         */
        public static void removeCart(List<int> cart)
        {
            // Check cart is not empty
            if (cart.Count == 0)
            {
                // display error, return to home screen
                Console.WriteLine("Cart is empty, there is nothing to remove. ");
                displayStore(cart);
            }

            // If not empty, remove item from cart
            else
            {
                // Prompt the client to enter product id
                Console.WriteLine("Enter ID of product you would like to remove: ");
                int id = Convert.ToInt32(Console.ReadLine());

                // Remove product from client cart
                cart.Remove(id);
                Console.WriteLine("Item removed from cart.");
            }
            // Return to cart screen
            viewCart(cart);
        }

        /*
         * @return: void
         * @Param: takes a clients active cart in the form of a list
         * Displays Checkout options
         */
        public static void checkout(List<int> cart)
        {
            // Read product inventory
            Product[] products = ReadInventory();
            double total = 0;

            // loop through client cart
            foreach (var num in cart)
            {
                // Add up total "cost"
                total += products[num].price;
            }

            // Display total price
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Total Price: {0}", total);
            Console.WriteLine("------------------------------------");
            Console.WriteLine("1: Place Order ");
            Console.WriteLine("2: Go Back Home ");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Enter Option: ");
            string line = Console.ReadLine();
            int option;

            // Loop until a correct option has been selected
            while (true)
            {
                // Confirm the option is an int
                if (!int.TryParse(line, out option))
                {
                    // Re-prompt user if not an int
                    Console.WriteLine("Enter an integer value: ");
                    line = Console.ReadLine();
                }
                // Confirm option selected is in menu
                else if (option > 3 || option < 1)
                {
                    // Re-prompt user for new int in menu
                    Console.WriteLine("Enter a valid integer (1 to 3): ");
                    line = Console.ReadLine();
                }
                // Break out of the loop once a correct option is selected
                else
                {
                    break;
                }
            }

            // Proceed to place order
            if (option == 1)
            {
                // Read product inventory
                products = ReadInventory();

                // Loop through client cart
                foreach (var num in cart)
                {
                    // Double check there is stock of that item
                    if (products[num].stock == 0)
                    {
                        // Print error and remove item from cartif out of stock
                        Console.WriteLine("Sorry item: {0} is out of stock and has been removed from your order.", products[num].name);
                        cart.Remove(num);
                    }
                }

                // Place order and send it to the server
                sendCart(cart);
            }

            // Return to the home screen
            displayStore(cart);
            
        }

        /*
         * @return: int
         * @Param: String[] args
         * Main entry point of program
         */
        public static int Main(String[] args)
        {
            // Run the client
            startClient();
            return 0;
        }

        /*
         * @return: void
         * @Param: takes a clients active cart in the form of a list
         * Sends the cart to the server through sockets
         */
        public static void sendCart(List<int> cart)
        {
            byte[] bytes = new byte[1024];

            // Try Catch Block: connect to the server
            try
            {
                // Connect to a Remote server
                // Get Host IP Address that is used to establish a connection
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Try Catch Block: Connect the socket to the remote endpoint
                try
                {
                    // Connect to Remote EndPoint
                    sender.Connect(remoteEP);

                    //Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    string result = string.Join(";", cart);

                    // Encode the data string into a byte array.
                    byte[] msg = Encoding.ASCII.GetBytes(result);

                    // Send the data through the socket.
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(bytes);
                    //Console.WriteLine("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));

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

        /*
         * @return: void
         * Starts up the client interface
         */
        public static void startClient()
        {
            // Initialize client cart
            List<int> cart = new List<int>(5);

            // Display home screen until exit 
            while (true)
            {
                displayStore(cart);
                break;
            }

            // Display exit message
            Console.WriteLine("Goodbye! Please shop at AMAZOOM again!");
        }
    }
}
