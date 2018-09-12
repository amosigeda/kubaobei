using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using YW.Data;
using YW.Model.Entity;
using YW.Utility;
using YW.Utility.Security;

namespace YW.Logic
{
    public class DeviceContact : Base
    {
        private static DeviceContact _object;
        private static readonly object LockHelper = new object();
        private readonly ConcurrentDictionary<int, Model.Entity.DeviceContact> _dictionaryByDeviceContactId;

        public static DeviceContact GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new DeviceContact();
                    }
                }
            }

            return _object;
        }

        public DeviceContact() : base(typeof(Model.Entity.DeviceContact))
        {
            var list = base.Get<Model.Entity.DeviceContact>();
            _dictionaryByDeviceContactId = new ConcurrentDictionary<int, Model.Entity.DeviceContact>();
            foreach (var item in list)
            {
                _dictionaryByDeviceContactId.TryAdd(item.DeviceContactId, item);
            }
        }

        public ConcurrentDictionary<int, Model.Entity.DeviceContact> GetDic()
        {
            return _dictionaryByDeviceContactId;
        }

        public Model.Entity.DeviceContact Get(int objId)
        {
            _dictionaryByDeviceContactId.TryGetValue(objId, out var obj);
            return obj;
        }

        public List<Model.Entity.DeviceContact> GetByDeviceId(int objId)
        {
            return _dictionaryByDeviceContactId.Where(x => x.Value != null && x.Value.DeviceID == objId).Select(x => x.Value).OrderBy(o => o.DeviceContactId).ToList();
        }

        public Model.Entity.DeviceContact GetForPhoto(int objId, string phoneNumber)
        {
            Model.Entity.DeviceContact obj = null;
            const string sql =
                "select top 1 [DeviceContact].* from [DeviceContact] where [DeviceContact].[DeviceID] =@DeviceId and [DeviceContact].[PhoneNumber] =@PhoneNumber";
            var commandParameters = new DbParameter[]
            {
                DBHelper.CreateInDbParameter("@DeviceId", DbType.Int32, objId),
                DBHelper.CreateInDbParameter("@PhoneNumber", DbType.String, phoneNumber)
            };
            var ds = DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, sql, commandParameters);
            var list = base.TableToList<Model.Entity.DeviceContact>(ds);
            if (list.Count > 0)
            {
                obj = list[0];
                _dictionaryByDeviceContactId.TryAdd(obj.DeviceContactId, obj);
            }

            return obj;
        }

        public void Save(Model.Entity.DeviceContact obj)
        {
            Save(obj, false);
        }

        public void Save(Model.Entity.DeviceContact obj, bool isUpdateSync)
        {
            int opt = obj.DeviceContactId == 0 ? 0 : 1;
            if (!isUpdateSync)
            {
                obj.Sync = 0;
            }

            base.Save(obj);
            if (obj.DeviceContactId != 0)
            {
                if (_dictionaryByDeviceContactId.ContainsKey(obj.DeviceContactId))
                {
                    obj.UpdateTime = DateTime.Now;
                    if (obj != _dictionaryByDeviceContactId[obj.DeviceContactId])
                        base.CopyValue<Model.Entity.DeviceContact>(obj, _dictionaryByDeviceContactId[obj.DeviceContactId]);
                }
                else
                {
                    obj.CreateTime = DateTime.Now;
                    obj.UpdateTime = DateTime.Now;
                    _dictionaryByDeviceContactId.TryAdd(obj.DeviceContactId, obj);
                }
            }

            if (!isUpdateSync)
            {
                PreModAgentNumber(obj, opt);
            }
        }

        public void CleanByDeviceId(int deviceId)
        {
            const string sql = "delete from [DeviceContact] where [DeviceContact].[DeviceID]=@DeviceID";
            var commandParameters = new DbParameter[]
            {
                DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, deviceId),
            };
            DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
            GetByDeviceId(deviceId).ForEach(x =>
            {
                _dictionaryByDeviceContactId.TryRemove(x.DeviceContactId, out var dc);
                if (dc != null)
                {
                    PreModAgentNumber(dc, 2);
                }
            });
        }

        public void CleanByUserId(int userId)
        {
            const string sql = "delete from [DeviceContact] where [DeviceContact].[Type]=2 and [DeviceContact].[ObjectId]=@ObjectId";
            var commandParameters = new DbParameter[]
            {
                DBHelper.CreateInDbParameter("@ObjectId", DbType.Int32, userId),
            };
            DBHelper.GetInstance().ExecuteNonQuery(CommandType.Text, sql, commandParameters);
            _dictionaryByDeviceContactId.Where(x => x.Value != null && x.Value.UserId == userId && x.Value.Type == 2)
                .Select(x => _dictionaryByDeviceContactId.TryRemove(x.Value.DeviceContactId, out var res));
        }

        public new void Del(int objId)
        {
            base.Del(objId);
            _dictionaryByDeviceContactId.TryRemove(objId, out var dc);
            if (dc != null)
            {
                PreModAgentNumber(dc, 2);
            }
        }

        public void PreModAgentNumber(Model.Entity.DeviceContact contact, int opt)
        {
            var enabled = AppConfig.GetValue(Constants.CLOUDPLATFORM_ENABLED);
            if (!"1".Equals(enabled))
            {
                return;
            }

            Model.Entity.Device device = Device.GetInstance().Get(contact.DeviceID);
            if (device == null || string.IsNullOrEmpty(device.PhoneNumber) || device.CloudPlatform == 0)
            {
                Logger.Debug("设备数据异常，不用同步！");
                return;
            }

//            Logger.Error("获取中间号列表");
            List<Model.Entity.AgentNumber> list = AgentNumber.GetInstance().Get(device.CloudPlatform);
            if (list == null || list.Count == 0)
            {
                Logger.Error("中间号为空，请先添加中间号");
                return;
            }

            if (string.IsNullOrEmpty(contact.AgentNumber) || string.IsNullOrEmpty(contact.CallOutNumber))
            {
//            Logger.Info("计算中间号："+contact.PhoneNumber);
                List<Model.Entity.DeviceContact> dcList = GetByDeviceId(contact.DeviceID);
                dcList.Remove(contact);
                List<string> agNumList = dcList.ConvertAll(x => x.AgentNumber).ToList();
                List<Model.Entity.AgentNumber> availableANs = list.Where(x => !agNumList.Contains(x.Number)).ToList();
                if (availableANs.Count == 0)
                {
                    Logger.Error("没有可用的中间号，请先添加足够的中间号！");
                    return;
                }

                contact.AgentNumber = availableANs[0].Number;
                contact.CallOutNumber = availableANs[0].CallOutNumber;
            }

            object[] obj = {contact, opt};
            new Thread(ProcessSync).Start(obj);
        }

        private void ProcessSync(object obj)
        {
            Thread.Sleep(100);
            object[] paras = (object[]) obj;
            Model.Entity.DeviceContact contact = (Model.Entity.DeviceContact) paras[0];
            int opt = (int) paras[1];

            Model.Entity.Device device = Device.GetInstance().Get(contact.DeviceID);
//            Logger.Info("开始同步通讯录绑定关系，DeviceID:" + device.DeviceID + " Platform:" + device.CloudPlatform);
            if (device == null || string.IsNullOrEmpty(device.PhoneNumber) || string.IsNullOrEmpty(contact.AgentNumber) || string.IsNullOrEmpty(contact.CallOutNumber))
            {
                Logger.Error("设备异常，不用同步！DeviceContactId:" + contact.DeviceContactId);
                return;
            }

            bool res = false;
            switch (device.CloudPlatform)
            {
                case 2:
                {
                    res = SyncAgentNumber(contact, device, device.PhoneNumber, contact.PhoneNumber, contact.AgentNumber, contact.CallOutNumber, opt);
                    Logger.Error("同步通讯录绑定关系(手表打手机)：" + contact.DeviceContactId + (res ? "成功!" : "失败!"));
                    if (res)
                    {
                        res = SyncAgentNumber(contact, device, contact.PhoneNumber, device.PhoneNumber, contact.CallOutNumber, contact.AgentNumber, opt);
                        Logger.Error("同步通讯录绑定关系(手机打手表)：" + contact.DeviceContactId + (res ? "成功!" : "失败!"));
                    }

                    break;
                }
                case 1:
                case 3:
                case 4:
                {
                    res = SyncE23AgentNumbers(contact, device, opt);
                    break;
                }
            }

            if (res && opt != 2)
            {
                contact.Sync = 1;
                Save(contact, true);
            }
        }

        private bool SyncE23AgentNumbers(Model.Entity.DeviceContact contact, Model.Entity.Device device, int opt)
        {
            if (opt == 2)
            {
                return true;
            }

            try
            {
                var url = "http://ask.968824.com/api/callback.aspx?calla={0}&callb={1}&zjh1={2}&zjh2={3}&tcid=27951224&dlname=1009";
                var client = new RestClient(string.Format(url, device.PhoneNumber, contact.PhoneNumber, contact.AgentNumber, contact.CallOutNumber));
                var request = new RestRequest(Method.GET);
                request.Timeout = 5000;
                request.ReadWriteTimeout = 5000;
                var response = client.Execute<JObject>(request);
                string code = null;
                string tips = null;
                try
                {
                    var json = response.Data;
                    if (json == null)
                    {
                        json = JObject.Parse(response.Content);
                    }

                    code = Utils.GetJObjVal(json["code"]);
                    tips = Utils.GetJObjVal(json["tips"]);
                }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
                catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
                {
                    Logger.Error("同步E23中间号接口输入json无效：" + response.Content);
                }

                if ("0".Equals(code) || (response.Content != null && response.Content.Contains("\"0\"")))
                {
                    return true;
                }

                Logger.Error("同步E23中间号失败：" + tips + " Contact:" + contact + " Device:" + device);
                return false;
            }
            catch (Exception e)
            {
                Logger.Error("同步E23中间号失败，异常：" + " Contact:" + contact + " Device:" + device, e);
            }

            return false;
        }

        private bool SyncAgentNumber(Model.Entity.DeviceContact contact, Model.Entity.Device device, string callerNum, string calledNum, string callInMidNum, string callOutMidNum, int opt)
        {
            try
            {
                SetBindNum sbn = new SetBindNum();
                sbn.header.SERVICENAME = "setBindNum";
                sbn.header.OPERATE = opt;
                CloudPlatformInfo cpi = AgentNumber.GetInstance().GetCloudPlatformInfo(2);
                string time = DateTime.Now.ToString("yyyyMMddHHmmss");
                string token = MD5Helper.MD5Encrypt(cpi.Key + time, null);
                sbn.header.TOKEN = token.ToLower();
                sbn.header.VCCID = int.Parse(cpi.VccId);

                sbn.body.TYPE = 1;
                sbn.body.STREAMNUMBER = Guid.NewGuid().ToString();
                sbn.body.MESSAGEID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                sbn.body.WAYBILLNUM = "";
                sbn.body.CALLERNUM = callerNum;
                sbn.body.MIDDLEINNUM = callInMidNum;
                sbn.body.MIDDLEOUTNUM = callOutMidNum;
                sbn.body.CALLEDNUM = calledNum;
                sbn.body.MAXDURATION = 3600;
                sbn.body.ISRECORD = 0;
                sbn.body.STATE = 0;
                sbn.body.VALIDTIME = "";

                var client = new RestClient(cpi.UrlNumberAnalysis);
                client.AddHandler("application/json", new CommAnalysisResponseDeserializer());
                var request = new RestRequest(Method.POST);
                request.AddJsonBody(sbn);
                request.Timeout = 5000;
                request.ReadWriteTimeout = 5000;
                JsonSerializer jsonSerializer = new JsonSerializer {ContentType = "application/json"};
                request.JsonSerializer = jsonSerializer;
                Logger.Error("开始同步通讯录绑定关系，内容为：" + jsonSerializer.Serialize(sbn));
                var response = client.Execute<CommAnalysisResponse>(request);
                return ProcessResponse(contact, opt, response);
            }
            catch (Exception e)
            {
                Logger.Error("同步通讯录绑定关系出错：", e);
                return false;
            }
        }

        private bool ProcessResponse(Model.Entity.DeviceContact contact, int opt, IRestResponse<CommAnalysisResponse> response)
        {
            Logger.Error("收到的数据为：" + response.Content + ", Exception:" + response.ErrorException);

            CommAnalysisResponse sbr = response.Data;
            if ("0000".Equals(sbr?.header?.STATE_CODE))
            {
                return true;
            }

            return false;
        }

        public void ChangeAgentNumber(string number, string target)
        {
            var contacts = _dictionaryByDeviceContactId.Values.Where(x => number.Equals(x.AgentNumber)).ToList();
            foreach (var ct in contacts)
            {
                ct.AgentNumber = target;
                Save(ct, false);

                Model.Entity.Device device = Device.GetInstance().Get(ct.DeviceID);
                device.ContactVersionNO++;
                Device.GetInstance().Save(device);

                //通知其他客户端更新通讯录
                List<Model.Entity.User> userList = UserDevice.GetInstance().GetUserByDeviceId(ct.DeviceID);
                Notification.GetInstance().Send(ct.DeviceID, 232, userList);
            }
        }

        public void SyncAgentNumberData()
        {
            List<Model.Entity.DeviceContact> dcts = _dictionaryByDeviceContactId.Values.ToList();
            dcts = dcts.Where(x => x.AgentNumber != null && x.Sync == 0).ToList();
            foreach (var dct in dcts)
            {
//                Logger.Error("正在执行：" + dct.PhoneNumber);
                if (dct.AgentNumber == null || dct.Sync == 1)
                {
                    continue;
                }

                PreModAgentNumber(dct, 1);
            }
        }
    }
}