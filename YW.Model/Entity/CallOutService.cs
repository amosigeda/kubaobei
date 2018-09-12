using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace YW.Model.Entity
{
    //Device
    [DataContract]
    [XmlRoot("callOutService")]
    public class CallOutService
    {
        private Service _service;

        [DataMember]
        public Service service
        {
            get => _service;
            set => _service = value;
        }
    }

    public class Service
    {
        private string _name;
        private string _messageID;
        private string _callingNumber;
        private string _calledNumber;
        private string _callerAreaID;
        private string _callerDisplayNum;
        private string _calledDisplayNum;
        private int _maxDuration;
        private string _vccID;
        private string _content;
        private string _isRecord;
        private string _chargeNumber;

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
        public string callingNumber
        {
            get => _callingNumber;
            set => _callingNumber = value;
        }

        [DataMember]
        public string calledNumber
        {
            get => _calledNumber;
            set => _calledNumber = value;
        }

        [DataMember]
        public string callerAreaID
        {
            get => _callerAreaID;
            set => _callerAreaID = value;
        }

        [DataMember]
        public string callerDisplayNum
        {
            get => _callerDisplayNum;
            set => _callerDisplayNum = value;
        }

        [DataMember]
        public string calledDisplayNum
        {
            get => _calledDisplayNum;
            set => _calledDisplayNum = value;
        }

        [DataMember]
        public int maxDuration
        {
            get => _maxDuration;
            set => _maxDuration = value;
        }

        [DataMember]
        public string vccID
        {
            get => _vccID;
            set => _vccID = value;
        }

        [DataMember]
        public string content
        {
            get => _content;
            set => _content = value;
        }

        [DataMember]
        public string isRecord
        {
            get => _isRecord;
            set => _isRecord = value;
        }

        [DataMember]
        public string chargeNumber
        {
            get => _chargeNumber;
            set => _chargeNumber = value;
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