using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using YW.Data;

namespace YW.Logic
{
    public class Device : Base
    {
        private static Device _object;
        private static readonly object LockHelper = new object();
        private readonly ConcurrentDictionary<int, Model.Entity.Device> _dictionaryByDeviceId;
        private readonly ConcurrentDictionary<string, Model.Entity.Device> _dictionaryBySerialNumber;
        private readonly ConcurrentDictionary<string, Model.Entity.Device> _dictionaryByBindNumber;

        public static Device GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new Device();
                    }
                }
            }

            return _object;
        }

        public Device() : base(typeof(Model.Entity.Device))
        {
            var list = base.Get<Model.Entity.Device>();
            _dictionaryByDeviceId = new ConcurrentDictionary<int, Model.Entity.Device>();
            _dictionaryBySerialNumber = new ConcurrentDictionary<string, Model.Entity.Device>();
            _dictionaryByBindNumber = new ConcurrentDictionary<string, Model.Entity.Device>();
            foreach (var item in list)
            {
                if (item.Deleted)
                {
                    continue;
                }

                _dictionaryByDeviceId.TryAdd(item.DeviceID, item);
                _dictionaryBySerialNumber.TryAdd(item.SerialNumber, item);
                _dictionaryByBindNumber.TryAdd(item.BindNumber, item);
            }
        }

        public ConcurrentDictionary<int,Model.Entity.Device> GetDic()
        {
            return _dictionaryByDeviceId;
        }

        public Model.Entity.Device Get(int objId)
        {
            _dictionaryByDeviceId.TryGetValue(objId, out var obj);
            return obj;
        }

        public Model.Entity.Device Get(string serialNumber)
        {
            _dictionaryBySerialNumber.TryGetValue(serialNumber, out var obj);
            return obj;
        }

        public Model.Entity.Device GetByBindNum(string bindNumber)
        {
            _dictionaryByBindNumber.TryGetValue(bindNumber, out var obj);
            return obj;
        }

        public void ResetDevice(Model.Entity.Device device)
        {
            device.UserId = 0;
            device.ContactVersionNO = 0;
            device.SetVersionNO = 0;
            device.State = 2;
            device.OperatorType = 0;
            device.SmsNumber = null;
            device.SmsBalanceKey = null;
            device.SmsFlowKey = null;
            device.Photo = null;
            device.BabyName = null;
            device.PhoneCornet = null;
            device.Gender = false;
            device.Birthday = null;
            device.Grade = 0;
            device.SchoolAddress = null;
            device.SchoolLat = null;
            device.SchoolLng = null;
            device.HomeAddress = null;
            device.HomeLat = null;
            device.HomeLng = null;
            device.ActiveDate = null;
            device.IsGuard = false;
            device.Password = null;
            device.UpdateTime = DateTime.Now;
            if (device.CloudPlatform == 0)
            {
                device.PhoneNumber = null;
            }
        }

        public void Save(Model.Entity.Device obj)
        {
            lock (_dictionaryByDeviceId)
            {
                if (obj.DeviceID == 0 && _dictionaryByBindNumber.ContainsKey(obj.BindNumber))
                {
                    throw new Exception("绑定号已经存在");
                }

                try
                {
                    base.Save(obj);
                }
                catch (Exception e)
                {
                    Logger.Error("Save Device error，device=" + obj.ToString(), e);
                    throw e;
                }

                if (obj.DeviceID != 0)
                {
                    if (_dictionaryByDeviceId.ContainsKey(obj.DeviceID))
                    {
                        obj.UpdateTime = DateTime.Now;
                        if (obj != _dictionaryByDeviceId[obj.DeviceID])
                            base.CopyValue<Model.Entity.Device>(obj, _dictionaryByDeviceId[obj.DeviceID]);
                    }
                    else
                    {
                        obj.CreateTime = DateTime.Now;
                        obj.UpdateTime = DateTime.Now;
                        _dictionaryByDeviceId.TryAdd(obj.DeviceID, obj);
                        _dictionaryBySerialNumber.TryAdd(obj.SerialNumber, obj);
                        _dictionaryByBindNumber.TryAdd(obj.BindNumber, obj);
                    }
                }
            }
        }

        public void ChangeBindNumber(string oldBindNumber, string newBindNumber)
        {
            /*var item = this.GetByBindNum(oldBindNumber);
            lock (_dictionaryByDeviceId)
            {
                if (item != null && !_dictionaryByBindNumber.ContainsKey(newBindNumber))
                {
                    _dictionaryByBindNumber.Remove(oldBindNumber);
                    _dictionaryByBindNumber.Add(newBindNumber, item);
                }
            }*/
        }

        public new void Del(int deviceId)
        {
            lock (_dictionaryByDeviceId)
            {
                if (_dictionaryByDeviceId.ContainsKey(deviceId))
                {
                    CleanRelation(deviceId);
                    UserDevice.GetInstance().DelDevice(deviceId);
                    _dictionaryByDeviceId[deviceId].Deleted = true;
                    _dictionaryByDeviceId[deviceId].IsGuard = false;
                    Save(_dictionaryByDeviceId[deviceId]);
                    _dictionaryBySerialNumber.TryRemove(_dictionaryByDeviceId[deviceId].SerialNumber,out var res);
                    _dictionaryByBindNumber.TryRemove(_dictionaryByDeviceId[deviceId].BindNumber,out var res1);
                    _dictionaryByDeviceId.TryRemove(deviceId,out var res3);
                }
            }
        }

        public void CleanRelation(int deviceId)
        {
            if (_dictionaryByDeviceId.ContainsKey(deviceId))
            {
                if (_dictionaryByDeviceId.ContainsKey(deviceId))
                {
                    Model.Entity.Device device = Get(deviceId);
                    _dictionaryByDeviceId[deviceId].IsGuard = false;
                    lock (device)
                    {
                        DeviceContact.GetInstance().CleanByDeviceId(deviceId); //删除联系人
                        GeoFence.GetInstence().CleanByDeviceId(deviceId); //删除电子围栏
                        SchoolGuardian.GetInstance().CleanByDeviceId(deviceId); //删除上学守护
                        DeviceSet.GetInstance().Del(deviceId); //删除配置信息

                        //删除绑定号用户
                        User.GetInstance().DelByBindNumber(device.BindNumber);
                        User.GetInstance().DelReal(device.UserId);
                        UserDevice.GetInstance().DelDevice(deviceId); //删除关联信息

                        //清理消息
                        Notification.GetInstance().CleanByDeviceId(deviceId);
                        DeviceException.GetInstance().CleanByDeviceId(deviceId);
                        DeviceSMS.GetInstance().CleanByDeviceId(deviceId);
                        DeviceVoice.GetInstance().CleanByDeviceId(deviceId);
                        DevicePhoto.GetInstance().CleanByDeviceId(deviceId);
                        DeviceState.GetInstance().CleanByDeviceId(deviceId);
                        DeviceFriend.GetInstance().DelDevice(deviceId);
                    }
                }
            }
        }


    }
}