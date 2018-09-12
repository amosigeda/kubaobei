using System;
using System.Collections.Generic;
using System.Data;

namespace YW.Logic
{
    public class DeviceState
    {
        private static DeviceState _object;
        private static readonly object LockHelper = new object();
        private readonly DataTable _dt;
        private readonly Dictionary<int, Model.Entity.DeviceState> _dictionary;
        private readonly bool _saveState;

        public static DeviceState GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new DeviceState();
                    }
                }
            }

            return _object;
        }

        public DeviceState()
        {
            _dt = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text,
                "select DeviceState.* from DeviceState inner join Device on Device.DeviceID=DeviceState.DeviceID where Device.Deleted=0 order by DeviceState.DeviceID asc").Tables[0];
            //_dt = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, "select DeviceState.* from DeviceState order by DeviceState.DeviceID asc").Tables[0];
            _dictionary = new Dictionary<int, Model.Entity.DeviceState>();
            for (int i = 0; i < _dt.Rows.Count; i++)
            {
                Model.Entity.DeviceState deviceState = new Model.Entity.DeviceState()
                {
                    Online = false,
                    SocketId = null
                };
                _dictionary.Add(deviceState.DeviceID, deviceState);
            }

            try
            {
                _saveState = bool.Parse(Utility.AppConfig.GetValue("State"));
            }
            catch
            {
                _saveState = false;
            }
        }

        public Model.Entity.DeviceState Get(int objId)
        {
            lock (_dictionary)
            {
                Model.Entity.DeviceState obj;
                _dictionary.TryGetValue(objId, out obj);
                if (obj != null)
                {
                    return obj;
                }

                DataRow dr = _dt.NewRow();
                Model.Entity.DeviceState deviceState = new Model.Entity.DeviceState()
                {
                    DeviceID = objId,
                    Online = false,
                    UpdateTime = DateTime.Now,
                    CreateTime = DateTime.Now,
                    Altitude = 0,
                    Course = 0,
                    Electricity = 0,
                    LocationType = 0,
                    GSM = 0,
                    SatelliteNumber = 0,
                    StopSendVoice = false,
                    Speed = 0
                };
                _dt.Rows.Add(dr);
                _dictionary.Add(deviceState.DeviceID, deviceState);
                return deviceState;
            }
        }

        public void Save()
        {
            if (_saveState)
                Data.DBHelper.GetInstance().ExecuteSave(_dt, "select top 0 * from DeviceState");
        }

        public Dictionary<int, Model.Entity.DeviceState> Dictionary
        {
            get { return _dictionary; }
        }

        public void CleanByDeviceId(int deviceId)
        {
            lock (_dictionary)
            {
                _dictionary.Remove(deviceId);
            }
        }
    }
}