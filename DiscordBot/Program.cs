using System;

namespace DiscordBot
{
    class Program
    {
        static void Main(string[] args)
        {
        	System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"Music\");
            System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"Save\");
            System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"lib\");
            YDCbot myBot = new YDCbot();
        }
    }
}
