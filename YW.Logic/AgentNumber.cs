using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using RestSharp;
using YW.Data;
using YW.Model.Entity;
using YW.Utility;
using YW.Utility.Security;

namespace YW.Logic
{
    public class AgentNumber : Base
    {
        private static AgentNumber _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<int, Model.Entity.AgentNumber> _agentNumbers;
        private readonly Dictionary<int, List<Model.Entity.AgentNumber>> _dictionaryByPlatform;

        public static AgentNumber GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new AgentNumber();
                    }
                }
            }

            return _object;
        }

        public AgentNumber() : base(typeof(Model.Entity.AgentNumber))
        {
            var list = base.Get<Model.Entity.AgentNumber>();
            _agentNumbers = new Dictionary<int, Model.Entity.AgentNumber>();
            _dictionaryByPlatform = new Dictionary<int, List<Model.Entity.AgentNumber>>();
            foreach (var item in list)
            {
                _agentNumbers.Add(item.AgentNumberID, item);
                List<Model.Entity.AgentNumber> its;
                _dictionaryByPlatform.TryGetValue(item.Platform, out its);
                if (its == null)
                {
                    its = new List<Model.Entity.AgentNumber>();
                    _dictionaryByPlatform.Add(item.Platform, its);
                }

                its.Add(item);
            }
        }

        public List<Model.Entity.AgentNumber> GetList(int pageindex, int pagesize, string midNumber, int? platfrom, out int total)
        {
            StringBuilder where = new StringBuilder();
            if (!string.IsNullOrEmpty(midNumber) && ProcessSqlStr(midNumber))
            {
                where.Append("and [Number] like '%" + midNumber + "%'");
            }

            if (platfrom != null && platfrom != 0)
            {
                where.Append("and Platform=" + platfrom);
            }

            StringBuilder sql = new StringBuilder();
            sql.Append("declare @Total int\n");
            sql.Append(
                "select @Total=count(0) from AgentNumber where 1=1 ");
            sql.Append(where);
            sql.Append("\n");
            sql.Append("select @Total as Total\n");
            if (pageindex == 1)
            {
                sql.Append("select top " + pagesize + " AN.* from AgentNumber AN where 1=1 ");
                sql.Append(where);
                sql.Append(" order by CreateTime desc\n");
            }
            else
            {
                sql.Append("select top " + pagesize + " * from (");
                sql.Append(
                    "select AN.*,ROW_NUMBER() OVER(order by AN.CreateTime desc) as Nid from  AN.* from AgentNumber AN");
                sql.Append(where);
                sql.Append(") as temp where Nid>" + (pageindex - 1) * pagesize);
            }

            var ds = DBHelper.GetInstance().ExecuteAdapter(sql.ToString());
            total = (int) ds.Tables[0].Rows[0]["Total"];
            return base.TableToList<Model.Entity.AgentNumber>(ds.Tables[1]);
        }

        public List<Model.Entity.AgentNumber> GetList()
        {
            return _agentNumbers.Values.ToList();
        }

        public List<Model.Entity.AgentNumber> Get(int platform)
        {
            List<Model.Entity.AgentNumber> obj;
            _dictionaryByPlatform.TryGetValue(platform, out obj);
            return obj;
        }

        public void Save(Model.Entity.AgentNumber obj)
        {
            Save(obj, false);
        }

        public void Save(Model.Entity.AgentNumber obj, bool isUpdateSync)
        {
            int opt = obj.AgentNumberID == 0 ? 0 : 1;
            if (!isUpdateSync)
            {
                obj.Sync = 0;
            }

            lock (_agentNumbers)
            {
                if (string.IsNullOrEmpty(obj.Number) || string.IsNullOrEmpty(obj.CallOutNumber) || obj.Platform <= 0)
                {
                    throw new Exception("中间号不能为空");
                }

                _dictionaryByPlatform.TryGetValue(obj.Platform, out var list);
                if (list != null && list.Count > 0 && list.Contains(obj))
                {
                    throw new Exception("中间号在同一组中不能重复！");
                }

                try
                {
                    base.Save(obj);
                }
                catch (Exception e)
                {
                    Logger.Error("Save AgentNumber error，AgentNumber=" + obj.ToString(), e);
                    throw e;
                }

                if (_agentNumbers.ContainsValue(obj))
                {
                    if (list != null)
                    {
                        var agt = list.FirstOrDefault(x => x.Number.Equals(obj.Number));
                        if (agt != null && !agt.Equals(default(Model.Entity.AgentNumber)))
                        {
                            base.CopyValue<Model.Entity.AgentNumber>(obj, agt);
                        }
                    }
                }
                else
                {
                    _agentNumbers.Add(obj.AgentNumberID, obj);
                    List<Model.Entity.AgentNumber> its;
                    _dictionaryByPlatform.TryGetValue(obj.Platform, out its);
                    if (its == null)
                    {
                        its = new List<Model.Entity.AgentNumber>();
                        _dictionaryByPlatform.Add(obj.Platform, its);
                    }

                    its.Add(obj);
                }
            }

            var enabled = AppConfig.GetValue(Constants.CLOUDPLATFORM_ENABLED);
            if (!isUpdateSync && "1".Equals(enabled) && obj.Platform == 2)
            {
//            SyncAgentNumber(obj, obj.Number, opt);
                object[] objs = {obj, opt};
                new Thread(ProcessSync).Start(objs);
            }
        }

        private void ProcessSync(object tgt)
        {
            object[] paras = (object[]) tgt;
            Model.Entity.AgentNumber agentNumber = (Model.Entity.AgentNumber) paras[0];
            int opt = (int) paras[1];

            bool res = SyncAgentNumber(agentNumber, agentNumber.Number, opt);
            Logger.Debug("同步呼入中间号" + agentNumber.Number + (res ? "成功!" : "失败!"));
            if (res)
            {
                res = SyncAgentNumber(agentNumber, agentNumber.CallOutNumber, opt);
                Logger.Debug("同步呼出中间号" + agentNumber.CallOutNumber + (res ? "成功!" : "失败!"));
            }

            if (res)
            {
                agentNumber.Sync = 1;
                Save(agentNumber, true);
            }
        }

        /// <summary>
        /// 同步中间号
        /// </summary>
        /// <param name="agentNumber"></param>
        /// <param name="opt">0 新增，1 修改，2 删除</param>
        private bool SyncAgentNumber(Model.Entity.AgentNumber agentNumber, String midNum, int opt)
        {
            try
            {
                var request = new RestRequest(Method.POST);
                request.AddParameter("SERVICENAME", "setIvrType");
                request.AddParameter("OPERATE", opt);
                request.AddParameter("STREAMNUMBER", Guid.NewGuid().ToString());
                CloudPlatformInfo cpi = GetCloudPlatformInfo(agentNumber.Platform);
                string time = DateTime.Now.ToString("yyyyMMddHHmmss");
                string token = MD5Helper.MD5Encrypt(cpi.Key + time, null);
                request.AddParameter("TOKEN", token.ToLower());
                request.AddParameter("VCCID", int.Parse(cpi.VccId));
                request.AddParameter("MIDDLEINNUM", midNum);
                request.AddParameter("TYPE", 1);
                request.AddParameter("SOURCEDATA", 11);

                var client = new RestClient(cpi.UrlNumberAnalysis + "?token=" + token.ToLower());
                client.AddHandler("application/json", new CommAnalysisResponseDeserializer());
                Logger.Debug("开始同步中间号，内容为：" + request.Parameters.Select(x => x.Name + ":" + x.Value).ToList());
                var response = client.Execute<CommAnalysisResponse>(request);
                return ProcessResponse(agentNumber, opt, response);

//                client.ExecuteAsync<CommAnalysisResponse>(request, response => { ProcessResponse(agentNumber, opt, response); });
//                return true;
            }
            catch (Exception e)
            {
                Logger.Error("同步中间号出错：", e);
                return false;
            }
        }

        private bool ProcessResponse(Model.Entity.AgentNumber agentNumber, int opt, IRestResponse<CommAnalysisResponse> response)
        {
            Logger.Debug("同步中间号收到的数据为：" + response.Content);
            CommAnalysisResponse sbr = response.Data;
            Logger.Debug("接收对象为：" + sbr);
            if ("0000".Equals(sbr?.header?.STATE_CODE))
            {
                return true;
            }

            Logger.Error("同步中间号失败，返回数据为：" + response.Content + ", Exception:" + response.ErrorException);
            return false;
        }

        public void Del(string number)
        {
            lock (_agentNumbers)
            {
                var agt = _agentNumbers.Values.FirstOrDefault(x => x.Number.Equals(number));
                if (agt != null && !agt.Equals(default(Model.Entity.AgentNumber)))
                {
                    _dictionaryByPlatform.TryGetValue(agt.Platform, out var agts);
                    if (agts == null || agts.Count == 0)
                    {
                        return;
                    }

                    List<Model.Entity.AgentNumber> numbers = agts.Where(x => !number.Equals(x.Number)).ToList();
                    if (numbers.Count == 0)
                    {
                        throw new FaultException("没有其它中间号，请先添加中间号！");
                    }

                    long tick = DateTime.Now.Ticks;
                    Random ran = new Random((int) (tick & 0xffffffffL) | (int) (tick >> 32));
                    int idx = ran.Next(numbers.Count);

                    DeviceContact.GetInstance().ChangeAgentNumber(number, numbers[idx].Number);
                    object[] objs = {agt, 2};
                    new Thread(ProcessSync).Start(objs);
//                    bool succ = SyncAgentNumber(agentNumber, 2);
//                    if (!succ)
//                    {
//                        throw new FaultException("同步删除中间号失败，暂不能从本地删除！");
//                    }

                    base.Del(agt.AgentNumberID);
                    agts.Remove(agt);
                    _agentNumbers.Remove(agt.AgentNumberID);
                }
            }
        }

        public CloudPlatformInfo GetCloudPlatformInfo(int platform)
        {
            CloudPlatformInfo cpi = new CloudPlatformInfo();
            cpi.VccId = AppConfig.GetValue(Constants.CLOUDPLATFORM_G_VCCID);
            cpi.Key = AppConfig.GetValue(Constants.CLOUDPLATFORM_G_KEY);
            cpi.UrlDial = AppConfig.GetValue(Constants.CLOUDPLATFORM_G_URL_DIAL);
            cpi.UrlNumberAnalysis = AppConfig.GetValue(Constants.CLOUDPLATFORM_G_URL_NUMBER_ANALYSIS);
            return cpi;
        }

        public void SyncAgentNumbers()
        {
            List<Model.Entity.AgentNumber> agts = GetList();
            foreach (Model.Entity.AgentNumber agt in agts)
            {
                object[] objs = {agt, 1};
                ProcessSync(objs);
                Thread.Sleep(200);
            }
        }
    }
}