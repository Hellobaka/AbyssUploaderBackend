using SqlSugar;
using System;
using System.Collections.Generic;

namespace StreamDanmaku_Server.Data
{
    public class UploadInfo
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int ID { get; set; }
        public int Type { get; set; }
        public string Token { get; set; }
        public string UploaderName { get; set; }
        public long Uploader { get; set; }
        public long BotID { get; set; }
        public DateTime UploadTime { get; set; }

        public void Save()
        {
            using var sql = SQLHelper.GetInstance();
            ID =  sql.Insertable(this).ExecuteReturnIdentity();
        }

        public static List<UploadInfo> QueryAbyss(DateTime time)
        {
            DayOfWeek dd = time.DayOfWeek;
            DateTime start = time, end = time;
            if (dd == DayOfWeek.Sunday)
            {
                while (start.DayOfWeek != DayOfWeek.Friday)
                {
                    start = start.AddDays(-1);
                }
                end = time;
            }
            else if (dd < DayOfWeek.Friday)
            {
                while (start.DayOfWeek != DayOfWeek.Monday)
                {
                    start = start.AddDays(-1);
                }
                end = start.AddDays(2);
            }
            else if (dd >= DayOfWeek.Friday)
            {
                while (start.DayOfWeek != DayOfWeek.Friday)
                {
                    start = start.AddDays(-1);
                }
                end = start.AddDays(2);
            }
            start = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);
            end = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);
            using var sql = SQLHelper.GetInstance();
            return sql.Queryable<UploadInfo>().Where(x => x.UploadTime >= start && x.UploadTime <= end && x.Type == 1).ToList();
        }
        public static List<UploadInfo> QueryMemoryField(DateTime time)
        {
            DayOfWeek dd = time.DayOfWeek;
            DateTime start = time, end = time;
            while (start.DayOfWeek != DayOfWeek.Tuesday)
            {
                start = start.AddDays(-1);
            }
            end = start.AddDays(7);
            start = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);
            end = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);
            using var sql = SQLHelper.GetInstance();
            return sql.Queryable<UploadInfo>().Where(x => x.UploadTime >= start && x.UploadTime <= end && x.Type == 2).ToList();
        }
    }
}
