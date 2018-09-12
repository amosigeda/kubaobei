using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YW.Model.Entity
{
    [Serializable]
	public class SchoolGuardian
    {
        private int _schoolGuardianID;
        /// <summary>
        /// 上下学守护编号
        /// </summary>		
        public int SchoolGuardianID
        {
            get { return _schoolGuardianID; }
            set { _schoolGuardianID = value; }
        }
        private int _deviceid;
        /// <summary>
        /// 设备编号
        /// </summary>		
        public int DeviceID
        {
            get { return _deviceid; }
            set { _deviceid = value; }
        }
        private string _schoolDay;
        /// <summary>
        /// 守护信息天标示，格式为：2015-10-25
        /// </summary>		
        public string SchoolDay
        {
            get { return _schoolDay; }
            set { _schoolDay = value; }
        }
        private int _guardState;
        /// <summary>
        /// 守护状态，0表示等待状态，1表示提醒第一次，2表示提醒第二次。
        /// </summary>		
        public int GuardState
        {
            get { return _guardState; }
            set { _guardState = value; }
        }     
        private string _schoolArriveContent;
        /// <summary>
        /// 到校提醒内容
        /// </summary>		
        public string SchoolArriveContent
        {
            get { return _schoolArriveContent; }
            set { _schoolArriveContent = value; }
        }
        private string _schoolArriveTime;
        /// <summary>
        /// 到校提醒时间
        /// </summary>		
        public string SchoolArriveTime
        {
            get { return _schoolArriveTime; }
            set { _schoolArriveTime = value; }
        }
        private string _schoolLeaveContent;
        /// <summary>
        /// 离校提醒内容
        /// </summary>		
        public string SchoolLeaveContent
        {
            get { return _schoolLeaveContent; }
            set { _schoolLeaveContent = value; }
        }
        private string _schoolLeaveTime;
        /// <summary>
        /// 离校提醒时间
        /// </summary>		
        public string SchoolLeaveTime
        {
            get { return _schoolLeaveTime; }
            set { _schoolLeaveTime = value; }
        }
        private string _roadStayContent;
        /// <summary>
        /// 路上逗留内容
        /// </summary>		
        public string RoadStayContent
        {
            get { return _roadStayContent; }
            set { _roadStayContent = value; }
        }
        private string _roadStayTime;
        /// <summary>
        /// 路上逗留时间
        /// </summary>		
        public string RoadStayTime
        {
            get { return _roadStayTime; }
            set { _roadStayTime = value; }
        }
        private string _homeBackContent;
        /// <summary>
        /// 最迟到家内容
        /// </summary>		
        public string HomeBackContent
        {
            get { return _homeBackContent; }
            set { _homeBackContent = value; }
        }
        private string _homeBackTime;
        /// <summary>
        /// 最迟到家时间
        /// </summary>		
        public string HomeBackTime
        {
            get { return _homeBackTime; }
            set { _homeBackTime = value; }
        }
    }
}
