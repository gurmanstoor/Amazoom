using System;
using System.Collections.Generic;
namespace Amazoom
{
    struct Item
    {
        public string name;
        public double weight;
        public double price;
        public int id;
        public int shelfId;
    }

    struct Shelf
    {
        public List<Item> items;
        public ShelfLocation shelfLocation;
        public int id;
        public double currWeight;
        public double maxWeight;

    }

    struct ShelfLocation
    {
        public int[] location;
        public string side;
        public int height;
    }

    public class Computer
    {
        public Computer()
        {
            int numRows = 5;
            int numCols = 8;
            int height = 3;
            int numShelves = (numCols - 1) * (numRows - 2) * height;

            Shelf[] shelves = new Shelf[numShelves];
            int shelfNum = 0;
            for(int j=0; j < numCols; j++)
            {
                for(int i=1; i < numRows - 1; i++)
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
                        else if (j == numCols-1)
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

            Item newItem;
            newItem.name = "test";
            newItem.weight = 99;
            newItem.price = 99;
            newItem.id = 1;
            Random rand = new Random();

            while(true)
            {
         
                int shelfNumber = rand.Next(0,shelfNum);
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
