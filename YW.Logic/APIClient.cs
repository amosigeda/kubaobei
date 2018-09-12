using System;
using System.Threading;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Deserializers;
using YW.Data;
using YW.Model.Entity;
using YW.Utility;
using YW.Utility.Stat;

namespace YW.Logic
{
    public class APIClient
    {
        private static APIClient _object;
        private static readonly object LockHelper = new object();

        private string _baseUri;

        public static APIClient GetInstance()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new APIClient();
                    }
                }
            }

            return _object;
        }

        public APIClient()
        {
            _baseUri = AppConfig.GetValue("API_SERVER");
        }

        public LocationModel GetPosition(string bts, string wifis)
        {
            if (string.IsNullOrEmpty(bts))
            {
                return null;
            }

            var client = new RestClient(_baseUri + "position/query?lbs=" + HttpUtility.UrlEncode(bts));
            var request = new RestRequest(Method.GET);
            var res = client.Execute(request);
            Logger.Debug("基站查询获取到的内容为：" + res.Content);
            if (!res.IsSuccessful || string.IsNullOrEmpty(res.Content))
            {
                Logger.Error("调用基站查询接口失败：" + res.ErrorMessage + " 基站数据：" + bts + " 内容：" + res.Content);
                return null;
            }

            JObject dic = JsonConvert.DeserializeObject<JObject>(res.Content);
            if (dic["code"].Value<int>() != 1)
            {
                Logger.Error("调用基站查询接口失败：" + res.ErrorMessage + " 基站数据：" + bts + " 内容：" + res.Content);
                return null;
            }

            try
            {
                JObject cont = (JObject) dic["body"];
                LocationModel loc = new LocationModel()
                {
                    Code = 11,
                    Lat = cont["lat"].Value<double>(),
                    Lng = cont["lng"].Value<double>(),
                    Radius = cont["radius"].Value<int>()
                };
                return loc;
            }
            catch (Exception e)
            {
                Logger.Error("解析基站数据失败:" + e.Message + " 基站数据：" + bts + "内容：" + res.Content);
                return null;
            }
        }

        public Address GetAddress(double lat, double lng)
        {
            if ((int) lat == 0 || (int) lng == 0)
            {
                return null;
            }

            Logger.Debug("获取地址，之前坐标WGS84：" + lat + "," + lng);
            LocationHelper.WGS84ToGCJ(lat, lng, out lat, out lng);
            Logger.Debug("获取地址，之后坐标GCJ02：" + lat + "," + lng);

            var url = _baseUri + "position/regeo?lat=" + lat + "&lng=" + lng;
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            var res = client.Execute(request);
            Logger.Debug("调用逆地理编码内容：" + res.Content);
            if (!res.IsSuccessful || string.IsNullOrEmpty(res.Content))
            {
                Logger.Error("调用逆地理编码接口失败：" + res.ErrorMessage + " 坐标：" + lat + "," + lng + " 内容：" + res.Content);
                return null;
            }

            JObject dic = JsonConvert.DeserializeObject<JObject>(res.Content);
            if (dic["code"].Value<int>() != 1)
            {
                Logger.Error("调用逆地理编码接口失败, 坐标：" + lat + "," + lng + "内容：" + res.Content);
                return null;
            }

            try
            {
                Address address = new Address();
                var area = dic["body"];
                address.Nearby.Add(Utils.GetJObjVal(area["nearby"]));
                address.Province = Utils.GetJObjVal(area?["province"]);
                address.City = Utils.GetJObjVal(area?["city"]);
                address.District = Utils.GetJObjVal(area?["district"]);
                address.Road = Utils.GetJObjVal(area?["road"]);
                address.Code = 1;
                if (string.IsNullOrEmpty(address.City))
                {
                    address.City = address.District;
                }

                if (string.IsNullOrEmpty(address.City))
                {
                    address.City = address.Province;
                }

                return address;
            }
            catch (Exception e)
            {
                Logger.Error("解析逆地理编码失败：" + e.Message + " 坐标：" + lat + "," + lng + "内容：" + res.Content);
                return null;
            }
        }


        public Weather Weather(string province, string city)
        {
            int ticker = Environment.TickCount;
            try
            {
                if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(province))
                {
                    string url = _baseUri + "position/weather?province=" + HttpUtility.UrlEncode(province) + "&city=" + HttpUtility.UrlEncode(city);
                    var client = new RestClient(url);
                    var req = new RestRequest(Method.GET);
                    var result = client.Execute(req);
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(String.Format("获取到天气结果：{0} ,原始数据:{1},{2}", result.Content, province, city));
                    }

                    if (result.IsSuccessful && !string.IsNullOrEmpty(result.Content))
                    {
                        string res = result.Content;
                        JObject weather = JObject.Parse(res);
                        if (weather != null && "1".Equals(Utils.GetJObjVal(weather["code"])))
                        {
                            //var dataSK = {"nameen":"shenzhen","cityname":"深圳","city":"101280601","temp":"24","tempf":"75","WD":"东风","wde":"E ","WS":"3级","wse":"&lt;12km/h","SD":"82%","time":"17:00","weather":"多云","weathere":"Cloudy","weathercode":"d01","qy":"1007.1","njd":"13145","sd":"82%","rain":"0","rain24h":"0","aqi":"57","limitnumber":"","aqi_pm25":"57","date":"03月30日(星期四)"}
                            weather = JObject.FromObject(weather["body"]);
                            Weather wea = new Weather()
                            {
                                Province = province,
                                City = city,
                                CityName = Utils.GetJObjVal(weather["cityName"]),
                                CityCode = Utils.GetJObjVal(weather["cityCode"]),
                                WeatherCont = Utils.GetJObjVal(weather["weather"]),
                                Temp = Utils.GetJObjVal(weather["temp"]),
                                Humidity = Utils.GetJObjVal(weather["humidity"]),
                                Pm25 = Utils.GetJObjVal(weather["pm25"]),
                                Wind = Utils.GetJObjVal(weather["wind"]),
                                WindGrade = Utils.GetJObjVal(weather["windGrade"])
                            };
                            return wea;
                        }
                    }

                    Logger.Error(String.Format("获取天气失败，结果：{0} ,原始数据:{1},{2}", result.Content, province, city));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            Logger.Error("TQ-查询天气失败   province:" + province + "city:" + city);
            RECORD_INFO record = new RECORD_INFO();
            record.set('1', "weather", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(record);

            return null;
        }


        /// <summary>
        /// 绑定图灵接口
        /// </summary>
        /// <param name="deviceId"></param>
        public void ProcessBind(int deviceId)
        {
            if (!"1".Equals(AppConfig.GetValue(Constants.TURING_SYNC_ENABLED)))
            {
                return;
            }

            new Thread(SyncTuringBind).Start(deviceId);
        }

        /// <summary>
        /// 解绑图灵接口
        /// </summary>
        /// <param name="deviceId"></param>
        public void ProcessUnBind(int deviceId)
        {
            if (!"1".Equals(AppConfig.GetValue(Constants.TURING_SYNC_ENABLED)))
            {
                return;
            }

            new Thread(SyncTuringUnBind).Start(deviceId);
        }

        private void SyncTuringBind(object obj)
        {
            var device = Device.GetInstance().Get((int) obj);
            if (device == null || device.DeviceType != Constants.DEVICE_TYPE_ROBOTS)
            {
                return;
            }

            try
            {
                var request = new RestRequest(Method.POST);
                request.Timeout = 5000;
                request.AddParameter("apiKey", AppConfig.GetValue(Constants.TURING_SYNC_API_KEY));
                request.AddParameter("uid", device.SerialNumber);

                request.AddParameter("deviceId", device.SerialNumber);
                request.AddParameter("name", string.IsNullOrEmpty(device.BabyName) ? "Robot" : device.BabyName);
                var img = string.IsNullOrEmpty(device.Photo) ? AppConfig.GetValue(Constants.TURING_SYNC_IMG_DEFAULT) : AppConfig.GetValue(Constants.TURING_SYNC_IMG_URL) + device.Photo;
                request.AddParameter("imageUrl", img);
                var client = new RestClient(AppConfig.GetValue(Constants.TURING_SYNC_URL) + Constants.TURING_SYNC_URL_BIND);
                client.AddHandler("application/x-www-form-urlencoded", new JsonDeserializer());
                var response = client.Execute<JObject>(request);
                Logger.ErrorFormat("同步Turing的数据[绑定],Device id:{0}, 数据为：{1}", device.DeviceID, response.Content);
                var dt = JObject.Parse(response.Content);
                if (!"0".Equals(Utils.GetJObjVal(dt["code"])))
                {
                    Logger.ErrorFormat("同步Turing数据[绑定]失败,Device id:{0}, 返回数据为：{1}", device.DeviceID, response.Content + ", Exception:" + response.ErrorException);
                }
            }
            catch (Exception e)
            {
                Logger.Error("同步Turing数据[绑定]出错：", e);
            }
        }

        private void SyncTuringUnBind(object obj)
        {
            int deviceId = (int) obj;

            var device = Device.GetInstance().Get(deviceId);
            if (device == null || device.DeviceType != Constants.DEVICE_TYPE_ROBOTS)
            {
                return;
            }

            try
            {
                var request = new RestRequest(Method.POST);
                request.Timeout = 5000;
                request.ReadWriteTimeout = 5000;
                request.AddParameter("apiKey", AppConfig.GetValue(Constants.TURING_SYNC_API_KEY));
                request.AddParameter("uid", device.SerialNumber);
                request.AddParameter("deviceId", device.SerialNumber);
                var client = new RestClient(AppConfig.GetValue(Constants.TURING_SYNC_URL) + Constants.TURING_SYNC_URL_UNBIND);
                client.AddHandler("application/x-www-form-urlencoded", new JsonDeserializer());
                var response = client.Execute<JObject>(request);
                Logger.ErrorFormat("同步Turing的数据[解绑],Device id:{0}, 数据为：{1}", deviceId, response.Content);
                var dt = JObject.Parse(response.Content);
                if (!"0".Equals(Utils.GetJObjVal(dt["code"])))
                {
                    Logger.ErrorFormat("同步Turing数据[解绑]失败,Device id:{0}, 返回数据为：{1}", deviceId, response.Content + ", Exception:" + response.ErrorException);
                }
            }
            catch (Exception e)
            {
                Logger.Error("同步Turing数据[解绑]出错：", e);
            }
        }
    }
}