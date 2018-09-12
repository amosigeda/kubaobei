using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
using System.Reflection;

namespace YW.Model.Entity{
	 	//DeviceContact
	[Serializable]
	public class DeviceContact : IEquatable<Object>
	{
      	private int _devicecontactid;
		/// <summary>
		/// 通讯录编号
        /// </summary>		
        public int DeviceContactId
        {
            get{ return _devicecontactid; }
            set{ _devicecontactid = value; }
        }        
		private int _deviceid;
		/// <summary>
		/// 设备编号
        /// </summary>		
        public int DeviceID
        {
            get{ return _deviceid; }
            set{ _deviceid = value; }
        }        
		private int _type;
		/// <summary>
		/// 1 表示普通联系人 2 表示用户 3表示手表
        /// </summary>		
        public int Type
        {
            get{ return _type; }
            set{ _type = value; }
        }        
		private int _objectid;
		/// <summary>
		/// 当 Type=2 的时候 这个是用户编号 当 Type=3 的 设备编号
        /// </summary>		
        public int ObjectId
        {
            get{ return _objectid; }
            set{ _objectid = value; }
        }        
		private string _name;
		/// <summary>
		/// 名称
        /// </summary>		
        public string Name
        {
            get{ return _name; }
            set{ _name = value; }
        }        
		private string _phonenumber;
		/// <summary>
		/// 手机号码
        /// </summary>		
        public string PhoneNumber
        {
            get{ return _phonenumber; }
            set{ _phonenumber = value; }
        }        
		private string _phoneshort;
		/// <summary>
		/// 短号码
        /// </summary>		
        public string PhoneShort
        {
            get{ return _phoneshort; }
            set{ _phoneshort = value; }
        }        
		private int _photo;
		/// <summary>
		/// 头像编号
        /// </summary>		
        public int Photo
        {
            get{ return _photo; }
            set{ _photo = value; }
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
        /// <summary>
        /// 真人头像
        /// </summary>
        private string _HeadImg;

        public string HeadImg
        {
            get { return _HeadImg; }
            set { _HeadImg = value; }
        }
        /// <summary>
        /// 头像版本号
        /// </summary>
        private int _HeadImgVersion;

        public int HeadImgVersion
        {
            get { return _HeadImgVersion; }
            set
            {
                if (value <= 999)
                    _HeadImgVersion = value;
                else
                    _HeadImgVersion = 1;
            }
        }

	    /// <summary>
	    /// 云平台中间号
	    /// </summary>
	    private string _AgentNumber;

	    public string AgentNumber
	    {
	        get => _AgentNumber;
	        set => _AgentNumber = value;
	    }

	    /// <summary>
	    /// 云平台呼出中间号
	    /// </summary>
	    private string _callOutNumber;

	    public string CallOutNumber
	    {
	        get => _callOutNumber;
	        set => _callOutNumber = value;
	    }

	    private int _sync;

	    public int Sync
	    {
	        get => _sync;
	        set => _sync = value;
	    }

	    private int _userId;

	    public int UserId
	    {
	        get => _userId;
	        set => _userId = value;
	    }

	    public override int GetHashCode()
	    {
	        return _devicecontactid;
	    }

	    public override bool Equals(object obj)
	    {
	        return obj is DeviceContact && _devicecontactid.Equals(((DeviceContact)obj)._devicecontactid);
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

