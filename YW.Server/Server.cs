using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using MongoDB;
using MongoDB.Configuration;
using Newtonsoft.Json.Linq;
using YW.Data;
using YW.Logic;
using YW.Model;
using YW.Model.Entity;
using YW.Server.Device;
using YW.Server.Socket;
using YW.Utility;
using YW.Utility.Stat;
using YW.WCF;
using Convert = System.Convert;
using Count = YW.Logic.Count;
using DeviceContact = YW.Logic.DeviceContact;
using DeviceException = YW.Model.Entity.DeviceException;
using DeviceFriend = YW.Logic.DeviceFriend;
using DeviceSet = YW.Logic.DeviceSet;
using DeviceState = YW.Model.Entity.DeviceState;
using DeviceTq = YW.Logic.DeviceTq;
using GeoFence = YW.Logic.GeoFence;
using Notification = YW.Logic.Notification;
using SchoolGuardian = YW.Model.Entity.SchoolGuardian;
using User = YW.Logic.User;
using UserDevice = YW.Logic.UserDevice;
using WifiData = YW.Logic.WifiData;
using YWLBS = YW.Logic.YWLBS;

namespace YW.Server
{
    internal class Server
    {
        private readonly ServiceHost _clientHost;
        private readonly ServiceHost _fileHost;
        private readonly Listen _listen;
        private readonly Thread _timerThread;
        private readonly Thread[] _gaodeThread;
        private readonly AdapterFactory _adapterFactory;
        private readonly Dictionary<Guid, MySAE> _dictMySae;
        private readonly Queue<Location> _queueLocation;
        private readonly Queue<Location> _queueLocationLbsWifi;
        private readonly Queue<Location> _queueLocationLbsWifiGaode;

        private readonly Thread[] _threadDoLocation;
        private readonly Thread[] _threadDoLocationLbsWifi;
        private readonly string _apiKey;
        private readonly bool _log;
        private readonly bool _history;

        private readonly MongoConfiguration _mongodbConfiguration;
        private readonly Queue<Queue<Location>> _insertQueue;
        private Queue<Location> _insertTempQueue;
        private const int InsertListSize = 2000;
        private readonly Thread _insertThread;
#pragma warning disable CS0169 // 从不使用字段“Server._collectThread”
        private readonly Thread _collectThread;
#pragma warning restore CS0169 // 从不使用字段“Server._collectThread”
        private readonly Queue<LBSWIFI> _collectTempQueue;

        public Server()
        {
            ThreadPool.SetMinThreads(200, 200); //设置最小线程池数量
            _queueLocation = new Queue<Location>();
            _queueLocationLbsWifi = new Queue<Location>();
            _queueLocationLbsWifiGaode = new Queue<Location>();

            _apiKey = AppConfig.GetValue("ApiKey");
            _log = bool.Parse(AppConfig.GetValue("Log"));
            _collectTempQueue = new Queue<LBSWIFI>();
            try
            {
                if (AppConfig.GetValue("MongoDBIp").Length > 0 &&
                    AppConfig.GetValue("MongoDBPort").Length > 0)
                {
                    string mongodb = "Server=" + AppConfig.GetValue("MongoDBIp") + ":" + AppConfig.GetValue("MongoDBPort");
                    var mongodbConfigurationBuilder = new MongoConfigurationBuilder();
                    mongodbConfigurationBuilder.Mapping(mapping =>
                    {
                        mapping.DefaultProfile(profile => profile.SubClassesAre(t => t.IsSubclassOf(typeof(Location))));
                        mapping.Map<Location>();
                    });
                    mongodbConfigurationBuilder.ConnectionString(mongodb);
                    _mongodbConfiguration = mongodbConfigurationBuilder.BuildConfiguration();
                    _history = true;
                    _insertTempQueue = new Queue<Location>();
                    _insertQueue = new Queue<Queue<Location>>();
                }
                else
                {
                    _history = false;
                }
            }
            catch (Exception e)
            {
                Logger.Error("初始化MongoDB失败",e);
                _history = false;
            }

            YWLBS.GetInstance();
            DeviceTq.GetInstance();
            Logic.Device.GetInstance();
            DeviceSet.GetInstance();
            DeviceContact.GetInstance();
            Logic.DeviceState.GetInstance();
            DeviceFriend.GetInstance();
            User.GetInstance();
            UserDevice.GetInstance();
            GeoFence.GetInstence();

            _listen = new Listen();
            _dictMySae = new Dictionary<Guid, MySAE>();
            _listen.OnAcceptConnect += new Listen.AcceptConnectHandler(_listen_OnAcceptConnect);
            _listen.OnMsgReceived += new Listen.ReceiveMsgHandler(_listen_OnMsgReceived);
            _listen.OnDisConnect += new Listen.DisConnectHandler(_listen_OnDisConnect);
            Client.OnSend += new Client.SendHandler(Client_OnSend);
            Client.OnDisConnect += new Client.DisConnectHandler(_listen_OnDisConnect);
            Client.OnGetOnlineHandler += new Client.GetOnlineHandler(Client_OnGetOnlineHandler);
            Client.OnGetDoLocationQueueCount += new Client.GetDoLocationQueueCount(Client_OnGetDoLocationQueueCount);
            Client.OnGetDoLocationQueueLbsWifiCount += new Client.GetDoLocationQueueCount(Client_OnGetDoLocationQueueLbsWifiCount);
            Client.OnGetInsertHistoryQueueCount += new Client.GetInsertHistoryQueueCount(Client_OnGetInsertHistoryQueueCount);
            Client.OnCollectLbsAndWifi += new Client.CollectLbsAndWifiHandler(Client_OnCollectLbsAndWifi);
            _clientHost = new ServiceHost(typeof(Client));
            _fileHost = new ServiceHost(typeof(GFile));

            if (_log)
            {
                foreach (var endpoint in _clientHost.Description.Endpoints)
                {
                    endpoint.Behaviors.Add(new LoggerEndpointBehavior());
                }

                foreach (var endpoint in _fileHost.Description.Endpoints)
                {
                    endpoint.Behaviors.Add(new LoggerEndpointBehavior());
                }
            }

            _listen.Init();
            _adapterFactory = new AdapterFactory(_listen.SendBinaryByTcp, Client_OnSend);
            _adapterFactory.OnLocation += new AdapterFactory.LocationHandler(_adapterFactory_OnLocation);
            _adapterFactory.OnLocationLbsWifi += new AdapterFactory.LocationHandler(_adapterFactory_OnLocationLbsWifi);
            _adapterFactory.OnLocationGaode += new AdapterFactory.LocationHandler(_adapterFactory_OnLocationGaode);

            _timerThread = new Thread(delegate()
            {
                var timewatch = new Stopwatch();
                int sleepTime = 60 * 1000;
                while (true)
                {
                    try
                    {
                        if (sleepTime > 0)
                            Thread.Sleep(sleepTime);
                        timewatch.Start();
                        Logic.DeviceState.GetInstance().Save();
                        //Data.Logger.Debug("Online：" + _dictMySae.Count);
                        SocketTimer.Timer.Do();
                        Count.GetInstance().Save();
                        CPhotoStat.GetInstance().print_info(DateTime.Now);
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                    finally
                    {
                        timewatch.Stop();
                        sleepTime = 60 * 1000 - (int) timewatch.ElapsedMilliseconds;
                        timewatch.Reset();
                    }
                }
            }) {IsBackground = true};

            _threadDoLocation = new Thread[10];
            for (int i = 0; i < _threadDoLocation.Length; i++)
            {
                _threadDoLocation[i] = new Thread(DoLocation) {IsBackground = true};
            }

            _threadDoLocationLbsWifi = new Thread[10];
            for (int i = 0; i < _threadDoLocationLbsWifi.Length; i++)
            {
                _threadDoLocationLbsWifi[i] = new Thread(DoLocationLbsWifi) {IsBackground = true};
            }

            _gaodeThread = new Thread[10];
            for (int i = 0; i < _gaodeThread.Length; i++)
            {
                _gaodeThread[i] = new Thread(DoLocationGaodeLbsWifi) {IsBackground = true};
            }


            if (_history)
            {
                _insertThread = new Thread(this.InsertHistory) {IsBackground = true};
                ;
            }

            //_collectThread = new Thread(this.CollectLBSWIFI) {IsBackground = true};
        }

        void Client_OnCollectLbsAndWifi(LBSWIFI lbswifi)
        {
            _collectTempQueue.Enqueue(lbswifi);
        }

/*        private void CollectLBSWIFI()
        {
            while (true)
            {
                try
                {
                    if (_collectTempQueue.Count > 0)
                    {
                        var wifilbs = _collectTempQueue.Dequeue();
                        LBSWIFIClient.Get() .CollectLbsAndWifiAsync(_apiKey, 2, wifilbs.bts, wifilbs.wifis, wifilbs.lat, wifilbs.lng, wifilbs.radius);
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (System.Threading.ThreadAbortException)
                {
                }
                catch (Exception ex) { Data.Logger.Error(ex); }
            }
        }*/

        int Client_OnGetOnlineHandler()
        {
            return _dictMySae.Count;
        }

        int Client_OnGetDoLocationQueueCount()
        {
            return _queueLocation.Count;
        }

        int Client_OnGetDoLocationQueueLbsWifiCount()
        {
            return _queueLocationLbsWifi.Count;
        }

        int Client_OnGetInsertHistoryQueueCount()
        {
            return _insertQueue.Count;
        }


        private static String keys = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static int exponent = keys.Length;

        public static int SafeInt(string text)
        {
            return SafeInt(text, 0);
        }

        /// <summary>
        /// 转为Int类型
        /// </summary>
        /// <param name="text"></param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static int SafeInt(string text, int defaultValue)
        {
            int num;
            if (int.TryParse(text, out num))
            {
                defaultValue = num;
            }

            return defaultValue;
        }

        public static long Str2Long(String value)
        {
            value = value.ToUpper();
            value = value.Replace(":", "");
            long result = 0;
            for (int i = 0; i < value.Length; i++)
            {
                int x = value.Length - i - 1;
                char ch = char.Parse(value.Substring(i, 1));
                result += keys.IndexOf(ch) * Pow(exponent, x);
            }

            return result;
        }

        private static long Pow(long baseNo, long x)
        {
            long value = 1;
            while (x > 0)
            {
                value = value * baseNo;
                x--;
            }

            return value;
        }

        public static byte[] replace(byte[] obytes)
        {
            //printable 32-126
            byte[] destbytes = new byte[obytes.Length];
            for (int i = 0; i < obytes.Length; ++i)
            {
                byte b = obytes[i];
                if (b > 126)
                {
                    b = (byte) (b - 126);
                    if (b < 32)
                    {
                        b = (byte) (b + 32);
                    }
                }
                else if (b < 32)
                {
                    b = (byte) (b + 126);
                    if (b < 32)
                    {
                        b = (byte) (b + 32);
                    }
                }

                destbytes[i] = b;
            }

            return destbytes;
        }

        public static string HttpPost(string Url, string postDataStr)
        {
            try
            {
                byte[] postData = Encoding.UTF8.GetBytes(postDataStr);
                // 设置提交的相关参数
                HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
                Encoding myEncoding = Encoding.UTF8;
                request.Method = "POST";
                request.ContentType = "text/plainStatus|Cause|Map|Lat|Lon|Address|Radius";
                request.UserAgent = "";
                request.ContentLength = postData.Length;

                // 提交请求数据
                Stream outputStream = request.GetRequestStream();
                outputStream.Write(postData, 0, postData.Length);
                outputStream.Close();

                HttpWebResponse response;
                Stream responseStream;
                StreamReader reader;
                string srcString;
                response = request.GetResponse() as HttpWebResponse;
                responseStream = response.GetResponseStream();
                reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
                srcString = reader.ReadToEnd();
                string result = srcString; //返回值赋值
                reader.Close();
                return result;
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                return "";
            }
        }

        public static string GetPage(string url)
        {
            if (url == "") return "";

            #region

            WebResponse response = null;
            Stream stream = null;
            StreamReader reader = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);

                request.UserAgent = @"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.40607; .NET CLR 1.1.4322)";
                request.Timeout = 60000;
                response = request.GetResponse();
                stream = response.GetResponseStream();


                if (Get_Chartset(response.Headers["Content-Type"].ToString()).ToLower() == "gbk")
                {
                    reader = new StreamReader(stream, Encoding.GetEncoding("GB2312"));
                }
                else if (Get_Chartset(response.Headers["Content-Type"].ToString()).ToLower() == "utf-8")
                {
                    reader = new StreamReader(stream, Encoding.UTF8);
                }
                else
                {
                    reader = new StreamReader(stream, Encoding.UTF8);
                }

                string buffer = reader.ReadToEnd();

                return buffer;
            }

#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                return "";
            }
            finally
            {
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (response != null) response.Close();
            }

            #endregion
        }

        public static string Get_Chartset(string s)
        {
            if (s.ToLower() == "text/html")
            {
                return "gbk";
            }
            else
            {
                int l = s.IndexOf("charset=") + 8;
                return s.Substring(l, s.Length - l);
            }
        }

        public static string TransLBS2MinGPS(string lbs)
        {
            string temp = null;

            string[] locArr = lbs.Split('|');
            int mmc = 0;
            int mnc = 0;
            for (int i = 0; i < locArr.Length; i++)
            {
                string str = locArr[i];
                string[] pos = str.Split(',');
                mmc = SafeInt(pos[0]);
                mnc = SafeInt(pos[1]);
                temp += "," + pos[2] + "," + pos[3] + "," + pos[4];
            }

            temp = Convert.ToString(locArr.Length) + ",1," + Convert.ToString(mmc) + "," + Convert.ToString(mnc) + temp;

            return temp;
        }

        public static string TransWifi2MinGPS(string lbs)
        {
            string temp = null;
            string[] locArr = lbs.Split('|');

            for (int i = 0; i < locArr.Length; i++)
            {
                string str = locArr[i];
                string[] pos = str.Split(',');
                temp += "," + pos[2] + "," + pos[0] + "," + pos[1];
            }

            temp = Convert.ToString(locArr.Length) + temp;
            return temp;
        }

        //gaode small device key
        public static string GetLatLngByLBSWifiAtGaode(string imei, string lbs, string wifi, ref Location location)
        {
            // http://apilocate.amap.com/position?output=json&key=f6da281d3ff92f04fa51e93ada9d74a8
            string address = "";
            try
            {
                if (!string.IsNullOrEmpty(lbs))
                {
                    string bts = null;
                    string nearbts = null;
                    string[] arr = lbs.Split('|');
                    if (arr.Count() > 0)
                        bts = arr[0];

                    for (int i = 1; i < arr.Count(); i++)
                    {
                        if (i > 1)
                        {
                            nearbts += "|";
                        }

                        nearbts += arr[i];
                    }

                    //
                    string gaodeUrl = "http://apilocate.amap.com/position?key=6788eb6c64386b8657ce5c3b26b84eea&accesstype=0";

                    gaodeUrl += "&cdma=0&network=GSM&bts=" + bts + "&imei=" + imei + "&nearbts=" + nearbts + "&macs=" + wifi;

                    string htm = GetPage(gaodeUrl);
                    //Data.Logger.Debug(gaodeUrl);

                    //Data.Logger.Debug(htm);

                    if (htm != "")
                    {
                        ///{"status":"1","info":"OK","infocode":"10000","result":{"type":"4","location":"82828.000,2992290.00"
                        string cause = htm.Substring(htm.IndexOf("location"), htm.IndexOf(",", htm.IndexOf(",", htm.IndexOf("location")) + 1) - htm.IndexOf("location"));
                        if (cause.Length > 8)
                            address = cause.Substring(11, cause.Length - 11 - 1);

                        string[] array = address.Split(',');
                        location.Lng = Convert.ToDouble(array[0]);
                        location.Lat = Convert.ToDouble(array[1]);

                        location.LocationType = 2;
                    }
                }
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                address = "";
            }

            return address;
        }

        //gaode address key
        private static String key = "caf74d5ede3735ad43ce38c5b480cc85";

        public static string GetByLatLngAtGaode(string lat, string lon, ref string city, ref string province)
        {
            // http://restapi.amap.com/v3/geocode/regeo?location=116.355,39.9876&extensions=base&output=json&key=f6da281d3ff92f04fa51e93ada9d74a8
            string address = "";
            try
            {
                if (lat != "" && lon != "")
                {
                    //
                    string gaodeUrl = "http://restapi.amap.com/v3/geocode/regeo?key=" + key;
                    lat = Convert.ToDouble(lat).ToString("0.000000");
                    lon = Convert.ToDouble(lon).ToString("0.000000");

                    gaodeUrl += "&location=" + lon + "," + lat;
                    string res = GetPage(gaodeUrl);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(String.Format("获取到逆地理编码结果：{0} ,原始数据：{1}", res, gaodeUrl));
                    }

                    if (res != "")
                    {
                        ///{"status":"1","info":"OK","infocode":"10000","regeocode":{"formatted_address":"广东省深圳市南山区粤海街道桑达科技大厦","addressComponent":{"country":"中国","province":"广东省","city":"深圳市","citycode":"0755","district":"南山区","adcode":"440305","township":"粤海街道","towncode":"440305007000","neighborhood":{"name":[],"type":[]},"building":{"name":"桑达科技大厦","type":"商务住宅;楼宇;商务写字楼"},"streetNumber":{"street":"深南大道","number":"9826号","location":"113.949455,22.5412","direction":"北","distance":"34.6765"},"businessAreas":[{"location":"113.94703575799998,22.542234107999988","name":"科技园","id":"440305"},{"location":"113.95482722485211,22.54298533136095","name":"大冲","id":"440305"}]}}}
                        JObject json = JObject.Parse(res);
                        city = json["regeocode"]["addressComponent"]["city"].Value<string>();
                        province = json["regeocode"]["addressComponent"]["province"].Value<string>();
                        address = json["regeocode"]["formatted_address"].Value<string>();


                        //string cause = htm.Substring(htm.IndexOf("formatted_address"), htm.IndexOf(",", htm.IndexOf("formatted_address")) - htm.IndexOf("formatted_address"));
                        //if (cause.Length > 20)
                        //    address = cause.Substring(20, cause.Length - 20 -1);
                        /////"province":"北京市","city":[]
                        //city = htm.Substring(htm.IndexOf("city"), htm.IndexOf(",", htm.IndexOf("city")) - htm.IndexOf("city"));
                        //if(city.Length > 7)
                        //    city = city.Substring(7, city.Length -7 -1);

                        //if (city == "")
                        //{
                        //    city = htm.Substring(htm.IndexOf("province"), htm.IndexOf(",", htm.IndexOf("province")) - htm.IndexOf("province"));
                        //    if (city.Length > 11)
                        //        city = city.Substring(11, city.Length - 11 -1);
                        //}
                    }
                }
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                address = "";
            }

            return address;
        }

        /// <summary>
        ///  没有地址返回，有wifi数据会慢
        /// </summary>
        /// <param name="lbs"></param>
        /// <param name="wifi"></param>
        /// <returns></returns>
        public static string GetLatLngByMiniWIFI(string lbs, string wifi, ref Location location)
        {
            string latLng = "";
            try
            {
                string minigpsUrl = "http://minigps.net/cw?x=";
                string[] arr = lbs.Split(',');
                string mccHex = Convert.ToString(SafeInt(arr[2]), 16);
                string mncHex = Convert.ToString(SafeInt(arr[3]), 16);
                minigpsUrl += mccHex + "-" + mncHex + "-";
                int ta = SafeInt(arr[1]);
                int lbsCount = SafeInt(arr[0]);
                for (int i = 0; i < lbsCount; i++)
                {
                    int index = 0;
                    if (i > 0)
                    {
                        index = i * 3;
                    }

                    string lacHex = Convert.ToString(SafeInt(arr[index + 4]), 16);
                    string cidHex = Convert.ToString(SafeInt(arr[index + 5]), 16);
                    string gsmHex = Convert.ToString(SafeInt(arr[index + 6]) + 110, 16);

                    minigpsUrl += lacHex + "-" + cidHex + "-" + gsmHex + "-";
                }

                minigpsUrl = minigpsUrl.TrimEnd('-') + "&p=1&mt=0&needaddress=0";
                //{"mac_address":"00:0b:0e:7d:17:82","singal_strength":8,"age":0}
                //{"ws":[{"s":"xo","r":81,"m":804380873802619826},{"s":"terry","r":69,"m":2018924576320342756},{"s":"TP-LINK_3225EE","r":53,"m":674173120793097686},{"s":"loushangshengyinxiaodian","r":49,"m":44590646795096412}]}
                //wifi数量,name,mac地址,信号,name,mac地址,信号
                string[] wifiArr = wifi.Split(',');
                int wifiCount = SafeInt(wifiArr[0]);
                StringBuilder wifiJson = new StringBuilder();
                string haoserviceStr = "[";
                wifiJson.Append("{\"ws\":[");
                string wifiMac = "", wifiSingal = "";
                for (int i = 0; i < wifiCount; i++)
                {
                    int index = 0;
                    if (i > 0)
                    {
                        index = i * 3;
                    }
                    else
                    {
                        wifiMac = wifiArr[index + 2];
                        wifiSingal = wifiArr[index + 3];
                    }

                    string macname = Encoding.UTF8.GetString(replace(Encoding.UTF8.GetBytes(wifiArr[index + 1])));
                    long m = Str2Long(wifiArr[index + 2]);
                    //string sinHex = Convert.ToString(Utility.SafeInt(wifiArr[index + 3]) + 110, 16);
                    string sinHex = (SafeInt(wifiArr[index + 3]) + 110).ToString();
                    string json = "{\"s\":\"" + macname + "\",\"r\":" + sinHex + ",\"m\":" + m + "},";
                    wifiJson.Append(json);

                    haoserviceStr += "{\"mac_address\":\"" + wifiArr[index + 2] + "\",\"singal_strength\":" + wifiArr[index + 3] + ",\"age\":0},";
                }

                haoserviceStr = haoserviceStr.TrimEnd(',');
                haoserviceStr += "]";

                string wJson = wifiJson.ToString().TrimEnd(',');
                wifiJson = new StringBuilder();
                wifiJson.Append(wJson);
                wifiJson.Append("]}");

                //minigpsUrl = "http://minigps.net/cw?x=1cc-0-508e-e2f6-f4-50e7-c711-f8-508e-e2f9-f7-508e-4c10-f2-50e7-c714-ed-50e7-e0b1-ec-50c7-3b4f-e9&p=1&mt=0&needaddress=0";
                //string htm = HttpPost(minigpsUrl, "{\"ws\":[{\"s\":\"CMCC-PPPOE\",\"r\":16,\"m\":922498427349191268}]}");

                string htm = HttpPost(minigpsUrl, wifiJson.ToString());
                if (htm != "")
                {
                    //Data.Logger.Info("\r\nminiwifi:" + htm);
                    string[] items = htm.Split(',');
                    string cause = htm.Substring(htm.IndexOf("cause"), 10);
                    if (cause.Length > 8)
                        cause = cause.Substring(8, 2);
                    if (cause == "OK")
                    {
                        string lat = items[items.Length - 2].Substring(items[items.Length - 2].IndexOf("lat") + 3 + 2);
                        lat = lat.Substring(0, lat.Length);

                        string lon = items[items.Length - 1].Substring(items[items.Length - 1].IndexOf("lon") + 3 + 2);
                        lon = lon.Substring(0, lon.Length - 1);

                        location.Lat = Convert.ToDouble(lat);
                        location.Lng = Convert.ToDouble(lon);

                        location.LocationType = 2;

                        latLng = lat + "," + lon;
                    }
                    else
                    {
                        //Data.Logger.Info("\r\n没获取到WIFI," + minigpsUrl + "," + wifiJson.ToString() + "," + htm);
                    }
                }
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                //Data.Logger.Error(ex.Message);
            }

            return latLng;
        }

        /// <summary>
        /// 暂时有地址返回，后边无
        /// </summary>
        /// <param name="lbs"></param>
        /// <returns></returns>
        public static string GetLatLngByMiniGPS(string lbs, ref Location location)
        {
            //http://minigps.net/as?x=1cc-0-6212-2F8C-AC&ta=1&p=1&mt=0
            //{"cause":"OK","address":"汉庭快捷酒店【无锡学前街店】,无锡市崇安区前西溪1号;无锡市华源专利事务所,无锡市学前街168号;","map":"std","status":0,"lat":31.56959122468348,"lon":120.28564363882276}
            string latLng = "";
            try
            {
                if (lbs != "")
                {
                    //460,0,1,4,9338,3692,150,9338,3691,145,9338,3690,140,9338,3692,139
                    string[] arr = lbs.Split(',');
                    string minigpsUrl = "http://minigps.net/as?x=";
                    string mccHex = Convert.ToString(SafeInt(arr[2]), 16);
                    string mncHex = Convert.ToString(SafeInt(arr[3]), 16);
                    minigpsUrl += mccHex + "-" + mncHex + "-";
                    int ta = SafeInt(arr[1]);
                    int lbsCount = SafeInt(arr[0]);
                    for (int i = 0; i < lbsCount; i++)
                    {
                        int index = 0;
                        if (i > 0)
                        {
                            index = i * 3;
                        }

                        string lacHex = Convert.ToString(SafeInt(arr[index + 4]), 16);
                        string cidHex = Convert.ToString(SafeInt(arr[index + 5]), 16);
                        string gsmHex = Convert.ToString(SafeInt(arr[index + 6]) + 110, 16);

                        minigpsUrl += lacHex + "-" + cidHex + "-" + gsmHex + "-";
                    }

                    minigpsUrl = minigpsUrl.TrimEnd('-') + "&p=1&mt=0&needaddress=0";

                    string htm = GetPage(minigpsUrl);
                    if (htm != "")
                    {
                        string[] items = htm.Split(',');
                        string cause = htm.Substring(htm.IndexOf("cause"), 10);
                        if (cause.Length > 8)
                            cause = cause.Substring(8, 2);
                        if (cause == "OK")
                        {
                            string lat = items[items.Length - 2].Substring(items[items.Length - 2].IndexOf("lat") + 3 + 2);
                            lat = lat.Substring(0, lat.Length);

                            string lon = items[items.Length - 1].Substring(items[items.Length - 1].IndexOf("lon") + 3 + 2);
                            lon = lon.Substring(0, lon.Length - 1);

                            string radius = items[2].Substring(items[2].IndexOf("radius") + 6 + 2);


                            string type = items[0].Substring(items[0].IndexOf("type") + 4 + 3);
                            type = type.Substring(0, type.Length - 1);

                            location.Lat = Convert.ToDouble(lat);
                            location.Lng = Convert.ToDouble(lon);
                            location.Radius = Convert.ToInt32(radius);
                            if (type == "cell")
                                location.LocationType = 2;
                            else
                                location.LocationType = 3;

                            latLng = lat + "," + lon;
                        }
                    }
                }
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                latLng = "";
            }

            return latLng;
        }

        private void InsertHistory()
        {
            Queue<Location> queue = null;
            while (true)
            {
                try
                {
                    lock (_insertQueue)
                    {
                        if (_insertQueue.Count > 0)
                        {
                            queue = _insertQueue.Dequeue();
                        }
                        else
                        {
                            if (_insertTempQueue.Count > 0)
                            {
                                queue = _insertTempQueue;
                                _insertTempQueue = new Queue<Location>();
                            }
                        }
                    }

                    if (queue != null && queue.Count > 0)
                    {
                        var dictInsertLocation = new Dictionary<string, List<Location>>();
                        while (queue.Count > 0)
                        {
                            Location location = queue.Dequeue();
                            string db = location.Time.ToString("yyyyMM");
                            location.UpdateTime = DateTime.Now;
                            if (!dictInsertLocation.ContainsKey(db))
                            {
                                dictInsertLocation.Add(db, new List<Location>());
                            }

                            dictInsertLocation[db].Add(location);
                        }

                        while (dictInsertLocation.Count > 0)
                        {
                            try
                            {
                                var temp = dictInsertLocation.FirstOrDefault();
                                var dbName = temp.Key;
                                using (var mongo = new Mongo(_mongodbConfiguration))
                                {
                                    mongo.Connect();
                                    try
                                    {
                                        var db = mongo.GetDatabase("History" + dbName);
                                        var collection = db.GetCollection<Location>("Location");
                                        collection.Insert(temp.Value);
                                    }
                                    finally
                                    {
                                        mongo.Disconnect();
                                    }
                                }

                                dictInsertLocation.Remove(temp.Key);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);
                                Thread.Sleep(500);
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(5);
                    }
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private void DoLocation()
        {
            while (true)
            {
                try
                {
                    Location location = null;

                    if (_queueLocation.Count > 0)
                    {
                        lock (_queueLocation)
                            if (_queueLocation.Count > 0)
                                location = _queueLocation.Dequeue();
                    }

                    if (location != null)
                    {
                        DeviceState deviceState = Logic.DeviceState.GetInstance().Get(location.DeviceId);
                        Model.Entity.Device device = Logic.Device.GetInstance().Get(location.DeviceId);

                        int ticker = Environment.TickCount;
                        RECORD_INFO info = new RECORD_INFO();

                        {
                            #region 解析基站和WIFI

                            if (device.Deleted)
                                continue;
                            if (location.LocationType == 0)
                            {
                                Count.GetInstance().LbsAndWifiTotal();

                                if (location.LBS == deviceState.LBS && location.WIFI == deviceState.Wifi)
                                {
                                    location.Lat = Convert.ToDouble(deviceState.Latitude);
                                    location.Lng = Convert.ToDouble(deviceState.Longitude);
                                    location.LocationType = deviceState.LocationType;
                                    location.Radius = deviceState.Radius;
                                    Count.GetInstance().LbsAndWifiCache();

                                    info.set('1', "DeviceStateLoc", "", 0, Environment.TickCount - ticker, 1);
                                    CPhotoStat.GetInstance().add_info(info);
                                }
                                else
                                {
                                    ///get wifi loc
                                    bool locationcacheWifi = WifiData.GetInstance().Get(location);

                                    ///get lbs loc
                                    bool locationcacheLBS = false;
                                    Location locationLBS = null;
                                    if (location.LBS != "")
                                    {
                                        locationLBS = new Location(location.DeviceId, location.Time, location.CreateTime);
                                        locationLBS.LBS = location.LBS;
                                        locationcacheLBS = YWLBS.GetInstance().Get(locationLBS);
                                    }

                                    ///check wifi loc
                                    if (locationcacheWifi == true && locationcacheLBS == true)
                                    {
                                        double distance = LocationHelper.GetDistance(locationLBS.Lat.Value,
                                            locationLBS.Lng.Value, location.Lat.Value, location.Lng.Value);

                                        if (distance > 5000)
                                            locationcacheWifi = false;
                                    }

                                    ///if no wifi, but has lbs loc
                                    if (locationcacheWifi == false && locationcacheLBS == true)
                                    {
                                        location.Lat = locationLBS.Lat;
                                        location.Lng = locationLBS.Lng;
                                        location.Radius = locationLBS.Radius;
                                        location.LocationType = locationLBS.LocationType;
                                    }

                                    if (locationcacheWifi == true || locationcacheLBS == true)
                                    {
                                        Count.GetInstance().LbsAndWifiCache();
                                    }
                                }
                            }

                            #endregion

                            if (location.LocationType != 0 && location.Lat != null && location.Lng != null &&
                                (int) location.Lat != 0 && (int) location.Lng != 0 && (deviceState.LocationSource != 3))
                            {
                                ProcessDeviceLocation(device, deviceState, location);
                            }
                            else
                            {
                                #region 发送消息给LSBWIFI线程去解析

                                try
                                {
                                    info.set('1', "OnLocatLbsWifi", "", 0, Environment.TickCount - ticker, 1);
                                    CPhotoStat.GetInstance().add_info(info);

                                    _adapterFactory_OnLocationLbsWifi(location); ////如果没有解析出基站WIFI信息或者当前有天气请求，则发送消息给LBSWIFI定位的线程去处理
                                }
                                catch (Exception ex)
                                {
                                    Count.GetInstance().LbsAndWifiFail();
                                    Logger.Error(ex);
                                    Logger.Error("DoLocation send msg err LBS:" + location.LBS + "     WIFI:" + location.WIFI);
                                }

                                #endregion
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private void DoLocationLbsWifi()
        {
            while (true)
            {
                try
                {
                    Location location = null;

                    if (_queueLocationLbsWifi.Count > 0)
                    {
                        lock (_queueLocationLbsWifi)
                            if (_queueLocationLbsWifi.Count > 0)
                                location = _queueLocationLbsWifi.Dequeue();
                    }

                    if (location != null)
                    {
                        DeviceState deviceState = Logic.DeviceState.GetInstance().Get(location.DeviceId);
                        Model.Entity.Device device = Logic.Device.GetInstance().Get(location.DeviceId);

                        int ticker = Environment.TickCount;
                        RECORD_INFO record = new RECORD_INFO();

                        string lc_result = "0";

                        {
                            #region 解析基站和WIFI

                            if (device.Deleted)
                                continue;
                            if (location.LocationType == 0)
                            {
                                try
                                {
                                    // <summary>
                                    // 通过WIFI及LBS获取位置信息
                                    // </summary>
                                    // <param name="key">验证码</param>
                                    // <param name="bts">连接的基站:非CDMA格式(mcc,mnc,lac,cellid,signal),CDMA格式(sid,nid,bid,lon,lat,signal 其中lon,lat可为空)，多个基站用“|”分隔</param>
                                    // <param name="wifis">热点信息:mac,signal,ssid，热点信息之间使用 “|”分隔, 请不要包 含移动 wifi 信息.</param>
                                    // <returns></returns>
//                                    var lbswifi = Logic.LBSWIFIClient.Get().GetPosition(_apiKey, location.LBS, location.WIFI);
                                    var lbswifi = APIClient.GetInstance().GetPosition(location.LBS, location.WIFI);
                                    //-1 验证失败,0 表示未获取到信息,1 wifi 数据库,2 wifi接口,11 lbs数据库 12 lbs接口
                                    if (lbswifi != null && lbswifi.Code > 0)
                                    {
                                        location.Lat = lbswifi.Lat;
                                        location.Lng = lbswifi.Lng;
                                        location.Radius = lbswifi.Radius;
                                        if (lbswifi.Code == 1 || lbswifi.Code == 2)
                                            location.LocationType = 3;
                                        else if (lbswifi.Code == 11 || lbswifi.Code == 12)
                                            location.LocationType = 2;

                                        if (location.LocationType != 0 && location.Lat != null && location.Lng != null && (int) location.Lat != 0 && (int) location.Lng != 0)
                                        {
                                            if (location.LocationType == 2)
                                            {
                                                YWLBS.GetInstance().Update(location);
                                            }
                                            else if (location.LocationType == 3)
                                            {
                                                WifiData.GetInstance().Update(location);
                                            }

                                            Count.GetInstance().LbsAndWifi();
                                            lc_result = "1";
                                        }
                                        else
                                        {
                                            Count.GetInstance().LbsAndWifiFail();
                                        }
                                    }
                                    else
                                    {
                                        Count.GetInstance().LbsAndWifiFail();
                                    }

                                    record.set('1', "GetPosition", "", 0, Environment.TickCount - ticker, 1);
                                    CPhotoStat.GetInstance().add_info(record);
                                }
                                catch (Exception ex)
                                {
                                    Count.GetInstance().LbsAndWifiFail();
                                    Logger.Error(ex);
                                    Logger.Error("LBS:" + location.LBS + "     WIFI:" + location.WIFI);

                                    record.set('1', "GetPosition", "", 0, Environment.TickCount - ticker, -1);
                                    CPhotoStat.GetInstance().add_info(record);
                                }
                            }

                            #endregion

                            ticker = Environment.TickCount;

                            #region 解析天气预报 定位请求源,0表示未定义源，1表示协议命令UD正常定位请求源，2表示协议命令UD2补偿定位请求源，3表示协议命令TQ天气预报定位请求源，4表示协议命令TPBK远程拍照相片定位请求源, 5表示协议命令AL报警数据定位请求源

                            if (location.LocationType != 0 && location.Lat != null && location.Lng != null && (int) location.Lat != 0 && (int) location.Lng != 0)
                                if (deviceState.LocationSource == 3)
                                {
                                    double wgslat = (double) location.Lat;
                                    double wgslng = (double) location.Lng;

//                                    Utility.LocationHelper.GCJToWGS84(wgslat, wgslng, out wgslat, out wgslng);

//                                    var address = LBSWIFIClient.Get().GetAddress(Utility.AppConfig.GetValue("ApiKey"), wgslat, wgslng);
                                    var address = APIClient.GetInstance().GetAddress(wgslat, wgslng);
                                    Weather weather = null;
                                    if (address != null && address.Code == 1 && !string.IsNullOrEmpty(address.City))
                                    {
                                        Model.Entity.DeviceTq tq = DeviceTq.GetInstance().Get(address.Province + "_" + address.City);
                                        if (tq != null && !string.IsNullOrWhiteSpace(tq.Info) && !string.IsNullOrWhiteSpace(tq.Temperature))
                                        {
                                            weather = new Weather()
                                            {
                                                WeatherCont = tq.Info,
                                                Temp = tq.Temperature
                                            };
                                        }
                                        else
                                        {
                                            weather = APIClient.GetInstance().Weather(address.Province, address.City);
                                            if (weather != null)
                                            {
                                                DeviceTq.GetInstance().Update(address.Province + "_" + address.City, weather.WeatherCont, weather.Temp, wgslat, wgslng);
                                            }
                                        }

                                        record.set('1', "GetAddress", "", 0, Environment.TickCount - ticker, 1);
                                        CPhotoStat.GetInstance().add_info(record);
                                    }

                                    if (weather != null)
                                    {
                                        string tqinfo = 0 + "," + (string.IsNullOrEmpty(weather.CityName) ? address.City : weather.CityName) + ","
                                                        + (string.IsNullOrEmpty(weather.WeatherCont) ? "" : weather.WeatherCont) + ","
                                                        + (string.IsNullOrEmpty(weather.Temp) ? "" : weather.Temp) + "," + location.Lat + "," + location.Lng;
                                        deviceState.TqInfo = tqinfo;
                                        deviceState.LocationSource = 0;
                                        if (deviceState.Online && deviceState.SocketId != null)
                                        {
                                            MySAE mySae = null;
                                            if (_dictMySae.TryGetValue(deviceState.SocketId.Value, out mySae))
                                            {
                                                lock (mySae)
                                                {
                                                    _adapterFactory.SendCommand(device, mySae, SendType.TqInfo, tqinfo);
                                                }
                                            }
                                        }
                                    }
                                }

                            #endregion

                            ticker = Environment.TickCount;

                            #region 解析短信定位的请求

                            if (location.LocationType != 0 && location.Lat != null && location.Lng != null && (int) location.Lat != 0 && (int) location.Lng != 0)
                                if (deviceState.LocationSource == 6)
                                {
                                    string addr = "";
                                    string info = "";

                                    double wgslat = (double) location.Lat;
                                    double wgslng = (double) location.Lng;

//                                    Utility.LocationHelper.GCJToWGS84(wgslat, wgslng, out wgslat, out wgslng);

//                                    var address = LBSWIFIClient.Get().GetAddress(Utility.AppConfig.GetValue("ApiKey"), wgslat, wgslng);
                                    var address = APIClient.GetInstance().GetAddress(wgslat, wgslng);

                                    if (address != null && address.Code == 1 && !string.IsNullOrEmpty(address.City))
                                    {
                                        StringBuilder nearby = new StringBuilder();
                                        StringBuilder strAddress = new StringBuilder();
                                        strAddress.Append(address.Province + address.City + address.District + address.Road);
                                        foreach (var item in address.Nearby)
                                        {
                                            if (nearby.Length == 0)
                                            {
                                                nearby.Append(Client.GetStr(item));
                                            }
                                            else
                                            {
                                                nearby.Append(Client.GetStr(item));
                                            }
                                        }

                                        addr = Client.GetStr(strAddress.ToString()) + nearby.ToString();
                                        lc_result = "2";
                                        info = lc_result + "," + location.Lat + "," + location.Lng + "," + addr;
                                    }

                                    if (lc_result.Equals("0")) //如果地址查询失败，也返回成功
                                    {
                                        lc_result = "0";
                                        info = "";
                                    }
                                    else if (lc_result.Equals("1"))
                                    {
                                        info = (lc_result + "," + location.Lat + "," + location.Lng);
                                    }

                                    if (deviceState.Online && deviceState.SocketId != null)
                                    {
                                        MySAE mySae = null;
                                        if (_dictMySae.TryGetValue(deviceState.SocketId.Value, out mySae))
                                        {
                                            lock (mySae)
                                            {
                                                _adapterFactory.SendCommand(device, mySae, SendType.DwInfo, info);
                                            }
                                        }
                                    }

                                    deviceState.LocationSource = 0;

                                    record.set('1', "GetAddress2", "", 0, Environment.TickCount - ticker, 1);
                                    CPhotoStat.GetInstance().add_info(record);
                                }

                            #endregion

                            ProcessDeviceLocation(device, deviceState, location);
                        }
                    }
                    else
                    {
                        Thread.Sleep(300);
                    }
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private void DoLocationGaodeLbsWifi()
        {
            while (true)
            {
                try
                {
                    Location location = null;

                    if (_queueLocationLbsWifiGaode.Count > 0)
                    {
                        lock (_queueLocationLbsWifiGaode)
                            if (_queueLocationLbsWifiGaode.Count > 0)
                                location = _queueLocationLbsWifiGaode.Dequeue();
                    }

                    if (location != null)
                    {
                        DeviceState deviceState = Logic.DeviceState.GetInstance().Get(location.DeviceId);
                        Model.Entity.Device device = Logic.Device.GetInstance().Get(location.DeviceId);

                        int ticker = Environment.TickCount;
                        RECORD_INFO record = new RECORD_INFO();


                        string lc_result = "0";

                        {
                            #region 解析基站和WIFI

                            if (deviceState == null || device == null || device.Deleted)
                                continue;
                            if (location.LocationType == 0)
                            {
                                try
                                {
                                    // <summary>
                                    // 通过WIFI及LBS获取位置信息
                                    // </summary>
                                    // <param name="bts">连接的基站:非CDMA格式(mcc,mnc,lac,cellid,signal),CDMA格式(sid,nid,bid,lon,lat,signal 其中lon,lat可为空)，多个基站用“|”分隔</param>
                                    // <param name="wifis">热点信息:mac,signal,ssid，热点信息之间使用 “|”分隔, 请不要包 含移动 wifi 信息.</param>
                                    // <returns></returns>
                                    string lbswifi = GetLatLngByLBSWifiAtGaode(device.SerialNumber, location.LBS, location.WIFI, ref location);
                                    /*if (location.WIFI.Length == 0)
                                        lbswifi = GetLatLngByMiniGPS(TransLBS2MinGPS(location.LBS), ref location);
                                    else
                                        lbswifi = GetLatLngByMiniWIFI(TransLBS2MinGPS(location.LBS), TransWifi2MinGPS(location.WIFI), ref location);
                                    */
                                    //-1 验证失败,0 表示未获取到信息,1 wifi 数据库,2 wifi接口,11 lbs数据库 12 lbs接口
                                    if (lbswifi.Length > 0)
                                    {
                                        if (location.LocationType != 0 && location.Lat != null && location.Lng != null && (int) location.Lat != 0 && (int) location.Lng != 0)
                                        {
                                            if (location.LocationType == 2)
                                            {
                                                //Logic.YWLBS.GetInstance().Update(location);
                                            }
                                            else if (location.LocationType == 3)
                                            {
                                                //Logic.WifiData.GetInstance().Update(location);
                                            }

                                            Count.GetInstance().LbsAndWifi();
                                            lc_result = "1";

                                            record.set('1', "gaode", "", 0, 0, 1);
                                            CPhotoStat.GetInstance().add_info(record);
                                        }
                                        else
                                        {
                                            Count.GetInstance().LbsAndWifiFail();
                                        }
                                    }
                                    else
                                    {
                                        Count.GetInstance().LbsAndWifiFail();
                                        record.set('1', "gaode", "", 0, 0, -1);
                                        CPhotoStat.GetInstance().add_info(record);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Count.GetInstance().LbsAndWifiFail();
                                    Logger.Error(ex);
                                    Logger.Error("LBS:" + location.LBS + "     WIFI:" + location.WIFI);
                                }
                            }

                            #endregion

                            ticker = Environment.TickCount;

                            #region 解析天气预报 定位请求源,0表示未定义源，1表示协议命令UD正常定位请求源，2表示协议命令UD2补偿定位请求源，3表示协议命令TQ天气预报定位请求源，4表示协议命令TPBK远程拍照相片定位请求源, 5表示协议命令AL报警数据定位请求源

                            if (location.LocationType != 0 && location.Lat != null && location.Lng != null && (int) location.Lat != 0 && (int) location.Lng != 0)
                                if (deviceState.LocationSource == 3)
                                {
                                    double wgslat = (double) location.Lat;
                                    double wgslng = (double) location.Lng;

//                                    var address = GetByLatLngAtGaode(Convert.ToString(location.Lat), Convert.ToString(location.Lng), ref city, ref province);
                                    var address = APIClient.GetInstance().GetAddress(wgslat, wgslng);
                                    Weather weather = null;
                                    if (address != null)
                                    {
                                        Model.Entity.DeviceTq tq = DeviceTq.GetInstance().Get(address.Province + "_" + address.City);
                                        if (tq != null && !string.IsNullOrWhiteSpace(tq.Info) && !string.IsNullOrWhiteSpace(tq.Temperature))
                                        {
                                            weather = new Weather()
                                            {
                                                Province = address.Province,
                                                City = address.City,
                                                WeatherCont = tq.Info,
                                                Temp = tq.Temperature
                                            };
                                        }
                                        else
                                        {
                                            weather = APIClient.GetInstance().Weather(address.Province, address.City);
                                            if (weather != null)
                                            {
                                                DeviceTq.GetInstance().Update(address.Province + "_" + address.City, weather.WeatherCont, weather.Temp, wgslat, wgslng);
                                            }
                                        }
                                    }

                                    if (weather != null)
                                    {
                                        string tqinfo = 0 + "," + (string.IsNullOrEmpty(weather.CityName) ? address.City : weather.CityName) + ","
                                                        + (string.IsNullOrEmpty(weather.WeatherCont) ? "" : weather.WeatherCont) + ","
                                                        + (string.IsNullOrEmpty(weather.Temp) ? "" : weather.Temp) + "," + location.Lat + "," + location.Lng;
                                        deviceState.TqInfo = tqinfo;
                                        deviceState.LocationSource = 0;
                                        if (deviceState.Online && deviceState.SocketId != null)
                                        {
                                            MySAE mySae = null;
                                            if (_dictMySae.TryGetValue(deviceState.SocketId.Value, out mySae))
                                            {
                                                lock (mySae)
                                                {
                                                    if (Logger.IsDebugEnabled)
                                                    {
                                                        Logger.Debug(String.Format("Server.cs==IMEI:{0} ,Socket ID:{1} ,IP:{2} ,UserToken:{3}", device.SerialNumber, mySae.SocketId, mySae.Ip,
                                                            mySae.UserToken));
                                                    }

                                                    _adapterFactory.SendCommand(device, mySae, SendType.TqInfo, tqinfo);
                                                }
                                            }
                                        }
                                    }
                                }

                            #endregion

                            ticker = Environment.TickCount;

                            #region 解析短信定位的请求

                            if (location.LocationType != 0 && location.Lat != null && location.Lng != null && (int) location.Lat != 0 && (int) location.Lng != 0)
                            {
                                if (deviceState.LocationSource == 6)
                                {
#pragma warning disable CS0219 // 变量“addr”已被赋值，但从未使用过它的值
                                    string addr = "";
#pragma warning restore CS0219 // 变量“addr”已被赋值，但从未使用过它的值
                                    string info = "";

                                    double wgslat = (double) location.Lat;
                                    double wgslng = (double) location.Lng;
                                    string city = "";
                                    string province = "";
                                    var address = GetByLatLngAtGaode(Convert.ToString(location.Lat), Convert.ToString(location.Lng), ref city, ref province);

                                    if (address.Length > 0)
                                    {
                                        lc_result = "2";
                                        info = lc_result + "," + location.Lat + "," + location.Lng + "," + address;
                                    }

                                    if (lc_result.Equals("0")) //如果地址查询失败，也返回成功
                                    {
                                        lc_result = "0";
                                        info = "";
                                    }
                                    else if (lc_result.Equals("1"))
                                    {
                                        info = (lc_result + "," + location.Lat + "," + location.Lng);
                                    }

                                    if (deviceState.Online && deviceState.SocketId != null)
                                    {
                                        MySAE mySae = null;
                                        if (_dictMySae.TryGetValue(deviceState.SocketId.Value, out mySae))
                                        {
                                            lock (mySae)
                                            {
                                                _adapterFactory.SendCommand(device, mySae, SendType.DwInfo, info);
                                            }
                                        }
                                    }

                                    deviceState.LocationSource = 0;
                                }
                            }

                            #endregion

                            ProcessDeviceLocation(device, deviceState, location);
                        }
                    }
                    else
                    {
                        Thread.Sleep(300);
                    }
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private void ProcessDeviceLocation(Model.Entity.Device device, DeviceState deviceState, Location location)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            if (location.LocationType != 0 && location.Lat != null && location.Lng != null && (int) location.Lat != 0 && (int) location.Lng != 0)
            {
                #region 电子围栏

                List<Model.Entity.User> sendUserList = null;
                try
                {
                    var listGeoFence = GeoFence.GetInstence().GetGeoFenceByDeviceId(deviceState.DeviceID);
                    if (listGeoFence != null && listGeoFence.Count > 0)
                    {
                        for (int i = 0; i < listGeoFence.Count; i++)
                        {
                            var item = listGeoFence[i];
                            if (item.Enable && item.Entry && item.Exit &&
                                !string.IsNullOrEmpty(item.LatAndLng))
                            {
                                if (item.LatAndLng.IndexOf('-') > 0)
                                {
                                    string[] latlngradius = item.LatAndLng.Split('-');
                                    string[] latlng = latlngradius[0].Split(',');
                                    double radius = double.Parse(latlngradius[1]);
                                    double lat = double.Parse(latlng[0]);
                                    double lng = double.Parse(latlng[1]);
                                    double distance1 =
                                        LocationHelper.GetDistance(lat, lng,
                                            Convert.ToDouble(deviceState.Latitude),
                                            Convert.ToDouble(deviceState.Longitude));
                                    double distance2 =
                                        LocationHelper.GetDistance(lat,
                                            lng, Convert.ToDouble(location.Lat),
                                            Convert.ToDouble(location.Lng));
                                    if (item.Entry)
                                    {
                                        if (distance1 > radius && distance2 < radius)
                                        {
                                            var exception = new DeviceException
                                                {
                                                    Type = 102,
                                                    DeviceID = location.DeviceId,
                                                    Content = item.FenceName,
                                                    Latitude =
                                                        location.Lat == null ? 0 : (decimal) location.Lat,
                                                    Longitude =
                                                        location.Lng == null ? 0 : (decimal) location.Lng,
                                                    CreateTime = location.CreateTime
                                                };
                                            Logic.DeviceException.GetInstance().Save(exception);
                                            if (sendUserList == null)
                                                sendUserList = UserDevice.GetInstance()
                                                    .GetUserByDeviceId(location.DeviceId);
                                            Notification.GetInstance().Send(exception, sendUserList);
                                        }
                                    }

                                    if (item.Exit)
                                    {
                                        if (distance1 < radius && distance2 > radius)
                                        {
                                            var exception = new DeviceException
                                                {
                                                    Type = 103,
                                                    DeviceID = location.DeviceId,
                                                    Content = item.FenceName,
                                                    Latitude =
                                                        location.Lat == null ? 0 : (decimal) location.Lat,
                                                    Longitude =
                                                        location.Lng == null ? 0 : (decimal) location.Lng,
                                                    CreateTime = location.CreateTime
                                                };
                                            Logic.DeviceException.GetInstance().Save(exception);
                                            if (sendUserList == null)
                                                sendUserList = UserDevice.GetInstance()
                                                    .GetUserByDeviceId(location.DeviceId);
                                            Notification.GetInstance().Send(exception, sendUserList);

                                            info.set('1', "GeoFenceNotif", "", 0, Environment.TickCount - ticker, 1);
                                            CPhotoStat.GetInstance().add_info(info);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    info.set('1', "GeoFenceProc", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                #endregion

                ticker = Environment.TickCount;

                #region 上下学守护

                try
                {
                    if (device.IsGuard)
                    {
                        Model.Entity.DeviceSet deviceSet =
                            DeviceSet.GetInstance().Get(deviceState.DeviceID);
                        string weekDisabled = deviceSet.WeekDisabled;
                        string latestTime = device.LatestTime; //最迟到家时间
                        string classDisabled1 = deviceSet.ClassDisabled1; //上午上课禁用时间段
                        string classDisabled2 = deviceSet.ClassDisabled2; //上午上课禁用时间段
                        string currentWeek = location.CreateTime.DayOfWeek != 0
                            ? Convert.ToInt32(location.CreateTime.DayOfWeek).ToString()
                            : "7"; //禁用星期，如果为周日转为7
                        if (!string.IsNullOrEmpty(weekDisabled) && !string.IsNullOrEmpty(latestTime) &&
                            !string.IsNullOrEmpty(classDisabled1) &&
                            !string.IsNullOrEmpty(classDisabled2) &&
                            weekDisabled.IndexOf(currentWeek, StringComparison.Ordinal) >= 0)
                        {
                            //开始上学时间点
                            DateTime arriveSchoolTime =
                                Convert.ToDateTime(location.CreateTime.ToString("yyyy/MM/dd ") +
                                                   classDisabled1.Split('-')[0] + ":00");
                            //开始放学时间点
                            DateTime leaveSchoolTime =
                                Convert.ToDateTime(location.CreateTime.ToString("yyyy/MM/dd ") +
                                                   classDisabled2.Split('-')[1] + ":00");
                            //最迟回家时间点
                            DateTime backtoHomeTime =
                                Convert.ToDateTime(location.CreateTime.ToString("yyyy/MM/dd ") +
                                                   latestTime +
                                                   ":00");

                            //与家的距离，double distance = Utility.LocationHelper.GetDistance(Convert.ToDouble(device.HomeLat), Convert.ToDouble(device.HomeLng), Convert.ToDouble(deviceState.Latitude), Convert.ToDouble(deviceState.Longitude));
                            //与学校的距离，double distance = Utility.LocationHelper.GetDistance(Convert.ToDouble(device.SchoolLat), Convert.ToDouble(device.SchoolLng), Convert.ToDouble(deviceState.Latitude), Convert.ToDouble(deviceState.Longitude));
                            //  decimal device.HomeLat

                            var nf = new Model.Entity.Notification
                            {
                                DeviceID = location.DeviceId,
                                Type = 0,
                                Content = ""
                            };
                            SchoolGuardian sg =
                                Logic.SchoolGuardian.GetInstance()
                                    .Get(device.DeviceID, DateTime.Now.ToString("yyyy-MM-dd"));

                            if (sg.GuardState < 2 &&
                                arriveSchoolTime.AddMinutes(-30) < location.CreateTime &&
                                location.CreateTime < arriveSchoolTime.AddMinutes(40))
                            {
                                //上学守护
                                double distance =
                                    LocationHelper.GetDistance(
                                        Convert.ToDouble(device.SchoolLat),
                                        Convert.ToDouble(device.SchoolLng), location.Lat.Value,
                                        location.Lng.Value);
                                if (((location.LocationType == 1 || location.LocationType == 3) &&
                                     distance < 500) || (location.LocationType == 2 && distance < 1000))
                                {
                                    //到校提醒
                                    nf.Type = 202;

                                    if (location.CreateTime < arriveSchoolTime.AddMinutes(5))
                                        nf.Content = "按时到校";
                                    else
                                        nf.Content = "未按时到校";
                                    sg.GuardState = 3;
                                    if (location.CreateTime < arriveSchoolTime.AddMinutes(5))
                                        sg.SchoolArriveContent = "2,按时到校";
                                    else
                                        sg.SchoolArriveContent = "3,未按时到校";
                                    sg.SchoolArriveTime = location.CreateTime.ToString("HH:mm");
                                }
                                //迟到提醒
                                else if (sg.GuardState < 1 && arriveSchoolTime < location.CreateTime)
                                {
                                    nf.Type = 200;
                                    nf.Content = "暂未到校";
                                    sg.GuardState = 1;
                                    sg.SchoolArriveContent = "1,暂未到校";
                                    sg.SchoolArriveTime = location.CreateTime.ToString("HH:mm");
                                }
                                else if (sg.GuardState < 2 &&
                                         location.CreateTime > arriveSchoolTime.AddMinutes(25))
                                {
                                    nf.Type = 201;
                                    nf.Content = "仍未到校,系统不再提醒";
                                    sg.GuardState = 2;
                                    sg.SchoolArriveContent = "1,仍未到校";
                                    sg.SchoolArriveTime = location.CreateTime.ToString("HH:mm");
                                }
                            }

                            if (sg.GuardState < 5 &&
                                leaveSchoolTime.AddMinutes(-60) < location.CreateTime)
                            {
                                double distance =
                                    LocationHelper.GetDistance(Convert.ToDouble(device.SchoolLat),
                                        Convert.ToDouble(device.SchoolLng), location.Lat.Value,
                                        location.Lng.Value);
                                //放学守护
                                if (((location.LocationType == 1 || location.LocationType == 3) &&
                                     distance > 500) || (location.LocationType == 2 && distance > 1000))
                                {
                                    //离校提醒，一次
                                    nf.Type = 205;
                                    nf.Content = "离开学校";
                                    sg.GuardState = 6;
                                    sg.SchoolLeaveContent = "2,离开学校";
                                    sg.SchoolLeaveTime = location.CreateTime.ToString("HH:mm");
                                }
                                //未离校提醒
                                else if (sg.GuardState < 4 &&
                                         leaveSchoolTime.AddMinutes(30) < location.CreateTime)
                                {
                                    nf.Type = 203;
                                    nf.Content = "暂未离校";
                                    sg.GuardState = 4;
                                    sg.SchoolLeaveContent = "1,暂未离校";
                                    sg.SchoolLeaveTime = location.CreateTime.ToString("HH:mm");
                                }
                                else if (sg.GuardState < 5 &&
                                         leaveSchoolTime.AddMinutes(60) < location.CreateTime)
                                {
                                    nf.Type = 204;
                                    nf.Content = "仍未离校,系统不再提醒";
                                    sg.GuardState = 5;
                                    sg.SchoolLeaveContent = "1,仍未离校";
                                    sg.SchoolLeaveTime = location.CreateTime.ToString("HH:mm");
                                }
                            }

                            //离开学校才能判断是否在路上停留
                            if (sg.GuardState >= 6 && sg.GuardState < 9 &&
                                leaveSchoolTime < location.CreateTime &&
                                location.CreateTime < backtoHomeTime.AddMinutes(40))
                            {
                                //到家守护
                                double distance =
                                    LocationHelper.GetDistance(Convert.ToDouble(device.HomeLat),
                                        Convert.ToDouble(device.HomeLng), location.Lat.Value,
                                        location.Lng.Value);
                                if (((location.LocationType == 1 || location.LocationType == 3) &&
                                     distance < 500) || (location.LocationType == 2 && distance < 1000))
                                {
                                    //到家提醒
                                    nf.Type = 209;
                                    if (location.CreateTime < backtoHomeTime.AddMinutes(5))
                                        nf.Content = "按时到家";
                                    else
                                        nf.Content = "未按时到家";
                                    sg.GuardState = 10;

                                    if (location.CreateTime < backtoHomeTime.AddMinutes(5))
                                        sg.HomeBackContent = "2,按时到家";
                                    else
                                        sg.HomeBackContent = "3,未按时到家";
                                    sg.HomeBackTime = location.CreateTime.ToString("HH:mm");
                                }
                                //未到家提醒
                                else if (sg.GuardState < 8 && backtoHomeTime < location.CreateTime)
                                {
                                    nf.Type = 207;
                                    nf.Content = "暂未到家";
                                    sg.GuardState = 8;
                                    sg.HomeBackContent = "1,暂未到家";
                                    sg.HomeBackTime = location.CreateTime.ToString("HH:mm");
                                }
                                else if (sg.GuardState < 9 &&
                                         backtoHomeTime.AddMinutes(25) < location.CreateTime)
                                {
                                    nf.Type = 208;
                                    nf.Content = "仍未到家,系统不再提醒";
                                    sg.GuardState = 9;
                                    sg.HomeBackContent = "1,仍未到家";
                                    sg.HomeBackTime = location.CreateTime.ToString("HH:mm");
                                }
                                else if (sg.GuardState < 7) //路上逗留先简单实现
                                {
                                    distance =
                                        LocationHelper.GetDistance(
                                            Convert.ToDouble(deviceState.Latitude),
                                            Convert.ToDouble(deviceState.Longitude), location.Lat.Value,
                                            location.Lng.Value);
                                    if (distance < 50)
                                    {
                                        double lat, lng;
                                        LocationHelper.WGS84ToGCJ(
                                            Convert.ToDouble(deviceState.Latitude),
                                            Convert.ToDouble(deviceState.Longitude), out lat, out lng);
                                        bool bremain = false;
                                        if (string.IsNullOrEmpty(sg.HomeBackContent) &&
                                            string.IsNullOrEmpty(sg.HomeBackTime))
                                        {
                                            bremain = true;
                                        }
                                        else //去相近的点去除
                                        {
                                            string[] content = sg.HomeBackContent.Split('-');
                                            string[] remain = content[content.Length - 1].Split(',');
                                            double lat2 = double.Parse(remain[3]);
                                            double lng2 = double.Parse(remain[4]);
                                            if (content.Length == 3 ||
                                                LocationHelper.GetDistance(lat2, lng2, lat, lng) <
                                                50)
                                            {
                                                bremain = false;
                                            }
                                        }

                                        if (bremain)
                                        {
                                            nf.Type = 206;
                                            nf.Content = "路上逗留";
                                            sg.GuardState = 7;

                                            if (string.IsNullOrEmpty(sg.HomeBackContent) &&
                                                string.IsNullOrEmpty(sg.HomeBackTime))
                                            {
                                                sg.HomeBackContent = "1,路上逗留," + lat.ToString("F6") +
                                                                     "," + lng.ToString("F6");
                                                sg.HomeBackTime = location.CreateTime.ToString("HH:mm");
                                            }
                                            else
                                            {
                                                sg.HomeBackContent += "-1,路上逗留," + lat.ToString("F6") +
                                                                      "," + lng.ToString("F6");
                                                sg.HomeBackTime += ("-" +
                                                                    location.CreateTime.ToString("HH:mm"));
                                            }
                                        }
                                    }
                                }
                            }

                            if (nf.Type > 0)
                            {
                                List<Model.Entity.User> userList =
                                    UserDevice.GetInstance().GetUserByDeviceId(deviceState.DeviceID);
                                Notification.GetInstance().Send(nf, userList);
                                Logic.SchoolGuardian.GetInstance().Save(sg);
                            }
                        }

                        info.set('1', "SchoolGuardNtf", "", 0, Environment.TickCount - ticker, 1);
                        CPhotoStat.GetInstance().add_info(info);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                #endregion

                ticker = Environment.TickCount;

                #region 更新deviceState

                lock (deviceState)
                {
                    deviceState.LocationType = location.LocationType;
                    deviceState.Latitude = Convert.ToDecimal(location.Lat);
                    deviceState.Longitude = Convert.ToDecimal(location.Lng);
                    deviceState.Radius = location.Radius != null ? location.Radius.Value : 0;
                    deviceState.ServerTime = location.CreateTime;
                    deviceState.Wifi = location.WIFI;
                    deviceState.LBS = location.LBS;
                    deviceState.Altitude = location.Altitude != null ? location.Altitude.Value : 0;
                    deviceState.Course = location.Course != null
                        ? Convert.ToDecimal(location.Course.Value)
                        : 0;
                    deviceState.Speed = location.Speed != null ? Convert.ToDecimal(location.Speed.Value) : 0;
                    if (location.Time != null)
                        deviceState.DeviceTime = location.Time;

                    info.set('1', "deviceStateUpd", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);
                }

                #endregion

                ticker = Environment.TickCount;

                #region 存在报警信息

                if (location.Status != 0)
                {
                    if (sendUserList == null)
                    {
                        sendUserList = UserDevice.GetInstance() .GetUserByDeviceId(location.DeviceId);
                    }

                    if ((location.Status & (Int32) Math.Pow(2, 18)) == (Int32) Math.Pow(2, 18)) //exceptionText = "通话位置报告"
                    {
                        var exception = new DeviceException
                        {
                            Type = 106,
                            DeviceID = location.DeviceId,
                            Content = "",
                            Latitude = location.Lat == null ? 0 : (decimal) location.Lat,
                            Longitude = location.Lng == null ? 0 : (decimal) location.Lng,
                            CreateTime = location.CreateTime
                        };
                        Logic.DeviceException.GetInstance().Save(exception);
                        Notification.GetInstance().Send(exception, sendUserList);
                    }

                    if ((location.Status & (Int32) Math.Pow(2, 19)) == (Int32) Math.Pow(2, 19)) //exceptionText = "手表脱落"
                    {
                        var exception = new DeviceException
                        {
                            Type = 101,
                            DeviceID = location.DeviceId,
                            Content = "",
                            Latitude = location.Lat == null ? 0 : (decimal) location.Lat,
                            Longitude = location.Lng == null ? 0 : (decimal) location.Lng,
                            CreateTime = location.CreateTime
                        };
                        Logic.DeviceException.GetInstance().Save(exception);
                        Notification.GetInstance().Send(exception, sendUserList);
                    }

                    /*if ((location.Status & (Int32) Math.Pow(2, 22)) == (Int32) Math.Pow(2, 22)) //exceptionText = "低电"
                    {
                        var exception = new Model.Entity.DeviceException
                        {
                            Type = 104,
                            DeviceID = location.DeviceId,
                            Content = "",
                            Latitude = location.Lat == null ? 0 : (decimal) location.Lat,
                            Longitude = location.Lng == null ? 0 : (decimal) location.Lng,
                            CreateTime = location.CreateTime
                        };
                        Logic.DeviceException.GetInstance().Save(exception);
                        Notification.GetInstance().Send(exception, sendUserList);
                    }*/

                    if ((location.Status & (Int32) Math.Pow(2, 20)) == (Int32) Math.Pow(2, 20)) //exceptionText = "SOS"
                    {
                        var exception = new DeviceException
                        {
                            Type = 105,
                            DeviceID = location.DeviceId,
                            Content = "",
                            Latitude = location.Lat == null ? 0 : (decimal) location.Lat,
                            Longitude = location.Lng == null ? 0 : (decimal) location.Lng,
                            CreateTime = location.CreateTime
                        };
                        Logic.DeviceException.GetInstance().Save(exception);
                        Notification.GetInstance().Send(exception, sendUserList);
                    }
                    //int nn = location.Status & 262144;
                    //for (int i = 8; i < 14; i++)
                    //{
                    //    //int exceptionType = 0;
                    //    //string exceptionText = "";
                    //    int notificationType = 0;
                    //    if (location.Status.Substring(i, 1) == "1")
                    //    {
                    //        switch (i)
                    //        {
                    //            case 13:
                    //                //exceptionType = 31;
                    //                //exceptionText = "通话位置报告";0000 0000 0000 0100 0000 0000 0000 0000
                    //                notificationType = 106;
                    //                break;
                    //            case 12:
                    //                //exceptionType = 31;
                    //                //exceptionText = "手表脱落";    0000 0000 0000 1000 0000 0000 0000 0000
                    //                notificationType = 101;
                    //                break;
                    //            //case 12:
                    //            //    //exceptionType = 4;
                    //            //    //exceptionText = "进围栏";
                    //            //    notificationType = 102;
                    //            //    break;
                    //            //case 13:
                    //            //    //exceptionType = 5;
                    //            //    //exceptionText = "出围栏";
                    //            //    notificationType = 103;
                    //            //    break;
                    //            case 9:
                    //                //exceptionType = 10;
                    //                //exceptionText = "低电";         0000 0000 0100 0000 0000 0000 0000 0000
                    //                notificationType = 104;
                    //                break;
                    //            case 11:
                    //                //exceptionType = 1;
                    //                //exceptionText = "SOS";          0000 0000 0001 0000 0000 0000 0000 0000
                    //                notificationType = 105;
                    //                break;
                    //        }
                    //    }
                    //    if (notificationType > 0)
                    //    {
                    //        Model.Entity.DeviceException exception = new Model.Entity.DeviceException
                    //        {
                    //            Type = notificationType,
                    //            DeviceID = location.DeviceId,
                    //            Content = "",
                    //            Latitude = location.Lat == null ? 0 : (decimal) location.Lat,
                    //            Longitude = location.Lng == null ? 0 : (decimal) location.Lng,
                    //            CreateTime = location.CreateTime
                    //        };
                    //        Logic.DeviceException.GetInstance().Save(exception);
                    //        Notification.GetInstance().Send(exception, sendUserList);
                    //    }
                    //}

                    info.set('1', "deviceExcep", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);
                }

                ticker = Environment.TickCount;

                #endregion

                #region 历史轨迹

                Enqueue2HistoryQueue(location);

                #endregion
            }
        }

        private void Enqueue2HistoryQueue(Location location)
        {
            if (location != null && _history)
            {
                if (location.LocationType != 0 && location.Lat != null && location.Lng != null &&
                    location.Lat != 0 && location.Lng != 0)
                {
                    lock (_insertQueue)
                    {
                        _insertTempQueue.Enqueue(location);
                        if (_insertTempQueue.Count >= InsertListSize)
                        {
                            _insertQueue.Enqueue(_insertTempQueue);
                            _insertTempQueue = new Queue<Location>();
                        }
                    }

                    //Data.Logger.Info(String.Format("location解析正确 imei:{0} time:{1} lbs:{2} wifi:{3}", device.SerialNumber, location.Time, location.LBS, location.WIFI));
                }
                else
                {
                    Logger.Info(String.Format("location解析失败 imei:{0} DeviceId:{1} lbs:{2} wifi:{3}", location.DeviceId, location.Time, location.LBS, location.WIFI));
                }
            }
        }


        private void _adapterFactory_OnLocation(Location location)
        {
            lock (_queueLocation)
            {
                _queueLocation.Enqueue(location);
            }
        }

        private void _adapterFactory_OnLocationLbsWifi(Location location)
        {
            if (_queueLocationLbsWifi.Count < 1000)
            {
                lock (_queueLocationLbsWifi)
                {
                    _queueLocationLbsWifi.Enqueue(location);
                }
            }
            else
            {
                _adapterFactory_OnLocationGaode(location);
            }
        }


        private void _adapterFactory_OnLocationGaode(Location location)
        {
            if (_queueLocationLbsWifiGaode.Count < 1000)
            {
                lock (_queueLocationLbsWifiGaode)
                {
                    _queueLocationLbsWifiGaode.Enqueue(location);
                }
            }
        }

        bool Client_OnSend(Model.Entity.Device device, SendType commandType, string paramter)
        {
            var deviceState = Logic.DeviceState.GetInstance().Get(device.DeviceID);
            if (deviceState.Online && deviceState.SocketId != null)
            {
                MySAE mySae = null;
                if (_dictMySae.TryGetValue(deviceState.SocketId.Value, out mySae))
                {
                    lock (mySae)
                    {
                        _adapterFactory.SendCommand(device, mySae, commandType, paramter);
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        void _listen_OnAcceptConnect(MySAE mySae)
        {
            lock (_dictMySae)
            {
                _dictMySae.Add(mySae.SocketId, mySae);
            }
        }

        bool _listen_OnMsgReceived(MySAE mySae, byte[] bytes, int startOffset, int length, ref int msgLength)
        {
            return _adapterFactory.Initialize(mySae, bytes, startOffset, length, ref msgLength);
        }

        void _listen_OnDisConnect(int deviceId)
        {
            var deviceState = Logic.DeviceState.GetInstance().Get(deviceId);
            if (deviceState.Online && deviceState.SocketId != null)
            {
                MySAE mySae = null;
                if (_dictMySae.TryGetValue(deviceState.SocketId.Value, out mySae))
                {
                    lock (mySae)
                    {
                        mySae.RecoverHandler(mySae);
                    }
                }
            }
        }

        void _listen_OnDisConnect(MySAE mySae)
        {
            lock (mySae)
            {
                lock (_dictMySae)
                {
                    _dictMySae.Remove(mySae.SocketId);
                }

                if (mySae.DeviceState != null)
                {
                    lock (mySae.DeviceState)
                    {
                        if (mySae.DeviceState.SocketId.Equals(mySae.SocketId))
                        {
                            mySae.DeviceState.Online = false;
                            mySae.DeviceState.SocketId = null;
                            mySae.DeviceState.UpdateTime = DateTime.Now;
                            if (_log && Logger.IsDebugEnabled)
                                Logger.Debug(String.Format("设备离线:[{0}]", mySae.DeviceState.DeviceID));
                        }
                    }
                }
            }
        }

        public void Start()
        {
            _listen.Start();
            _clientHost.Open();
            _fileHost.Open();
            _timerThread.Start();

            foreach (Thread t in _threadDoLocation)
            {
                t.Start();
            }

            foreach (Thread t in _threadDoLocationLbsWifi)
            {
                t.Start();
            }

            foreach (Thread t in _gaodeThread)
            {
                t.Start();
            }

            if (_history)
            {
                _insertThread.Start();
            }

            //_collectThread.Start();
        }

        public void Stop()
        {
            _listen.Stop();
            _listen.Dispose();
            _clientHost.Close();
            _fileHost.Close();
            try
            {
                _timerThread.Abort();
            }
            catch
            {
            }

            try
            {
                Logic.DeviceState.GetInstance().Save();
                Count.GetInstance().Save();
                DeviceTq.GetInstance().Save();
            }
            catch
            {
            }

            foreach (Thread t in _threadDoLocation)
            {
                try
                {
                    t.Abort();
                }
                catch
                {
                }
            }

            foreach (Thread t in _threadDoLocationLbsWifi)
            {
                try
                {
                    t.Abort();
                }
                catch
                {
                }
            }

            foreach (Thread t in _gaodeThread)
            {
                try
                {
                    t.Abort();
                }
                catch
                {
                }
            }

            if (_history)
            {
                try
                {
                    _insertThread.Abort();
                }
                catch
                {
                }
            }

            try
            {
                //_collectThread.Abort();
            }
            catch
            {
            }
        }
    }
}