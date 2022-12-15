using SqlSugar;

namespace StreamDanmaku_Server.Data
{
    /// <summary>
    /// 数据库操作类
    /// </summary>
    public static class SQLHelper
    {
        public static SqlSugarClient GetInstance() => new(new ConnectionConfig()
        {
            ConnectionString = Config.GetConfig("DBConnectString", $"data source=db.db"),
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
        });

        public static void Init()
        {
            var instance = GetInstance();
            instance.DbMaintenance.CreateDatabase("db.db");
            instance.CodeFirst.InitTables(typeof(UploadInfo));
            instance.CodeFirst.InitTables(typeof(RuntimeLog));
        }
    }
}