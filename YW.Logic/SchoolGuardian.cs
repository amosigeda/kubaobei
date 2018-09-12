using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YW.Model.Entity;
using System.Data;
using System.Data.Common;
using YW.Model;

namespace YW.Logic
{
    public class SchoolGuardian : Base
    {
        private static SchoolGuardian _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<int, Dictionary<string,Model.Entity.SchoolGuardian>> _dictionaryByDevice;
        public static SchoolGuardian GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new SchoolGuardian();
                    }
                }
            }
            return _object;
        }
        public SchoolGuardian()
            : base(typeof(Model.Entity.SchoolGuardian))
        {
            _dictionaryByDevice = new Dictionary<int,Dictionary<string,Model.Entity.SchoolGuardian>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId">设备号</param>
        /// <param name="day">日期</param>
        /// <returns></returns>
        public Model.Entity.SchoolGuardian Get(int deviceId, string day)
        {
            Dictionary<string, Model.Entity.SchoolGuardian> dict;
            lock (_dictionaryByDevice)
            {
                _dictionaryByDevice.TryGetValue(deviceId, out dict);
                if (dict == null)
                {
                    dict = new Dictionary<string, Model.Entity.SchoolGuardian>();
                    _dictionaryByDevice.Add(deviceId, dict);
                }
            }
            Model.Entity.SchoolGuardian schoolGuardian;
            lock (dict)
            {
                dict.TryGetValue(day, out schoolGuardian);
                if (schoolGuardian == null)
                {
                    const string sqlCommond =
                        "select Top 1 * from SchoolGuardian where DeviceID=@DeviceID and SchoolDay=@SchoolDay";
                    DbParameter[] dp = new DbParameter[]
                    {
                        Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId),
                        Data.DBHelper.CreateInDbParameter("@SchoolDay", DbType.String, day)
                    };
                    DataSet ds = Data.DBHelper.GetInstance().ExecuteAdapter(sqlCommond, dp);
                    var list = base.TableToList<Model.Entity.SchoolGuardian>(ds);
                    if (list.Count > 0)
                    {
                        schoolGuardian = list[0];
                    }
                    else
                    {
                        schoolGuardian=new Model.Entity.SchoolGuardian {DeviceID = deviceId, SchoolDay = day};
                    }
                    dict.Add(day, schoolGuardian);
                }
            }
            return schoolGuardian;
        }

        public void Save(Model.Entity.SchoolGuardian obj)
        {
            base.Save(obj);
        }
        public void CleanByDeviceId(int deviceId)
        {
            const string sql = "delete from [SchoolGuardian] where [SchoolGuardian].[DeviceID]=@DeviceID";
            var commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId),
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
            lock (_dictionaryByDevice)
            {
                if (_dictionaryByDevice.ContainsKey(deviceId))
                    _dictionaryByDevice.Remove(deviceId);
            }
        }
    }
}
