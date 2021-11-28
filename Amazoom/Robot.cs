using System;
using System.Windows;
using System.Collections.Generic;

namespace Amazoom
{
    public class Robot
    {
        private int id;
        private double currBatteryLevel;
        private double maxLoadingCap = 100.0;
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
        public void QueueItem((Item, Shelf) item)
        {
            this.robotQueue.Enqueue(item);
            return;
        }

        /*
         * @return: void
         * move robot to item's location in warehouse and retrieve item. Decrement inventory
         * */
        public void getOrders()
        {
            //process all items of current order in queue
            while(this.robotQueue.Count > 0)
            {
                (Item, Shelf) currItem = this.robotQueue.Dequeue();
                Shelf currShelf = currItem.Item2;
                this.location = currShelf.shelfLocation.location; //location of a specific item within our warehouse grid
                for(int i=0;i<currShelf.items.Count; i++) //iterate over items in that shelf and remove item being processed
                {
                    if(currShelf.items[i].id == currItem.Item1.id)
                    {
                        currShelf.items.RemoveAt(i);
                        currShelf.currWeight -= currItem.Item1.weight;
                        //** decrement inventory as well for removed item **
                    }
                }

            }
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

    }
}
