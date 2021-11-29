using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Threading;
namespace Amazoom
{
    /* public struct Item
     {
         public string name;
         public double weight;
         public double price;
         public int id;
         public int shelfId;
     }*/
    public class Item
    {
        public Item(string name, double weight, double price, int id, int shelfId, int stock)
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

    /*public struct Shelf
    {
        public List<Item> items;
        public ShelfLocation shelfLocation;
        public int id;
        public double currWeight;
        public double maxWeight;

    }*/
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

    /* public struct Order
     {
         public int id;
         public List<Item> items;
         public string status;
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
        private List<Order> orders { get; set; }
        private int id { set; get; }
        private bool isDeliveryTruck { set; get; }
        private double weightCapacity { set; get; }

        public Truck(double weightCapacity, int id, bool isDeliveryTruck = true)
        {
            this.isDeliveryTruck = isDeliveryTruck;
            this.id = id;
            this.weightCapacity = weightCapacity;

        }
    }


    // ****    REMEMBER TO CHANGE PUBLIC CLASS MEMBER VARIABLES TO PRIVATE **** //
    public class Computer
    {
        public Shelf[] shelves;
        public Robot[] robots;
        private readonly int numRobots = 5;
        private bool dockInUse { set; get; } //use sempahores when implementing multi-threaded to confirm whether dock is in use
        private Queue<Order> processedOrders = new Queue<Order>(); //queue to identify which orders are ready for delivery
        private List<Order> orderBin { get; set; } //bin to hold orders that are being completed, will be pushed into queue when status indicates FINISHED


        public Computer()
        {
            //initialize warehouse shelves and robots
            initializeShelves();
            initializeRobots();

            //placeholder item
            Item newItem = new Item("test", 99,99,1,-1, 5);
            /*newItem.name = "test";
            newItem.weight = 99;
            newItem.price = 99;
            newItem.id = 1;
            newItem.shelfId = -1;*/ //shelfId not initially available, set to -1

            restockItem(newItem);
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
                foreach ((Item item, int quantity) orderItem in order.items) //figure out whether 'orders' is an array or struct or class
                {
                    Item item = orderItem.item;
                    (Item, Shelf) currItem = (item, shelves[item.shelfId]);
                    //check if any other robot currently is at current item's cell in warehouse grid. If there is, wait till it empties (threading implementation using Mutex??)
                    tempRobot.QueueItem(currItem, orderItem.quantity);

                }
                tempRobot.getOrder();
                tempRobot.setActiveStatus(false);


            }
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

                    newItem.shelfId = shelves[shelfNumber].id;
                    shelves[shelfNumber].currWeight += newItem.weight;
                    shelves[shelfNumber].items.Add(newItem);
                    inventory[newItem.id].stock += 1;
                    break;
                }

            }
            UpdateInventory(inventory);

        }

        public static void UpdateInventory(Item[] newItems)
        {
            string fileName = "../../../testing.json";
            string jsonString = JsonSerializer.Serialize(newItems);
            File.WriteAllText(fileName, jsonString);
            
        }
        public static Item[] ReadInventory()
        {
            string fileName = "../../../testing.json";
            string jsonString = File.ReadAllText(fileName);
            //Item[] items = new Item[2];

            Item[] items = JsonSerializer.Deserialize<Item[]>(jsonString);
            return items;
        }
    }
}
