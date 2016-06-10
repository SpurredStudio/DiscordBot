using System;

namespace DiscordBot
{
    class Program
    {
        static void Main(string[] args)
        {
        	System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"Music\");
            System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"Save\");
            System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"dll\");
            var dllDirectory = System.AppDomain.CurrentDomain.BaseDirectory+@"dll/";
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + dllDirectory);

            YDCbot myBot = new YDCbot();
        }
    }
}
