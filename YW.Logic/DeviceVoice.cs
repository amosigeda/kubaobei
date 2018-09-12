using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using YW.Data;

namespace YW.Logic
{
    public class DeviceVoice : Base
    {
        private static DeviceVoice _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<int, Model.Entity.DeviceVoice> _dictionaryById;
        public static DeviceVoice GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new DeviceVoice();
                    }
                }
            }
            return _object;
        }
        public DeviceVoice()
            : base(typeof(Model.Entity.DeviceVoice))
        {
            _dictionaryById = new Dictionary<int, Model.Entity.DeviceVoice>();
        }
        /// <summary>
        /// 获取未读语音信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public List<Model.Entity.DeviceVoice> GetList(int userId, int deviceId)
        {
            const string sql =
                "select dv.*  from [UserNotification] un left join [DeviceVoice] dv on un.ObjectId=dv.DeviceVoiceID where un.UserID=@UserId and un.DeviceID=@DeviceID and un.Get=0 and un.Type=2";
            DbParameter[] commandParameters = {
                DBHelper.CreateInDbParameter("@UserID", DbType.Int32, userId),
                DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId)
            };
            var ds = DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql, commandParameters);
            List<Model.Entity.DeviceVoice> list = null;
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                list = TableToList<Model.Entity.DeviceVoice>(ds);
                string ids = string.Join(",", list.ConvertAll(x => x.DeviceVoiceId.ToString()));
                string delSql = "delete from [UserNotification] where UserID=@UserId and ObjectId in (" + ids + ")";
                DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, delSql, commandParameters);
                Notification.GetInstance().CleanNotificationTemp(userId);
            }

            return list;
        }

        public Model.Entity.DeviceVoice Get(int objId)
        {
            Model.Entity.DeviceVoice obj;
            _dictionaryById.TryGetValue(objId, out obj);
            if (obj == null)
            {
                const string sql =
                    "select top 1 [DeviceVoice].* from [DeviceVoice] where [DeviceVoice].[DeviceVoiceId]=@DeviceVoiceId";
                DbParameter[] commandParameters = new DbParameter[]
                {
                    DBHelper.CreateInDbParameter("@DeviceVoiceId", DbType.Int32, objId),
                };
                var ds = DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql, commandParameters);
                var list = base.TableToList<Model.Entity.DeviceVoice>(ds);
                if (list.Count > 0)
                {
                    obj = list[0];
                    lock (_dictionaryById)
                    {
                        if (!_dictionaryById.ContainsKey(objId))
                            _dictionaryById.Add(obj.DeviceVoiceId, obj);
                    }

                }
            }
            return obj;
        }

        public void Save(Model.Entity.DeviceVoice obj)
        {
            base.Save(obj);
            if (obj.DeviceVoiceId != 0)
            {
                if (_dictionaryById.ContainsKey(obj.DeviceVoiceId))
                {
                    obj.UpdateTime = DateTime.Now;
                    if (obj != _dictionaryById[obj.DeviceVoiceId])
                        base.CopyValue<Model.Entity.DeviceVoice>(obj, _dictionaryById[obj.DeviceVoiceId]);
                }
                else
                {
                    obj.CreateTime = DateTime.Now;
                    obj.UpdateTime = DateTime.Now;
                    _dictionaryById.Add(obj.DeviceVoiceId, obj);
                }
            }
        }
        public Model.Entity.DeviceVoice GetByDeviceIdAndMark(int deviceId, string mark)
        {
            Model.Entity.DeviceVoice obj = new Model.Entity.DeviceVoice();
            const string sql =
                   "select top 1 [DeviceVoice].DeviceVoiceID from [DeviceVoice] where [DeviceVoice].[DeviceId]=@DeviceId and [DeviceVoice].[Mark]=@Mark and ([DeviceVoice].[Type]=2 or [DeviceVoice].[Type]=4)";
            DbParameter[] commandParameters = new DbParameter[]
                {
                    DBHelper.CreateInDbParameter("@DeviceId", DbType.Int32, deviceId),
                    DBHelper.CreateInDbParameter("@Mark", DbType.String, mark)
                };
            var deviceVoiceId = DBHelper.GetInstance().ExecuteScalar(CommandType.Text, sql, commandParameters);
            if (deviceVoiceId != null)
            {
                return this.Get((int)deviceVoiceId);
            }
            else
                return null;

        }
        public Model.Entity.DeviceVoice GetNewOne(int deviceId) //Get one Devices
        {
            //状态 0 正在接收 1接收完成 2正在发送 3 发送完成
            Model.Entity.DeviceVoice obj = null;
            const string sql =
                "SELECT Top 1 * FROM Devicevoice WHERE DeviceId = @DeviceId and (Type=3 or Type =4) AND (State=1 or State=2 or State=4) order by CreateTime asc";
            DbParameter[] commandParameters = new DbParameter[]
            {
                DBHelper.CreateInDbParameter("@DeviceId", DbType.Int32, deviceId),
            };
            var ds = DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql, commandParameters);
            var list = base.TableToList<Model.Entity.DeviceVoice>(ds);
            if (list.Count > 0)
            {
                obj = list[0];
            }
            return obj;
        }
        public void CleanByDeviceId(int deviceId)
        {
            const string sql = "delete from [DeviceVoice] where [DeviceVoice].[DeviceID]=@DeviceID";
            var commandParameters = new DbParameter[]
            {
                DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId),
            };
            DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
        }
    }
}
