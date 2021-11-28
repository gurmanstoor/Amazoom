using System;

namespace Amazoom
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Item newItem = new Item("test",100,1,69,-1);
            Computer comp = new Computer();
            comp.testingJson(newItem);
            int[] itemLocation = new int[] { 1, 2 }; //randomly generated
            Robot newRobot = new Robot(1, itemLocation);


        }
    }
}
