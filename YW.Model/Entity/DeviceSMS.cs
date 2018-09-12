using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//DeviceSMS
	[Serializable]
	public class DeviceSMS
	{
      	private int _devicesmsid;
		/// <summary>
		/// 设备短信编号
        /// </summary>		
        public int DeviceSMSID
        {
            get{ return _devicesmsid; }
            set{ _devicesmsid = value; }
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
        /// 类型 1 设备发送 2 设备接收
        /// </summary>		
        public int Type
        {
            get { return _type; }
            set { _type = value; }
        }        
        private int _state;
		/// <summary>
        /// 状态 0 未发送，1已发送 2发送成功 3发送失败
        /// </summary>		
        public int State
        {
            get{ return _state; }
            set { _state = value; }
        }
		private string _phone;
		/// <summary>
		/// 号码
        /// </summary>		
        public string Phone
        {
            get{ return _phone; }
            set{ _phone = value; }
        }        
		private string _sms;
		/// <summary>
		/// 短信内容
        /// </summary>		
        public string SMS
        {
            get{ return _sms; }
            set{ _sms = value; }
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
            get { return _updatetime; }
            set { _updatetime = value; }
        }        
	}
}

