using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace YW.Model.Entity{
    [DataContract]
	public class BillListJson
    {
        private BillListHeader _header;
        private BillList _body;

        [DataMember]
        public BillListHeader header
        {
            get => _header;
            set => _header = value;
        }

        [DataMember]
        public BillList body
        {
            get => _body;
            set => _body = value;
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
    public class BillListHeader
    {
        private string _serviceName;

        [DataMember]
        public string serviceName
        {
            get => _serviceName;
            set => _serviceName = value;
        }
    }
}

