using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//Notification
	[Serializable]
	public class Notification
	{
      	private int _notificationid;
		/// <summary>
		/// NotificationID
        /// </summary>		
        public int NotificationID
        {
            get{ return _notificationid; }
            set{ _notificationid = value; }
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
        /// 通知类型 Type:1 语音信息，2发信息给管理员关联确认，3管理员确认关联，4管理员拒绝关联, 5设备升级成功 6设备配置已经同步 7设备通讯录已经同步 8设备收到短信 9解除关联 10更新设备信息 100以上的都是报警信息
        /// </summary>		
        public int Type
        {
            get{ return _type; }
            set{ _type = value; }
        }        
		private string _content;
		/// <summary>
		/// Content
        /// </summary>		
        public string Content
        {
            get{ return _content; }
            set{ _content = value; }
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

