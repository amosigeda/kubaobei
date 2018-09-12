using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Manage
{
	 	//DealerDevice
    [Serializable]
    public class DealerDevice : Entity.DealerDevice
    {
        private string _serialnumber;
        /// <summary>
        /// 设备号
        /// </summary>		
        public string SerialNumber
        {
            get { return _serialnumber; }
            set { _serialnumber = value; }
        }
        private int _devicemodelid;
        /// <summary>
        /// 设备类型
        /// </summary>	
        public int DeviceModelID
        {
            get { return _devicemodelid; }
            set { _devicemodelid = value; }
        }
        private string _bindnumber;
        /// <summary>
        /// 设备绑定号
        /// </summary>		
        public string BindNumber
        {
            get { return _bindnumber; }
            set { _bindnumber = value; }
        }
        private string _devicenote;
        /// <summary>
        /// 设备备注
        /// </summary>		
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

        private int _devicetype;
        /// <summary>
        /// 设备类型
        /// </summary>	
        public int DeviceType
        {
            get { return _devicetype; }
            set { _devicetype = value; }
        }

        private int _state;
        /// <summary>
        /// 0未激活,1已激活
        /// </summary>	
        public int State
        {
            get { return _state; }
            set { _state = value; }
        }  
        private DateTime? _activedate;
        /// <summary>
        /// 设备激活时间
        /// </summary>		
        public DateTime? ActiveDate
        {
            get { return _activedate; }
            set { _activedate = value; }
        }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool Online { get; set; }

        /// <summary>
        /// 设备时间
        /// </summary>	
        public DateTime? DeviceTime { get; set; }

        /// <summary>
        /// 否定位类型,0表示没定位，1表示GPS定位，2表示LBS定位，3表示WIFI定位
        /// </summary>
        public int LocationType { get; set; }

        public string PurchaserName { get; set; }

        public int UserCount { get; set; }
    }
}

