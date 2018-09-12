using System; 
namespace YW.Model.Entity{
	 	//DeviceSet
	[Serializable]
	public class DeviceSet
	{
      	private int _deviceid;
		/// <summary>
		/// 设备号
        /// </summary>		
        public int DeviceID
        {
            get{ return _deviceid; }
            set{ _deviceid = value; }
        }             

		private string _setinfo;
		/// <summary>
		/// 1-1-1-1-1-1-1-1-1-1-1-1，配置信息。自动接通、报告通话位置、体感接听、预留应急电量、上课禁用、定时开关机、拒绝陌生人来电、预留、手表来电响铃、手表来电振动、手表来消息响铃、手表来消息振动
        /// </summary>		
        public string SetInfo
        {
		    get
		    {
                if (_setinfo==null)
                    return "";
                else
		            return _setinfo;
		    }
            set{ _setinfo = value; }
        }        
		private string _classdisabled1;
		/// <summary>
		/// 上课禁用时间段1
        /// </summary>		
        public string ClassDisabled1
        {
		    get
		    {
                if (_classdisabled1 == null)
                    return "";
                else
		            return _classdisabled1;
		    }
            set{ _classdisabled1 = value; }
        }        
		private string _classdisabled2;
		/// <summary>
		/// 上课禁用时间段2
        /// </summary>		
        public string ClassDisabled2
        {
		    get
		    {
                if (_classdisabled2 == null)
                    return "";
                else
                    return _classdisabled2;
		    }
            set{ _classdisabled2 = value; }
        }        
		private string _weekdisabled;
		/// <summary>
		/// 禁用时间段星期说明13567，表示周一、周三、周五、周六和周日
        /// </summary>		
        public string WeekDisabled
        {
		    get
		    {
                if (_weekdisabled == null)
                    return "";
                else
                    return _weekdisabled;
		    }
            set{ _weekdisabled = value; }
        }        
		private string _timeropen;
		/// <summary>
		/// 定时开机时间
        /// </summary>		
        public string TimerOpen
        {
		    get
		    {
                if (_timeropen == null)
                    return "";
                else
                    return _timeropen;
		    }
            set{ _timeropen = value; }
        }        
		private string _timerclose;
		/// <summary>
		/// 定时关机时间
        /// </summary>		
        public string TimerClose
        {
		    get
		    {
                if (_timerclose == null)
                    return "";
                else
                    return _timerclose;
		    }
            set{ _timerclose = value; }
        }        
		private int _brightscreen;
		/// <summary>
		/// 亮屏时间，单位为“秒”
        /// </summary>		
        public int BrightScreen
        {
            get{ return _brightscreen; }
            set{ _brightscreen = value; }
        }

        private string _weekAlarm1;
        /// <summary>
        ///  闹钟时间段，周1-周日 0表示不设置
        /// </summary>		
        public string WeekAlarm1
        {
            get
            {
                if (_weekAlarm1 == null)
                    return "";
                else
                    return _weekAlarm1;
            }
            set { _weekAlarm1 = value; }
        }

        private string _weekAlarm2;
        /// <summary>
        ///  闹钟时间段，周1-周日 0表示不设置
        /// </summary>		
        public string WeekAlarm2
        {
            get
            {
                if (_weekAlarm2 == null)
                    return "";
                else
                    return _weekAlarm2;
            }
            set { _weekAlarm2 = value; }
        }

        private string _weekAlarm3;
        /// <summary>
        ///  闹钟时间段，周1-周日 0表示不设置
        /// </summary>		
        public string WeekAlarm3
        {
            get
            {
                if (_weekAlarm3 == null)
                    return "";
                else
                    return _weekAlarm3;
            }
            set { _weekAlarm3 = value; }
        }

        private string _alarm1;
        /// <summary>
        /// 闹钟1
        /// </summary>		
        public string Alarm1
        {
            get
            {
                if (_alarm1 == null)
                    return "";
                else
                    return _alarm1;
            }
            set { _alarm1 = value; }
        }

        private string _alarm2;
        /// <summary>
        ///  闹钟2
        /// </summary>		
        public string Alarm2
        {
            get
            {
                if (_alarm2== null)
                    return "";
                else
                    return _alarm2;
            }
            set { _alarm2 = value; }
        }

        private string _alarm3;
        /// <summary>
        ///  闹钟3
        /// </summary>		
        public string Alarm3
        {
            get
            {
                if (_alarm3 == null)
                    return "";
                else
                    return _alarm3;
            }
            set { _alarm3 = value; }
        }

        private string _locationMode;
        /// <summary>
        /// 定时关机时间
        /// </summary>		
        public string LocationMode
        {
            get
            {
                if (_locationMode == null)
                    return "";
                else
                    return _locationMode;
            }
            set { _locationMode = value; }
        }

        private string _locationTime;
        /// <summary>
        /// 亮屏时间，单位为“秒”
        /// </summary>		
        public string LocationTime
        {
            get
            {
                if (_locationTime == null)
                    return "";
                else
                    return _locationTime;
            }
            set { _locationTime = value; }
        }

        private string _flowerNumber;
        /// <summary>
        /// 亮屏时间，单位为“秒”
        /// </summary>		
        public string FlowerNumber
        {
            get
            {
                if (_flowerNumber == null)
                    return "";
                else
                    return _flowerNumber;
            }
            set { _flowerNumber = value; }
        }

        private string _sleepCalculate;
        /// <summary>
        /// 睡眠
        /// </summary>		
        public string SleepCalculate
        {
            get
            {
                if (_sleepCalculate == null)
                    return "";
                else
                    return _sleepCalculate;
            }
            set { _sleepCalculate = value; }
        }

        private string _stepCalculate;
        /// <summary>
        /// 计步
        /// </summary>		
        public string StepCalculate
        {
            get
            {
                if (_stepCalculate == null)
                    return "";
                else
                    return _stepCalculate;
            }
            set { _stepCalculate = value; }
        }

        private string _hrCalculate;
        /// <summary>
        /// 心率
        /// </summary>		
        public string HrCalculate
        {
            get
            {
                if (_hrCalculate == null)
                    return "";
                else
                    return _hrCalculate;
            }
            set { _hrCalculate = value; }
        }

        private string _sosMsgswitch;
        /// <summary>
        /// sos短信开关
        /// </summary>		
        public string SosMsgswitch
        {
            get
            {
                if (_sosMsgswitch == null)
                    return "";
                else
                    return _sosMsgswitch;
            }
            set { _sosMsgswitch = value; }
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
		   
	}
}

