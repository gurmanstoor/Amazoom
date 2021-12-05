using System;
using System.Windows;
using System.Collections.Generic;

namespace Amazoom
{
    public class Admin
    {
        private static Computer warehouse;

        public Admin()
        {
            // Create a new warehouse
            warehouse = new Computer();
            // Start Admin console
            startAdmin();
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
            Console.WriteLine("3: Exit Console ");
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
                // Break out of the looponce a correct option is selected
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
            foreach (Order order in warehouse.orderLog)
            {
                // display order log and status
                Console.WriteLine("OrderID: {0}, status: {1}", order.id, order.status);
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
            Console.WriteLine("|             Products             |");
            Console.WriteLine("------------------------------------");

            // Read catalogue JSON file
            Product[] products = Computer.ReadCatalog();

            // Loop through all products
            foreach (var item in products)
            {
                // Display products and current stock
                Console.WriteLine("Product: {0}, Price: {1}, ID: {2}, Stock: {3}", item.name, item.price, item.id, item.stock);
            }
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Press Enter to return to the console");
            Console.ReadLine();

            // Return to console
            displayAdmin();
        }

        /*
         * @return: void
         * @param: Order item from server that contains all the products that a client wishes to order
         * Sends client order to warehouse to be fulfilled and sent out for delivery
         */
        public void sendOrder(Order order)
        {
            warehouse.fulfillOrder(order);
        }

        /*
         * @return: void
         * @Param: takes a product object that is out of stock
         * Ouptuts an alert and then calls on the warehouse to replace all items that are below max capacity
         */
        public void notifyAdmin(Product item)
        {
            // Console output
            Console.Clear();
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|              *ALERT*             |");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Product: {0} is out of stock, replenishing stock...", item.name);

            // Replace all items that are below ax capacity stock
            warehouse.ReadAndReplaceCatalogStock();

            Console.WriteLine("------------------------------------");
            Console.WriteLine("Press Enter to return to the console");
            Console.ReadLine();

            // Return to admin console
            displayAdmin();

        }
    }
}