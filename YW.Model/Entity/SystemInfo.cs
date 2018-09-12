using System; 
using System.Text;
using System.Collections.Generic; 
using System.Data;
namespace YW.Model.Entity{
	 	//SystemInfo
	[Serializable]
	public class SystemInfo
	{
      	private string _serviceagreement;
		/// <summary>
		/// 用户服务协议
        /// </summary>		
        public string ServiceAgreement
        {
            get{ return _serviceagreement; }
            set{ _serviceagreement = value; }
        }        
		   
	}
}

