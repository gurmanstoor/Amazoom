using System;
using System.Collections.Generic;
<<<<<<< HEAD
=======
using System.Text.Json;
using System.IO;
>>>>>>> Added client and server files

namespace Amazoom
{
    class Program
    {
        static void Main(string[] args)
        {
<<<<<<< HEAD
            Console.WriteLine("TEST STARTED");
            Computer comp = new Computer();
            //RestockTruck truck = new RestockTruck(1000.0, 0);
            //string[] names = new string[5] { "f", "g", "h", "i", "e" };


            //Product[] catalog = Computer.ReadCatalog();
            //for (int i = 0; i < catalog.Length; i++)
            //{
            //    //Item item = new Item(names[i], 100, 99, i, -1, 0); //when restocking, would first read our current inventory to determine which items we need to restock
            //    truck.items.Add((catalog[i], i));
            //}
            //comp.addRestockTruckToQueue(truck);
            //comp.ReadAndReplaceInventoryStock();

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
           
=======
            //Console.WriteLine("Hello World!");
            //Item[] items = new Item[2] { new Item("test", 100, 1, 79, -1, 1), new Item("test2", 99, 2, 70, 1,0) };
            ///*Item newItem = new Item("test",100,1,69,-1);
            //Item new2 = new Item("test2", 99, 2, 70, 1);*/

            Computer comp = new Computer();
>>>>>>> Added client and server files

            //int[] itemLocation = new int[] { 1, 2 }; //randomly generated
            //Robot newRobot = new Robot(1, itemLocation);

            //Server.startServer();
            Product[] items = Computer.ReadInventory();

            //Client.clientSocket();

            Console.WriteLine(" ");

        }
    }
}
