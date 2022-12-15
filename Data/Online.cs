using System.Collections.Generic;

namespace StreamDanmaku_Server.Data
{
    /// <summary>
    /// 在线相关
    /// </summary>
    public static class Online
    {
        /// <summary>
        /// 在线用户
        /// </summary>
        public static List<User> Users { get; set; } = new();
    }
}