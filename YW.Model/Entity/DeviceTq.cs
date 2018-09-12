using System;
using System.Data;

namespace YW.Model.Entity
{
    [Serializable]
    public class DeviceTq
    {
        /// <summary>
        /// ID号
        /// </summary>		
        public int _ID;

        /// <summary>
        ///城市
        /// </summary>		
        public string _city;

        /// <summary>
        ///天气信息
        /// </summary>		
        public string _info;
        /// <summary>
        ///温度
        /// </summary>		
        public string _temperature;

        /// <summary>
        /// 纬度
        /// </summary>		
        public decimal? _latitude;

        /// <summary>
        /// 经度
        /// </summary>		
        public decimal? _longitude;

        /// <summary>
        /// CreateTime
        /// </summary>		
        public DateTime _createTime;

        /// <summary>
        /// UpdateTime
        /// </summary>		
        public DateTime _updateTime;

        public int ID
        {
            get => _ID;
            set => _ID = value;
        }

        public string City
        {
            get => _city;
            set => _city = value;
        }

        public string Info
        {
            get => _info;
            set => _info = value;
        }

        public string Temperature
        {
            get => _temperature;
            set => _temperature = value;
        }

        public decimal? Latitude
        {
            get => _latitude;
            set => _latitude = value;
        }

        public decimal? Longitude
        {
            get => _longitude;
            set => _longitude = value;
        }

        public DateTime CreateTime
        {
            get => _createTime;
            set => _createTime = value;
        }

        public DateTime UpdateTime
        {
            get => _updateTime;
            set => _updateTime = value;
        }
    }
}
