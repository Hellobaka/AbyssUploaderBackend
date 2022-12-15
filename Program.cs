using System;
using System.Linq;

namespace StreamDanmaku_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Helper.StartUp();
            Helper.ReloadBotList();
            while (true)
            {
                string cmd = Console.ReadLine();
                switch (cmd.Split(' ').First())
                {
                    case "add":
                        cmd = cmd.Replace("add", "").Replace(" ", "");
                        Helper.BotIDList.Add(Convert.ToInt64(cmd));
                        Config.SetConfig("BotList", Helper.BotIDList.Join("|"));
                        break;
                    case "remove":
                        cmd = cmd.Replace("remove", "").Replace(" ", "");
                        Helper.BotIDList.Remove(Convert.ToInt64(cmd));
                        Config.SetConfig("BotList", Helper.BotIDList.Join("|"));
                        break;
                    case "reload":
                        Helper.ReloadBotList();
                        break;
                    case "list":
                        Console.WriteLine(Helper.BotIDList.Join("|"));
                        break;
                    case "?":
                        Console.WriteLine("add\nremove\nreload\nlist");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
