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

            Product prod1 = new Product("d", 100, 99);
            //Product prod2 = new Product("c", 100, 99);
            //Product prod3 = new Product("d", 100, 99);

            List<(Product, int)> orderItems1 = new List<(Product, int)>();
            orderItems1.Add((prod1, 2));
            List<(Product, int)> orderItems2 = new List<(Product, int)>();
            orderItems2.Add((prod1, 1));
            List<(Product, int)> orderItems3 = new List<(Product, int)>();
            orderItems3.Add((prod1, 3));

            //orderItems.Add((prod2, 2));
            //orderItems.Add((prod3, 3));
            Order testOrder = new Order(1, orderItems1, "");
            Computer.processedOrders.Enqueue(testOrder);
            Order testOrder2 = new Order(2, orderItems2, "");
            Computer.processedOrders.Enqueue(testOrder2);
            Order testOrder3 = new Order(3, orderItems3, "");
            comp.fulfillOrder(testOrder3);
            //comp.ReadAndReplaceCatalogStock();
            Console.WriteLine("test complete");

        }
    }
}
