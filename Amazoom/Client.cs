using System;
using System.Windows;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Amazoom
{
    public class Client
    {
        //List<(int ID, int count, Item product)> catalogue = new List<(int ID, int count, Item product)>();

        public static int displayStore()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|           AMAZOOM STORE          |");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("1: View Products ");
            Console.WriteLine("2: View Cart ");
            Console.WriteLine("3: Exit Store ");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Enter Option: ");
            return Convert.ToInt32(Console.ReadLine());
        }

        public static void viewProducts()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|          AMAZOOM Products        |");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("1: Add item to Cart ");
            Console.WriteLine("2: View Cart ");
            Console.WriteLine("3: Go Back Home ");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Enter Option: ");
            return Convert.ToInt32(Console.ReadLine());
        }
        public static void viewCart()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine("|         Your AMAZOOM Cart        |");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("1: Remove Item ");
            Console.WriteLine("2: Checkout ");
            Console.WriteLine("3: Go Back Home ");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Enter Option: ");
            return Convert.ToInt32(Console.ReadLine());
        }
        public static void addCart()
        {

        }
        public static void removeCart()
        {

        }
        public static void checkout()
        {

        }

        public static void loadCatalogue(List<(int ID, int count, Item product)> catalogue)
        {

        }

        static void Main(string[] args)
        {
            List<(int ID, int count, Item product)> catalogue = new List<(int ID, int count, Item product)>();

            loadCatalogue(catalogue);
        }
    }
}