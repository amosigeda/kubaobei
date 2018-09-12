using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace YW.Model.Entity
{
    //Device
    [DataContract]
    [XmlRoot("callOutACRService")]
    public class CallOutACRService
    {
        private ACRService _service;

        [DataMember]
        public ACRService service
        {
            get => _service;
            set => _service = value;
        }
    }

    public class ACRService
    {
        private string _name;
        private string _messageID;
        private string _acrCallID;
        private string _callInNum;
        private string _callEdNum;
        private string _displayNumber;
        private string _callerStreamNo;
        private string _startCallTime;
        private string _stopCallTime;
        private int _duration;
        private int _callCost;
        private int _CallerrelCause;
        private int _callerOriRescode;
        private string _calledStreamNo;
        private string _startCalledTime;
        private int _calledDuration;
        private int _calledCost;
        private int _releaseCause;
        private int _calledOriRescode;
        private string _srfmsgid;
        private string _chargeNumber;
        private string _callerRelReason;
        private string _calledRelReason;
        private string _msserver;

        [DataMember]
        [XmlAttribute("name")]
        public string name
        {
            get => _name;
            set => _name = value;
        }

        [DataMember]
        public string messageID
        {
            get => _messageID;
            set => _messageID = value;
        }

        [DataMember]
        public string acrCallID
        {
            get => _acrCallID;
            set => _acrCallID = value;
        }

        [DataMember]
        public string callInNum
        {
            get => _callInNum;
            set => _callInNum = value;
        }

        [DataMember]
        public string callEdNum
        {
            get => _callEdNum;
            set => _callEdNum = value;
        }

        [DataMember]
        public string displayNumber
        {
            get => _displayNumber;
            set => _displayNumber = value;
        }

        [DataMember]
        public string callerStreamNo
        {
            get => _callerStreamNo;
            set => _callerStreamNo = value;
        }

        [DataMember]
        public string startCallTime
        {
            get => _startCallTime;
            set => _startCallTime = value;
        }

        [DataMember]
        public string stopCallTime
        {
            get => _stopCallTime;
            set => _stopCallTime = value;
        }

        [DataMember]
        public int duration
        {
            get => _duration;
            set => _duration = value;
        }

        [DataMember]
        public int callCost
        {
            get => _callCost;
            set => _callCost = value;
        }

        [DataMember]
        public int CallerrelCause
        {
            get => _CallerrelCause;
            set => _CallerrelCause = value;
        }

        [DataMember]
        public int callerOriRescode
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
        public int calledDuration
        {
            get => _calledDuration;
            set => _calledDuration = value;
        }

        [DataMember]
        public int calledCost
        {
            get => _calledCost;
            set => _calledCost = value;
        }

        [DataMember]
        public int releaseCause
        {
            get => _releaseCause;
            set => _releaseCause = value;
        }

        [DataMember]
        public int calledOriRescode
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
        public string msserver
        {
            get => _msserver;
            set => _msserver = value;
        }

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
    }
}