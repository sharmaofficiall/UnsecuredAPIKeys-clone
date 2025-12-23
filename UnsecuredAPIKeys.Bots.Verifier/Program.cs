using System;

namespace UnsecuredAPIKeys.Bots.Verifier
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Delegate to the asynchronous verifier implementation and block until complete
            System.Threading.Tasks.Task.Run(async () => await Verifier_Program.Main()).GetAwaiter().GetResult();
        }
    }
}
