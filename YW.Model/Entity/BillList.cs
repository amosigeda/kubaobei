using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace YW.Model.Entity{
	 	//Device
    [DataContract]
	public class BillList
    {
        private int _billListID;
        private string _serviceName;
        private string _messageId;
        private string _serviceKey;
        private string _callId;
        private string _callerNum;
        private string _calledNum;
        private string _middleNumber;
        private string _callerDisplayNumber;
        private string _calledDisplayNumber;
        private string _callerStreamNo;
        private string _startCallerTime;
        private string _abStartCallTime;
        private string _abStopCallTime;
        private string _callerDuration;
        private string _callerCost;
        private string _callerRelCause;
        private string _callerOriRescode;
        private string _calledStreamNo;
        private string _startCalledTime;
        private string _calledDuration;
        private string _calledCost;
        private string _calledRelCause;
        private string _calledOriRescode;
        private string _srfmsgid;
        private string _chargeNumber;
        private string _callerRelReason;
        private string _calledRelReason;
        private string _msServer;
        private string _middleStartTime;
        private string _middleCallTime;
        private string _duration;
        private string _costCount;
        private DateTime _createTime;

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

        [DataMember]
        public int billListId
        {
            get => _billListID;
            set => _billListID = value;
        }

        [DataMember]
        public string serviceName
        {
            get => _serviceName;
            set => _serviceName = value;
        }

        [DataMember]
        public string messageId
        {
            get => _messageId;
            set => _messageId = value;
        }

        [DataMember]
        public string serviceKey
        {
            get => _serviceKey;
            set => _serviceKey = value;
        }

        [DataMember]
        public string callId
        {
            get => _callId;
            set => _callId = value;
        }

        [DataMember]
        public string callerNum
        {
            get => _callerNum;
            set => _callerNum = value;
        }

        [DataMember]
        public string calledNum
        {
            get => _calledNum;
            set => _calledNum = value;
        }

        [DataMember]
        public string middleNumber
        {
            get => _middleNumber;
            set => _middleNumber = value;
        }

        [DataMember]
        public string callerDisplayNumber
        {
            get => _callerDisplayNumber;
            set => _callerDisplayNumber = value;
        }

        [DataMember]
        public string calledDisplayNumber
        {
            get => _calledDisplayNumber;
            set => _calledDisplayNumber = value;
        }

        [DataMember]
        public string callerStreamNo
        {
            get => _callerStreamNo;
            set => _callerStreamNo = value;
        }

        [DataMember]
        public string startCallerTime
        {
            get => _startCallerTime;
            set => _startCallerTime = value;
        }

        [DataMember]
        public string abStartCallTime
        {
            get => _abStartCallTime;
            set => _abStartCallTime = value;
        }

        [DataMember]
        public string abStopCallTime
        {
            get => _abStopCallTime;
            set => _abStopCallTime = value;
        }

        [DataMember]
        public string callerDuration
        {
            get => _callerDuration;
            set => _callerDuration = value;
        }

        [DataMember]
        public string callerCost
        {
            get => _callerCost;
            set => _callerCost = value;
        }

        [DataMember]
        public string callerRelCause
        {
            get => _callerRelCause;
            set => _callerRelCause = value;
        }

        [DataMember]
        public string callerOriRescode
        {
            get => _callerOriRescode;
            set => _callerOriRescode = value;
        }

        [DataMember]
        public string calledStreamNo
        {
            get => _calledStreamNo;
            set => _calledStreamNo = value;
        }

        [DataMember]
        public string startCalledTime
        {
            get => _startCalledTime;
            set => _startCalledTime = value;
        }

        [DataMember]
        public string calledDuration
        {
            get => _calledDuration;
            set => _calledDuration = value;
        }

        [DataMember]
        public string calledCost
        {
            get => _calledCost;
            set => _calledCost = value;
        }

        [DataMember]
        public string calledRelCause
        {
            get => _calledRelCause;
            set => _calledRelCause = value;
        }

        [DataMember]
        public string calledOriRescode
        {
            get => _calledOriRescode;
            set => _calledOriRescode = value;
        }

        [DataMember]
        public string srfmsgid
        {
            get => _srfmsgid;
            set => _srfmsgid = value;
        }

        [DataMember]
        public string chargeNumber
        {
            get => _chargeNumber;
            set => _chargeNumber = value;
        }

        [DataMember]
        public string callerRelReason
        {
            get => _callerRelReason;
            set => _callerRelReason = value;
        }

        [DataMember]
        public string calledRelReason
        {
            get => _calledRelReason;
            set => _calledRelReason = value;
        }

        [DataMember]
        public string msServer
        {
            get => _msServer;
            set => _msServer = value;
        }

        [DataMember]
        public string middleStartTime
        {
            get => _middleStartTime;
            set => _middleStartTime = value;
        }

        [DataMember]
        public string middleCallTime
        {
            get => _middleCallTime;
            set => _middleCallTime = value;
        }

        [DataMember]
        public string duration
        {
            get => _duration;
            set => _duration = value;
        }

        [DataMember]
        public string costCount
        {
            get => _costCount;
            set => _costCount = value;
        }

        [DataMember]
        public DateTime createTime
        {
            get => _createTime;
            set => _createTime = value;
        }
    }
}

