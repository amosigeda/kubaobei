using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Runtime.Serialization;

namespace YW.Model.Entity{
	 	//Device
    [DataContract]
	public class Device
	{
      	private int _deviceid;
		/// <summary>
		/// 设备编号
        /// </summary>	
        [DataMember]	
        public int DeviceID
        {
            get{ return _deviceid; }
            set{ _deviceid = value; }
        }        
		private string _serialnumber;
		/// <summary>
		/// 设备号
        /// </summary>		
        [DataMember]	
        public string SerialNumber
        {
            get{ return _serialnumber; }
            set{ _serialnumber = value; }
        }        
		private int _state;
		/// <summary>
		/// 0未激活,1已激活
        /// </summary>	
        [DataMember]		
        public int State
        {
            get{ return _state; }
            set{ _state = value; }
        }

        private int _deviceflag;
        /// <summary>
        /// 设备标记的状态 1设备需要恢复出厂设置
        /// </summary>	
        [DataMember]
        public int Deviceflag
        {
            get
            {
                return _deviceflag;
            }
            set { _deviceflag = value; }
        }

        private bool _deleted;
		/// <summary>
		/// 是否删除
        /// </summary>		
        public bool Deleted
        {
            get{ return _deleted; }
            set{ _deleted = value; }
        }        
		private int _userid;
		/// <summary>
		/// 管理员
        /// </summary>		
        public int UserId
        {
            get{ return _userid; }
            set{ _userid = value; }
        }        
		private int _devicemodelid;
		/// <summary>
		/// 设备类型
        /// </summary>	
        [DataMember]		
        public int DeviceModelID
        {
            get{ return _devicemodelid; }
            set{ _devicemodelid = value; }
        }        
		private string _bindnumber;
		/// <summary>
		/// 设备绑定号
        /// </summary>		
        [DataMember]	
        public string BindNumber
        {
            get{ return _bindnumber; }
            set{ _bindnumber = value; }
        }        
		private string _firmware;
		/// <summary>
		/// 需要升级的固件版本号
        /// </summary>		
        public string Firmware
        {
		    get
		    {
                if (_firmware == null)
		            return "";
                else
                    return _firmware;
		    }
            set{ _firmware = value; }
        }        
		private string _currentfirmware;
		/// <summary>
		/// 当前固件版本号
        /// </summary>		
        public string CurrentFirmware
        {
            get
            {
                if (_currentfirmware == null)
                    return "";
                else
                    return _currentfirmware;
            }
            set{ _currentfirmware = value; }
        }
        private int _setVersionNO;
        /// <summary>
        /// 设备配置版本号
        /// </summary>		
        public int SetVersionNO
        {
            get { return _setVersionNO; }
            set
            {
                if (value <= 9999)
                    _setVersionNO = value;
                else
                    _setVersionNO = 1;
            }
        }
        private int _contactVersionNO;
        /// <summary>
        /// 通信录版本号
        /// </summary>		
        public int ContactVersionNO
        {
            get { return _contactVersionNO; }
            set
            {
                if (value <= 9999)
                    _contactVersionNO = value;
                else
                    _contactVersionNO = 1;
            }
        }
        private int _operatorType;
        /// <summary>
        /// 运营商类型
        /// </summary>		
        public int OperatorType
        {
            get { return _operatorType; }
            set { _operatorType = value; }
        }
        private string _smsNumber;
        /// <summary>
        /// 查询流量及短信的运营商号码
        /// </summary>		
        public string SmsNumber
        {
            get
            {
                if (_smsNumber == null)
                    return "";
                else
                    return _smsNumber;
            }
            set { _smsNumber = value; }
        }
        private string _smsBalanceKey;
        /// <summary>
        /// 查询话费的指令
        /// </summary>		
        public string SmsBalanceKey
        {
            get
            {
                if (_smsBalanceKey == null)
                    return "";
                else
                    return _smsBalanceKey;
            }
            set { _smsBalanceKey = value; }
        }
        private string _smsFlowKey;
        /// <summary>
        /// 查询流量的指令
        /// </summary>		
        public string SmsFlowKey
        {
            get
            {
                if (_smsFlowKey == null)
                    return "";
                else
                    return _smsFlowKey;
            }
            set { _smsFlowKey = value; }
        }
		private string _photo;
		/// <summary>
		/// 宝贝头像地址
        /// </summary>		
        public string Photo
        {
            get
            {
                if (_photo == null)
                    return "";
                else
                    return _photo;
            }
            set{ _photo = value; }
        }        
		private string _babyname;
		/// <summary>
		/// 设备名称，宝贝名称
        /// </summary>		
        public string BabyName
        {
            get
            {
                if (_babyname == null)
                    return "";
                else
                    return _babyname;
            }
            set{ _babyname = value; }
        }        
		private string _phonenumber;
		/// <summary>
		/// 设备电话号码
        /// </summary>
		[DataMember]
        public string PhoneNumber
        {
            get
            {
                if (_phonenumber == null)
                    return "";
                else
                    return _phonenumber;
            }
            set{ _phonenumber = value; }
        }        
		private string _phonecornet;
		/// <summary>
		/// 短号、亲情号
        /// </summary>		
        public string PhoneCornet
        {
            get
            {
                if (_phonecornet == null)
                    return "";
                else
                    return _phonecornet;
            }
            set{ _phonecornet = value; }
        }        
		private bool _gender;
		/// <summary>
		/// 宝贝性别：真为男孩，假为女孩
        /// </summary>		
        public bool Gender
        {
            get{ return _gender; }
            set{ _gender = value; }
        }        
		private DateTime? _birthday;
		/// <summary>
		/// 生日
        /// </summary>		
        public DateTime? Birthday
        {
            get{ return _birthday; }
            set{ _birthday = value; }
        }        
		private int _grade;
		/// <summary>
		/// 宝贝读书年纪，从未读书：0，幼儿园小：1、中：2、大：3班，学前班：4，到小学六年级：10，到其他：11
        /// </summary>		
        public int Grade
        {
            get{ return _grade; }
            set{ _grade = value; }
        }        
		private string _schooladdress;
		/// <summary>
		/// 学校地址
        /// </summary>		
        public string SchoolAddress
        {
            get
            {
                if (_schooladdress == null)
                    return "";
                else
                    return _schooladdress;
            }
            set{ _schooladdress = value; }
        }        
		private decimal? _schoollng;
		/// <summary>
		/// 学校经度
        /// </summary>		
        public decimal? SchoolLng
        {
            get{ return _schoollng; }
            set{ _schoollng = value; }
        }        
		private decimal? _schoollat;
		/// <summary>
		/// 学校纬度
        /// </summary>		
        public decimal? SchoolLat
        {
            get{ return _schoollat; }
            set{ _schoollat = value; }
        }        
		private string _homeaddress;
		/// <summary>
		/// 家地址
        /// </summary>		
        public string HomeAddress
        {
            get
            {
                if (_homeaddress == null)
                    return "";
                else
                    return _homeaddress;
            }
            set{ _homeaddress = value; }
        }        
		private decimal? _homelng;
		/// <summary>
		/// 家经度
        /// </summary>		
        public decimal? HomeLng
        {
            get{ return _homelng; }
            set{ _homelng = value; }
        }        
		private decimal? _homelat;
		/// <summary>
		/// 家纬度
        /// </summary>		
        public decimal? HomeLat
        {
            get{ return _homelat; }
            set{ _homelat = value; }
        }        
		private bool _isguard;
		/// <summary>
		/// 是否开启守护功能，开启守护功能有消息提醒。
        /// </summary>		
        public bool IsGuard
        {
            get{ return _isguard; }
            set{ _isguard = value; }
        }   
		private string _password;
		/// <summary>
		/// 预留，设备密码
        /// </summary>		
        public string Password
        {
            get
            {
                if (_password == null)
                    return "";
                else
                    return _password;
            }
            set{ _password = value; }
        }

        private string _devicenote;
        /// <summary>
        /// 设备备注
        /// </summary>		
        [DataMember]
        public string DeviceNote
        {
            get
            {
                if (_devicenote == null)
                    return "";
                else
                    return _devicenote;
            }
            set { _devicenote = value; }
        }

        private string _devicecustomer;
        /// <summary>
        /// 设备备注
        /// </summary>		
        [DataMember]
        public string DeviceCustomer
        {
            get
            {
                if (_devicecustomer == null)
                    return "";
                else
                    return _devicecustomer;
            }
            set { _devicecustomer = value; }
        }

        private string _deviceproject;
        /// <summary>
        /// 设备备注
        /// </summary>		
        [DataMember]
        public string DeviceProject
        {
            get
            {
                if (_deviceproject == null)
                    return "";
                else
                    return _deviceproject;
            }
            set { _deviceproject = value; }
        }

        private int _devicetype;
        /// <summary>
        /// 设备类型
        /// </summary>	
        [DataMember]
        public int DeviceType
        {
            get { return _devicetype; }
            set { _devicetype = value; }
        }

        private DateTime? _activedate;
		/// <summary>
        /// 设备激活时间
        /// </summary>		
        [DataMember]	
        public DateTime? ActiveDate
        {
            get{ return _activedate; }
            set{ _activedate = value; }
        }        
		private DateTime? _hirestartdate;
		/// <summary>
        /// 服务开始时间
        /// </summary>		
        public DateTime? HireStartDate
        {
            get{ return _hirestartdate; }
            set{ _hirestartdate = value; }
        }        
		private DateTime? _hireexpiredate;
		/// <summary>
		/// 服务到期时间
        /// </summary>		
        public DateTime? HireExpireDate
        {
            get{ return _hireexpiredate; }
            set{ _hireexpiredate = value; }
        }        
		private DateTime _createtime;
		/// <summary>
		/// CreateTime
        /// </summary>		
        public DateTime CreateTime
        {
            get{ return _createtime; }
            set{ _createtime = value; }
        }        
		private DateTime _updatetime;
		/// <summary>
		/// UpdateTime
        /// </summary>		
        public DateTime UpdateTime
        {
            get{ return _updatetime; }
            set{ _updatetime = value; }
        }
        private string latestTime;
        /// <summary>
        /// 最晚到家时间
        /// </summary>
        public string LatestTime
        {
            get
            {
                if (latestTime == null)
                    return "";
                else
                    return latestTime;
            }
            set { latestTime = value; }
        }

        private int _cloudPlatform;

        public int CloudPlatform {
            get {
                return _cloudPlatform;
            }
            set {
                _cloudPlatform = value;
            }
        }

	    private int _lengthCountType;

	    /// <summary>
	    /// 指令长度的计算方式
	    /// Constants.LENGTH_COUNT_BYTE 为字节计算方式。
	    /// Constants.LENGTH_COUNT_CHAR 为字符计算方式。
	    /// </summary>
	    public int LengthCountType
	    {
	        get => _lengthCountType;
	        set => _lengthCountType = value;
	    }

	    public override string ToString()
        {
            PropertyInfo[] propertyInfoList = GetType().GetProperties();
            string result = "";
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                result += string.Format("{0}={1} ", propertyInfo.Name, propertyInfo.GetValue(this, null));
            }

            return result;
        }
    }
}

