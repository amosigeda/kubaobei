using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace YW.Logic
{
    public class GeoFence:Base
    {
        private static GeoFence _object;
        private static readonly object LockHelper = new object();
        private static Dictionary<int, Model.Entity.GeoFence> _dictionary;
        private static Dictionary<int, Dictionary<int, Model.Entity.GeoFence>> _dictionaryByDeviceId;
        public static GeoFence GetInstence()
        {
            if (_object==null)
            {
                lock (LockHelper)
                {
                    if (_object==null)
                    {
                        _object = new GeoFence();
                    }
                }
            }
            return _object;
        }
        public GeoFence()
            : base(typeof(Model.Entity.GeoFence))
        {
            _dictionaryByDeviceId = new Dictionary<int, Dictionary<int, Model.Entity.GeoFence>>();
            _dictionary = new Dictionary<int, Model.Entity.GeoFence>();
            var list = base.Get<Model.Entity.GeoFence>();
            foreach (var item in list)
            {
                    _dictionary.Add(item.GeofenceID, item);
                    Dictionary<int, Model.Entity.GeoFence> listByDeviceId;
                    if (!_dictionaryByDeviceId.TryGetValue(item.DeviceID,out listByDeviceId))
                    {
                        listByDeviceId = new Dictionary<int, Model.Entity.GeoFence>();
                        _dictionaryByDeviceId.Add(item.DeviceID, listByDeviceId);
                    }
                    listByDeviceId.Add(item.GeofenceID,item);
            }
        }

        public List<Model.Entity.GeoFence> GetGeoFenceByDeviceId(int deviceId)
        {
            lock (_dictionaryByDeviceId)
            {
                Dictionary<int, Model.Entity.GeoFence> listByDeviceId;
                if(_dictionaryByDeviceId.TryGetValue(deviceId, out listByDeviceId))
                    return listByDeviceId.Values.OrderBy(o => o.GeofenceID).ToList();
                else
                    return null;
            }
        }

        public void SaveGeoFence(Model.Entity.GeoFence obj)
        {
            base.Save(obj);
            if (obj.GeofenceID!=0)
            {
                if (_dictionary.ContainsKey(obj.GeofenceID))
                {
                    
                    if (_dictionary[obj.GeofenceID]!=obj)
                    {
                        obj.UpdateTime = DateTime.Now;
                        base.CopyValue<Model.Entity.GeoFence>(obj,_dictionary[obj.GeofenceID]);
                    }

                }
                else
                {
                    obj.UpdateTime = DateTime.Now;
                    _dictionary.Add(obj.GeofenceID, obj);
                     Dictionary<int,Model.Entity.GeoFence> listGeoFence;
                     if (!_dictionaryByDeviceId.TryGetValue(obj.DeviceID, out listGeoFence))
                     {
                         listGeoFence = new Dictionary<int, Model.Entity.GeoFence>();
                         _dictionaryByDeviceId.Add(obj.DeviceID, listGeoFence);
                     }
                     listGeoFence.Add(obj.GeofenceID, obj);
                }
            }
        }

        public void DleteGeoFenceById(int geoFenceId)
        {
            base.Del(geoFenceId);
            lock (_dictionary)
            {
                if (_dictionary.ContainsKey(geoFenceId))
                {
                    if (_dictionaryByDeviceId.ContainsKey(_dictionary[geoFenceId].DeviceID))
                    {
                        _dictionaryByDeviceId[_dictionary[geoFenceId].DeviceID].Remove(geoFenceId);
                    }
                    _dictionary.Remove(geoFenceId);
                } 
            }

        }
        public void CleanByDeviceId(int deviceId)
        {
            const string sql = "delete from [GeoFence] where [GeoFence].[DeviceID]=@DeviceID";
            var commandParameters = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId),
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
            lock (_dictionaryByDeviceId)
            {
                Dictionary<int, Model.Entity.GeoFence> listByDeviceId;
                if (_dictionaryByDeviceId.TryGetValue(deviceId, out listByDeviceId))
                {
                    _dictionaryByDeviceId.Remove(deviceId);
                    lock (_dictionary)
                    {
                        foreach (var item in listByDeviceId)
                        {
                            _dictionary.Remove(item.Value.GeofenceID);
                        }
                    }
                }
                
            }
        }
    }
}
