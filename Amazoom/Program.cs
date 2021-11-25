using System;

namespace Amazoom
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Item newItem;
            newItem.name = "test";
            newItem.weight = 100;
            newItem.id = 1;
            newItem.price = 69;


            int[] itemLocation = new int[] { 1, 2 }; //randomly generated
            Robot newRobot = new Robot(1, itemLocation);


        }
    }
}
