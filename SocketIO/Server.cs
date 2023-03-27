using Newtonsoft.Json;
using StreamDanmaku_Server.Data;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace StreamDanmaku_Server.SocketIO
{
    /// <summary>
    /// WebSocket 连接器
    /// </summary>
    public class Server
    {
        /// <summary>
        /// WebSocket 实例
        /// </summary>
        private readonly WebSocketServer _instance;

        /// <summary>
        /// WebSocket 监听端口
        /// </summary>
        private readonly ushort _port;

        private static DateTime LastAbyssBoardcastTime { get; set; }

        private static DateTime LastMemoryFiledBoardcastTime { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">监听端口</param>
        public Server(ushort port)
        {
            _port = port;
            _instance = new WebSocketServer(_port);
            _instance.AddWebSocketService<MsgHandler>("/ws");
        }

        /// <summary>
        /// 开启端口监听
        /// </summary>
        public void StartServer()
        {
            _instance.Start();
            Console.WriteLine($"WebSocket URL: ws://127.0.0.1:{_port}/ws");
            RuntimeLog.WriteSystemLog("WebSocketServer", $"WebSocket服务器开启 监听端口: {_port}...", true);
        }

        /// <summary>
        /// 停止端口监听
        /// </summary>
        public void StopServer()
        {
            _instance.Stop();
            RuntimeLog.WriteSystemLog("WebSocketServer", "WebSocket服务器关闭...", true);
        }

        public static void BoardCast(string type, object msg)
        {
            // 向在线用户广播
            foreach (var item in Online.Users.Where(x => x.WebSocket.Authed))
            {
                item.WebSocket.Emit(type, msg);
            }
        }
        public static void BoardCast(APIResult api)
        {
            // 向在线用户广播
            foreach (var item in Online.Users.Where(x => x.WebSocket.Authed))
            {
                item.WebSocket.Emit(api);
            }
        }

        /// <summary>
        /// 分发消息
        /// </summary>
        /// <param name="socket">WebSocket连接</param>
        /// <param name="jsonText">消息内容</param>
        private static void HandleMessage(MsgHandler socket, string jsonText)
        {
            APIResult request = JsonConvert.DeserializeObject<APIResult>(jsonText);
            if (request.Type != "Auth" && socket.Authed == false) return;
            try
            {
                switch (request.Type)
                {
                    case "Auth":
                        if (long.TryParse(request.Data.ToString(), out long qq) && Helper.BotIDList.Contains(qq))
                        {
                            socket.CurrentUser = new User { Id = qq, WebSocket = socket };
                            socket.Authed = true;
                            Online.Users.Add(socket.CurrentUser);
                            RuntimeLog.WriteSystemLog("Auth", $"QQ={qq} 连接成功", true);
                        }
                        else
                        {
                            socket.Authed = false;
                            RuntimeLog.WriteSystemLog("Auth", $"{request.Data} 连接失败", false);
                        }
                        break;
                    case "Heartbeat":
                        socket.Emit("Heartbeat", "");
                        break;
                    case "UploadAbyssInfo":
                        HandleAbyssUpload(request, socket);
                        break;
                    case "UploadMemoryField":
                        HandleMemoryFieldUpload(request, socket);
                        break;
                    case "QueryAbyssInfo":
                        HandleAbyssQuery(request, socket);
                        break;
                    case "QueryMemoryFieldInfo":
                        HandleMemoryFieldQuery(request, socket);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                socket.Error("消息处理错误", request.Token, "QueryMemoryFieldInfo");
                RuntimeLog.WriteSystemLog("WebSocketServer", $"消息处理错误, error={e.Message}", false);
            }
        }

        private static void HandleMemoryFieldQuery(APIResult request, MsgHandler socket)
        {
            RuntimeLog.WriteSystemLog("MemoryFieldQuery", $"QQ={socket.CurrentUser.Id} 调用记忆战场查询", true);
            var ls = UploadInfo.QueryMemoryField(DateTime.Now);
            var info = ls.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            if (info == null)
            {
                socket.Error("未查询到结果", request.Token, "QueryMemoryFieldInfo");
            }
            else
            {
                string base64 = Convert.ToBase64String(File.ReadAllBytes(Path.Combine("Image", $"{info.Token}.png")));
                socket.Emit(new APIResult
                {
                    Token = request.Token,
                    Data = new APIResult.Info
                    {
                        PicBase64 = base64,
                        UploaderName = info.UploaderName,
                        UploadTime = info.UploadTime
                    },
                    Type = "QueryMemoryFieldInfo"
                });
            }
        }

        private static void HandleAbyssQuery(APIResult request, MsgHandler socket)
        {
            RuntimeLog.WriteSystemLog("AbyssQuery", $"QQ={socket.CurrentUser.Id} 调用深渊查询", true);
            var ls = UploadInfo.QueryAbyss(DateTime.Now);
            var info = ls.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            if (info == null)
            {
                socket.Error("未查询到结果", request.Token, "QueryAbyssInfo");
            }
            else
            {
                string base64 = Convert.ToBase64String(File.ReadAllBytes(Path.Combine("Image", $"{info.Token}.png")));
                socket.Emit(new APIResult
                {
                    Token = request.Token,
                    Data = new APIResult.Info
                    {
                        PicBase64 = base64,
                        UploaderName = info.UploaderName,
                        UploadTime = info.UploadTime
                    },
                    Type = "QueryAbyssInfo"
                });
            }
        }

        private static void HandleMemoryFieldUpload(APIResult request, MsgHandler socket)
        {
            RuntimeLog.WriteSystemLog("MemoryFieldUpload", $"QQ={socket.CurrentUser.Id} 调用记忆战场上传", true);
            APIResult.Info info = null;
            try
            {
                info = JsonConvert.DeserializeObject<APIResult.Info>(request.Data.ToString());
            }
            catch
            {
                socket.Error("无法解析的消息", request.Token, "UploadMemoryField");
            }
            if (info == null) return;
            Helper.SaveImage(info.PicBase64, request.Token);
            UploadInfo upload = new()
            {
                Token = request.Token,
                Type = 2,
                UploaderName = info.UploaderName,
                BotID = socket.CurrentUser.Id,
                UploadTime = info.UploadTime,
                Uploader = info.Uploader
            };
            upload.Save();
            socket.Emit(new APIResult { Token = request.Token, Type = "UploadMemoryField" });
            RuntimeLog.WriteSystemLog("MemoryFieldUpload", $"QQ={socket.CurrentUser.Id} 记忆战场上传成功 Token={request.Token} Remark={info.Remark}", true);

            DateTime baseTuesday = LastMemoryFiledBoardcastTime;
            while(baseTuesday.DayOfWeek != DayOfWeek.Tuesday)
            {
                baseTuesday = baseTuesday.AddDays(-1);
            }
            if ((DateTime.Now - baseTuesday).Days >= 7)
            {
                request.Type = "BoardcastMemoryField";
                info.ID = upload.ID;
                request.Data = info.ToJson();
                BoardCast(request);
                LastMemoryFiledBoardcastTime = DateTime.Now;
            }
        }

        private static void HandleAbyssUpload(APIResult request, MsgHandler socket)
        {
            RuntimeLog.WriteSystemLog("AbyssUpload", $"QQ={socket.CurrentUser.Id} 调用深渊上传", true);
            APIResult.Info info = null;
            try
            {
                info = JsonConvert.DeserializeObject<APIResult.Info>(request.Data.ToString());
            }
            catch
            {
                socket.Error("无法解析的消息", request.Token, "UploadMemoryField");
            }
            if (info == null) return;
            Helper.SaveImage(info.PicBase64, request.Token);
            UploadInfo upload = new()
            {
                Token = request.Token,
                Type = 1,
                UploaderName = info.UploaderName,
                BotID = socket.CurrentUser.Id,
                UploadTime = info.UploadTime,
                Uploader = info.Uploader
            };
            upload.Save();
            socket.Emit(new APIResult { Token = request.Token, Type = "UploadAbyssInfo" });
            RuntimeLog.WriteSystemLog("AbyssUpload", $"QQ={socket.CurrentUser.Id} 深渊上传成功 Token={request.Token} Remark={info.Remark}", true);

            bool boardCastFlag = false;
            int weekDay = GetDayofWeek(DateTime.Now);
            if (weekDay >= 1 && weekDay <= 3)
            {
                boardCastFlag = !IsSameWeek(LastAbyssBoardcastTime, DateTime.Now);
            }
            else if (weekDay >= 5 && weekDay <= 7)
            {
                boardCastFlag = GetDayofWeek(LastAbyssBoardcastTime) < 4;
            }

            if (boardCastFlag)
            {
                request.Type = "BoardcastAbyss";
                info.ID = upload.ID;
                request.Data = info.ToJson();
                BoardCast(request);
                LastAbyssBoardcastTime = DateTime.Now;
            }
        }

        public static int GetDayofWeek(DateTime dt)
        {
            int weekDay = (int)dt.DayOfWeek;
            if (weekDay == 0)
            {
                weekDay = 7;
            }
            return weekDay;
        }

        public static bool IsSameWeek(DateTime date1, DateTime date2)
        {
            // 获取当前区域性的日历
            CultureInfo ci = CultureInfo.CurrentCulture;
            Calendar calendar = ci.Calendar;

            // 获取日期所在的周数
            int week1 = calendar.GetWeekOfYear(date1, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
            int week2 = calendar.GetWeekOfYear(date2, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);

            // 判断是否处于同一周
            if (week1 == week2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 消息处理端
        /// </summary>
        public class MsgHandler : WebSocketBehavior
        {
            /// <summary>
            /// 连接用户
            /// </summary>
            public User CurrentUser { get; set; }

            /// <summary>
            /// 连接对方IP
            /// </summary>
            public IPAddress ClientIP { get; set; }

            /// <summary>
            /// 有效连接
            /// </summary>
            public bool Authed { get; set; }

            /// <summary>
            /// 触发消息
            /// </summary>
            /// <param name="e">消息事件</param>
            protected override void OnMessage(MessageEventArgs e)
            {
                HandleMessage(this, e.Data);
            }

            /// <summary>
            /// 连接建立
            /// </summary>
            protected override void OnOpen()
            {
                ClientIP = Context.UserEndPoint.Address;
                RuntimeLog.WriteSystemLog("WebSocketServer", $"连接已建立, id={ID}, ip={ClientIP}", true);
            }

            /// <summary>
            /// 连接断开 认为是异常断开 包含房间销毁判断
            /// </summary>
            /// <param name="e">断开事件</param>
            protected override void OnClose(CloseEventArgs e)
            {
                if (Online.Users.Contains(CurrentUser))
                {
                    // 从在线列表移除此用户
                    Online.Users.Remove(CurrentUser);
                }

                // 广播在线人数变化
                BoardCast("OnlineUserChange", Online.Users.Count);
                RuntimeLog.WriteSystemLog("WebSocketServer", $"连接断开, id={ID}", true);
            }

            public void Emit(string type, object msg)
            {
                Send(new APIResult { Type = type, Data = msg }.ToJson());
            }
            public void Emit(APIResult request)
            {
                Send(request.ToJson());
            }

            public void Error(string msg, string token, string type)
            {
                Send(new APIResult { IsSuccess = false, Message = msg, Token = token, Type = type }.ToJson());
            }

            public void CloseConnection()
            {
                Context.WebSocket.Close(CloseStatusCode.Normal, "Reconnect.");
            }
        }
    }
}