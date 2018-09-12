using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//UserNotification
	[Serializable]
	public class UserNotification
	{
        private int _usernotificationid;
        /// <summary>
        /// 通知编号
        /// </summary>		
        public int UserNotificationId
        {
            get { return _usernotificationid; }
            set { _usernotificationid = value; }
        }  
      	private int _userid;
		/// <summary>
		/// 用户编号
        /// </summary>		
        public int UserID
        {
            get{ return _userid; }
            set{ _userid = value; }
        }
        private int _deviceid;
        /// <summary>
        /// 设备编号
        /// </summary>		
        public int DeviceID
        {
            get { return _deviceid; }
            set { _deviceid = value; }
        }        
		private int _type;
		/// <summary>
        /// Type 1 普通消息 2 语音 3 短信 4 报警 5系统公告 6数据更新 7照片
        /// </summary>		
        public int Type
        {
            get{ return _type; }
            set{ _type = value; }
        }        
		private int _objectid;
		/// <summary>
		/// 当类型为语音的时候对应设备语音编号 当类型是消息的时候对应消息编号
        /// </summary>		
        public int ObjectId
        {
            get{ return _objectid; }
            set{ _objectid = value; }
        }        
		private bool _get;
		/// <summary>
		/// 是否获取
        /// </summary>		
        public bool Get
        {
            get{ return _get; }
            set{ _get = value; }
        }        
		private bool _notification;
		/// <summary>
		/// 是否推送语音信息
        /// </summary>		
        public bool Notification
        {
            get{ return _notification; }
            set{ _notification = value; }
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
		   
	}
}

