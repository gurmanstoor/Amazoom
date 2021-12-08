using System;
using System.Threading;
using System.Windows;
using System.Collections.Generic;

namespace Amazoom
{
    public class Admin
    {

        private static Computer warehouse;

        private static Queue<Order> receivedOrders = new Queue<Order>();
        //private static bool deliveryFlag = false;
        private static int deliveryInterval = 10;
        private Thread timeThread = new Thread(() => deliveryTimer(deliveryInterval));

        private List<Thread> orderThreads = new List<Thread>();


        public Admin()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Do you wish to create a small, medium or large warehouse");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("1: Small");
            Console.WriteLine("2: Medium");
            Console.WriteLine("3: Large");
            string line = Console.ReadLine();
            int option;
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
            warehouse = new Computer(option);
            timeThread.Start();
        } 

        /*
         * @return: void
         * Continually run the admin console
         */
        public void startAdmin()
        {
            while (true)
            {
                // Show admin console options
                displayAdmin();
                break;
            }

            // Exit message
            Console.WriteLine("Goodbye!");
        }

        /*
         * @return: void
         * Displays console options and allows the admin to select from a menu
         */
        public void displayAdmin()
        {
            // Console outputs menu
            Console.Clear();
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|           ADMIN CONSOLE          |");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("1: View All Orders ");
            Console.WriteLine("2: View Inventory ");
            Console.WriteLine("3: View Alerts ");
            Console.WriteLine("4: Exit Console ");
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
                else if (option > 4 || option < 1)
                {
                    // Re-prompt user for new int in menu
                    Console.WriteLine("Enter a valid integer (1 to 4): ");
                    line = Console.ReadLine();
                }
                // Break out of the loop once a correct option is selected
                else
                {
                    break;
                }
            }
            // View all Orders 
            if (option == 1)
            {
                viewOrders();
            }
            // View all inventory
            else if (option == 2)
            {
                viewStock();
            }
            // view alerts
            else if (option == 3)
            {
                notifyAdmin();
            }
            // Exit the console
            else
            {
                return;
            }
        }

        /*
         * @return: void
         * Display all past and present orders along with their status
         */
        public void viewOrders()
        {
            // Console output
            Console.Clear();
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|              ORDERS              |");
            Console.WriteLine("------------------------------------");

            // Loop through order log

            foreach ((Order, int) order in warehouse.orderLog)
            {
                Console.WriteLine("OrderID: {0}, status: {1}", order.Item1.id, order.Item1.status);

            }
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Press Enter to return to the console");
            Console.ReadLine();

            // Return to admin console
            displayAdmin();
        }

        /*
         * @return: void
         * view all stock of products in JSON database
         */
        public void viewStock()
        {
            // Console output
            Console.Clear();
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|             PRODUCTS             |");
            Console.WriteLine("------------------------------------");

            // Read catalogue JSON file
            Product[] products = Computer.ReadCatalog();

            // Loop through all products
            foreach (var item in products)
            {
                // Display products and current stock
                Console.WriteLine("Product: {0}, Price: {1}, ID: {2}, Stock: {3}", item.name, item.price, item.id, item.stock);
            }
            
            Console.WriteLine("1: Discontinue item");
            Console.WriteLine("2: Add items to Catalogue");
            Console.WriteLine("3: Return to the console");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Enter Option: ");

            string line = Console.ReadLine();
            int option;
            while (true)
            {
                if (!int.TryParse(line, out option))
                {
                    // Re-prompt user if not an int
                    Console.WriteLine("Enter an integer value: ");
                    line = Console.ReadLine();
                }
                // Confirm option selected is in menu
                else if (option == 1)
                {
                    // Re-prompt user for new int in menu
                    Console.WriteLine("Enter the Product id you wish to discontiue:");
                    line = Console.ReadLine();
                    while (true)
                    {
                        if (!int.TryParse(line, out option))
                        {
                            // Re-prompt user if not an int
                            Console.WriteLine("Enter an integer value: ");
                            line = Console.ReadLine();
                        }
                        else if(option>=0 && option<products.Length)
                        {
                            /*List<(Product item, int quantity)> discontinueOrder = new List<(Product item, int quantity)>();
                            discontinueOrder.Add((products[option], products[option].stock));
                            sendOrder(new Order(-1, discontinueOrder, ""));
                            */
                            warehouse.discontiueProduct(products[option]);
                            products[option].name = products[option].name + " is now discontinued";
                            products[option].stock = -1;
                            Computer.UpdateCatalog(products);
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Enter a valid ID within the range: ");
                            line = Console.ReadLine();
                        }

                    }
                    break;
                }
                else if(option == 2)
                {
                    Console.WriteLine("Enter Product name:");
                    string name = Console.ReadLine();
                    double weight;
                    while (true)
                    {
                        Console.WriteLine("Enter Product weight:");
                        line = Console.ReadLine();
                        if (!double.TryParse(line, out weight))
                        {
                            // Re-prompt user if not an int
                            Console.WriteLine("Weight entry invalid, please enter again");
                            line = Console.ReadLine();
                        }
                        break;
                    }
                    double price;
                    while (true)
                    {
                        Console.WriteLine("Enter Product price:");
                        line = Console.ReadLine();

                        if (!double.TryParse(line, out price))
                        {
                            // Re-prompt user if not an int
                            Console.WriteLine("Price entry invalid, please enter again");
                            line = Console.ReadLine();
                            
                        }
                        break;
                    }
                    Computer.AddNewCatalogItem(new Product(name, weight, price));
                    Console.WriteLine("Product added to catalogue");
                    displayAdmin();
                }
                else if (option == 3) 
                {
                    break;
                }
            }

            // Return to console
            displayAdmin();
        }

        public void sendOrder(Order newOrder)
        {
            warehouse.fulfillOrder(newOrder);
            double orderWeight = 0;
            foreach (Order order in Computer.processedOrders)
            {
                foreach ((Product, int) item in order.products)
                {
                    orderWeight += item.Item1.weight * item.Item2;  // order has multiple quantities
                    if (orderWeight > warehouse.maxTruckCapacity)
                    {
                        bool needNewTruck = true;
                        foreach (Truck truck in warehouse.dockingQueue)
                        {
                            if (truck.GetType() == typeof(DeliveryTruck))
                            {
                                needNewTruck = false;
                                break;
                            }
                        }
                        DeliveryTruck currTruck = warehouse.serviceNextTruck(needNewTruck);
                        warehouse.loadProcessedOrders(currTruck);
                    }
                }
            }
        }


        public static void deliveryTimer(int interval)
        {
            while (true)
            {
                if (warehouse.orderLog.Count > 0)
                {
                    int timeDifference =
                        (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds - warehouse.orderLog[warehouse.orderLog.Count - 1].Item2;

                    if (timeDifference > interval && Computer.processedOrders.Count > 0)
                    {
                        bool needNewTruck = true;
                        foreach (Truck truck in warehouse.dockingQueue)
                        {
                            if (truck.GetType() == typeof(DeliveryTruck))
                            {
                                needNewTruck = false;
                                break;
                            }
                        }
                        DeliveryTruck currTruck = warehouse.serviceNextTruck(needNewTruck);
                        warehouse.loadProcessedOrders(currTruck);
                    }

                }
            }
        }

        /*
         * @return: void
         * Ouptuts an alert and then calls on the warehouse to replace all items that are below max capacity
         */
        public void notifyAdmin()
        {
            // Console output
            Console.Clear();
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|              *ALERTS*            |");
            Console.WriteLine("------------------------------------");

            Product[] products = Computer.ReadCatalog();
            foreach (var item in products)
            {
                if (item.stock == 0)
                {
                    Console.WriteLine("Product: {0} is out of stock", item.name);
                }
            }

            Console.WriteLine("------------------------------------");
            Console.WriteLine("1: Replenish stock ");
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
                else if (option > 2 || option < 1)
                {
                    // Re-prompt user for new int in menu
                    Console.WriteLine("Enter a valid integer (1 to 2): ");
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
                // Replace all items that are below max capacity stock
                string truckId = warehouse.ReadAndReplaceCatalogStock();
                
                
                Console.WriteLine("------------------------------------");
                Console.WriteLine("Restock truck(s) " + truckId + "has arrived to the warehouse and will be serviced soon");
                

                Console.WriteLine("Press Enter to return to Admin menu");
                Console.ReadLine();

                // Return to admin console
                displayAdmin();
            }

            // Checkout the cart
            else 
            {
                displayAdmin();
            }
        }
    }
}