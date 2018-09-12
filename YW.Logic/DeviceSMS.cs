using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
namespace YW.Logic
{
    public class DeviceSMS:Base
    {
        private static DeviceSMS _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<int, Model.Entity.DeviceSMS> _dictionaryById;
        public static DeviceSMS GetInstance()
        {
            if (_object==null)
            {
                lock (LockHelper)
                {
                    if (_object==null)
                    {
                        _object = new DeviceSMS();
                    }
                }
            }
            return _object;
        }
        public DeviceSMS()
            : base(typeof(Model.Entity.DeviceSMS))
        {
            _dictionaryById = new Dictionary<int, Model.Entity.DeviceSMS>();
        }

        /// <summary>
        /// 获取未获取过的设备收到的短信
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public List<Model.Entity.DeviceSMS> GetList(int userId,int deviceId)
        {
            const string sql =
                "declare @list table(UserNotificationId int,DeviceSMSID int)\n" +
                "insert into @list\n" +
                "select [UserNotification].UserNotificationId,[UserNotification].ObjectId from [UserNotification] where [UserNotification].UserId=@UserId and [UserNotification].Get=0  and [UserNotification].Type=3 and [UserNotification].DeviceID=@DeviceID\n" +
                "select [DeviceSMS].* from [DeviceSMS] inner join @list list on list.DeviceSMSID=[DeviceSMS].DeviceSMSID and [DeviceSMS].Type=2\n" +
                "delete from [UserNotification] from [UserNotification] un inner join @list list on list.UserNotificationId=un.UserNotificationId\n";
            DbParameter[] dp = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@UserId",DbType.Int32,userId),
                Data.DBHelper.CreateInDbParameter("@DeviceID",DbType.Int32,deviceId)
            };
            DataSet ds = Data.DBHelper.GetInstance().ExecuteAdapter(sql, dp);
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                Notification.GetInstance().CleanNotificationTemp(userId);
            return base.TableToList<Model.Entity.DeviceSMS>(ds);
        }
        /// <summary>
        /// 获取一个需要发送的短信
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public Model.Entity.DeviceSMS GetOne(int deviceId)
        {
            Model.Entity.DeviceSMS deviceSms = null;
            const string sqlCommond =
                "select Top 1 * from DeviceSMS where DeviceID=@deviceId and Type=1 and (State=0 or (State=1 and datediff(s,UpdateTime,getdate())>30)) order by DeviceSMSID asc";
            DbParameter[] dp = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@deviceId", DbType.Int32, deviceId)
            };
            DataSet ds = Data.DBHelper.GetInstance().ExecuteAdapter(sqlCommond, dp);
            var list = base.TableToList<Model.Entity.DeviceSMS>(ds);
            if (list.Count > 0)
            {
                deviceSms = list[0];
            }
            return deviceSms;
        }

        public Model.Entity.DeviceSMS Get(int objId)
        {
            Model.Entity.DeviceSMS obj;
            _dictionaryById.TryGetValue(objId, out obj);
            if (obj == null)
            {
                const string sql =
                    "select top 1 [DeviceSMS].* from [DeviceSMS] where [DeviceSMS].[DeviceSMSID]=@DeviceSMSID";
                DbParameter[] commandParameters = new DbParameter[]
                {
                    Data.DBHelper.CreateInDbParameter("@DeviceSMSID", DbType.Int32, objId),
                };
                var ds = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql, commandParameters);
                var list = base.TableToList<Model.Entity.DeviceSMS>(ds);
                if (list.Count > 0)
                {
                    obj = list[0];
                    lock (_dictionaryById)
                    {
                        if (!_dictionaryById.ContainsKey(objId))
                            _dictionaryById.Add(obj.DeviceSMSID, obj);
                    }

                }
            }
            return obj;
        }

        public void Save(Model.Entity.DeviceSMS obj)
        {
            base.Save(obj);
            if (obj.DeviceSMSID != 0)
            {
                if (_dictionaryById.ContainsKey(obj.DeviceSMSID))
                {
                    obj.UpdateTime = DateTime.Now;
                    if (obj != _dictionaryById[obj.DeviceSMSID])
                        base.CopyValue<Model.Entity.DeviceException>(obj, _dictionaryById[obj.DeviceSMSID]);
                }
                else
                {
                    obj.CreateTime = DateTime.Now;
                    obj.UpdateTime = DateTime.Now;
                    _dictionaryById.Add(obj.DeviceSMSID, obj);
                }
            }
        }
        public void CleanByDeviceId(int deviceId)
        {
            const string sql = "delete from [DeviceSMS] where [DeviceSMS].[DeviceID]=@DeviceID";
            var commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId),
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
        }
    }
}
