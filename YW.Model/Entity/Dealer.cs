using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//Dealer
	[Serializable]
	public class Dealer
	{
      	private int _dealerid;
		/// <summary>
		/// 经销商编号
        /// </summary>		
        public int DealerId
        {
            get{ return _dealerid; }
            set{ _dealerid = value; }
        }        
		private int _parentid;
		/// <summary>
		/// ParentId
        /// </summary>		
        public int ParentId
        {
            get{ return _parentid; }
            set{ _parentid = value; }
        }        
		private int _status;
		/// <summary>
		/// 状态，0 表示停用 1表示正常
        /// </summary>		
        public int Status
        {
            get{ return _status; }
            set{ _status = value; }
        }
        private int _dealerype;
		/// <summary>
        /// 1,经销商,2代理商.3,厂家,4平台管理者
        /// </summary>		
        public int DealerType
        {
            get{ return _dealerype; }
            set { _dealerype = value; }
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
		private string _phonenumber;
		/// <summary>
        /// 联系电话
        /// </summary>		
        public string PhoneNumber
        {
            get{ return _phonenumber; }
            set{ _phonenumber = value; }
        }        
		private string _address;
		/// <summary>
		/// 联系地址
        /// </summary>		
        public string Address
        {
            get{ return _address; }
            set{ _address = value; }
        }        
		private string _remark;
		/// <summary>
		/// 备注
        /// </summary>		
        public string Remark
        {
            get{ return _remark; }
            set{ _remark = value; }
        }        
		private DateTime _createtime;
		/// <summary>
		/// 创建时间
        /// </summary>		
        public DateTime CreateTime
        {
            get{ return _createtime; }
            set{ _createtime = value; }
        }        
		private DateTime _updatetime;
		/// <summary>
		/// 更新时间
        /// </summary>		
        public DateTime UpdateTime
        {
            get{ return _updatetime; }
            set{ _updatetime = value; }
        }        
		   
	}
}

