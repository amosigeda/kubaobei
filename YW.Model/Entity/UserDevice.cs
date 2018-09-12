using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//UserDevice
	[Serializable]
	public class UserDevice
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
		private int _deviceid;
		/// <summary>
		/// DeviceID
        /// </summary>		
        public int DeviceID
        {
            get{ return _deviceid; }
            set{ _deviceid = value; }
        }
        private int _status;
        /// <summary>
        /// 状态  0表示 待确认，1表示确认
        /// </summary>		
        public int Status
        {
            get { return _status; }
            set { _status = value; }
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

