using System;
using System.Collections.Generic;

namespace Amazoom
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TEST STARTED");
            Computer comp = new Computer();

            Product prod1 = new Product("e", 100, 99);
            //Product prod2 = new Product("c", 100, 99);
            //Product prod3 = new Product("d", 100, 99);

            List<(Product, int)> orderItems = new List<(Product, int)>();
            orderItems.Add((prod1, 4));
            //orderItems.Add((prod2, 2));
            //orderItems.Add((prod3, 3));
            Order testOrder = new Order(1, orderItems, "PLACED");
            comp.fulfillOrder(testOrder);
            //comp.ReadAndReplaceCatalogStock();
            Console.WriteLine("test complete");

        }
    }
}
