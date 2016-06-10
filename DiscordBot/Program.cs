using System;

namespace DiscordBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var dllDirectory = System.AppDomain.CurrentDomain.BaseDirectory+@"dll/";
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + dllDirectory);

            YDCbot myBot = new YDCbot();
        }
    }
}
