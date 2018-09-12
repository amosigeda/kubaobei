using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//DealerDevice
	[Serializable]
	public class DealerDevice
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
	    private int _deviceid;
		/// <summary>
		/// 设备编号
        /// </summary>		
        public int DeviceId
        {
            get{ return _deviceid; }
            set{ _deviceid = value; }
        }        
		private DateTime? _stocktime;
		/// <summary>
		/// 入库时间
        /// </summary>		
        public DateTime? StockTime 
        {
            get{ return _stocktime; }
            set{ _stocktime = value; }
        }        
		private int? _purchaser;
		/// <summary>
		/// 购买的人，0表示零售
        /// </summary>		
        public int? Purchaser
        {
            get{ return _purchaser; }
            set{ _purchaser = value; }
        }        
		private DateTime? _salestime;
		/// <summary>
		/// 销售时间
        /// </summary>		
        public DateTime? SalesTime
        {
            get{ return _salestime; }
            set{ _salestime = value; }
        }        
		private DateTime? _reworktime;
		/// <summary>
		/// 返修时间
        /// </summary>		
        public DateTime? ReworkTime
        {
            get{ return _reworktime; }
            set{ _reworktime = value; }
        }        
		private int _status;
		/// <summary>
		/// 状态 0路途  1入库 2销售 3返修 4换货
        /// </summary>		
        public int Status
        {
            get{ return _status; }
            set{ _status = value; }
        }        
		private string _remark;
		/// <summary>
		/// 备注，仅供该经销商查看
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

