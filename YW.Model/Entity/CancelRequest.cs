using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace YW.Model.Entity
{
    //Device
    [DataContract]
    [XmlRoot("callOutService")]
    public class CancelRequest
    {
        private CancelRequestService _service;

        [DataMember]
        public CancelRequestService service
        {
            get => _service;
            set => _service = value;
        }
    }

    public class CancelRequestService
    {
        private string _name;
        private string _messageID;
        private string _callID;
        private string _vccID;

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
        public string callID
        {
            get => _callID;
            set => _callID = value;
        }

        [DataMember]
        public string vccID
        {
            get => _vccID;
            set => _vccID = value;
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