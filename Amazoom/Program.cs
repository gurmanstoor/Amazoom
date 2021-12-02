using System;

namespace Amazoom
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TEST STARTED");
            Computer comp = new Computer();
            RestockTruck truck = new RestockTruck(1000.0, 0);
            string[] names = new string[5] { "f", "g", "h", "i", "e" };
            /*for(int i = 0; i < 5; i++)
            {
                comp.AddNewCatalogueItem(new Item(names[i], 100, 99)); //adding new items to our inventory, not specifying item id, stock, or shelfId
                //items[i]= new Item(names[i], 100, 99, i, -1, 0);
                //truck.items.Add((item, 5));
            }*/
            //Computer.UpdateInventory(items); //initialize our inventory JSON file with the 5 items above with stock = 0 for each item


            Product[] catalog = Computer.ReadCatalog();
            for (int i = 0; i < catalog.Length; i++)
            {
                //Item item = new Item(names[i], 100, 99, i, -1, 0); //when restocking, would first read our current inventory to determine which items we need to restock
                truck.items.Add((catalog[i], i));
            }
            comp.addRestockTruckToQueue(truck);
            //comp.ReadAndReplaceInventoryStock();
            


            Console.WriteLine("test complete");
            
            /*int[] itemLocation = new int[] { 1, 2 }; //randomly generated
            Robot newRobot = new Robot(1, itemLocation);*/


        }
    }
}
