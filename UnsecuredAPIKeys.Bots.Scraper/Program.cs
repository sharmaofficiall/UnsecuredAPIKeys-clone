using System;

namespace UnsecuredAPIKeys.Bots.Scraper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Delegate to the asynchronous scraper implementation and block until complete
            System.Threading.Tasks.Task.Run(async () => await Scraper_Program.Main()).GetAwaiter().GetResult();
        }
    }
}
