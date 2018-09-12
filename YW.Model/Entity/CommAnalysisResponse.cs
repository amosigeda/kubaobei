using System;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Deserializers;

namespace YW.Model.Entity
{
    [DataContract]
    public class CommAnalysisResponse
    {
        private AnalysisResponseHeader _header = new AnalysisResponseHeader();

        [DataMember]
        public AnalysisResponseHeader header
        {
            get => _header;
            set => _header = value;
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

    [DataContract]
    public class AnalysisResponseHeader
    {
        private string _STATE_CODE;
        private string _STATE_NAME;
        private string _REMARK;

        [DataMember]
        public string STATE_CODE
        {
            get => _STATE_CODE;
            set => _STATE_CODE = value;
        }
        [DataMember]
        public string STATE_NAME
        {
            get => _STATE_NAME;
            set => _STATE_NAME = value;
        }
        [DataMember]
        public string REMARK
        {
            get => _REMARK;
            set => _REMARK = value;
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

    public class CommAnalysisResponseDeserializer : IDeserializer
    {
        public CommAnalysisResponse Deserialize<CommAnalysisResponse>(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<CommAnalysisResponse>(response.Content);
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
    }
}