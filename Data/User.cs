using SqlSugar;
using static StreamDanmaku_Server.SocketIO.Server;

namespace StreamDanmaku_Server.Data
{
    /// <summary>
    /// 用户相关类
    /// </summary>
    public class User
    {
        /// <summary>
        /// QQ
        /// </summary>
        public long Id { get; set; }
        [SugarColumn(IsIgnore = true)]

        public MsgHandler WebSocket { get; set; }
    }
}