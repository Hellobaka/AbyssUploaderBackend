using System;

namespace StreamDanmaku_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Helper.StartUp();
            while(true)
            {
                string cmd = Console.ReadLine();
                switch (cmd)
                {
                    default:
                        break;
                }
            }
        }
    }
}
