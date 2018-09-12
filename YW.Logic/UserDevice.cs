using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Deserializers;
using YW.Data;
using YW.Model.Entity;
using YW.Utility;

namespace YW.Logic
{
    public class UserDevice : Base
    {
        private static UserDevice _object;
        private static readonly object LockHelper = new object();
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, Model.Entity.UserDevice>> _dictionaryByDevice;
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, Model.Entity.UserDevice>> _dictionaryByUser;

        public static UserDevice GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new UserDevice();
                    }
                }
            }

            return _object;
        }

        public UserDevice() : base(typeof(Model.Entity.UserDevice))
        {
            var list = base.Get<Model.Entity.UserDevice>();
            _dictionaryByDevice = new ConcurrentDictionary<int, ConcurrentDictionary<int, Model.Entity.UserDevice>>();
            _dictionaryByUser = new ConcurrentDictionary<int, ConcurrentDictionary<int, Model.Entity.UserDevice>>();

            foreach (var item in list)
            {
                if (!_dictionaryByDevice.TryGetValue(item.DeviceID, out var dictUser))
                {
                    dictUser = new ConcurrentDictionary<int, Model.Entity.UserDevice>();
                    _dictionaryByDevice.TryAdd(item.DeviceID, dictUser);
                }

                dictUser.TryAdd(item.UserID, item);

                if (!_dictionaryByUser.TryGetValue(item.UserID, out var dictDevice))
                {
                    dictDevice = new ConcurrentDictionary<int, Model.Entity.UserDevice>();
                    _dictionaryByUser.TryAdd(item.UserID, dictDevice);
                }

                dictDevice.TryAdd(item.DeviceID, item);
            }
        }

        public bool IsUserDeviceExists(int userId, int deviceId)
        {
            _dictionaryByUser.TryGetValue(userId, out var dictDevice);
            if (dictDevice == null || dictDevice.Count == 0)
            {
                return false;
            }

            dictDevice.TryGetValue(deviceId, out var ud);
            return ud != null;
        }

        public ConcurrentDictionary<int, Model.Entity.UserDevice> GetByDeviceId(int objId)
        {
            _dictionaryByDevice.TryGetValue(objId, out var obj);
            return obj ?? new ConcurrentDictionary<int, Model.Entity.UserDevice>();
        }

        public ConcurrentDictionary<int, Model.Entity.UserDevice> GetByUserId(int objId)
        {
            _dictionaryByUser.TryGetValue(objId, out var obj);
            return obj ?? new ConcurrentDictionary<int, Model.Entity.UserDevice>();
        }

        public List<Model.Entity.User> GetUserByDeviceId(int objId)
        {
            _dictionaryByDevice.TryGetValue(objId, out var ud);
            var res = ud?.Select(x => User.GetInstance().Get(x.Value.UserID)).Where(x => x != null && !x.Deleted);
            return res != null ? res.ToList() : new List<Model.Entity.User>();
        }

        public List<Model.Entity.Device> GetDeviceByUserId(int objId)
        {
            _dictionaryByUser.TryGetValue(objId, out var uds);
            var res = uds?.Select(x => Device.GetInstance().Get(x.Value.DeviceID)).Where(x => x != null && !x.Deleted);
            return res != null ? res.ToList() : new List<Model.Entity.Device>();
        }

        public void New(Model.Entity.UserDevice obj)
        {
            const string sqlCommond = "Insert into UserDevice (UserID,DeviceID,Status,CreateTime,UpdateTime) values (@UserID,@DeviceID,@Status,@CreateTime,@UpdateTime)";
            var dp = new DbParameter[]
            {
                DBHelper.CreateInDbParameter("@UserID", DbType.Int32, obj.UserID),
                DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, obj.DeviceID),
                DBHelper.CreateInDbParameter("@Status", DbType.Int32, obj.Status),
                DBHelper.CreateInDbParameter("@CreateTime", DbType.DateTime, obj.CreateTime),
                DBHelper.CreateInDbParameter("@UpdateTime", DbType.DateTime, obj.UpdateTime)
            };

            DBHelper.GetInstance().ExecuteNonQuery(sqlCommond, dp);

            if (!_dictionaryByDevice.TryGetValue(obj.DeviceID, out var dictUser))
            {
                dictUser = new ConcurrentDictionary<int, Model.Entity.UserDevice>();
                _dictionaryByDevice.TryAdd(obj.DeviceID, dictUser);
            }

            dictUser.TryAdd(obj.UserID, obj);
            if (!_dictionaryByUser.TryGetValue(obj.UserID, out var dictDevice))
            {
                dictDevice = new ConcurrentDictionary<int, Model.Entity.UserDevice>();
                _dictionaryByUser.TryAdd(obj.UserID, dictDevice);
            }

            dictDevice.TryAdd(obj.DeviceID, obj);
        }

        public void Update(int userId, int deviceId, int status)
        {
            if (_dictionaryByUser.TryGetValue(userId, out var dictDevice))
            {
                if (dictDevice.TryGetValue(deviceId, out var item))
                {
                    item.UpdateTime = DateTime.Now;
                    item.Status = status;
                    const string sqlCommond = "Update UserDevice set Status=@Status,UpdateTime=getdate() where UserID=@UserID and DeviceID=@DeviceID";
                    var dp = new[]
                    {
                        DBHelper.CreateInDbParameter("@UserID", DbType.Int32, userId),
                        DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId),
                        DBHelper.CreateInDbParameter("@Status", DbType.Int32, status)
                    };
                    DBHelper.GetInstance().ExecuteNonQuery(sqlCommond, dp);
                }
            }
        }

        public void DelUser(int userId)
        {
            if (_dictionaryByUser.TryGetValue(userId, out var dictDevice))
            {
                _dictionaryByUser.TryRemove(userId, out var res);
                foreach (var item in dictDevice)
                {
                    if (_dictionaryByDevice.TryGetValue(item.Value.DeviceID, out var dictUserDevice))
                    {
                        dictUserDevice.TryRemove(userId, out var res1);
                    }

                    const string sqlCommond = "Delete from UserDevice where UserID=@UserID and DeviceID=@DeviceID";
                    var dp = new DbParameter[]
                    {
                        DBHelper.CreateInDbParameter("@UserID", DbType.Int32, userId),
                        DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, item.Value.UserID)
                    };
                    DBHelper.GetInstance().ExecuteNonQuery(sqlCommond, dp);
                }
            }
        }

        public void DelDevice(int deviceId)
        {
            if (_dictionaryByDevice.TryGetValue(deviceId, out var dictUser))
            {
                _dictionaryByDevice.TryRemove(deviceId, out var res);
                foreach (var item in dictUser)
                {
                    if (_dictionaryByUser.TryGetValue(item.Value.UserID, out var dictUserDevice))
                    {
                        dictUserDevice.TryRemove(deviceId, out var res1);
                    }

                    const string sqlCommond = "Delete from UserDevice where UserID=@UserID and DeviceID=@DeviceID";
                    var dp = new DbParameter[]
                    {
                        DBHelper.CreateInDbParameter("@UserID", DbType.Int32, item.Value.UserID),
                        DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId)
                    };
                    DBHelper.GetInstance().ExecuteNonQuery(sqlCommond, dp);
                }
            }
        }

        public void Del(int userId, int deviceId)
        {
            const string sqlCommond = "Delete from UserDevice where UserID=@UserID and DeviceID=@DeviceID";
            var dp = new DbParameter[]
            {
                DBHelper.CreateInDbParameter("@UserID", DbType.Int32, userId),
                DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId)
            };
            DBHelper.GetInstance().ExecuteNonQuery(sqlCommond, dp);
            ConcurrentDictionary<int, Model.Entity.UserDevice> dictDevice;
            if (
                _dictionaryByUser.TryGetValue(userId, out dictDevice))
            {
                if (dictDevice.ContainsKey(deviceId))
                {
                    dictDevice.TryRemove(deviceId, out var res);
                }
            }

            ConcurrentDictionary<int, Model.Entity.UserDevice> dictUser;
            if (
                _dictionaryByDevice.TryGetValue(deviceId, out dictUser))
            {
                if (dictUser.ContainsKey(userId))
                {
                    dictUser.TryRemove(userId, out var res);
                }
            }
        }
    }
}