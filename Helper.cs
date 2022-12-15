﻿using Newtonsoft.Json;
using StreamDanmaku_Server.Data;
using System;
using System.IO;

namespace StreamDanmaku_Server
{
    /// <summary>
    /// 通用帮助类
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// 连接保存对象
        /// </summary>
        static SocketIO.Server Server;
        /// <summary>
        /// 启动初始化
        /// </summary>
        public static void StartUp()
        {
            SQLHelper.Init();
            Server = new(Config.GetConfig<ushort>("ServerPort", 30308));
            Server.StartServer();
        }

        /// <summary>
        /// 扩展方法 快捷调用对象序列化
        /// </summary>
        /// <param name="json">需要序列化的对象</param>
        public static string ToJson(this object json) => JsonConvert.SerializeObject(json, Formatting.None);

        /// <summary>
        /// 毫秒级时间戳
        /// </summary>
        public static long TimeStampms =>
            (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;

        /// <summary>
        /// 秒级时间戳
        /// </summary>
        public static long TimeStamp => (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

        public static void SaveImage(string base64, string token)
        {
            Directory.CreateDirectory("Image");
            File.WriteAllBytes(Path.Combine("Image", $"{token}.png"), Convert.FromBase64String(base64));
        }
    }
}