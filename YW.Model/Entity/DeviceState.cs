using System;
using System.Data;
namespace YW.Model.Entity{
    //DeviceState
    [Serializable]
    public class DeviceState
    {
        /// <summary>
        /// 设备表，ID号
        /// </summary>		
        public int _deviceID;

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool _online;

        /// <summary>
        /// 连接编号
        /// </summary>
        public Guid? _socketId;

        /// <summary>
        /// 服务器时间
        /// </summary>	
        public DateTime? _serverTime;

        /// <summary>
        /// 设备时间
        /// </summary>	
        public DateTime? _deviceTime;

        /// <summary>
        /// 纬度
        /// </summary>		
        public decimal? latitude;

        /// <summary>
        /// 经度
        /// </summary>		
        public decimal? _longitude;

        /// <summary>
        /// Speed
        /// </summary>		
        public decimal _speed;

        /// <summary>
        /// 方向
        /// </summary>		
        public decimal _course;

        /// <summary>
        /// 海拔，单位：米
        /// </summary>		
        public double _altitude;

        /// <summary>
        /// 卫星数量
        /// </summary>		
        public int _satelliteNumber;

        /// <summary>
        /// 通信卡信号：0-100
        /// </summary>		
        public int _GSM;

        /// <summary>
        /// 整数，表示百分比
        /// </summary>		
        public int _electricity;

        /// <summary>
        /// 
        /// </summary>		
        public string _tqInfo;


        /// <summary>
        /// 定位请求源,0表示未定义源，1表示协议命令UD正常定位请求源，2表示协议命令UD2补偿定位请求源，3表示协议命令TQ天气预报定位请求源，4表示协议命令TPBK远程拍照相片定位请求源, 5表示协议命令AL报警数据定位请求源
        /// </summary>
        public int _locationSource;

        /// <summary>
        /// 定位类型,0表示没定位，1表示GPS定位，2表示LBS定位，3表示WIFI定位
        /// </summary>
        public int _locationType;

        /// <summary>
        /// 此字段用来存放基站数据：基站数量|基站TA-国家码(MCC)-运营商(MNC)|基站1区域码(LAC)-基站1编码(CellID)-基站1信号强度|基站2区域码(LAC)-基站2编码(CellID)-基站2信号强度|基站n区域码(LAC)-基站n编码(CellID)-基站n信号强度。如：2|1-460-02|465-797-90|6864-789-80
        /// </summary>		
        public string _LBS;

        /// <summary>
        /// 此字段描述wifi序列信息：wifi数量|wifi1名称-MAC地址-信号强度|wifi2-n名称-MAC地址-信号强度。如：3|abc-1c:fa:68:13:a5:b4-80|abc-1c:fa:68:13:a5:b4-80
        /// </summary>		
        public string _wifi;

        /// <summary>
        /// 整数，LBS和WIFI定位的时候半径
        /// </summary>		
        public int _radius;

        /// <summary>
        /// 布尔型，控制是否继续对终端发送语音包，true表示停止发送，false表示可以继续发送语音包
        /// </summary>		
        public bool _stopSendVoice;

        /// <summary>
        ///计步数
        /// </summary>		
        public string _step;

        /// <summary>
        ///健康参数
        /// </summary>		
        public string _health;

        /// <summary>
        /// CreateTime
        /// </summary>		
        public DateTime _createTime;

        /// <summary>
        /// UpdateTime
        /// </summary>		
        public DateTime _updateTime;

        public bool _isLowPowerAlarmed;

        public int DeviceID
        {
            get => _deviceID;
            set => _deviceID = value;
        }

        public bool Online
        {
            get => _online;
            set => _online = value;
        }

        public Guid? SocketId
        {
            get => _socketId;
            set => _socketId = value;
        }

        public DateTime? ServerTime
        {
            get => _serverTime;
            set => _serverTime = value;
        }

        public DateTime? DeviceTime
        {
            get => _deviceTime;
            set => _deviceTime = value;
        }

        public decimal? Latitude
        {
            get => latitude;
            set => latitude = value;
        }

        public decimal? Longitude
        {
            get => _longitude;
            set => _longitude = value;
        }

        public decimal Speed
        {
            get => _speed;
            set => _speed = value;
        }

        public decimal Course
        {
            get => _course;
            set => _course = value;
        }

        public double Altitude
        {
            get => _altitude;
            set => _altitude = value;
        }

        public int SatelliteNumber
        {
            get => _satelliteNumber;
            set => _satelliteNumber = value;
        }

        public int GSM
        {
            get => _GSM;
            set => _GSM = value;
        }

        public int Electricity
        {
            get => _electricity;
            set => _electricity = value;
        }

        public string TqInfo
        {
            get => _tqInfo;
            set => _tqInfo = value;
        }

        public int LocationSource
        {
            get => _locationSource;
            set => _locationSource = value;
        }

        public int LocationType
        {
            get => _locationType;
            set => _locationType = value;
        }

        public string LBS
        {
            get => _LBS;
            set => _LBS = value;
        }

        public string Wifi
        {
            get => _wifi;
            set => _wifi = value;
        }

        public int Radius
        {
            get => _radius;
            set => _radius = value;
        }

        public bool StopSendVoice
        {
            get => _stopSendVoice;
            set => _stopSendVoice = value;
        }

        public string Step
        {
            get => _step;
            set => _step = value;
        }

        public string Health
        {
            get => _health;
            set => _health = value;
        }

        public DateTime CreateTime
        {
            get => _createTime;
            set => _createTime = value;
        }

        public DateTime UpdateTime
        {
            get => _updateTime;
            set => _updateTime = value;
        }

        public bool IsLowPowerAlarmed
        {
            get => _isLowPowerAlarmed;
            set => _isLowPowerAlarmed = value;
        }
    }
}

