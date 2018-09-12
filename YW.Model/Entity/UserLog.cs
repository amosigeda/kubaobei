using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//UserLog
	[Serializable]
	public class UserLog
	{
      	private int _userlogid;
		/// <summary>
		/// UserLogID
        /// </summary>		
        public int UserLogID
        {
            get{ return _userlogid; }
            set{ _userlogid = value; }
        }        
		private int _userid;
		/// <summary>
		/// UserID
        /// </summary>		
        public int UserID
        {
            get{ return _userid; }
            set{ _userid = value; }
        }        
		private int _typeid;
		/// <summary>
		/// TypeID
        /// </summary>		
        public int TypeID
        {
            get{ return _typeid; }
            set{ _typeid = value; }
        }        
		private string _logcontent;
		/// <summary>
		/// LogContent
        /// </summary>		
        public string LogContent
        {
            get{ return _logcontent; }
            set{ _logcontent = value; }
        }        
		private string _objectname;
		/// <summary>
		/// ObjectName
        /// </summary>		
        public string ObjectName
        {
            get{ return _objectname; }
            set{ _objectname = value; }
        }        
		private int _objectid;
		/// <summary>
		/// ObjectID
        /// </summary>		
        public int ObjectID
        {
            get{ return _objectid; }
            set{ _objectid = value; }
        }        
		private string _ip;
		/// <summary>
		/// IP
        /// </summary>		
        public string IP
        {
            get{ return _ip; }
            set{ _ip = value; }
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

