using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace YW.Logic
{
    public class WifiData : Base
    {
        private static WifiData _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<string, Model.Entity.WifiData> _dictionary;
#pragma warning disable CS0414 // 字段“WifiData._saveState”已被赋值，但从未使用过它的值
        private readonly bool _saveState = true;
#pragma warning restore CS0414 // 字段“WifiData._saveState”已被赋值，但从未使用过它的值
        public static WifiData GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new WifiData();
                    }
                }
            }
            return _object;
        }

        public WifiData()
            : base(typeof(Model.Entity.WifiData))
        {
            try
            {
                _dictionary = new Dictionary<string, Model.Entity.WifiData>();

            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
        }


        public bool Get(Model.Location location)
        {
            try
            {
                string[] contentArr = null;

                if (location.WIFI != null)
                    contentArr = location.WIFI.Split('|');


                if ((contentArr != null) && (contentArr.Length > 0))
                {
                    foreach (var strArr in contentArr)
                    {
                        string[] str = strArr.Split(',');
                        
                        if ((str != null)&&(str.Length == 3))
                        {

                            const string sql =
                            "select top 1 [WifiData].* from [WifiData] where [WifiData].[macAddress]=@macAddress order by UpdateTime desc";
                            DbParameter[] commandParameters = new DbParameter[]
                            {
                                Data.DBHelper.CreateInDbParameter("@macAddress", DbType.String, str[0]),
                            };
                            var ds = Data.DBHelper.GetInstance().ExecuteAdapter(Data.Config.GetInstance().DB_LBS, CommandType.Text, sql, commandParameters);
                            var list = base.TableToList<Model.Entity.WifiData>(ds);
                            if (list.Count > 0)
                            {
                                location.Lat = (double)list[0].latitude;
                                location.Lng = (double)list[0].longitude;
                                location.Radius = list[0].accuracy;
                                location.LocationType = 3;

                                return true;
                            }

                        }
                    }

                }

                return false;

            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
            return false;
        }


        public Model.Entity.WifiData Update(Model.Location location)
        {
            try
            {

                if (location.WIFI != null && location.LocationType == 3 &&
                location.Lat != null && location.Lng != null &&
                location.Lat != 0 && location.Lng != 0)
                {
                    string[] contentArr = location.WIFI.Split('|');
                    string[] Str = null;

                    if ((contentArr != null) && (contentArr.Length > 0))
                    {
                        Str = contentArr[0].Split(',');

                        if ((Str == null) || (Str.Length != 3))
                        {
                            return null;
                        }

                        Model.Entity.WifiData obj = new Model.Entity.WifiData()
                        {
                            macAddress = Str[0],
                            age = 0,
                            singal = Convert.ToInt32(Str[1]),
                            address = null,
                            latitude = (decimal)location.Lat,
                            longitude = (decimal)location.Lng,
                            accuracy = 0,
                            DESC = "damizk_cache",
                            CreateTime = DateTime.Now,
                            UpdateTime = DateTime.Now
                        };

                        DbParameter[] commandParameters = new DbParameter[]
                        {
                            //Data.DBHelper.CreateInDbParameter("@ID",  DbType.Int32,  23),
                            Data.DBHelper.CreateInDbParameter("@macAddress", DbType.String, Str[0]),
                            Data.DBHelper.CreateInDbParameter("@age", DbType.Int32, 0),
                            Data.DBHelper.CreateInDbParameter("@singal", DbType.Int32, Convert.ToInt32(Str[1])),
                            Data.DBHelper.CreateInDbParameter("@address", DbType.String, "0"),
                            Data.DBHelper.CreateInDbParameter("@latitude", DbType.Decimal, (decimal)location.Lat),
                            Data.DBHelper.CreateInDbParameter("@longitude", DbType.Decimal, (decimal)location.Lng),
                            Data.DBHelper.CreateInDbParameter("@accuracy", DbType.Int32, 0),
                            Data.DBHelper.CreateInDbParameter("@DESC", DbType.String, "damizk_cache"),
                            Data.DBHelper.CreateInDbParameter("@CreateTime", DbType.DateTime, DateTime.Now),
                            Data.DBHelper.CreateInDbParameter("@UpdateTime", DbType.DateTime, DateTime.Now),
                        };

                        const string sql = "Insert into [WifiData] ([macAddress],[age],[singal],[address],[latitude],[longitude],[accuracy],[DESC],[CreateTime],[UpdateTime]) values(@macAddress,@age,@singal,@address,@latitude,@longitude,@accuracy,@DESC,@CreateTime,@UpdateTime)\n select @@IDENTITY as ID";
                        obj.ID = int.Parse(Data.DBHelper.GetInstance().ExecuteScalar(Data.Config.GetInstance().DB_LBS, CommandType.Text, sql, commandParameters).ToString());


                        return obj;

                    }
                }
                return null;

            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
            return null;

        }


        public Dictionary<string, Model.Entity.WifiData> Dictionary
        {
            get { return _dictionary; }
        }
    }
}
