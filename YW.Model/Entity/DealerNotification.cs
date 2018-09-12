using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//Notification
	[Serializable]
    public class DealerNotification
	{
        private int _dealernotificationid;
        /// <summary>
        /// NotificationID
        /// </summary>		
        public int DealerNotificationID
        {
            get { return _dealernotificationid; }
            set { _dealernotificationid = value; }
        }
        private int _dealerid;
        /// <summary>
        /// 经销商编号
        /// </summary>		
        public int DealerId
        {
            get { return _dealerid; }
            set { _dealerid = value; }
        }    
      	private int _notificationid;
		/// <summary>
		/// NotificationID
        /// </summary>		
        public int NotificationID
        {
            get{ return _notificationid; }
            set{ _notificationid = value; }
        }
        private int _status;
        /// <summary>
        /// 状态，0 正在发送 1表示发送完成
        /// </summary>		
        public int Status
        {
            get { return _status; }
            set { _status = value; }
        }
        private int _usercount;
        /// <summary>
        /// 用户数量
        /// </summary>		
        public int UserCount
        {
            get { return _usercount; }
            set { _usercount = value; }
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
        /// 创建时间
        /// </summary>		
        public DateTime CreateTime
        {
            get { return _createtime; }
            set { _createtime = value; }
        }
        private DateTime _updatetime;
        /// <summary>
        /// 更新时间
        /// </summary>		
        public DateTime UpdateTime
        {
            get { return _updatetime; }
            set { _updatetime = value; }
        }            
		   
	}
}

