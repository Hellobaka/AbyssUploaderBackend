using SqlSugar;
using System;

namespace StreamDanmaku_Server.Data
{
    /// <summary>
    /// 日志帮助类
    /// </summary>
    [SugarTable("Log")]
    public class RuntimeLog
    {
        /// <summary>
        /// 日志列ID
        /// </summary>
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        /// <summary>
        /// 调用方名称
        /// </summary>
        public string Account { get; private init;}
        /// <summary>
        /// 操作名称
        /// </summary>
        public string ActionName { get; private init;}
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Status { get; set; }
        /// <summary>
        /// 日志内容
        /// </summary>
        public string Action { get; private init;}
        /// <summary>
        /// 日志发生时间
        /// </summary>
        public DateTime Time { get; private init;}
        /// <summary>
        /// 以系统作为调用方写日志
        /// </summary>
        /// <param name="type">操作名称</param>
        /// <param name="content">日志内容</param>
        /// <param name="status">操作是否成功</param>
        public static void WriteSystemLog(string type, string content, bool status)
        {
            WriteLog("System", type, content, status, DateTime.Now);
        }
        /// <summary>
        /// 写日志根方法
        /// </summary>
        /// <param name="origin">调用方</param>
        /// <param name="type">操作名称</param>
        /// <param name="content">日志内容</param>
        /// <param name="status">操作是否成功</param>
        /// <param name="time">日志发生时间</param>
        public static void WriteLog(string origin, string type, string content, bool status, DateTime time)
        {
            Console.WriteLine(
                $"{(status ? "[+]" : "[-]")} [{time:yyyy-MM-dd HH:mm:ss}] Origin: {origin}, Type: {type}, Content: {content}");
            var o = new RuntimeLog
            {
                Account = origin,
                ActionName = type,
                Action = content,
                Status = status,
                Time = time
            };
            SQLHelper.GetInstance().Insertable(o).ExecuteCommandAsync();
        }
    }
}
