using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace YW.Logic
{
    public class YWLBS : Base
    {
        private static YWLBS _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<string, Model.Entity.YWLBS> _dictionary;
#pragma warning disable CS0414 // 字段“YWLBS._saveState”已被赋值，但从未使用过它的值
        private readonly bool _saveState = true;
#pragma warning restore CS0414 // 字段“YWLBS._saveState”已被赋值，但从未使用过它的值
        public static YWLBS GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new YWLBS();
                    }
                }
            }
            return _object;
        }

        public YWLBS()
            : base(typeof(Model.Entity.YWLBS))
        {
            try
            {
                _dictionary = new Dictionary<string, Model.Entity.YWLBS>();

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

                if(location.LBS != null)
                    contentArr = location.LBS.Split('|');

                if ((contentArr != null) && (contentArr.Length > 0))
                {
                    foreach (var strArr in contentArr)
                    {
                        string[] str = strArr.Split(',');
                        if ((str != null) && (str.Length == 5))
                        {

                            string LbsStr = str[0] + "," + str[1] + "," + str[2] + "," + str[3];

                            const string sql =
                            "select top 1 [YWLBS].* from [YWLBS] where [YWLBS].[MCC]=@MCC and  [YWLBS].[MNC ]=@MNC and [YWLBS].[LAC]=@LAC and  [YWLBS].[CID]=@CID order by UpdateTime desc";
                            DbParameter[] commandParameters = new DbParameter[]
                            {
                                Data.DBHelper.CreateInDbParameter("@MCC", DbType.String, str[0]),
                                Data.DBHelper.CreateInDbParameter("@MNC", DbType.String, str[1]),
                                Data.DBHelper.CreateInDbParameter("@LAC", DbType.String, str[2]),
                                Data.DBHelper.CreateInDbParameter("@CID", DbType.String, str[3]),
                            };
                            var ds = Data.DBHelper.GetInstance().ExecuteAdapter(Data.Config.GetInstance().DB_LBS, CommandType.Text, sql, commandParameters);
                            var list = base.TableToList<Model.Entity.YWLBS>(ds);
                            if (list.Count > 0)
                            {
                                location.Lat = (double)list[0].OLat;
                                location.Lng = (double)list[0].OLng;
                                location.Radius = 50;
                                location.LocationType = 2;

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


        public Model.Entity.YWLBS Update(Model.Location location)
        {

            try
            {
                if (location.LBS != null && location.LocationType == 2 &&
                location.Lat != null && location.Lng != null &&
                location.Lat != 0 && location.Lng != 0)
                {
                    string[] contentArr = location.LBS.Split('|');
                    string[] Str = null;

                    if ((contentArr != null) && (contentArr.Length > 0))
                    {
                        Str = contentArr[0].Split(',');

                        if ((Str == null) || (Str.Length != 5))
                        {
                            return null;
                        }

                        Model.Entity.YWLBS obj = new Model.Entity.YWLBS()
                        {
                            MCC = Str[0],
                            MNC = Str[1],
                            LAC = Str[2],
                            CID = Str[3],
                            OLat = (decimal)location.Lat,
                            OLng = (decimal)location.Lng,
                            Infos = "damizk_cache",
                            UpdateTime = DateTime.Now

                        };

                        DbParameter[] commandParameters = new DbParameter[]
                        {
                            //Data.DBHelper.CreateInDbParameter("@ID",  DbType.Int32,  23),
                            Data.DBHelper.CreateInDbParameter("@MCC", DbType.String, Str[0]),
                            Data.DBHelper.CreateInDbParameter("@MNC", DbType.String, Str[1]),
                            Data.DBHelper.CreateInDbParameter("@LAC", DbType.String, Str[2]),
                            Data.DBHelper.CreateInDbParameter("@CID", DbType.String, Str[3]),
                            Data.DBHelper.CreateInDbParameter("@OLat", DbType.Decimal, (decimal)location.Lat),
                            Data.DBHelper.CreateInDbParameter("@OLng", DbType.Decimal, (decimal)location.Lng),
                            Data.DBHelper.CreateInDbParameter("@UpdateTime", DbType.DateTime, DateTime.Now),
                            Data.DBHelper.CreateInDbParameter("@Range", DbType.Double, 10),
                            Data.DBHelper.CreateInDbParameter("@Infos", DbType.String, "damizk_cache"),
                            Data.DBHelper.CreateInDbParameter("@TA", DbType.Int32, 0),
                        };

                        //const string sql = "Insert into [YWLBS] ([ID], [MCC],[MNC],[LAC],[CID],[OLat],[OLng],[UpdateTime],[Range],[Infos],[TA]) values(@ID,@MCC,@MNC,@LAC,@CID,@OLat,@OLng,@UpdateTime,@Range,@Infos,@TA)\n select @@IDENTITY as ID";
                        const string sql = "Insert into [YWLBS] ([MCC],[MNC],[LAC],[CID],[OLat],[OLng],[UpdateTime],[Range],[Infos],[TA]) values(@MCC,@MNC,@LAC,@CID,@OLat,@OLng,@UpdateTime,@Range,@Infos,@TA)\n select @@IDENTITY as ID";
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


        public Dictionary<string, Model.Entity.YWLBS> Dictionary
        {
            get { return _dictionary; }
        }
    }
}
