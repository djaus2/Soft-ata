using Meadow;
using System;
using System.Threading.Tasks;

namespace MeadowDesktopHello
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await MeadowOS.Start(args);
        }

    }
}