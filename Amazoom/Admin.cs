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
            warehouse = new Computer();
            startAdmin();
        }

        public void startAdmin()
        {
            while (true)
            {
                displayAdmin();
                break;
            }

            Console.WriteLine("Goodbye!");
        }
        public void displayAdmin()
        {
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
            if(option == 1)
            {
                viewOrders();
            }
            else if (option == 2)
            {
                viewStock();
            }
            else
            {
                return;
            }
        }

        public void viewOrders()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|              ORDERS              |");
            Console.WriteLine("------------------------------------");

            foreach(Order order in warehouse.orderLog)
            {
                Console.WriteLine("OrderID: {0}, status: {1}", order.id, order.status);
            }
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Press Enter to return to the console");
            Console.ReadLine();
            displayAdmin();
        }

        public void viewStock()
        {
            Product[] products = Computer.ReadCatalog();
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|             Products             |");
            Console.WriteLine("------------------------------------");
            foreach (var item in products)
            {
                Console.WriteLine("Product: {0}, Price: {1}, ID: {2}, Stock: {3}", item.name, item.price, item.id, item.stock);
            }
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Press Enter to return to the console");
            Console.ReadLine();
            displayAdmin();
        }

        public void sendOrder(Order order)
        {
            warehouse.fulfillOrder(order);
        }

    }
}