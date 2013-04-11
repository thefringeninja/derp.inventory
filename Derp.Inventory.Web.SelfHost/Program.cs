using System;
using Derp.Inventory.Web.Bootstrap;
using Nancy.Hosting.Self;

namespace Derp.Inventory.Web.SelfHost
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var host = new NancyHost(new Uri("http://localhost:11380"), new Bootstrapper());
            host.Start();
            Console.WriteLine("Application started. Hit q to quit. duh.");
            do
            {
            } while (Console.ReadKey(true).Key != ConsoleKey.Q);
            host.Stop();
        }
    }
}