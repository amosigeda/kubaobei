using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using YW.Data;
namespace YW.Logic
{
    public class DeviceSet : Base
    {
        private static DeviceSet _object;
        private static readonly object LockHelper = new object();
        private static ConcurrentDictionary<int, Model.Entity.DeviceSet> _dictionaryByDeviceId;

        public static DeviceSet GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new DeviceSet();
                    }
                }
            }
            return _object;
        }

        public DeviceSet()
            : base(typeof(Model.Entity.DeviceSet))
        {
            var list = base.Get<Model.Entity.DeviceSet>();
            _dictionaryByDeviceId = new ConcurrentDictionary<int, Model.Entity.DeviceSet>();
            foreach (var item in list)
            {
                _dictionaryByDeviceId.TryAdd(item.DeviceID, item);
            }
        }

        public Model.Entity.DeviceSet Get(int deviceId)
        {
            _dictionaryByDeviceId.TryGetValue(deviceId, out var obj);
            return obj;
        }

        public void SaveDeviceSet(Model.Entity.DeviceSet obj)
        {
            string sqlCommond = "insert into DeviceSet(DeviceID,SetInfo,ClassDisabled1,ClassDisabled2,WeekDisabled,TimerOpen,TimerClose,BrightScreen,CreateTime,UpdateTime)";
            sqlCommond += " values(@DeviceID,@SetInfo,@ClassDisabled1,@ClassDisabled2,@WeekDisabled,@TimerOpen,@TimerClose,@BrightScreen,@CreateTime,@UpdateTime)";
            var dp = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DeviceID",DbType.Int32,obj.DeviceID),
                Data.DBHelper.CreateInDbParameter("@SetInfo",DbType.String,obj.SetInfo),
                Data.DBHelper.CreateInDbParameter("@ClassDisabled1",DbType.String,obj.ClassDisabled1==null?DBNull.Value.ToString():obj.ClassDisabled1),
                Data.DBHelper.CreateInDbParameter("@ClassDisabled2",DbType.String,obj.ClassDisabled2==null?DBNull.Value.ToString():obj.ClassDisabled2),
                Data.DBHelper.CreateInDbParameter("@WeekDisabled",DbType.String,obj.WeekDisabled==null?DBNull.Value.ToString():obj.WeekDisabled),
                Data.DBHelper.CreateInDbParameter("@TimerOpen",DbType.String,obj.TimerOpen==null?DBNull.Value.ToString():obj.TimerOpen),
                Data.DBHelper.CreateInDbParameter("@TimerClose",DbType.String,obj.TimerClose==null?DBNull.Value.ToString():obj.TimerClose),
                Data.DBHelper.CreateInDbParameter("@BrightScreen",DbType.Int32,obj.BrightScreen),
                Data.DBHelper.CreateInDbParameter("@CreateTime",DbType.DateTime,obj.CreateTime),
                Data.DBHelper.CreateInDbParameter("@UpdateTime",DbType.DateTime,obj.UpdateTime)
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(sqlCommond,dp);
            if (obj.DeviceID != 0)
            {
                if (_dictionaryByDeviceId.ContainsKey(obj.DeviceID))
                {
                    obj.UpdateTime = DateTime.Now;
                    if (obj != _dictionaryByDeviceId[obj.DeviceID])
                        base.CopyValue<Model.Entity.DeviceSet>(obj, _dictionaryByDeviceId[obj.DeviceID]);
                }
                else
                {
                    obj.CreateTime = DateTime.Now;
                    obj.UpdateTime = DateTime.Now;
                    _dictionaryByDeviceId.TryAdd(obj.DeviceID, obj);
                }
            }
        }
        public new void Del(int objId)
        {
            const string sql = "delete from [DeviceSet] where [DeviceSet].[DeviceID]=@DeviceID";
            var commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, objId),
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);

                    _dictionaryByDeviceId.TryRemove(objId,out var res);

        }
        
        public void Save(Model.Entity.DeviceSet obj)
        {

            base.Save(obj);
            if (obj.DeviceID != 0)
            {
                if (_dictionaryByDeviceId.ContainsKey(obj.DeviceID))
                {
                    obj.UpdateTime = DateTime.Now;
                    if (obj != _dictionaryByDeviceId[obj.DeviceID])
                        base.CopyValue<Model.Entity.DeviceSet>(obj, _dictionaryByDeviceId[obj.DeviceID]);
                }
                else
                {
                    obj.CreateTime = DateTime.Now;
                    obj.UpdateTime = DateTime.Now;
                    _dictionaryByDeviceId.TryAdd(obj.DeviceID, obj);
                }
            }
        }

        public Model.Entity.DeviceSet CreateDefaultDeviceSet(int deviceId)
        {
            var deviceSet = new Model.Entity.DeviceSet()
            {
                DeviceID = deviceId,
                SetInfo = "1-1-1-1-0-0-0-0-1-0-1-0",
                BrightScreen = 10,
                ClassDisabled1 = "08:00-12:00",
                ClassDisabled2 = "14:00-17:00",
                TimerClose = "00:00",
                TimerOpen = "07:00",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };
            Save(deviceSet);
            return deviceSet;
        }
    }
}
