using System;
using System.Windows;
using System.Collections.Generic;
using System.Threading;


namespace Amazoom
{
    public class Robot
    {
        private int id;
        private double currBatteryLevel;
        private double maxLoadingCap = 300.0;
        private double currentLoad = 0.0;
        private int[] location;
        private bool isActive = false;
        private Queue<(Item, Shelf)> robotQueue = new Queue<(Item, Shelf)>();
        


        public Robot(int id, int[] location)
        {
            this.id = id;
            this.currBatteryLevel = 100;
            this.location = location;
        }

        /*
         * @param: Tuple<Item,Shelf>
         * @return: void
         * queues items to be processed from an Order for current robot
         * */
        public void QueueItem((Item, Shelf) item, int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                this.robotQueue.Enqueue(item);
            }
            return;
        }

        /*
         * @return: void
         * move robot to item's location in warehouse and retrieve item. Decrement inventory
         * */
        public void getOrder(ref Mutex[,] gridCellMutices)
        {
            //process all items of current order in queue
            while (this.robotQueue.Count > 0)
            {
                (Item, Shelf) currItem = this.robotQueue.Peek();
                Shelf currShelf = currItem.Item2;
                if (this.currentLoad + currItem.Item1.weight <= this.maxLoadingCap && this.currBatteryLevel > 0)
                {
                    this.robotQueue.Dequeue();
                    bool isEmpty = gridCellMutices[currShelf.shelfLocation.location[0], currShelf.shelfLocation.location[0]].WaitOne(100);
                    if (isEmpty)
                    {
                        this.location = currShelf.shelfLocation.location; //location of a specific item within our warehouse grid
                        this.currBatteryLevel -= 1;
                        for (int i = 0; i < currShelf.items.Count; i++) //iterate over items in that shelf and remove item being processed
                        {
                            if (currShelf.items[i].id == currItem.Item1.id)
                            {
                                currShelf.items.RemoveAt(i);
                                currShelf.currWeight -= currItem.Item1.weight;


                            }
                        }
                        this.currentLoad += currItem.Item1.weight;
                        gridCellMutices[currShelf.shelfLocation.location[0], currShelf.shelfLocation.location[0]].ReleaseMutex();
                    }
                    else
                    {
                        this.robotQueue.Enqueue(this.robotQueue.Dequeue());
                    }
                }
                else if (this.currBatteryLevel == 0)
                {
                    rechargeBattery();
                }
                else
                {
                    //move robot to dock, drop stuff off at bin, come back for remaining items
                    bool isEmpty = gridCellMutices[Computer.numRows - 1, Computer.numCols - 1].WaitOne(1000);
                    if (isEmpty)
                    {
                        this.location = new int[2] { Computer.numRows - 1, Computer.numCols - 1 }; //this location should be wherever we de-load our items if robot capacity is full
                        gridCellMutices[Computer.numRows - 1, Computer.numCols - 1].ReleaseMutex();
                    }

                    this.currentLoad = 0.0; //reset load

                }
            }
            //order completed, queue item for delivery
            this.location = new int[2] { -1, -1 };
            return;
        }

        private void rechargeBattery()
        {
            this.location = new int[2] { -1, -1 }; //move rebot to charging station for battery replacement outside of the warehouse
            this.currBatteryLevel = 100.0;
            return;
        }

        //setters and getters
        public void setActiveStatus(bool isActive)
        {
            this.isActive = isActive;
        }

        public bool getActiveStatus()
        {
            return this.isActive;
        }

        public void setLocation(int[] location)
        {
            this.location = location;
        }

        public int[] getLocation()
        {
            return this.location;
        }

        public void setBatteryLevel(double level)
        {
            this.currBatteryLevel = level;
        }

        public double getBatteryLevel()
        {
            return this.currBatteryLevel;
        }
        public int getId()
        {
            return this.id;
        }

        public bool queueIsEmpty()
        {
            return this.robotQueue.Count == 0;
        }

    }
}