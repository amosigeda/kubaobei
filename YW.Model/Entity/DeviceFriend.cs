using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//DeviceFriend
	[Serializable]
	public class DeviceFriend
    {
      	private int _devicefriendid;
		/// <summary>
		/// 好友表编号
        /// </summary>		
        public int DeviceFriendId
        {
            get{ return _devicefriendid; }
            set{ _devicefriendid = value; }
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
        /// 1 加好友中 2 被通知完，3 为好友
        /// </summary>		
        public int Type
        {
            get{ return _type; }
            set{ _type = value; }
        }        
		private int _objectid;
		/// <summary>
		/// 设备编号
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

