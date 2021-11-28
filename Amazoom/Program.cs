using System;

namespace Amazoom
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Item[] items = new Item[2] { new Item("test", 100, 1, 79, -1, 1), new Item("test2", 99, 2, 70, 1,0) };
            /*Item newItem = new Item("test",100,1,69,-1);
            Item new2 = new Item("test2", 99, 2, 70, 1);*/
            
            Computer comp = new Computer();
            
            int[] itemLocation = new int[] { 1, 2 }; //randomly generated
            Robot newRobot = new Robot(1, itemLocation);


        }
    }
}
