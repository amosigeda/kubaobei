using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using YW.Model;

namespace YW.Logic
{
    public class DeviceFriend : Base
    {
        private static DeviceFriend _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<int, Model.Entity.DeviceFriend> _dictionaryByDeviceFriendId;
        private readonly Dictionary<int, Dictionary<int, Model.Entity.DeviceFriend>> _dictionaryByDeviceId;

        public static DeviceFriend GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new DeviceFriend();
                    }
                }
            }

            return _object;
        }

        public DeviceFriend()
            : base(typeof(Model.Entity.DeviceFriend))
        {
            var list = base.Get<Model.Entity.DeviceFriend>();
            _dictionaryByDeviceFriendId = new Dictionary<int, Model.Entity.DeviceFriend>();
            _dictionaryByDeviceId = new Dictionary<int, Dictionary<int, Model.Entity.DeviceFriend>>();

            foreach (var item in list)
            {
                _dictionaryByDeviceFriendId.Add(item.DeviceFriendId, item);
                Dictionary<int, Model.Entity.DeviceFriend> listByDeviceId;
                if (!_dictionaryByDeviceId.TryGetValue(item.DeviceID, out listByDeviceId))
                {
                    listByDeviceId = new Dictionary<int, Model.Entity.DeviceFriend>();
                    _dictionaryByDeviceId.Add(item.DeviceID, listByDeviceId);
                }

                listByDeviceId.Add(item.DeviceFriendId, item);
            }
        }

        public Model.Entity.DeviceFriend Get(int objId)
        {
            Model.Entity.DeviceFriend obj;
            _dictionaryByDeviceFriendId.TryGetValue(objId, out obj);
            return obj;
        }

        public List<Model.Entity.DeviceFriend> GetByDeviceId(int objId) //取好友列表
        {
            Dictionary<int, Model.Entity.DeviceFriend> listByDeviceId;
            _dictionaryByDeviceId.TryGetValue(objId, out listByDeviceId);
            return listByDeviceId == null ? new List<Model.Entity.DeviceFriend>() : listByDeviceId.Values.OrderBy(o => o.DeviceFriendId).ToList();
        }


        public void Save(Model.Entity.DeviceFriend obj) ///new insert, call twice when used
        {
            obj.UpdateTime = DateTime.Now;
            base.Save(obj);
            if (obj.DeviceFriendId != 0)
            {
                if (_dictionaryByDeviceFriendId.ContainsKey(obj.DeviceFriendId))
                {
                    if (obj != _dictionaryByDeviceFriendId[obj.DeviceFriendId])
                    {
                        base.CopyValue<Model.Entity.DeviceFriend>(obj, _dictionaryByDeviceFriendId[obj.DeviceFriendId]);
                    }

                    Dictionary<int, Model.Entity.DeviceFriend> listByDeviceId;
                    if (_dictionaryByDeviceId.TryGetValue(obj.DeviceID, out listByDeviceId))
                    {
                        foreach (var item in listByDeviceId)
                        {
                            if (item.Value.DeviceFriendId == obj.DeviceFriendId)
                            {
                                _dictionaryByDeviceId[item.Value.DeviceID].Remove(item.Value.DeviceFriendId);
                                _dictionaryByDeviceId[item.Value.DeviceID].Add(item.Value.DeviceFriendId, obj);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    obj.CreateTime = DateTime.Now;
                    obj.UpdateTime = DateTime.Now;
                    _dictionaryByDeviceFriendId.Add(obj.DeviceFriendId, obj);
                    Dictionary<int, Model.Entity.DeviceFriend> listByDeviceId;
                    if (!_dictionaryByDeviceId.TryGetValue(obj.DeviceID, out listByDeviceId))
                    {
                        listByDeviceId = new Dictionary<int, Model.Entity.DeviceFriend>();
                        _dictionaryByDeviceId.Add(obj.DeviceID, listByDeviceId);
                    }

                    listByDeviceId.Add(obj.DeviceFriendId, obj);
                }
            }
        }

        public void CleanByDeviceId(int deviceId, int friendId) ///删除好友
        {
            const string sql = "delete from [DeviceFriend] where [DeviceFriend].[DeviceID]=@DeviceID and [DeviceFriend].[ObjectId]=@DeviceFriendID ";
            var commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId),
                Data.DBHelper.CreateInDbParameter("@DeviceFriendID", DbType.Int32, friendId)
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);

            CleanByFriendId(deviceId, friendId);

            lock (_dictionaryByDeviceId)
            {
                _dictionaryByDeviceId.Where(x => x.Key == deviceId).ToList().ForEach(x =>
                {
                    var vs = x.Value;
                    vs.Values.Where(k => k.ObjectId == friendId).ToList().ForEach(s => vs.Remove(s.DeviceFriendId));
                });
                _dictionaryByDeviceFriendId.Where(x => x.Key == friendId && x.Value.DeviceID == deviceId).ToList().ForEach(x => _dictionaryByDeviceFriendId.Remove(x.Key));
            }
        }

        public void CleanByFriendId(int friendId, int deviceId) ///被删除好友  内部调用
        {
            const string sql = "delete from [DeviceFriend] where [DeviceFriend].[DeviceID]=@DeviceID and [DeviceFriend].[ObjectId]=@ObjectId ";
            var commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, friendId),
                Data.DBHelper.CreateInDbParameter("@ObjectId", DbType.Int32, deviceId)
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);

            lock (_dictionaryByDeviceId)
            {
                _dictionaryByDeviceId.Where(x => x.Key == deviceId).ToList().ForEach(x =>
                {
                    var vs = x.Value;
                    vs.Values.Where(k => k.ObjectId == friendId).ToList().ForEach(s => vs.Remove(s.DeviceFriendId));
                });
                _dictionaryByDeviceFriendId.Where(x => x.Key == friendId && x.Value.DeviceID == deviceId).ToList().ForEach(x => _dictionaryByDeviceFriendId.Remove(x.Key));
            }
        }


        public void DelDevice(int deviceId) ///删除行
        {
            const string sql = "delete from [DeviceFriend] where [DeviceFriend].[DeviceID]=@DeviceID or [DeviceFriend].[ObjectId]=@DeviceID ";
            var commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId),
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);


            lock (_dictionaryByDeviceId)
            {
                _dictionaryByDeviceId.Where(x => x.Key == deviceId).ToList().ForEach(x => _dictionaryByDeviceId.Remove(x.Key));
                _dictionaryByDeviceFriendId.Where(x => x.Value.DeviceID == deviceId).ToList().ForEach(x => _dictionaryByDeviceFriendId.Remove(x.Key));
            }
        }
    }
}