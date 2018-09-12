using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace YW.Logic
{
    public class Count
    {
        private Model.Entity.Count _count;
        private static Count _object;
        private static readonly object LockHelper = new object();
        private static Dictionary<String, Model.Entity.Count> _dictionary;
        private static readonly String FMT = "yyyy-MM-dd";
        public Count()
        {
            _dictionary = new Dictionary<String, Model.Entity.Count>();
            const string getSql = "select * from Count";
            DataSet ds = Data.DBHelper.GetInstance().ExecuteAdapter(getSql);
           foreach(DataRow dr in ds.Tables[0].Rows)
           {
               var item = new Model.Entity.Count { Date = (DateTime)dr["Date"] };
                if (dr["Address"] != DBNull.Value)
                    item.Address = (int)dr["Address"];
                if (dr["LbsAndWifi"] != DBNull.Value)
                    item.LbsAndWifi = (int)dr["LbsAndWifi"];
                if (dr["LbsAndWifiCache"] != DBNull.Value)
                    item.LbsAndWifiCache = (int)dr["LbsAndWifiCache"];
                if (dr["LbsAndWifiFail"] != DBNull.Value)
                    item.LbsAndWifiFail = (int)dr["LbsAndWifiFail"];
                if (dr["AddressTotal"] != DBNull.Value)
                    item.AddressTotal = (int)dr["AddressTotal"];
                if (dr["LbsAndWifiTotal"] != DBNull.Value)
                    item.LbsAndWifiTotal = (int)dr["LbsAndWifiTotal"];
                if (dr["SMS"] != DBNull.Value)
                    item.SMS = (int)dr["SMS"];
                _dictionary.Add(item.Date.ToString(FMT), item);
            }
            _count = this.Get();
        }

        public List<Model.Entity.Count> GetList()
        {
            return _dictionary.Values.ToList();
        }

        public static Count GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new Count();
                    }
                }
            }
            return _object;
        }
        private Model.Entity.Count Get()
        {
            lock (LockHelper)
            {
                DateTime date = DateTime.Now.Date;
                String dateStr = date.ToString(FMT);
              
                if (_dictionary.ContainsKey(dateStr))
                {
                    return _dictionary[dateStr];
                }
                else
                {
                    var item = new Model.Entity.Count {Date = date};
                    const string insertSql = "Insert into Count (Date,Address,LbsAndWifi,LbsAndWifiCache,LbsAndWifiFail,AddressTotal,LbsAndWifiTotal,SMS) values (@Date,0,0,0,0,0,0,0)";
                    DbParameter[] insertDp = new DbParameter[]
                    {
                        Data.DBHelper.CreateInDbParameter("@Date", DbType.DateTime, date)
                    };
                    Data.DBHelper.GetInstance().ExecuteNonQuery(insertSql, insertDp);
                    _dictionary.Add(dateStr, item);
                    return item;
                }
            }


        }

        public void Address()
        {
            lock (_count)
                _count.Address++;
        }

        public void LbsAndWifi()
        {
            lock (_count)
                _count.LbsAndWifi++;
        }

        public void LbsAndWifiCache()
        {
            lock (_count)
                _count.LbsAndWifiCache++;
        }

        public void LbsAndWifiFail()
        {
            lock (_count)
                _count.LbsAndWifiFail++;
        }

        public void AddressTotal()
        {
            lock (_count)
                _count.AddressTotal++;
        }

        public void LbsAndWifiTotal()
        {
            lock (_count)
                _count.LbsAndWifiTotal++;
        }

        public void SMS()
        {
            lock (_count)
                _count.SMS++;
        }
        public void  Save()
        {
            Model.Entity.Count count = _count;
            if (count.Date != DateTime.Now.Date)
            {
                _count = this.Get();
            }
            const string insertSql = "Update Count set Address=@Address,LbsAndWifi=@LbsAndWifi,LbsAndWifiCache=@LbsAndWifiCache,LbsAndWifiFail=@LbsAndWifiFail,AddressTotal=@AddressTotal,LbsAndWifiTotal=@LbsAndWifiTotal,SMS=@SMS,UpdateTime=getdate() where Date=@Date";
            DbParameter[] insertDp = new DbParameter[]
                    {
                        Data.DBHelper.CreateInDbParameter("@Date", DbType.DateTime, count.Date),
                        Data.DBHelper.CreateInDbParameter("@Address", DbType.Int32, count.Address),
                        Data.DBHelper.CreateInDbParameter("@LbsAndWifi", DbType.Int32, count.LbsAndWifi),
                        Data.DBHelper.CreateInDbParameter("@LbsAndWifiCache", DbType.Int32, count.LbsAndWifiCache),
                        Data.DBHelper.CreateInDbParameter("@LbsAndWifiFail", DbType.Int32, count.LbsAndWifiFail),
                        Data.DBHelper.CreateInDbParameter("@AddressTotal", DbType.Int32, count.AddressTotal),
                        Data.DBHelper.CreateInDbParameter("@LbsAndWifiTotal", DbType.Int32, count.LbsAndWifiTotal),
                        Data.DBHelper.CreateInDbParameter("@SMS", DbType.Int32, count.SMS)
                    };
            Data.DBHelper.GetInstance().ExecuteNonQuery(insertSql, insertDp);
        }
    }
}
