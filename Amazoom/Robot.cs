using System;
using System.Windows;
using System.Collections;

namespace Amazoom
{
    public class Robot
    {
        private int id;
        private double currBatteryLevel;
        private double maxLoadingCap = 100.0;
        private int[] location;
        private bool isActive = false;
        private Queue robotQueue = new Queue();

        

        public Robot(int id, int[] location)
        {
            this.id = id;
            this.currBatteryLevel = 100;
            this.location = location;
        }

        //public void queueOrder()
        //{
        //}

        public void getOrders()
        {
            return;
        }

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
