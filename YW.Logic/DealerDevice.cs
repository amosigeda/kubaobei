using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
namespace YW.Logic
{
    public class DealerDevice : Base 
    {
        private static DealerDevice _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<int, Dictionary<int, Model.Entity.DealerDevice>> _dictionaryByDealer;
        public static DealerDevice GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new DealerDevice();
                    }
                }
            }
            return _object;
        }
        public DealerDevice()
            : base(typeof(Model.Entity.DealerDevice))
        {
            _dictionaryByDealer = new Dictionary<int, Dictionary<int, Model.Entity.DealerDevice>>();
            const string sqlCommond ="select * from DealerDevice";
            var ds=Data.DBHelper.GetInstance().ExecuteAdapter(sqlCommond);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    var item = new Model.Entity.DealerDevice();
                    item.DealerId = (int)row["DealerId"];
                    item.DeviceId = (int)row["DeviceId"];
                    item.CreateTime = (DateTime)row["CreateTime"];
                    if (row["Purchaser"] != DBNull.Value)
                    item.Purchaser = (int)row["Purchaser"];
                    item.Status = (int)row["Status"];
                    if (row["Remark"] != DBNull.Value)
                        item.Remark = (string) row["Remark"];
                    if (row["ReworkTime"] != DBNull.Value)
                        item.ReworkTime = (DateTime)row["ReworkTime"];
                    if (row["SalesTime"] != DBNull.Value)
                        item.SalesTime = (DateTime)row["SalesTime"];
                    if (row["StockTime"] != DBNull.Value)
                        item.StockTime = (DateTime)row["StockTime"];
                    item.UpdateTime = (DateTime)row["UpdateTime"];
                    Dictionary<int, Model.Entity.DealerDevice> dictDevice;
                    if (!_dictionaryByDealer.TryGetValue(item.DealerId, out dictDevice))
                    {
                        dictDevice = new Dictionary<int, Model.Entity.DealerDevice>();
                        _dictionaryByDealer.Add(item.DealerId, dictDevice);
                    }
                    dictDevice.Add(item.DeviceId, item);
                }
            }
            
        }

        public List<Model.Entity.DealerDevice> GetList(int dealerId, int pageindex, int pagesize, string serialNumber,
            int? model, int? status, bool? active, out int total)
        {
            StringBuilder where = new StringBuilder();
            var dealer = Dealer.GetInstance().Get(dealerId);
            if (dealer.DealerType == 4)
                where.Append(" where DealerDevice.DealerId in(select DealerId from Dealer where parentId=" + dealerId +
                             " or DealerId=" + dealerId + ") and Device.Deleted=0");
            else
                where.Append(" where DealerDevice.DealerId=" + dealerId + " and Device.Deleted=0");
            if (!string.IsNullOrEmpty(serialNumber) && base.ProcessSqlStr(serialNumber))
                where.Append("and (Device.SerialNumber like '%" + serialNumber + "%' or Device.BindNumber like '%" +
                             serialNumber + "%')");
            if (model != null)
                where.Append("and Device.DeviceModelID=" + model.Value);
            if (active != null)
                where.Append("and Device.State=" + (active.Value ? "1" : "0"));
            if (status != null)
                where.Append("and DealerDevice.Status=" + status.Value);
            StringBuilder sql = new StringBuilder();
            sql.Append("declare @Total int\n");
            sql.Append(
                "select @Total=count(0) from DealerDevice inner join Device on DealerDevice.DeviceId=Device.DeviceId");
            sql.Append(where);
            sql.Append("\n");
            sql.Append("select @Total as Total\n");
            if (pageindex == 1)
            {
                sql.Append("select top " + pagesize +
                           " DealerDevice.* from DealerDevice inner join Device on DealerDevice.DeviceId=Device.DeviceId");
                sql.Append(where);
                sql.Append(" order by Device.DeviceId desc\n");
            }
            else
            {
                sql.Append("select top " + pagesize + " * from (");
                sql.Append(
                    "select DealerDevice.*,ROW_NUMBER() OVER(order by Device.DeviceId desc) as Nid from DealerDevice inner join Device on DealerDevice.DeviceId=Device.DeviceId");
                sql.Append(where);
                sql.Append(") as temp where Nid>" + (pageindex - 1)*pagesize);
            }
            var ds = Data.DBHelper.GetInstance().ExecuteAdapter(sql.ToString());
            total = (int) ds.Tables[0].Rows[0]["Total"];
            return base.TableToList<Model.Entity.DealerDevice>(ds.Tables[1]);
        }

        public Dictionary<int, Model.Entity.DealerDevice> GetByDealerId(int objId)
        {
            Dictionary<int, Model.Entity.DealerDevice> obj;
            _dictionaryByDealer.TryGetValue(objId, out obj);
            return obj ?? new Dictionary<int, Model.Entity.DealerDevice>();
        }
        public Model.Entity.DealerDevice Get(int dealerId,int deviceId)
        {
            Dictionary<int, Model.Entity.DealerDevice> obj;
            if(_dictionaryByDealer.TryGetValue(dealerId, out obj))
            {
                if (obj.ContainsKey(deviceId))
                    return obj[deviceId];
                else
                    return null;

            }
            return null;
        }
        public void New(Model.Entity.DealerDevice obj)
        {
            const string sqlCommond = "Insert into DealerDevice (DealerId,DeviceId,StockTime,Purchaser,SalesTime,ReworkTime,Status,Remark,CreateTime,UpdateTime) values (@DealerId,@DeviceId,@StockTime,@Purchaser,@SalesTime,@ReworkTime,@Status,@Remark,@CreateTime,@UpdateTime)";
            var dp = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DealerId", DbType.Int32, obj.DealerId),
                Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, obj.DeviceId),
                Data.DBHelper.CreateInDbParameter("@StockTime", DbType.DateTime, GetItemValue(obj.StockTime)),
                Data.DBHelper.CreateInDbParameter("@Purchaser", DbType.Int32, GetItemValue(obj.Purchaser)),
                Data.DBHelper.CreateInDbParameter("@SalesTime", DbType.DateTime, GetItemValue(obj.SalesTime)),
                Data.DBHelper.CreateInDbParameter("@ReworkTime", DbType.DateTime, GetItemValue(obj.ReworkTime)),
                Data.DBHelper.CreateInDbParameter("@Status", DbType.Int32, obj.Status),
                Data.DBHelper.CreateInDbParameter("@Remark", DbType.String,  GetItemValue(obj.Remark)),
                Data.DBHelper.CreateInDbParameter("@CreateTime", DbType.DateTime, obj.CreateTime),
                Data.DBHelper.CreateInDbParameter("@UpdateTime", DbType.DateTime, obj.UpdateTime)
            };
            Data.DBHelper.GetInstance().ExecuteNonQuery(sqlCommond, dp);
            lock (this)
            {
                Dictionary<int, Model.Entity.DealerDevice> dictDevice;
                if (!_dictionaryByDealer.TryGetValue(obj.DealerId, out dictDevice))
                {
                    dictDevice = new Dictionary<int, Model.Entity.DealerDevice>();
                    _dictionaryByDealer.Add(obj.DealerId, dictDevice);
                }
                dictDevice.Add(obj.DeviceId, obj);
            }
            
        }

        public Model.Manage.Count GetCount(int dealerId)
        {
            Model.Manage.Count count = new Model.Manage.Count();
            var dealer = Dealer.GetInstance().Get(dealerId);
            string sqlCommond;
            if (dealer.DealerType == 4)
                sqlCommond = "select Count(0) as Total,sum(Device.State) as Active,sum(case when Device.UserId>0 then 1 else 0 end) as Binding from DealerDevice inner join Dealer on Dealer.DealerId=DealerDevice.DealerId inner join Device on Device.DeviceId=DealerDevice.DeviceId where Dealer.ParentId=@DealerId and Device.Deleted=0";
            else
                sqlCommond = "select Count(0) as Total,sum(Device.State) as Active,sum(case when Device.UserId>0 then 1 else 0 end) as Binding from DealerDevice inner join Device on Device.DeviceId=DealerDevice.DeviceId where DealerDevice.DealerId=@DealerId and Device.Deleted=0";
            var dp = new DbParameter[]
            {
                Data.DBHelper.CreateInDbParameter("@DealerId", DbType.Int32, dealerId)
            };
            var ds=Data.DBHelper.GetInstance().ExecuteAdapter(sqlCommond, dp);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["Total"] != DBNull.Value)
                    count.Total = (int) ds.Tables[0].Rows[0]["Total"];
                if (ds.Tables[0].Rows[0]["Active"] != DBNull.Value)
                    count.Active = (int) ds.Tables[0].Rows[0]["Active"];
                if (ds.Tables[0].Rows[0]["Binding"] != DBNull.Value)
                    count.Binding = (int)ds.Tables[0].Rows[0]["Binding"];
            }
            return count;
        }

        public void Update(Model.Entity.DealerDevice obj)
        {
            Dictionary<int, Model.Entity.DealerDevice> dictDevice;
            if (
                _dictionaryByDealer.TryGetValue(obj.DealerId, out dictDevice))
            {
                Model.Entity.DealerDevice item;
                if (dictDevice.TryGetValue(obj.DeviceId, out item))
                {
                    item.UpdateTime = DateTime.Now;
                    const string sqlCommond =
                        "Update DealerDevice set StockTime=@StockTime,Purchaser=@Purchaser,SalesTime=@SalesTime,ReworkTime=@ReworkTime,Status=@Status,Remark=@Remark,UpdateTime=getdate() where DealerId=@DealerId and DeviceId=@DeviceId";
                    var dp = new DbParameter[]
                    {
                        Data.DBHelper.CreateInDbParameter("@DealerId", DbType.Int32, obj.DealerId),
                        Data.DBHelper.CreateInDbParameter("@DeviceID", DbType.Int32, obj.DeviceId),
                        Data.DBHelper.CreateInDbParameter("@StockTime", DbType.DateTime, GetItemValue(obj.StockTime)),
                        Data.DBHelper.CreateInDbParameter("@Purchaser", DbType.Int32, GetItemValue(obj.Purchaser)),
                        Data.DBHelper.CreateInDbParameter("@SalesTime", DbType.DateTime, GetItemValue(obj.SalesTime)),
                        Data.DBHelper.CreateInDbParameter("@ReworkTime", DbType.DateTime, GetItemValue(obj.ReworkTime)),
                        Data.DBHelper.CreateInDbParameter("@Status", DbType.Int32, obj.Status),
                        Data.DBHelper.CreateInDbParameter("@Remark", DbType.String,  GetItemValue(obj.Remark))
                    };
                    Data.DBHelper.GetInstance().ExecuteNonQuery(sqlCommond, dp);
                    if (obj != item)
                    {
                        item.StockTime = obj.StockTime;
                        item.Purchaser = obj.Purchaser;
                        item.SalesTime = obj.SalesTime;
                        item.ReworkTime = obj.ReworkTime;
                        item.Status = obj.Status;
                        item.Remark = obj.Remark;
                    }
                }
            }
        }
        private object GetItemValue(Object obj)
        {
            if (obj == null)
                return DBNull.Value;
            else
                return obj;
        }

    }
}
