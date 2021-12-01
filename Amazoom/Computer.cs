using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Threading;
namespace Amazoom
{
    public class Item
    {
        public Item(string name, double weight, double price, int shelfId = -1, int stock = 0, int id = -1)
        {
            this.name = name;
            this.weight = weight;
            this.price = price;
            this.id = id;
            this.shelfId = shelfId;
            this.stock = stock;
        }
        public Item() { }
        public string name { get; set; }
        public double weight { get; set; }
        public double price { get; set; }
        public int id { get; set; }
        public int shelfId { get; set; }
        public int stock { get; set; }
    }

    ////used for displaying items to client
    //public class CatalogueItem: Item
    //{
    //    public CatalogueItem(string name, double weight, double price, int id = -1) : base(name, weight, price, id)
    //    {

    //    }
    //}

    ////used in our actual inventory, has extra fields like stock and shelfId
    //public class InventoryItem : Item
    //{
    //    public int stock { get; set; }
    //    public int shelfId { get; set; }
    //    public InventoryItem(string name, double weight, double price, int shelfId = -1, int stock = 0, int id = -1) : base(name, weight, price, id)
    //    {
    //        this.stock = stock;
    //        this.shelfId = shelfId;
    //    }
    //}

    public class Shelf
    {
        public Shelf(List<Item> items, ShelfLocation shelfLocation, int id, double currWeight, double maxWeight)
        {
            this.items = items;
            this.shelfLocation = shelfLocation;
            this.id = id;
            this.currWeight = currWeight;
            this.maxWeight = maxWeight;
        }

        public Shelf() { }
        public List<Item> items { get; set; }
        public ShelfLocation shelfLocation;
        public int id { get; set; }
        public double currWeight { get; set; }
        public double maxWeight { get; set; }
    }

    public struct ShelfLocation
    {
        public int[] location;
        public string side;
        public int height;
    }

    /*public class ShelfLocation
    {
        public ShelfLocation(int[] location, string side, int height)
        {
            this.location = location;
            this.side = side;
            this.height = height;
        }

        public int[] location { get; set; }
        public string side { get; set; }
        public int height { get; set; }
    }*/

    public class Order
    {
        public Order(int id, List<(Item item, int quantity)> items, string status)
        {
            this.id = id;
            this.items = items;
            this.status = status;
        }

        public Order() { }
        public int id { get; set; }
        public List<(Item item, int quantity)> items { get; set; }
        public string status { get; set; }
    }

    public class Truck
    {
        //public List<Order> orders { get; set; }
        public int id { get; set; }
        public double maxWeightCapacity { get; set; }
        public double currWeight { get; set; }

        public Truck(double maxWeightCapacity, int id)
        {
            this.id = id;
            this.maxWeightCapacity = maxWeightCapacity;
            this.currWeight = 0.0;

        }
    }

    public class DeliveryTruck: Truck
    {
        public List<Order> orders = new List<Order>();
        public DeliveryTruck(double maxWeightCapacity, int id) : base(maxWeightCapacity,id)
        {

        }
    }

    public class RestockTruck : Truck
    {
        public List<(Item, int)> items = new List<(Item, int)>();
        public RestockTruck(double maxWeightCapacity, int id) : base(maxWeightCapacity, id)
        {

        }
    }

    // ****    REMEMBER TO CHANGE PUBLIC CLASS MEMBER VARIABLES TO PRIVATE **** //
    public class Computer
    {
        public Shelf[] shelves;
        public Robot[] robots;
        public Truck[] trucks;
        private readonly int numRobots = 5;
        private readonly int numTrucks = 5;
        private readonly double maxTruckCapacity = 1000.0;
        private readonly int maxItemStock = 5; //****how do we determine what the max number of stock for each item is? based on shelves and total items??
        private bool dockInUse { set; get; } //use sempahores when implementing multi-threaded to confirm whether dock is in use

        //**implement threadsafe queues to allow robots to queue up their orders once processed
        public static Queue<Order> processedOrders = new Queue<Order>(); //queue to identify which orders are ready for delivery, will be loaded into trucks on a FIFO basis
        public static List<Order> orderBin { get; set; } //bin to hold orders that are being completed, will be pushed into queue when status indicates FINISHED
        private Queue<Truck> dockingQueue { get; set; } //queue to track which trucks are waiting to be serviced, could be a restocking or delivery truck
        private Queue<RestockTruck> restockTruckQueue { get; set; }
        private Queue<DeliveryTruck> deliveryTruckQueue { get; set; }

        public Computer()
        {
            //initialize warehouse shelves, robots, and trucks
            initializeShelves();
            initializeRobots();
            initializeTrucks();

            //placeholder item
            this.dockInUse = false;

        }

        /*
         * @return: void
         * initialize robots for our warehouse. Potentially spin new threads for each robot to handle orders simulatenously
         * */
        private void initializeRobots()
        {
            //currently initializing 5 robots on the same thread. Could spin a new thread for each robot to handle its own orders
            this.robots = new Robot[this.numRobots];
            for(int i = 0; i < this.numRobots; i++)
            {
                this.robots[i] = new Robot(0, new int[] { 0, 0 });
            }

        }

        /*
         * @return: void
         * initialize delivery and restocking trucks for our warehouse
         * */
        private void initializeTrucks()
        {
            //currently initializing 'numTrucks' delivery trucks only. Would also need to later intialize restocking trucks as well
            for(int i = 0; i < this.numTrucks; i++)
            {
                this.deliveryTruckQueue.Enqueue(new DeliveryTruck(this.maxTruckCapacity, i));
                this.restockTruckQueue.Enqueue(new RestockTruck(this.maxTruckCapacity, i));
            }

        }


        /*
         * @return: void
         * initialize shelves for our warehouse based on warehouse layout. Generate unique id's for each shelf
         * */
        private void initializeShelves()
        {
            int numRows = 3;
            int numCols = 4;
            int height = 2;
            int numShelves = (numCols-1) * (numRows - 2) * height*2;

            this.shelves = new Shelf[numShelves];
            int shelfNum = 0;

            //initializing each shelf with properties and assigning an id
            for (int j = 0; j < numCols; j++)
            {
                for (int i = 1; i < numRows - 1; i++)
                {
                    for (int k = 0; k < height; k++)
                    {

                        Shelf currShelf = new Shelf();
                        currShelf.items = new List<Item>();
                        currShelf.id = shelfNum;
                        currShelf.shelfLocation.location = new int[2] { i, j };
                        currShelf.shelfLocation.height = k;
                        currShelf.maxWeight = 500;
                        currShelf.currWeight = 0;


                        if (j == 0)
                        {
                            currShelf.shelfLocation.side = "right";
                            shelves[shelfNum] = currShelf;
                        }
                        else if (j == numCols - 1)
                        {
                            currShelf.shelfLocation.side = "left";
                            shelves[shelfNum] = currShelf;
                        }
                        else
                        {
                            currShelf.shelfLocation.side = "left";
                            shelves[shelfNum] = currShelf;
                            shelfNum++;

                            currShelf.items = new List<Item>();
                            currShelf.id = shelfNum;
                            currShelf.shelfLocation.location = new int[2] { i, j };
                            currShelf.shelfLocation.height = k;
                            currShelf.shelfLocation.side = "right";
                            currShelf.maxWeight = 500;
                            currShelf.currWeight = 0;
                            shelves[shelfNum] = currShelf;

                        }

                        shelfNum++;
                    }


                }
            }
        }

        /*
         * @param: an Order to be fulfilled by a robot
         * @return: void
         * fulfills a client's order by first validating that all items are available in inventory and then assigning a robot to that order
         * */
        public void fulfillOrder(Order order)
        {

            if (orderIsValid(order)) //helper method to validate each order to confirm if all items are available
            {
                //**add logic to figure out which robot is available, assign order to that robot
                Robot tempRobot = null;
                while(tempRobot == null)
                {
                    foreach (Robot robot in this.robots)
                    {
                        if (robot.getActiveStatus() == false)
                        {
                            tempRobot = robot;
                            break;
                        }
                    }

                }

                tempRobot.setActiveStatus(true);
                foreach ((Item item, int quantity) orderItem in order.items)
                {
                    Item item = orderItem.item;
                    (Item, Shelf) currItem = (item, shelves[item.shelfId]);
                    //check if any other robot currently is at current item's cell in warehouse grid. If there is, wait till it empties (threading implementation using Mutex??)
                    tempRobot.QueueItem(currItem, orderItem.quantity);

                }
                tempRobot.getOrder(order); //invoke Robot getOrder() method to retrieve all items from warehouse
                tempRobot.setActiveStatus(false);
                //loadProcessedOrders(); // --> replace with "loadOrder()" method which will move the most recent order to the current delivery truck. If truck gets full, pop off queue and start loading next truck
                serviceNextTruck();
            }
        }

        private void serviceNextTruck()
        {
            if(this.deliveryTruckQueue.Count == this.numTrucks)
            {
                this.dockingQueue.Enqueue(this.deliveryTruckQueue.Dequeue());
            }
            while (this.dockingQueue.Peek().GetType() == typeof(RestockTruck))
            {
                RestockTruckItems((RestockTruck)this.dockingQueue.Dequeue());
            }

            DeliveryTruck currTruck = (DeliveryTruck) this.dockingQueue.Dequeue();
            loadProcessedOrders(currTruck);
        }
        /*
         * @param: an Order to be validated
         * @return: boolean
         * validates an order by ensuring all items are in inventory and stock is available
         * */
        private bool orderIsValid(Order order)
        { 
            
            Item[] inventory = ReadInventory();

            //Console.WriteLine(items);

            foreach ((Item, int) item in order.items)
            {
                int item_id = item.Item1.id;
                int quantity = item.Item2;
                foreach(Item inv_item in inventory)
                {
                    if (item_id == inv_item.id)
                    {
                        if (inv_item.stock < quantity)
                        {
                            return false;
                        }
                        break;
                    }
                    
                }
            }
            return true;

        }
        /*
         * @param: an item to be restocked in inventory
         * @return: void
         * restocks item to a shelf. Shelf id generated using "Random" class. If shelf full, try new shelf. Loop until empty shelf found
         * */

        public void restockItem(Item newItem)
        {
            Item[] inventory = ReadInventory();
            Random rand = new Random();
            while (true)
            {

                int shelfNumber = rand.Next(0, this.shelves.Length-1);
                if (shelves[shelfNumber].currWeight + newItem.weight <= shelves[shelfNumber].maxWeight)
                {

                    inventory[newItem.id].shelfId = shelves[shelfNumber].id;
                    shelves[shelfNumber].currWeight += newItem.weight;
                    shelves[shelfNumber].items.Add(newItem);
                    inventory[newItem.id].stock += 1;
                    break;
                }

            }
            UpdateInventory(inventory);

        }

        public void loadProcessedOrders(DeliveryTruck currTruck)
        {
            //load processed orders into delivery truck as long as maxWeightCap of truck not exceeded 
            while (processedOrders.Count > 0)
            {
                Order currOrder = processedOrders.Peek();
                double currOrderWeight = 0.0;
                foreach ((Item, int) item in currOrder.items)
                {
                    currOrderWeight += item.Item1.weight;
                }

                if (currOrderWeight <= currTruck.maxWeightCapacity - currTruck.currWeight)
                {

                    currTruck.orders.Add(processedOrders.Dequeue());
                    currTruck.currWeight += currOrderWeight;
                }
                else
                {
                    serviceNextTruck();
                    break;         
                }
            }

            deliverOrders(currTruck);

            //SENDING OUT DELIVERY TRUCK AT THIS POINT. PUT THAT DELIVERY TRUCK BACK IN DELIVERYTRUCKQUEUE AND PUT A NEW DELVIERYTRUCK FROM DELIVERYTRUCKQUEUE INTO DOCKINGQUEUE
            //****spin a new thread, send in current delivery truck into the thread along with a method which will put that truck at the back of the deliveryTrucks queue after a timer expires

        }

        public void deliverOrders(DeliveryTruck truck)
        {
            //****mark which trucks are out for delivery
            truck.orders.Clear();
            this.deliveryTruckQueue.Enqueue(truck);
        }

        public void RestockTruckItems(RestockTruck truck)
        {
            //iterate over every item in the restocking truck and use the restockItem() method to update inventory
            foreach ((Item,int) item in truck.items)
            {
                for(int i=0; i < item.Item2; i++) // 'item.Item2' represents the quantity of each item within the truck. Call restockItem() for each individual item
                {
                    restockItem(item.Item1);
                }

            }
            //clear the restock truck items and put back into restockTruckqueue for reuse
            truck.items.Clear();
            this.restockTruckQueue.Enqueue(truck);

        }

        //**need to also implement logic to check restock truck capacity
        public void ReadAndReplaceInventoryStock()
        {
            Item[] currentInventory = ReadInventory();
            List<(Item, int)> itemsToRestock = new List<(Item, int)>();
            foreach(Item item in currentInventory)
            {
                if(item.stock == 0)
                {
                    itemsToRestock.Add((item, this.maxItemStock));
                }
            }

            if(itemsToRestock.Count > 0) //check if there are any items that need to be restocked
            {
                RestockTruck availableRestockTruck = null;
                while (availableRestockTruck == null)
                {
                    if (this.restockTruckQueue.Count > 0) //***need to actually check the restockTruck queue here, which needs to be implemented
                    {
                        availableRestockTruck = this.restockTruckQueue.Dequeue();
                        break;  
                    }
                    else
                    {
                        serviceNextTruck(); //no restock truck available, service the dockingQueue trucks first and wait till restock truck put back into restockTruck queue
                    }
                }

                availableRestockTruck.items = itemsToRestock; //assign a truck to bring in the inventory that needs to be replaced
                this.dockingQueue.Enqueue(availableRestockTruck);
               
            }
        }

        //only adds an item to our catalogue of available items, doesn't actually place anything in the inventory
        public void AddNewCatalogueItem(Item item)
        {
            Item[] currInventory = ReadInventory(); //read in current inventory
            Item[] updatedInventory = new Item[currInventory.Length + 1]; //create new Item[] array with size = previous size + 1 to accomodate new catalogue item
            int newItemId = currInventory.Length;
            item.id = newItemId;
            for(int i=0; i < currInventory.Length; i++) //copy over all existing catalogue items
            {
                updatedInventory[i] = currInventory[i];
            }
            updatedInventory[updatedInventory.Length - 1] = item; //add the newest item
            UpdateInventory(updatedInventory); //update the inventory JSON file

        }

        public static void UpdateInventory(Item[] newItems)
        {
            string fileName = "../../../inventory.json";
            string jsonString = JsonSerializer.Serialize(newItems);
            File.WriteAllText(fileName, jsonString);
            
        }
        public static Item[] ReadInventory()
        {
            string fileName = "../../../inventory.json";
            string jsonString = File.ReadAllText(fileName);
            //Item[] items = new Item[2];

            Item[] items = JsonSerializer.Deserialize<Item[]>(jsonString);
            return items;
        }

        ////methods to update the catalogue items available for user to order

        //public static void UpdateCatalogue(Item[] newItems)
        //{
        //    string fileName = "../../../inventory.json";
        //    string jsonString = JsonSerializer.Serialize(newItems);
        //    File.WriteAllText(fileName, jsonString);

        //}
        //public static Product[] ReadCatalogue()
        //{
        //    string fileName = "../../../inventory.json";
        //    string jsonString = File.ReadAllText(fileName);
        //    //Item[] items = new Item[2];

        //    Item[] items = JsonSerializer.Deserialize<Item[]>(jsonString);
        //    return items;
        //}
    }
}
