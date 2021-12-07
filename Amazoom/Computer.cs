using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.Json;
using System.IO;
using System.Threading;
namespace Amazoom
{
    public class BasicItem
    {
        public BasicItem(string name, double weight, double price, int id = -1)
        {
            this.name = name;
            this.weight = weight;
            this.price = price;
            this.id = id;
        }
        public BasicItem() { }
        public string name { get; set; }
        public double weight { get; set; }
        public double price { get; set; }
        public int id { get; set; }
    }
    public class Item : BasicItem
    {
        public Item(string name, double weight, double price, int id = -1) : base(name, weight, price, id) { }
        public Item() : base() { }
        public int shelfId { get; set; }
    }
    public class Product : BasicItem
    {
        public Product(string name, double weight, double price, int id = -1) : base(name, weight, price, id) { }
        public Product() : base() { }
        public int stock { get; set; }
    }

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


    public class Order
    {
        public Order(int id, List<(Product item, int quantity)> products, string status)
        {
            this.id = id;
            this.products = products;
            this.status = status;
        }

        public Order() { }
        public int id { get; set; }
        public List<(Product, int)> products { get; set; }
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

    public class DeliveryTruck : Truck
    {
        public List<Order> orders = new List<Order>();
        public DeliveryTruck(double maxWeightCapacity, int id) : base(maxWeightCapacity, id)
        {

        }
    }

    public class RestockTruck : Truck
    {
        public List<(Product, int)> items = new List<(Product, int)>();
        public RestockTruck(double maxWeightCapacity, int id) : base(maxWeightCapacity, id)
        {

        }
    }

    // ****    REMEMBER TO CHANGE PUBLIC CLASS MEMBER VARIABLES TO PRIVATE **** //
    public class Computer
    {
        public Shelf[] shelves;
        public List<Robot> workingRobots = new List<Robot>();
        public Queue<Robot> standbyRobots = new Queue<Robot>();
        public Truck[] trucks;
        private Thread[] threads;
        private static Mutex[,] gridCellMutices;
        private readonly int numRobots = 5;
        private readonly int numTrucks = 5;
        public readonly double maxTruckCapacity = 10000.0;
        public readonly int maxItemStock = 5; //****how do we determine what the max number of stock for each item is? based on shelves and total items??
        public static int numRows;
        public static int numCols;
        public static int height;
        public static int invId=0;

        //order status keywords
        public readonly string ORDER_FAILED = "ORDER FAILED";
        public readonly string ORDER_PROCESSING = "ORDER PROCESSING";
        public readonly string ORDER_FULFILLED = "ORDER FULFILLED AND WAITING FOR DELIVERY";
        public readonly string ORDER_DELIVERY = "ORDER OUT FOR DELIVERY";
        public readonly string ORDER_DELIVERED = "ORDER HAS BEEN DELIVERED";
        public readonly string ORDER_RECEIVED = "ORDER RECEIVED";
        public readonly string DISCONTINUED_ITEM = "DISCONTINUED ITEMS HAVE BEEN PICKED UP";
        public readonly string DISCONTINUED_ORDER = "DISCONTINUED ITEMS HAVE LEFT THE WAREHOUSE";

        public static ConcurrentQueue<Order> processedOrders = new ConcurrentQueue<Order>(); //queue to identify which orders are ready for delivery, will be loaded into trucks on a FIFO basis
        public static List<Order> orderBin { get; set; } //bin to hold orders that are being completed, will be pushed into queue when status indicates FINISHED
        public List<(Order, int)> orderLog = new List<(Order, int)>();
        public Queue<Truck> dockingQueue = new Queue<Truck>(); //queue to track which trucks are waiting to be serviced, could be a restocking or delivery truck
        private Queue<RestockTruck> restockTruckQueue = new Queue<RestockTruck>();
        public ConcurrentQueue<DeliveryTruck> deliveryTruckQueue = new ConcurrentQueue<DeliveryTruck>();


        public Computer(int size)
        {
            //initialize warehouse shelves, robots, and trucks
            if(size == 1)
            {
                numRows = 3;
                numCols = 5;
                height = 2;
                maxItemStock = 5;
            }
            else if (size == 3)
            {
                numRows = 7;
                numCols = 10;
                height = 4;
                maxItemStock = 20;
            }
            else
            {
                numRows = 5;
                numCols = 7;
                height = 3;
                maxItemStock = 10;
            }

            initializeInventory(size);
            initializeShelves();
            initializeRobots();
            initializeTrucks();
        }

        private void initializeInventory(int size)
        {
            string fileName;
            string jsonString;
            string fileName2;
            string jsonString2;


            if (size == 0)
            {
                fileName = "../../../defaultCatalogueS.json";
                jsonString = File.ReadAllText(fileName);
                fileName2 = "../../../defaultInventoryS.json";
                jsonString2 = File.ReadAllText(fileName2);
            }
            else if (size == 2)
            {
                fileName = "../../../defaultCatalogueL.json";
                jsonString = File.ReadAllText(fileName);
                fileName2 = "../../../defaultInventoryL.json";
                jsonString2 = File.ReadAllText(fileName2);
            }
            else
            {
                fileName = "../../../defaultCatalogueM.json";
                jsonString = File.ReadAllText(fileName);
                fileName2 = "../../../defaultInventoryM.json";
                jsonString2 = File.ReadAllText(fileName2);
            }
            
            Product[] products = JsonSerializer.Deserialize<Product[]>(jsonString);
            UpdateCatalog(products);

            List<Item> items = JsonSerializer.Deserialize<List<Item>>(jsonString2);
            UpdateInventory(items);
        }

        /*
         * @return: void
         * initialize robots for our warehouse. Potentially spin new threads for each robot to handle orders simulatenously
         * */
        private void initializeRobots()
        {
            this.threads = new Thread[this.numRobots];
            //currently initializing 5 robots on the same thread. Could spin a new thread for each robot to handle its own orders
            for (int i = 0; i < this.numRobots; i++)
            {
                this.workingRobots.Add(new Robot(i, new int[] { -1, -1 }));
            }

        }

        /*
         * @return: void
         * initialize delivery and restocking trucks for our warehouse
         * */
        private void initializeTrucks()
        {
            //currently initializing 'numTrucks' delivery trucks only. Would also need to later intialize restocking trucks as well
            for (int i = 0; i < this.numTrucks; i++)
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
           // numRows = 3;
           // numCols = 4;
           // int height = 2;
            int numShelves = (numCols - 1) * (numRows - 2) * height * 2;

            gridCellMutices = new Mutex[numRows, numCols];
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
                        currShelf.maxWeight = 5000;
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
                            currShelf.maxWeight = 5000;
                            currShelf.currWeight = 0;
                            shelves[shelfNum] = currShelf;

                        }

                        shelfNum++;
                    }

                    for(int row=0; row < numRows; row++)
                    {
                        gridCellMutices[row,j] = new Mutex();
                    }
                }

            }
            loadShelves(); //loading shelves with initial inventory

        }

        public void loadShelves()
        {
            List<Item> currInventory = ReadInventory();
            foreach (Item item in currInventory)
            {
                shelves[item.shelfId].items.Add(item);
            }
            invId = currInventory.Count;
        }
        /*
         * @param: an Order to be fulfilled by a robot
         * @return: void
         * fulfills a client's order by first validating that all items are available in inventory and then assigning a robot to that order
         * */
        // After checking order is valid we need to change the order to a list of items (repeated items if multiple quantities)
        public void fulfillOrder(Order order)
        {
            Console.WriteLine("Order {0}", order.id);
            setOrderStatus(order, this.ORDER_RECEIVED);
            orderLog.Add((order, (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds));
            
            setOrderStatus(order, this.ORDER_PROCESSING);
                
            collectOrder(order);
            setOrderStatus(order, this.ORDER_FULFILLED);
            
        }

        private void collectOrder(Order order)
        {
            List<Item> orderItems = orderToItems(order); //convert our order into a list of individual items
            int currRobot = 0;
            List<Item> currInventory = ReadInventory();
            for (int i = 0; i < orderItems.Count; i++)
            {
                for (int j = 0; j < currInventory.Count; j++)
                {
                    if (currInventory[j].id == orderItems[i].id)
                    {
                        currInventory.RemoveAt(j);
                        break;
                    }
                }

                if (currRobot == this.numRobots)
                {
                    currRobot = 0;
                }

                (Item, Shelf) currItem = (orderItems[i], shelves[orderItems[i].shelfId]);
                this.workingRobots[currRobot].QueueItem(currItem, 1);
                currRobot += 1;
            }

            int robotsInUse = 0;
            for (int i = 0; i < numRobots; i++)
            {
                int idx = i;
                if (!workingRobots[i].queueIsEmpty())
                {
                    robotsInUse += 1;
                    threads[idx] = new Thread(() => workingRobots[idx].getOrder(ref Computer.gridCellMutices));
                    threads[idx].Start();
                }
            }

            for (int i = 0; i < robotsInUse; i++)
            {
                int idx = i;
                threads[idx].Join();
            }

            UpdateInventory(currInventory);
            processedOrders.Enqueue(order);

        }
        /*
         * @param: an Order to be validated
         * @return: boolean
         * validates an order by ensuring all items are in inventory and stock is available
         * */
        public bool OrderIsValid(Order order)
        {
            List<Item> inventory = ReadInventory();


            foreach ((Product, int) product in order.products)
            {
                string item_name = product.Item1.name;

                int quantity = product.Item2;
                foreach (Item item in inventory)
                {
                    // idk if we can use id can be used cause each product will have multiple stock items and each of those items has sep id
                    if (item_name == item.name)
                    {
                        quantity -= 1;
                    }

                }
                if (quantity > 0)
                {
                    return false;
                }
            }
            return true;

        }

        private List<Item> orderToItems(Order order)
        {
            List<Item> inventory = ReadInventory();
            List<Item> items = new List<Item>();
            foreach ((Product, int) product in order.products)
            {
                int quantity = product.Item2;

                for (int i = 0; i < inventory.Count; i++)
                {
                    if (product.Item1.name == inventory[i].name)
                    {
                        items.Add(inventory[i]);
                        quantity--;
                    }
                    if (quantity == 0)
                    {
                        break;
                    }
                }

            }
            return items;
        }

        public void loadProcessedOrders(DeliveryTruck currTruck)
        {
            //load processed orders into delivery truck as long as maxWeightCap of truck not exceeded 
            while (processedOrders.Count > 0) //3
            {

                Order currOrder;
                processedOrders.TryPeek(out currOrder);
                double currOrderWeight = 0.0;

                foreach ((Product, int) item in currOrder.products)
                {
                    currOrderWeight += ((item.Item1.weight) * item.Item2);  // order has multiple quantities
                }

                if (currOrderWeight > currTruck.maxWeightCapacity)
                {
                    Console.WriteLine("ERROR: ORDER WEIGHT EXCEEDS TRUCK CAPACITY. NEED BIGGER TRUCK");
                    processedOrders.TryDequeue(out Order order);
                    setOrderStatus(order, "TRUCK FAILED");
                    break;
                }
                else
                {
                    if (currOrderWeight <= currTruck.maxWeightCapacity - currTruck.currWeight)
                    {
                        processedOrders.TryDequeue(out Order order);
                        currTruck.orders.Add(order);
                        currTruck.currWeight += currOrderWeight;
                    }
                    else
                    {
                        bool needDeliveryTruck = true;
                        DeliveryTruck tempTruck1 = (DeliveryTruck)this.dockingQueue.Dequeue();
                        Thread deliveryThread1 = new Thread(() => deliverOrders(tempTruck1));
                        deliveryThread1.Start();
                        currTruck = serviceNextTruck(needDeliveryTruck);
                    }
                }

            }
            DeliveryTruck tempTruck2 = (DeliveryTruck)this.dockingQueue.Dequeue();
            serviceNextTruck();
            Thread deliveryThread2 = new Thread(() => deliverOrders(tempTruck2));
            deliveryThread2.Start();

        }

        public DeliveryTruck serviceNextTruck(bool needNewDeliveryTruck = false)
        {
            if (needNewDeliveryTruck) //check if another delivery truck is required and whether there is at least 1 available. If not, they are already in the dockingQueue
            {
                bool deliveryTruckInDock = false;
                foreach (Truck dockedTruck in dockingQueue)
                {
                    if (dockedTruck.GetType() == typeof(DeliveryTruck))
                    {
                        deliveryTruckInDock = true;
                        break;
                    }
                }
                if (!deliveryTruckInDock)
                {
                    while (true)
                    {
                        if (this.deliveryTruckQueue.TryDequeue(out DeliveryTruck truck))
                        {
                            this.dockingQueue.Enqueue(truck);
                            break;
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }

            }

            if (this.dockingQueue.Count > 0)
            {
                while (this.dockingQueue.Peek().GetType() == typeof(RestockTruck))
                {
                    //Console.WriteLine("Restock truck {0} has arrived", this.dockingQueue.Peek().id);
                    RestockTruckItems((RestockTruck)this.dockingQueue.Dequeue());
                    if (this.dockingQueue.Count == 0)
                    {
                        break;
                    }
                }
            }

            DeliveryTruck currTruck = new DeliveryTruck(-1, -1);

            if (this.dockingQueue.Count > 0)
            {
                currTruck = (DeliveryTruck)this.dockingQueue.Peek();
            }

            //loadProcessedOrders(currTruck);
            return currTruck;
        }

        public void deliverOrders(DeliveryTruck truck)
        {
            Console.WriteLine("Truck {0} is going out for delivery.", truck.id);
            foreach (Order order in truck.orders)
            {
                setOrderStatus(order, this.ORDER_DELIVERY);
            }
            foreach (Order order in truck.orders)
            {
                Thread.Sleep(10);
                if (order.id == -1)
                {
                    setOrderStatus(order, this.DISCONTINUED_ORDER);
                }
                else 
                { 
                    setOrderStatus(order, this.ORDER_DELIVERED); 
                }

                
            }
            truck.orders.Clear();
            Console.WriteLine("Truck {0} has delivered all items and has returned to warehouse.", truck.id);
            this.deliveryTruckQueue.Enqueue(truck);
        }

        public void RestockTruckItems(RestockTruck truck)
        {
            Product[] catalog = ReadCatalog();
            //iterate over every item in the restocking truck and use the restockItem() method to update inventory
            foreach ((Product, int) product in truck.items)
            {
                // update the catalog with new stock
                catalog[product.Item1.id].stock += product.Item2;

                Item item = new Item(product.Item1.name, product.Item1.weight, product.Item1.price);
                for (int i = 0; i < product.Item2; i++) // 'item.Item2' represents the quantity of each item within the truck. Call restockItem() for each individual item
                {
                    restockItem(item);
                }

            }
            //clear the restock truck items and put back into restockTruckqueue for reuse
            UpdateCatalog(catalog);
            truck.items.Clear();
            this.restockTruckQueue.Enqueue(truck);


        }

        /*
         * @param: an item to be restocked in inventory
         * @return: void
         * restocks item to a shelf. Shelf id generated using "Random" class. If shelf full, try new shelf. Loop until empty shelf found
         * */

        // *****what happens if shelf weight is maxed out and have items to restock, stuck in while true loop forever currently
        public void restockItem(Item newItem)
        {
            List<Item> inventory = ReadInventory();

            newItem.id = invId+5;
            invId += 20;

            Random rand = new Random();
            while (true)
            {

                int shelfNumber = rand.Next(0, this.shelves.Length - 1);
                if (shelves[shelfNumber].currWeight + newItem.weight <= shelves[shelfNumber].maxWeight)
                {
                    newItem.shelfId = shelves[shelfNumber].id;
                    shelves[shelfNumber].currWeight += newItem.weight;
                    shelves[shelfNumber].items.Add(newItem);
                    inventory.Add(newItem);
                    break;
                }
                else
                {
                    // ************************************ move extra items to other warehouse if full
                    break;
                }
            }
            UpdateInventory(inventory);

        }


        // need to update catalog with new stock
        //**need to also implement logic to check restock truck capacity
        public int ReadAndReplaceCatalogStock()
        {
            Product[] currentCatalog = ReadCatalog();
            List<(Product, int)> productToRestock = new List<(Product, int)>();
            foreach (Product product in currentCatalog)
            {
                if ((product.stock < this.maxItemStock) && product.stock!=-1)
                {
                    productToRestock.Add((product, this.maxItemStock - product.stock));
                }
            }

            if (productToRestock.Count > 0) //check if there are any items that need to be restocked
            {
                RestockTruck availableRestockTruck = null;
                while (availableRestockTruck == null)
                {
                    if (this.restockTruckQueue.Count > 0) //***need to actually check the restockTruck queue here, which needs to be implemented
                    {
                        availableRestockTruck = this.restockTruckQueue.Dequeue();
                        break;
                    }
                    //else
                    //{
                    //    serviceNextTruck(); //no restock truck available, service the dockingQueue trucks first and wait till restock truck put back into restockTruck queue
                    //}
                }

                availableRestockTruck.items = productToRestock; //assign a truck to bring in the inventory that needs to be replaced
                this.dockingQueue.Enqueue(availableRestockTruck);
                serviceNextTruck();
                return availableRestockTruck.id;

            }
            return -1;
        }

        //only adds an item to our catalogue of available items, doesn't actually place anything in the inventory
        public static void AddNewCatalogItem(Product item)
        {
            Product[] currCatalog = ReadCatalog(); //read in current inventory
            //check if item was already in our catalogue,
            foreach (Product prod in currCatalog)
            {
                if (prod.name == item.name) return;
            }

            Product[] updatedCatalog = new Product[currCatalog.Length + 1]; //create new Item[] array with size = previous size + 1 to accomodate new catalogue item
            int newItemId = currCatalog.Length;
            item.id = newItemId;
            for (int i = 0; i < currCatalog.Length; i++) //copy over all existing catalogue items
            {
                updatedCatalog[i] = currCatalog[i];
            }
            updatedCatalog[updatedCatalog.Length - 1] = item; //add the newest item
            UpdateCatalog(updatedCatalog); //update the inventory JSON file
            // ***** check to see if the item already exists to avoid accidental duplicates
        }

        public string notifyUser(Order order)
        {
            return order.status;
        }


        public void setOrderStatus(Order order, String status)
        {
            foreach ((Order, int) logOrder in orderLog)
            {
                if (logOrder.Item1.id == order.id)
                {
                    logOrder.Item1.status = status;
                    break;
                }
            }
        }

        public String queryOrderStatus(Order order)
        {
            foreach ((Order, int) logOrder in orderLog)
            {
                if (logOrder.Item1.id == order.id)
                {
                    return logOrder.Item1.status;
                }
            }
            return "ORDER NOT FOUND";
        }

        public static void UpdateInventory(List<Item> newItems)
        {
            string fileName = "../../../inventory.json";
            string jsonString = JsonSerializer.Serialize(newItems);
            File.WriteAllText(fileName, jsonString);

        }

        public static List<Item> ReadInventory()
        {
            string fileName = "../../../inventory.json";
            string jsonString = File.ReadAllText(fileName);
            //Item[] items = new Item[2];
            if (jsonString == "")
            {
                return new List<Item>();
            }
            List<Item> items = JsonSerializer.Deserialize<List<Item>>(jsonString);
            return items;
        }

        public static void UpdateCatalog(Product[] newItems)
        {
            string fileName = "../../../catalogue.json";
            string jsonString = JsonSerializer.Serialize(newItems);

            using(var mutex = new Mutex(false, "catalog mutex"))
            {
                mutex.WaitOne();
                File.WriteAllText(fileName, jsonString);
                mutex.ReleaseMutex();
            }
            

        }

        public static Product[] ReadCatalog()
        {
            string fileName = "../../../catalogue.json";
            string jsonString;

            using (var mutex = new Mutex(false, "catalog mutex"))
            {
                mutex.WaitOne();
                jsonString = File.ReadAllText(fileName);
                mutex.ReleaseMutex();
            }
            Product[] products = JsonSerializer.Deserialize<Product[]>(jsonString);
            return products;
        }
        public void discontiueProduct(Product product)
        {
            List<(Product item, int quantity)> discontinueOrder = new List<(Product item, int quantity)>();
            discontinueOrder.Add((product, product.stock));
            Order order = new Order(-1, discontinueOrder, "");
            collectOrder(order);
            Console.WriteLine("Discontinued products are collected at the warehouse and will be sent out in the next truck");
        }
    }
}