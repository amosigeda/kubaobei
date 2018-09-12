using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//DeviceException
	[Serializable]
	public class DeviceException
	{
      	private int _deviceexceptionid;
		/// <summary>
		/// 设备异常信息编号
        /// </summary>		
        public int DeviceExceptionID
        {
            get{ return _deviceexceptionid; }
            set{ _deviceexceptionid = value; }
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
		/// 异常类型
        /// </summary>		
        public int Type
        {
            get{ return _type; }
            set{ _type = value; }
        }        
		private string _content;
		/// <summary>
		/// 异常内容
        /// </summary>		
        public string Content
        {
            get{ return _content; }
            set{ _content = value; }
        }        
		private decimal _latitude;
		/// <summary>
		/// 发生异常的纬度
        /// </summary>		
        public decimal Latitude
        {
            get{ return _latitude; }
            set{ _latitude = value; }
        }        
		private decimal _longitude;
		/// <summary>
		/// 发生异常的精度
        /// </summary>		
        public decimal Longitude
        {
            get{ return _longitude; }
            set{ _longitude = value; }
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
		   
	}
}

