using System;
namespace YW.Model.Entity
{
    //User
    [Serializable]
	public class User
	{
      	private int _userid;
		/// <summary>
		/// UserID
        /// </summary>		
        public int UserID
        {
            get{ return _userid; }
            set{ _userid = value; }
        }        
		private string _phonenumber;
		/// <summary>
		/// 电话号码
        /// </summary>		
        public string PhoneNumber
        {
            get{ return _phonenumber; }
            set{ _phonenumber = value; }
        }        
		private string _password;
		/// <summary>
		/// Password
        /// </summary>	
        [System.Runtime.Serialization.IgnoreDataMember]		
        public string Password
        {
            get{ return _password; }
            set{ _password = value; }
        }        
		private string _loginid;
		/// <summary>
		/// LoginID
        /// </summary>		
        public string LoginID
        {
            get{ return _loginid; }
            set{ _loginid = value; }
        }

        private int _logintype;
        /// <summary>
        /// 登录方式,0,未登录,1 Android,2 IOS,3 Web,4为接口登录
        /// </summary>		
        public int LoginType
        {
            get { return _logintype; }
            set { _logintype = value; }
        }        
		private int _usertype;
		/// <summary>
		/// 1,普通用户.2,管理员
        /// </summary>		
        public int UserType
        {
            get{ return _usertype; }
            set{ _usertype = value; }
        }        
		private string _name;
		/// <summary>
		/// Name
        /// </summary>		
        public string Name
        {
            get{ return _name; }
            set{ _name = value; }
        }        
		private bool _deleted;
		/// <summary>
		/// 是否删除 0否，1是
        /// </summary>		
        public bool Deleted
        {
            get{ return _deleted; }
            set{ _deleted = value; }
        }        
		private bool _notification;
		/// <summary>
		/// 1
        /// </summary>		
        public bool Notification
        {
            get{ return _notification; }
            set{ _notification = value; }
        }        
		private bool _notificationsound;
		/// <summary>
		/// 1
        /// </summary>		
        public bool NotificationSound
        {
            get{ return _notificationsound; }
            set{ _notificationsound = value; }
        }        
		private bool _notificationvibration;
		/// <summary>
		/// NotificationVibration
        /// </summary>		
        public bool NotificationVibration
        {
            get{ return _notificationvibration; }
            set{ _notificationvibration = value; }
        }        
		private string _appid;
		/// <summary>
		/// AppID
        /// </summary>		
        public string AppID
        {
            get{ return _appid; }
            set{ _appid = value; }
        }
        private string _project;
        /// <summary>
        /// 包名
        /// </summary>		
        public string Project
        {
            get { return _project; }
            set { _project = value; }
        }
        private DateTime? _activitytime;
        /// <summary>
        /// CreateTime
        /// </summary>		
        public DateTime? ActivityTime
        {
            get { return _activitytime; }
            set { _activitytime = value; }
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

        private string _bindNumber;
        public string BindNumber { get => _bindNumber; set => _bindNumber = value; }

        private string _salt;

        [System.Runtime.Serialization.IgnoreDataMember]
        public string Salt { get => _salt; set => _salt = value; }
	}
}

