using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YW.Model.Entity
{
    [Serializable]
    public class WifiData
    {
        private int _id;
        /// <summary>
        /// 
        /// </summary>		
        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _macAddress;
        /// <summary>
        /// 
        /// </summary>		
        public string macAddress
        {
            get { return _macAddress; }
            set { _macAddress = value; }
        }

        private int _age;
        public int age
        {
            get { return _age; }
            set { _age = value; }
        }

        private int _singal;
        public int singal
        {
            get { return _singal; }
            set { _singal = value; }
        }

        private string _address;
        /// <summary>
        /// 
        /// </summary>		
        public string address
        {
            get { return _address; }
            set { _address = value; }
        }

        private decimal? _latitude;
        /// <summary>
        /// 
        /// </summary>		
        public decimal? latitude
        {
            get { return _latitude; }
            set { _latitude = value; }
        }

        private decimal? _longitude;
        /// <summary>
        /// 
        /// </summary>		
        public decimal? longitude
        {
            get { return _longitude; }
            set { _longitude = value; }
        }
        
        private int _accuracy;
        public int accuracy
        {
            get { return _accuracy; }
            set { _accuracy = value; }
        }

        private string _DESC;
        /// <summary>
        /// 
        /// </summary>		
        public string DESC
        {
            get { return _DESC; }
            set { _DESC = value; }
        }

        private DateTime _createtime;
        /// <summary>
        /// 
        /// </summary>		
        public DateTime CreateTime
        {
            get { return _createtime; }
            set { _createtime = value; }
        }

        private DateTime _updatetime;
        /// <summary>
        /// 
        /// </summary>		
        public DateTime UpdateTime
        {
            get { return _updatetime; }
            set { _updatetime = value; }
        }

    }
}
