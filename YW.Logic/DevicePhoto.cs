using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace YW.Logic
{
    public class DevicePhoto : Base
    {
        private static DevicePhoto _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<int, Model.Entity.DevicePhoto> _dictionaryById;
        public static DevicePhoto GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new DevicePhoto();
                    }
                }
            }
            return _object;
        }
        public DevicePhoto()
            : base(typeof(Model.Entity.DevicePhoto))
        {
            _dictionaryById = new Dictionary<int, Model.Entity.DevicePhoto>();
        }
        /// <summary>
        /// 获取未读照片信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public List<Model.Entity.DevicePhoto> GetList(int userId, int deviceId)
        {
            const string sql =
                "declare @list table(UserNotificationId int,DevicePhotoID int)\n" +
                "insert into @list\n" +
                "select [UserNotification].UserNotificationId,[UserNotification].ObjectId from [UserNotification] where [UserNotification].UserId=@UserId and [UserNotification].Get=0 and [UserNotification].Type=7 and [UserNotification].DeviceID=@DeviceID\n" +
                "select [DevicePhoto].* from [DevicePhoto] inner join @list list on list.DevicePhotoID=[DevicePhoto].DevicePhotoID\n" +
                "delete from [UserNotification] from [UserNotification] un inner join @list list on list.UserNotificationId=un.UserNotificationId\n";
            DbParameter[] commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@UserID", DbType.Int32, userId),
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId)
            };
            var ds = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql, commandParameters);
            if(ds.Tables.Count>0&&ds.Tables[0].Rows.Count>0)
                Notification.GetInstance().CleanNotificationTemp(userId);
            return base.TableToList<Model.Entity.DevicePhoto>(ds);
        }

        public Model.Entity.DevicePhoto Get(int objId)
        {
            Model.Entity.DevicePhoto obj;
            _dictionaryById.TryGetValue(objId, out obj);
            if (obj == null)
            {
                const string sql =
                    "select top 1 [DevicePhoto].* from [DevicePhoto] where [DevicePhoto].[DevicePhotoId]=@DevicePhotoId";
                DbParameter[] commandParameters = new DbParameter[]
                {
                    Data.DBHelper.CreateInDbParameter("@DevicePhotoId", DbType.Int32, objId),
                };
                var ds = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql, commandParameters);
                var list = base.TableToList<Model.Entity.DevicePhoto>(ds);
                if (list.Count > 0)
                {
                    obj = list[0];
                    lock (_dictionaryById)
                    {
                        if (!_dictionaryById.ContainsKey(objId))
                            _dictionaryById.Add(obj.DevicePhotoId, obj);
                    }

                }
            }
            return obj;
        }

        public void Save(Model.Entity.DevicePhoto obj)
        {
            base.Save(obj);
            if (obj.DevicePhotoId != 0)
            {
                lock (_dictionaryById)
                {
                    if (_dictionaryById.ContainsKey(obj.DevicePhotoId))
                    {
                        obj.UpdateTime = DateTime.Now;
                        if (obj != _dictionaryById[obj.DevicePhotoId])
                            base.CopyValue<Model.Entity.DevicePhoto>(obj, _dictionaryById[obj.DevicePhotoId]);
                    }
                    else
                    {
                        obj.CreateTime = DateTime.Now;
                        obj.UpdateTime = DateTime.Now;
                        _dictionaryById.Add(obj.DevicePhotoId, obj);
                    }
                }
            }
        }
        public Model.Entity.DevicePhoto GetByDeviceIdAndMark(int deviceId, string mark)
        {
            Model.Entity.DevicePhoto obj = new Model.Entity.DevicePhoto();
            const string sql =
                   "select top 1 [DevicePhoto].DevicePhotoID from [DevicePhoto] where [DevicePhoto].[DeviceId]=@DeviceId and [DevicePhoto].[Mark]=@Mark";
            DbParameter[] commandParameters = new DbParameter[]
                {
                    Data.DBHelper.CreateInDbParameter("@DeviceId", DbType.Int32, deviceId),
                    Data.DBHelper.CreateInDbParameter("@Mark", DbType.String, mark)
                };
            var devicePhotoId = Data.DBHelper.GetInstance().ExecuteScalar(CommandType.Text, sql, commandParameters);
            if (devicePhotoId != null)
            {
                return this.Get((int)devicePhotoId);
            }
            else
                return null;
        }
        public void DelByDevicePhotoId(int DevicePhotoID)
        {
            const string sql = "delete from [DevicePhoto] where [DevicePhoto].[DevicePhotoId]=@DevicePhotoId ";
            var commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DevicePhotoId", DbType.Int32, DevicePhotoID),
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);

            lock (_dictionaryById)
            {
                if (!_dictionaryById.ContainsKey(DevicePhotoID))
                    _dictionaryById.Remove(DevicePhotoID);
            }
        }

        public void CleanByDeviceId(int deviceId)
        {
            const string sql = "delete from [DevicePhoto] where [DevicePhoto].[DeviceID]=@DeviceID";
            var commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId),
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
        }

    }
}
