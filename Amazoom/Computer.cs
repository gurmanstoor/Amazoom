using System;
using System.Collections.Generic;
using System.Text.Json;
namespace Amazoom
{
    public struct Item
    {
        public string name;
        public double weight;
        public double price;
        public int id;
        public int shelfId;
    }

    public struct Shelf
    {
        public List<Item> items;
        public ShelfLocation shelfLocation;
        public int id;
        public double currWeight;
        public double maxWeight;

    }

    public struct ShelfLocation
    {
        public int[] location;
        public string side;
        public int height;
    }

    public struct Order
    {
        public int id;
        public List<Item> items;
        public string status;

    }

    public class Computer
    {
        public Shelf[] shelves;
        public Robot[] robots;
        private readonly int numRobots = 5;

        
        public Computer()
        {
            //initialize warehouse shelves and robots
            initializeShelves();
            initializeRobots();

            //placeholder item
            Item newItem;
            newItem.name = "test";
            newItem.weight = 99;
            newItem.price = 99;
            newItem.id = 1;
            newItem.shelfId = -1; //shelfId not initially available, set to -1

            restockItem(newItem);

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
            int numRows = 5;
            int numCols = 8;
            int height = 3;
            int numShelves = (numCols - 1) * (numRows - 2) * height;

            this.shelves = new Shelf[numShelves];
            int shelfNum = 0;

            //initializing each shelf with properties and assigning an id
            for (int j = 0; j < numCols; j++)
            {
                for (int i = 1; i < numRows - 1; i++)
                {
                    for (int k = 0; k < height; k++)
                    {

                        Shelf currShelf;
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
                foreach (Item item in orders) //figure out whether 'orders' is an array or struct or class
                {

                    (Item, Shelf) currItem = (item, shelves[item.shelfId]);
                    //check if any other robot currently is at current item's cell in warehouse grid. If there is, wait till it empties (threading implementation using Mutex??)
                    tempRobot.QueueItem(currItem);

                }
                tempRobot.getOrders();
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
            foreach(Item item in order.items)
            {
                foreach(shelf)
            }
         
        }
        /*
         * @param: an item to be restocked in inventory
         * @return: void
         * restocks item to a shelf. Shelf id generated using "Random" class. If shelf full, try new shelf. Loop until empty shelf found
         * */

        public void restockItem(Item newItem)
        {
            Random rand = new Random();
            while (true)
            {

                int shelfNumber = rand.Next(0, this.shelves.Length-1);
                if (shelves[shelfNumber].currWeight + newItem.weight <= shelves[shelfNumber].maxWeight)
                {

                    newItem.shelfId = shelves[shelfNumber].id;
                    shelves[shelfNumber].currWeight += newItem.weight;
                    shelves[shelfNumber].items.Add(newItem);
                    break;
                }

            }

        }

    }
}
