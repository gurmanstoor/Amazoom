using System;

namespace Amazoom
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Computer comp = new Computer();
            RestockTruck truck = new RestockTruck(1000.0, 0);
            string[] names = new string[5] { "a", "b", "c", "d", "e" };
            Item[] items = new Item[5];
            for(int i = 0; i < 5; i++)
            {
                items[i]= new Item(names[i], 100, 99, i, -1, 0);
                //truck.items.Add((item, 5));
            }
            Computer.UpdateInventory(items);

            for (int i = 0; i < 5; i++)
            {
                Item item = new Item(names[i], 100, 99, i, -1, 0);
                truck.items.Add((item, 5));
            }

            comp.RestockTruckItems(truck);
            /*Item newItem = new Item("test",100,1,69,-1);
            Item new2 = new Item("test2", 99, 2, 70, 1);*/
            Item[] returned = Computer.ReadInventory();
            Console.WriteLine("test complete");
            
            /*int[] itemLocation = new int[] { 1, 2 }; //randomly generated
            Robot newRobot = new Robot(1, itemLocation);*/


        }
    }
}
