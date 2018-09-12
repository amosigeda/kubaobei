using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace YW.Logic
{
    public class DeviceTq
    {
        private static DeviceTq _object;
        private static readonly object LockHelper = new object();
        private readonly Dictionary<string, Model.Entity.DeviceTq> _dictionary=new Dictionary<string, Model.Entity.DeviceTq>();
        private Int32 _idCount = 0;

        public static DeviceTq GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new DeviceTq();
                    }
                }
            }

            return _object;
        }

        public DeviceTq()
        {
            /*try
            {
                var dt = Data.DBHelper.GetInstance().ExecuteAdapter(CommandType.Text, "select DeviceTq.* from DeviceTq order by DeviceTq.ID asc").Tables[0];

                _dictionary = new Dictionary<string, Model.Entity.DeviceTq>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var row = dt.Rows[i];
                    Model.Entity.DeviceTq tq = new Model.Entity.DeviceTq()
                    {
                        ID = row["ID"]==DBNull.Value?0:(int)row["ID"],
                        City = row["City"].ToString(),
                        Info = row["Info"].ToString(),
                        Temperature = row["Temperature"].ToString(),
                        Latitude = Decimal.Parse(row["Latitude"].ToString()),
                        Longitude = Decimal.Parse(row["Longitude"].ToString()),
                        CreateTime=(DateTime)row["CreateTime"],
                        UpdateTime=(DateTime)row["UpdateTime"]
                    };

                    if (!_dictionary.ContainsKey(tq.City))
                    {
                        _dictionary.Add(tq.City, tq);
                    }

                    _idCount = tq.ID;
                }
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }*/
        }


        public Model.Entity.DeviceTq Get(string city)
        {
            try
            {
                lock (_dictionary)
                {
                    Model.Entity.DeviceTq obj;
                    _dictionary.TryGetValue(city, out obj);

                    if (obj != null)
                    {
                        DateTime delay = obj.UpdateTime.AddHours(2);
                        if (delay > DateTime.Now)
                        {
                            return obj;
                        }
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


        public Model.Entity.DeviceTq Update(string City, string Info, string Temperature, double Latitude, double Longitude)
        {
            try
            {
                lock (_dictionary)
                {
                    Model.Entity.DeviceTq obj;
                    _dictionary.TryGetValue(City, out obj);

                    if (obj != null)
                    {
                        DateTime delay = obj.UpdateTime.AddHours(2);
                        if (delay > DateTime.Now)
                        {
                            return null;
                        }

                        obj.Info = Info;
                        obj.Temperature = Temperature;
                        obj.Latitude = (decimal) Latitude;
                        obj.Longitude = (decimal) Longitude;
                        obj.UpdateTime = DateTime.Now;
                    }
                    else
                    {
                        Model.Entity.DeviceTq deviceTq = new Model.Entity.DeviceTq()
                        {
                            ID = ++_idCount,
                            City = City,
                            Info = Info,
                            Temperature = Temperature,
                            Latitude = (decimal) Latitude,
                            Longitude = (decimal) Longitude,
                            CreateTime = DateTime.Now,
                            UpdateTime = DateTime.Now,
                        };
                        _dictionary.Add(City, deviceTq);
                    }

                    return obj;
                }
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }

            return null;
        }


        public void Save()
        {
//            if (_saveState)
//                Data.DBHelper.GetInstance().ExecuteSave(_dt, "select top 0 * from DeviceTq");
        }


        public Dictionary<string, Model.Entity.DeviceTq> Dictionary
        {
            get { return _dictionary; }
        }
    }
}