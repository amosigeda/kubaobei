using System;
using System.Data;
namespace YW.Model.Entity
{
    //YWLBS
    [Serializable]
    public class YWLBS
    {

        private int _id;
        /// <summary>
        /// 通讯录编号
        /// </summary>		
        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _mcc;
        /// <summary>
        /// 名称
        /// </summary>		
        public string MCC
        {
            get { return _mcc; }
            set { _mcc = value; }
        }

        private string _mnc;
        /// <summary>
        /// 名称
        /// </summary>		
        public string MNC
        {
            get { return _mnc; }
            set { _mnc = value; }
        }

        private string _lac;
        /// <summary>
        /// 名称
        /// </summary>		
        public string LAC
        {
            get { return _lac; }
            set { _lac = value; }
        }

        private string _cid;
        /// <summary>
        /// 名称
        /// </summary>		
        public string CID
        {
            get { return _cid; }
            set { _cid = value; }
        }

        private decimal? _olat;
        /// <summary>
        /// 家经度
        /// </summary>		
        public decimal? OLat
        {
            get { return _olat; }
            set { _olat = value; }
        }

        private decimal? _olng;
        /// <summary>
        /// 家经度
        /// </summary>		
        public decimal? OLng
        {
            get { return _olng; }
            set { _olng = value; }
        }

        private DateTime _updatetime;
        /// <summary>
        /// 更新时间
        /// </summary>		
        public DateTime UpdateTime
        {
            get { return _updatetime; }
            set { _updatetime = value; }
        }

        private float _range;
        /// <summary>
        /// 名称
        /// </summary>		
        public float Range
        {
            get { return _range; }
            set { _range = value; }
        }


        private string _infos;
        /// <summary>
        /// 名称
        /// </summary>		
        public string Infos
        {
            get { return _infos; }
            set { _infos = value; }
        }

        private int _ta;
        /// <summary>
        /// 通讯录编号
        /// </summary>		
        public int TA
        {
            get { return _ta; }
            set { _ta = value; }
        }

    }
}

