using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using YW.Model;

namespace YW.Logic
{
    public class DeviceException : Base
    {
        private static DeviceException _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<int, Model.Entity.DeviceException> _dictionaryById;
        public static DeviceException GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new DeviceException();
                    }
                }
            }
            return _object;
        }
        public DeviceException()
            : base(typeof(Model.Entity.DeviceException))
        {
            _dictionaryById = new Dictionary<int, Model.Entity.DeviceException>();
        }
        public Model.Entity.DeviceException Get(int objId)
        {
            Model.Entity.DeviceException obj;
            _dictionaryById.TryGetValue(objId, out obj);
            if (obj == null)
            {
                const string sql =
                    "select top 1 [DeviceException].* from [DeviceException] where [DeviceException].[DeviceExceptionID]=@DeviceExceptionID";
                DbParameter[] commandParameters = new DbParameter[]
                {
                    Data.DBHelper.CreateInDbParameter("@DeviceExceptionID", DbType.Int32, objId),
                };
                var ds = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql, commandParameters);
                var list = base.TableToList<Model.Entity.DeviceException>(ds);
                if (list.Count > 0)
                {
                    obj = list[0];
                    lock (_dictionaryById)
                    {
                        if (!_dictionaryById.ContainsKey(objId))
                            _dictionaryById.Add(obj.DeviceExceptionID, obj);
                    }

                }
            }
            return obj;
        }

        public void Save(Model.Entity.DeviceException obj)
        {
            base.Save(obj);
            if (obj.DeviceExceptionID != 0)
            {
                if (_dictionaryById.ContainsKey(obj.DeviceExceptionID))
                {
                    if (obj != _dictionaryById[obj.DeviceExceptionID])
                        base.CopyValue<Model.Entity.DeviceException>(obj, _dictionaryById[obj.DeviceExceptionID]);
                }
                else
                {
                    obj.CreateTime = DateTime.Now;
                    _dictionaryById.Add(obj.DeviceExceptionID, obj);
                }
            }
        }

        public void CleanByDeviceId(int deviceId)
        {
            const string sql ="delete from [DeviceException] where [DeviceException].[DeviceID]=@DeviceID";
            var commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId),
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
        }
    }
}
