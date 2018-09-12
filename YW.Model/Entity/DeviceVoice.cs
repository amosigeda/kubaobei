using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
using System.Reflection;

namespace YW.Model.Entity{
	 	//DeviceVoice
	[Serializable]
	public class DeviceVoice
	{
      	private int _devicevoiceid;
		/// <summary>
		/// 设备语音编号
        /// </summary>		
        public int DeviceVoiceId
        {
            get{ return _devicevoiceid; }
            set{ _devicevoiceid = value; }
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
		private int _state;
		/// <summary>
		/// 状态 0 正在接收 1接收完成 2正在发送 3 发送完成 4发送失败
        /// </summary>		
        public int State
        {
            get{ return _state; }
            set{ _state = value; }
        }        
		private int _totalpackage;
		/// <summary>
		/// 总包数
        /// </summary>		
        public int TotalPackage
        {
            get{ return _totalpackage; }
            set{ _totalpackage = value; }
        }        
		private int _currentpackage;
		/// <summary>
		/// 当前包
        /// </summary>		
        public int CurrentPackage
        {
            get{ return _currentpackage; }
            set{ _currentpackage = value; }
        }        
		private int _type;
		/// <summary>
		/// 1 表发给指定的人( ObjectId 人的编号 )  2表发给通讯录所有人(ObjectId=0) 3.人发给表及其他人 (ObjectId 人的编号) 4 表发给表（ObjectId 目标表的编号） //change to from
        /// </summary>		
        public int Type
        {
            get{ return _type; }
            set{ _type = value; }
        }        
		private int _objectid;
		/// <summary>
		/// ObjectId
        /// </summary>		
        public int ObjectId
        {
            get{ return _objectid; }
            set{ _objectid = value; }
        }        
		private string _mark;
		/// <summary>
		/// 语音标识
        /// </summary>		
        public string Mark
        {
            get{ return _mark; }
            set{ _mark = value; }
        }        
		private string _path;
		/// <summary>
		/// 保存路径
        /// </summary>		
        public string Path
        {
            get{ return _path; }
            set{ _path = value; }
        }        
		private decimal _length;
		/// <summary>
		/// 语音长度
        /// </summary>		
        public decimal Length
        {
            get{ return _length; }
            set{ _length = value; }
        }

        private int _msgtype;
        /// <summary>
        /// 消息类型 0语音 1文字
        /// </summary>	
        public int MsgType
        {
            get { return _msgtype; }
            set { _msgtype = value; }
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

	    public override string ToString()
	    {
	        PropertyInfo[] propertyInfoList = GetType().GetProperties();
	        string result = "";
	        foreach (PropertyInfo propertyInfo in propertyInfoList)
	        {
	            result += string.Format("{0}={1} ", propertyInfo.Name, propertyInfo.GetValue(this, null));
	        }

	        return result;
	    }
	}
}

