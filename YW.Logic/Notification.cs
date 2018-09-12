using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using YW.Notification;

namespace YW.Logic
{
    public class Notification : Base
    {
        private static Notification _object;
        private static readonly object LockHelper = new object();
        private readonly ConcurrentDictionary<int, Model.Entity.Notification> _dictionaryById;
        private readonly Dictionary<string, NotificationService> _dictionaryServer;
        private readonly ConcurrentDictionary<int, List<Model.Entity.UserNotification>> _dictionaryTempNotification;
        private readonly ConcurrentDictionary<int, List<Model.NotificationCount>> _dictionaryTempNotificationCount;

        public static Notification GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new Notification();
                    }
                }
            }

            return _object;
        }

        public Notification()
            : base(typeof(Model.Entity.Notification))
        {
            _dictionaryById = new ConcurrentDictionary<int, Model.Entity.Notification>();
            _dictionaryServer = new Dictionary<string, NotificationService>();
            _dictionaryTempNotification = new ConcurrentDictionary<int, List<Model.Entity.UserNotification>>();
            _dictionaryTempNotificationCount = new ConcurrentDictionary<int, List<Model.NotificationCount>>();
        }

        public void CleanNotificationTemp(int userId)
        {
            _dictionaryTempNotification.TryRemove(userId, out var res);
            _dictionaryTempNotificationCount.TryRemove(userId, out var res1);
        }

        public NotificationService GetServer(string project)
        {
            lock (_dictionaryServer)
            {
                if (_dictionaryServer.ContainsKey(project))
                {
                    return _dictionaryServer[project];
                }
                else
                {
                    string p12Filename = System.IO.Path.Combine(Data.Config.GetInstance().Path + "\\Apple\\",
                        project + ".p12");
                    var service = new NotificationService(false, p12Filename, "123456", 1)
                    {
                        SendRetries = 5,
                        ReconnectDelay = 5000
                    };
                    service.Error += new NotificationService.OnError(service_Error);
                    service.NotificationTooLong +=
                        new NotificationService.OnNotificationTooLong(service_NotificationTooLong);

                    service.BadDeviceToken += new NotificationService.OnBadDeviceToken(service_BadDeviceToken);
                    service.NotificationFailed +=
                        new NotificationService.OnNotificationFailed(service_NotificationFailed);
                    service.NotificationSuccess +=
                        new NotificationService.OnNotificationSuccess(service_NotificationSuccess);
                    service.Connecting += new NotificationService.OnConnecting(service_Connecting);
                    service.Connected += new NotificationService.OnConnected(service_Connected);
                    service.Disconnected += new NotificationService.OnDisconnected(service_Disconnected);
                    _dictionaryServer.Add(project, service);
                    return service;
                }
            }
        }

        public Model.Entity.Notification Get(int objId)
        {
            _dictionaryById.TryGetValue(objId, out var obj);
            if (obj == null)
            {
                const string sql = "select top 1 [Notification].* from [Notification] where [Notification].[NotificationID]=@NotificationID";
                DbParameter[] commandParameters =
                {
                    Data.DBHelper.CreateInDbParameter("@NotificationID", DbType.Int32, objId),
                };
                var ds = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql, commandParameters);
                var list = TableToList<Model.Entity.Notification>(ds);
                if (list.Count > 0)
                {
                    obj = list[0];
                    _dictionaryById.TryAdd(obj.NotificationID, obj);
                }
            }

            return obj;
        }

        public void Save(Model.Entity.Notification obj)
        {
            obj.NotificationID = 0;
            base.Save(obj);
            _dictionaryById.TryAdd(obj.NotificationID, obj);
        }

        /// <summary>
        /// 获取该用户未获取通知
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Model.Entity.UserNotification> GetNotification(int userId)
        {
            if (!_dictionaryTempNotification.TryGetValue(userId, out var list))
            {
                const string sql = "select un.* from [UserNotification] un where UserID=@UserId and Notification=0";
                DbParameter[] commandParameters =
                {
                    Data.DBHelper.CreateInDbParameter("@UserID", DbType.Int32, userId)
                };
                var ds = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql, commandParameters).Tables[0];
                list = new List<Model.Entity.UserNotification>(ds.Rows.Count);
                if (ds.Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        list.Add(new Model.Entity.UserNotification
                        {
                            UserNotificationId = (int) (dr["UserNotificationId"]),
                            UserID = (int) (dr["UserID"]),
                            DeviceID = (int) (dr["DeviceID"]),
                            Type = (int) (dr["Type"]),
                            ObjectId = (int) (dr["ObjectId"]),
                            Get = dr["Get"] != DBNull.Value && (bool) (dr["Get"]),
                            Notification = dr["Notification"] != DBNull.Value && (bool) (dr["Notification"]),
                            CreateTime = (DateTime) (dr["CreateTime"]),
                            UpdateTime = (DateTime) (dr["UpdateTime"])
                        });
                    }

                    string ids = string.Join(",", list.ConvertAll(x => x.UserNotificationId.ToString()));
                    string upSql = "update [UserNotification] set Notification=1 where UserNotificationId in (" + ids + ")";
                    Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, upSql, null);
                }

                if (list.Count == 0)
                {
                    _dictionaryTempNotification.TryAdd(userId, list);
                }
            }

            return list;
        }

        public void CleanRenewNotification(int userId)
        {
            const string sql = "delete from [UserNotification] where UserID=@UserId and Type=6";
            DbParameter[] commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@UserID", DbType.Int32, userId)
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
        }

        public List<Model.NotificationCount> GetNotificationCount(int userId)
        {
            List<Model.NotificationCount> list;
            if (!_dictionaryTempNotificationCount.TryGetValue(userId, out list))
            {
                const string sql =
                    "select [UserDevice].DeviceID," +
                    "SUM(case when [UserNotification].[Type]=1 or [UserNotification].[Type]=4 then 1 else 0 end) as [Message]," +
                    "SUM(case when [UserNotification].[Type]=2 then 1 else 0 end) as [Voice]," +
                    "SUM(case when [UserNotification].[Type]=3 then 1 else 0 end) as [SMS]," +
                    "SUM(case when [UserNotification].[Type]=7 then 1 else 0 end) as [Photo]" +
                    " from [UserDevice] left join [UserNotification] on [UserNotification].UserID=@UserID and [UserNotification].Get=0 and [UserDevice].DeviceID=[UserNotification].DeviceID where UserDevice.UserID=@UserID and UserDevice.Status=1 group by [UserDevice].DeviceID";
                DbParameter[] commandParameters = new DbParameter[]
                {
                    Data.DBHelper.CreateInDbParameter("@UserID", DbType.Int32, userId)
                };
                var ds = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql, commandParameters).Tables[0];
                list = new List<Model.NotificationCount>(ds.Rows.Count);
                if (ds.Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        list.Add(new Model.NotificationCount
                        {
                            DeviceID = (int) (dr["DeviceID"]),
                            Message = dr["Message"] != DBNull.Value ? (int) (dr["Message"]) : 0,
                            Voice = dr["Voice"] != DBNull.Value ? (int) (dr["Voice"]) : 0,
                            SMS = dr["SMS"] != DBNull.Value ? (int) (dr["SMS"]) : 0,
                            Photo = dr["Photo"] != DBNull.Value ? (int) (dr["Photo"]) : 0
                        });
                    }
                }

                if (!_dictionaryTempNotificationCount.ContainsKey(userId))
                {
                    _dictionaryTempNotificationCount.TryAdd(userId, list);
                }
                else
                {
                    _dictionaryTempNotificationCount[userId] = list;
                }
            }

            return list;
        }

        /// <summary>
        /// 获取未读系统消息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public List<Model.Entity.UserNotification> GetList(int userId, int deviceId)
        {
            const string sql =
                "select * from [UserNotification] where UserId=@UserId  and Get=0 and (Type=1 or Type=4 or Type=5) and (DeviceID=@DeviceID or DeviceID=0) order by UserNotificationId asc";
            DbParameter[] commandParameters =
            {
                Data.DBHelper.CreateInDbParameter("@UserID", DbType.Int32, userId),
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId)
            };
            var ds = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql, commandParameters).Tables[0];
            List<Model.Entity.UserNotification> list = new List<Model.Entity.UserNotification>(ds.Rows.Count);
            if (ds.Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Rows)
                {
                    list.Add(new Model.Entity.UserNotification
                    {
                        UserNotificationId = (int) (dr["UserNotificationId"]),
                        UserID = (int) (dr["UserID"]),
                        DeviceID = (int) (dr["DeviceID"]),
                        Type = (int) (dr["Type"]),
                        ObjectId = (int) (dr["ObjectId"]),
                        Get = dr["Get"] != DBNull.Value && (bool) (dr["Get"]),
                        Notification = dr["Notification"] != DBNull.Value && (bool) (dr["Notification"]),
                        CreateTime = (DateTime) (dr["CreateTime"]),
                        UpdateTime = (DateTime) (dr["UpdateTime"])
                    });
                }

                string ids = string.Join(",", list.ConvertAll(x => x.UserNotificationId));
                string delSql = "delete from [UserNotification] where UserNotificationId in (" + ids + ")";
                Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, delSql, null);
            }

            if (list.Count > 0)
                this.CleanNotificationTemp(userId);
            return list;
        }

        /// <summary>
        /// 推送系统消息
        /// </summary>
        /// <param name="notification">消息</param>
        /// <param name="users">用户列表</param>
        public void Send(Model.Entity.Notification notification, List<Model.Entity.User> users)
        {
            if (notification.NotificationID == 0)
                this.Save(notification);
            foreach (var user in users)
            {
                this.Send(notification, user);
            }
        }

        /// <summary>
        /// 推送系统消息
        /// </summary>
        /// <param name="notification">消息</param>
        /// <param name="user">用户</param>
        public void Send(Model.Entity.Notification notification, Model.Entity.User user)
        {
            if (notification.NotificationID == 0)
            {
                this.Save(notification);
            }

            if (user == null)
            {
                return;
            }

            var un = new Model.Entity.UserNotification
            {
                UserID = user.UserID,
                DeviceID = notification.DeviceID,
                Type = notification.DeviceID == 0 ? 5 : 1,
                ObjectId = notification.NotificationID,
                Get = false,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Notification = false
            };
            this.NewUserNotification(un);
            return;
#pragma warning disable CS0162 // 检测到无法访问的代码
            if (user.LoginType == 2 && !string.IsNullOrEmpty(user.AppID) && user.AppID.Length == 64 && user.Notification)
#pragma warning restore CS0162 // 检测到无法访问的代码
            {
                var getList = Logic.Notification.GetInstance().GetNotificationCount(user.UserID);
                var getTotal = getList.Sum(s => s.Message + s.Voice + s.SMS + s.Photo);
                YW.Notification.Notification alert = new YW.Notification.Notification(user.AppID);
                alert.Payload.Alert.Body = GetNotificationDescription(notification.Type,
                    notification);
                if (user.NotificationSound)
                    alert.Payload.Sound = "default";
                alert.Payload.CustomItems.Add("Content",
                    new object[]
                    {
                        notification.Type, notification.DeviceID, notification.Content
                    });
                alert.UserNotification = un;
                alert.Tag = notification;
                alert.Payload.Badge = getTotal;
                this.GetServer(user.Project).QueueNotification(alert);
            }
        }

        /// <summary>
        /// 推送更新通知
        /// </summary>
        /// <param name="deviceId">设备编号</param>
        /// <param name="type">类型</param>
        /// <param name="users">用户列表</param>
        public void Send(int deviceId, int type, List<Model.Entity.User> users)
        {
            foreach (var user in users)
            {
                this.Send(deviceId, type, user);
            }
        }

        /// <summary>
        /// 推送更新通知
        /// </summary>
        /// <param name="deviceId">设备编号</param>
        /// <param name="type">类型</param>
        /// <param name="user">用户</param>
        public void Send(int deviceId, int type, Model.Entity.User user)
        {
            if (user == null)
            {
                return;
            }

            var un = new Model.Entity.UserNotification
            {
                UserID = user.UserID,
                DeviceID = deviceId,
                Type = 6,
                ObjectId = type,
                Get = false,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Notification = false
            };
            this.NewUserNotification(un);
        }


        /// <summary>
        /// 推送短信
        /// </summary>
        /// <param name="sms">消息</param>
        /// <param name="users">用户列表</param>
        public void Send(Model.Entity.DeviceSMS sms, List<Model.Entity.User> users)
        {
            foreach (var user in users)
            {
                this.Send(sms, user);
            }
        }

        /// <summary>
        /// 推送短信
        /// </summary>
        /// <param name="sms">短信</param>
        /// <param name="user">用户</param>
        public void Send(Model.Entity.DeviceSMS sms, Model.Entity.User user)
        {
            if (user == null)
            {
                return;
            }

            Model.Entity.UserNotification un = new Model.Entity.UserNotification
            {
                UserID = user.UserID,
                Type = 3,
                DeviceID = sms.DeviceID,
                ObjectId = sms.DeviceSMSID,
                Get = false,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Notification = false
            };
            this.NewUserNotification(un);
            return;
#pragma warning disable CS0162 // 检测到无法访问的代码
            if (user.LoginType == 2 && !string.IsNullOrEmpty(user.AppID) && user.AppID.Length == 64 && user.Notification)
#pragma warning restore CS0162 // 检测到无法访问的代码
            {
                var getList = Logic.Notification.GetInstance().GetNotificationCount(user.UserID);
                var getTotal = getList.Sum(s => s.Message + s.Voice + s.SMS + s.Photo);
                YW.Notification.Notification alert = new YW.Notification.Notification(user.AppID);
                alert.Payload.Alert.Body = GetNotificationDescription(8,
                    sms);
                if (user.NotificationSound)
                    alert.Payload.Sound = "default";
                alert.Payload.CustomItems.Add("Content",
                    new object[]
                    {
                        8, sms.DeviceID, sms.Phone
                    });
                alert.UserNotification = un;
                alert.Payload.Badge = getTotal;
                this.GetServer(user.Project).QueueNotification(alert);
            }
        }

        /// <summary>
        /// 推送报警
        /// </summary>
        /// <param name="ex">报警</param>
        /// <param name="users">用户列表</param>
        public void Send(Model.Entity.DeviceException ex, List<Model.Entity.User> users)
        {
            foreach (var user in users)
            {
                this.Send(ex, user);
            }
        }

        /// <summary>
        /// 推送报警
        /// </summary>
        /// <param name="ex">报警</param>
        /// <param name="user">用户</param>
        public void Send(Model.Entity.DeviceException ex, Model.Entity.User user)
        {
            if (user == null)
            {
                return;
            }

            Model.Entity.UserNotification un = new Model.Entity.UserNotification
            {
                UserID = user.UserID,
                Type = 4,
                DeviceID = ex.DeviceID,
                ObjectId = ex.DeviceExceptionID,
                Get = false,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Notification = false
            };
            this.NewUserNotification(un);
            return;
#pragma warning disable CS0162 // 检测到无法访问的代码
            if (user.LoginType == 2 && !string.IsNullOrEmpty(user.AppID) && user.AppID.Length == 64 && user.Notification)
#pragma warning restore CS0162 // 检测到无法访问的代码
            {
                var getList = Logic.Notification.GetInstance().GetNotificationCount(user.UserID);
                var getTotal = getList.Sum(s => s.Message + s.Voice + s.SMS + s.Photo);
                YW.Notification.Notification alert = new YW.Notification.Notification(user.AppID);
                alert.Payload.Alert.Body = GetNotificationDescription(ex.Type,
                    ex);
                if (user.NotificationSound)
                    alert.Payload.Sound = "default";
                alert.Payload.CustomItems.Add("Content",
                    new object[]
                    {
                        ex.Type, ex.DeviceID, ""
                    });
                alert.UserNotification = un;
                alert.Payload.Badge = getTotal;
                this.GetServer(user.Project).QueueNotification(alert);
            }
        }

        /// <summary>
        /// 推送相片信息
        /// </summary>
        /// <param name="photo">语音</param>
        /// <param name="users">用户列表</param>
        public void Send(Model.Entity.DevicePhoto photo, List<Model.Entity.User> users)
        {
            foreach (var user in users)
            {
                this.Send(photo, user);
            }
        }

        /// <summary>
        /// 推送相片信息
        /// </summary>
        /// <param name="photo">语音</param>
        /// <param name="user">用户</param>
        public void Send(Model.Entity.DevicePhoto photo, Model.Entity.User user)
        {
            if (user == null)
            {
                return;
            }

            Model.Entity.UserNotification un = new Model.Entity.UserNotification
            {
                UserID = user.UserID,
                Type = 7,
                DeviceID = photo.DeviceID,
                ObjectId = photo.DevicePhotoId,
                Get = false,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Notification = false
            };
            this.NewUserNotification(un);
            return;
#pragma warning disable CS0162 // 检测到无法访问的代码
            if (!un.Notification && user.LoginType == 2 && !string.IsNullOrEmpty(user.AppID) && user.AppID.Length == 64 && user.Notification)
#pragma warning restore CS0162 // 检测到无法访问的代码
            {
                var getList = Logic.Notification.GetInstance().GetNotificationCount(user.UserID);
                var getTotal = getList.Sum(s => s.Message + s.Voice + s.SMS + s.Photo);
                YW.Notification.Notification alert = new YW.Notification.Notification(user.AppID);
                alert.Payload.Alert.Body = GetNotificationDescription(11, photo);
                if (user.NotificationSound)
                    alert.Payload.Sound = "default";
                alert.Payload.CustomItems.Add("Content",
                    new object[] {11, photo.DeviceID, ""});
                alert.UserNotification = un;
                alert.Payload.Badge = getTotal;
                this.GetServer(user.Project).QueueNotification(alert);
            }
        }

        /// <summary>
        /// 推送语音信息
        /// </summary>
        /// <param name="voice">语音</param>
        /// <param name="users">用户列表</param>
        public void Send(Model.Entity.DeviceVoice voice, List<Model.Entity.User> users)
        {
            foreach (var user in users)
            {
                this.Send(voice, user);
            }
        }

        /// <summary>
        /// 推送语音信息
        /// </summary>
        /// <param name="voice">语音</param>
        /// <param name="user">用户</param>
        public void Send(Model.Entity.DeviceVoice voice, Model.Entity.User user)
        {
            if (user == null||voice==null)
            {
                return;
            }

            Model.Entity.UserNotification un = new Model.Entity.UserNotification
            {
                UserID = user.UserID,
                Type = 2,
                DeviceID = voice.DeviceID,
                ObjectId = voice.DeviceVoiceId,
                Get = false,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Notification = false
            };
            if (voice.Type == 3 && user.UserID == voice.ObjectId) //如果是自己发的，则不通知自己
                un.Notification = true;
            this.NewUserNotification(un);
            return;
#pragma warning disable CS0162 // 检测到无法访问的代码
            if (!un.Notification && user.LoginType == 2 && !string.IsNullOrEmpty(user.AppID) && user.AppID.Length == 64 && user.Notification)
#pragma warning restore CS0162 // 检测到无法访问的代码
            {
                var getList = Logic.Notification.GetInstance().GetNotificationCount(user.UserID);
                var getTotal = getList.Sum(s => s.Message + s.Voice + s.SMS + s.Photo);
                YW.Notification.Notification alert = new YW.Notification.Notification(user.AppID);
                alert.Payload.Alert.Body = GetNotificationDescription(1, voice);
                if (user.NotificationSound)
                    alert.Payload.Sound = "default";
                alert.Payload.CustomItems.Add("Content",
                    new object[] {1, voice.DeviceID, ""});

                alert.UserNotification = un;
                alert.Payload.Badge = getTotal;
                this.GetServer(user.Project).QueueNotification(alert);
            }
        }

        public string GetNotificationDescription(int type, object content)
        {
            Model.Entity.Notification nf;
            Model.Entity.DeviceVoice voice;
            Model.Entity.DeviceSMS sms;
            Model.Entity.DeviceException exception;
            Model.Entity.Device device;
            Model.Entity.DevicePhoto photo;
            string str = null;
            switch (type)
            {
                case 1:
                    voice = content as Model.Entity.DeviceVoice;
                    try
                    {
                        if (voice.Type == 1 || voice.Type == 2)
                        {
                            device = Logic.Device.GetInstance().Get(voice.DeviceID);
                            if (voice.MsgType == 1)
                            {
                                if (!string.IsNullOrEmpty(device.BabyName))
                                    str = "收到来至" + device.BabyName + "的新消息";
                                else
                                    str = "收到来至设备的新消息";
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(device.BabyName))
                                    str = "收到来至" + device.BabyName + "的新语音";
                                else
                                    str = "收到来至设备的新语音";
                            }
                        }
                        else if (voice.Type == 3)
                        {
                            var fuser = Logic.User.GetInstance().Get(voice.ObjectId);

                            if (voice.MsgType == 1)
                            {
                                if (fuser != null && !string.IsNullOrEmpty(fuser.Name))
                                    str = "收到来至" + fuser.Name + "的新消息";
                                else
                                    str = "收到新的消息";
                            }
                            else
                            {
                                if (fuser != null && !string.IsNullOrEmpty(fuser.Name))
                                    str = "收到来至" + fuser.Name + "的新语音";
                                else
                                    str = "收到新的语音";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        str = "收到新的语音";
                        Data.Logger.Error(ex);
                        Data.Logger.Error(voice.DeviceVoiceId);
                    }

                    break;
                case 2:
                    nf = content as Model.Entity.Notification;
                    str = nf.Content.Split(',')[1] + "请求关联设备";
                    break;
                case 3:
                    nf = content as Model.Entity.Notification;
                    str = "管理员已经同意您关联该设备";
                    break;
                case 4:
                    nf = content as Model.Entity.Notification;
                    str = "管理员已经拒绝您关联该设备";
                    break;
                case 5:
                    nf = content as Model.Entity.Notification;
                    device = Logic.Device.GetInstance().Get(nf.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = device.BabyName + "升级成功";
                    else
                        str = "升级成功";
                    break;
                case 6:
                    nf = content as Model.Entity.Notification;
                    device = Logic.Device.GetInstance().Get(nf.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = device.BabyName + "配置已经同步";
                    else
                        str = "配置已经同步";
                    break;
                case 7:
                    nf = content as Model.Entity.Notification;
                    device = Logic.Device.GetInstance().Get(nf.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = device.BabyName + "通讯录已经同步";
                    else
                        str = "通讯录已经同步";
                    break;
                case 8:
                    sms = content as Model.Entity.DeviceSMS;
                    device = Logic.Device.GetInstance().Get(sms.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = device.BabyName + "收到来至" + sms.Phone + "的短信";
                    else
                        str = "收到来至" + sms.Phone + "的短信";
                    break;
                case 9:
                    nf = content as Model.Entity.Notification;
                    device = Logic.Device.GetInstance().Get(nf.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = "管理员已经解除您与设备" + device.BabyName + "的关联";
                    else
                        str = "管理员已经解除您的关联";
                    break;
                case 10:
                    nf = content as Model.Entity.Notification;
                    device = Logic.Device.GetInstance().Get(nf.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = device.BabyName + "信息已经修改";
                    else
                        str = "信息已经修改";
                    break;
                case 11:
                    photo = content as Model.Entity.DevicePhoto;
                    device = Logic.Device.GetInstance().Get(photo.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = "收到来至" + device.BabyName + "的新照片";
                    else
                        str = "收到新照片";
                    break;
                case 101:
                    exception = content as Model.Entity.DeviceException;
                    device = Logic.Device.GetInstance().Get(exception.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = device.BabyName + "产生脱落报警";
                    else
                        str = "产生脱落报警";
                    break;
                case 102:
                    exception = content as Model.Entity.DeviceException;
                    device = Logic.Device.GetInstance().Get(exception.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = device.BabyName + "产生进电子围栏[" + exception.Content + "]报警";
                    else
                        str = "产生进电子围栏[" + exception.Content + "]报警";
                    break;
                case 103:
                    exception = content as Model.Entity.DeviceException;
                    device = Logic.Device.GetInstance().Get(exception.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = device.BabyName + "产生出电子围栏[" + exception.Content + "]报警";
                    else
                        str = "产生出电子围栏[" + exception.Content + "]报警";
                    break;
                case 104:
                    exception = content as Model.Entity.DeviceException;
                    device = Logic.Device.GetInstance().Get(exception.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = device.BabyName + "产生低电报警";
                    else
                        str = "产生低电报警";
                    break;
                case 105:
                    exception = content as Model.Entity.DeviceException;
                    device = Logic.Device.GetInstance().Get(exception.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = device.BabyName + "产生呼救报警";
                    else
                        str = "产生呼救报警";
                    break;
                case 106:
                    exception = content as Model.Entity.DeviceException;
                    device = Logic.Device.GetInstance().Get(exception.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = device.BabyName + "进行过通话";
                    else
                        str = "设备进行过通话";
                    break;
                case 200:
                case 201:
                case 202:
                case 203:
                case 204:
                case 205:
                case 206:
                case 207:
                case 208:
                case 209:
                    nf = content as Model.Entity.Notification;
                    device = Logic.Device.GetInstance().Get(nf.DeviceID);
                    if (!string.IsNullOrEmpty(device.BabyName))
                        str = device.BabyName + " " + nf.Content + "";
                    else
                        str = nf.Content + "";
                    break;
                case 210:
                    nf = content as Model.Entity.Notification;
                    str = "最新公告";
                    break;
                case 230:
                case 231:
                case 232:
                case 233:
                    break;
            }

            return str;
        }

        /// <summary>
        /// 新的用户和消息关联信息
        /// </summary>
        /// <param name="un"></param>
        private void NewUserNotification(Model.Entity.UserNotification un)
        {
            DbParameter[] commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@UserID", DbType.Int32, un.UserID),
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, un.DeviceID),
                Data.DBHelper.CreateInDbParameter("@Type", DbType.Int32, un.Type),
                Data.DBHelper.CreateInDbParameter("@ObjectId", DbType.Int32, un.ObjectId),
                Data.DBHelper.CreateInDbParameter("@Get", DbType.Boolean, un.Get),
                Data.DBHelper.CreateInDbParameter("@Notification", DbType.Boolean, un.Notification),
                Data.DBHelper.CreateInDbParameter("@CreateTime", DbType.DateTime, un.CreateTime),
                Data.DBHelper.CreateInDbParameter("@UpdateTime", DbType.DateTime, un.UpdateTime)
            };
            const string sql =
                "Insert into [UserNotification] ([UserID],[DeviceID],[Type],[ObjectId],[Get],[Notification],[CreateTime],[UpdateTime]) values(@UserID,@DeviceID,@Type,@ObjectId,@Get,@Notification,@CreateTime,@UpdateTime)\n select @@IDENTITY as UserNotificationId";
            un.UserNotificationId = int.Parse(Data.DBHelper.GetInstance().ExecuteScalar(CommandType.Text, sql, commandParameters).ToString());
            this.CleanNotificationTemp(un.UserID);
        }

        /// <summary>
        /// 更新用户消息推送状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userNotificationId"></param>
        /// <param name="notification"></param>
        private void UpdateUserNotificationStatus(int userId, int userNotificationId, bool notification)
        {
            DbParameter[] commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@UserNotificationId", DbType.Int32, userNotificationId),
                Data.DBHelper.CreateInDbParameter("@Notification", DbType.Boolean, notification)
            };
            const string sql = "Update [UserNotification] set [Notification]=@Notification,[UpdateTime]=getdate() where [UserNotificationId]=@UserNotificationId";
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
            this.CleanNotificationTemp(userId);
        }

        private void service_BadDeviceToken(object sender, BadDeviceTokenException ex)
        {
            Console.WriteLine("Bad Device Token: {0}", ex.Message);
        }

        private void service_Disconnected(object sender)
        {
            Console.WriteLine("Disconnected...");
        }

        private void service_Connected(object sender)
        {
            Console.WriteLine("Connected...");
        }

        private void service_Connecting(object sender)
        {
            Console.WriteLine("Connecting...");
        }

        private void service_NotificationTooLong(object sender, YW.Notification.NotificationLengthException ex)
        {
            Console.WriteLine(string.Format("Notification Too Long: {0}", ex.Notification.ToString()));
        }

        private void service_NotificationSuccess(object sender, YW.Notification.Notification notification)
        {
            //如果是绑定相关，则不设置成推送成功
            if (notification.UserNotification.Type == 1)
            {
                Model.Entity.Notification item = (Model.Entity.Notification) notification.Tag;
                if (item.Type == 2 || item.Type == 3 || item.Type == 4 || item.Type == 9)
                    return;
            }

            UpdateUserNotificationStatus(notification.UserNotification.UserID, notification.UserNotification.UserNotificationId, true);
            Console.WriteLine(string.Format("Notification Success: {0}", notification.ToString()));
        }

        private void service_NotificationFailed(object sender, YW.Notification.Notification notification)
        {
            UpdateUserNotificationStatus(notification.UserNotification.UserID, notification.UserNotification.UserNotificationId, false);
            Console.WriteLine(string.Format("Notification Failed: {0}", notification.ToString()));
        }

        private void service_Error(object sender, Exception ex)
        {
            Console.WriteLine(string.Format("Error: {0}", ex.Message));
        }

        public void CleanByDeviceId(int deviceId)
        {
            const string sql = "delete from [UserNotification] where [UserNotification].[DeviceID]=@DeviceID\n" +
                               "delete from [Notification] where [Notification].[DeviceID]=@DeviceID";
            var commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId),
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
        }
    }
}