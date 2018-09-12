using System;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//Dealer
     [DataContract]
    public class DealerUser
	{

      	private int _dealeruserid;
		/// <summary>
		/// 经销商编号
        /// </summary>		
        [DataMember]
        public int DealerUserId
        {
            get { return _dealeruserid; }
            set { _dealeruserid = value; }
        }
        private int _dealerid;
        /// <summary>
        /// 经销商编号
        /// </summary>		
        [DataMember]
        public int DealerId
        {
            get { return _dealerid; }
            set { _dealerid = value; }
        }
	    private int _status;
		/// <summary>
		/// 状态，0 表示停用 1表示正常
        /// </summary>	
        [DataMember]	
        public int Status
        {
            get{ return _status; }
            set{ _status = value; }
        }        
		private string _username;
		/// <summary>
		/// 登陆用户名
        /// </summary>	
        [DataMember]	
        public string UserName
        {
            get{ return _username; }
            set{ _username = value; }
        }        
		private string _password;
		/// <summary>
		/// 登陆密码
        /// </summary>	
        //[System.Runtime.Serialization.IgnoreDataMember]	
        public string Password
        {
            get{ return _password; }
            set{ _password = value; }
        }        
		private Guid? _loginid;
		/// <summary>
		/// 登录编号
        /// </summary>		
        //[System.Runtime.Serialization.IgnoreDataMember]	
        public Guid ? LoginID
        {
            get{ return _loginid; }
            set{ _loginid = value; }
        }        
		private int _usertype;
		/// <summary>
        /// 1普通用户,2管理员,3系统管理员
        /// </summary>		
        [DataMember]
        public int UserType
        {
            get{ return _usertype; }
            set{ _usertype = value; }
        }        
		private string _name;
		/// <summary>
		/// 名称
        /// </summary>	
        [DataMember] 
        public string Name
        {
            get{ return _name; }
            set{ _name = value; }
        }        
		private string _phonenumber;
		/// <summary>
        /// 联系电话
        /// </summary>		
        [DataMember]
        public string PhoneNumber
        {
            get{ return _phonenumber; }
            set{ _phonenumber = value; }
        }        
		private string _remark;
		/// <summary>
		/// 备注
        /// </summary>	
        [DataMember]	
        public string Remark
        {
            get{ return _remark; }
            set{ _remark = value; }
        }        
		private DateTime _createtime;
		/// <summary>
		/// 创建时间
        /// </summary>		
        [DataMember]
        public DateTime CreateTime
        {
            get{ return _createtime; }
            set{ _createtime = value; }
        }        
		private DateTime _updatetime;
		/// <summary>
		/// 更新时间
        /// </summary>	
        [DataMember]	
        public DateTime UpdateTime
        {
            get{ return _updatetime; }
            set{ _updatetime = value; }
        }

        public string Salt { get => _salt; set => _salt = value; }

        private string _salt;
		   
	}
}

