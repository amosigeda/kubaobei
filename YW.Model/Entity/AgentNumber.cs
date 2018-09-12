using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace YW.Model.Entity
{
    //Device
    [DataContract]
    public class AgentNumber
    {
        private int _agentNumberID;
        private string _number;
        private string _callOutNumber;
        private int _platform;
        private int _sync;
        private DateTime _createTime;

        [DataMember]
        public int AgentNumberID
        {
            get => _agentNumberID;
            set => _agentNumberID = value;
        }

        [DataMember]
        public string Number
        {
            get => _number;
            set => _number = value;
        }

        [DataMember]
        public int Platform
        {
            get => _platform;
            set => _platform = value;
        }

        [DataMember]
        public DateTime CreateTime
        {
            get => _createTime;
            set => _createTime = value;
        }

        public int Sync
        {
            get => _sync;
            set => _sync = value;
        }

        [DataMember]
        public string CallOutNumber
        {
            get => _callOutNumber;
            set => _callOutNumber = value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AgentNumber))
            {
                return false;
            }

            var tgt = (AgentNumber) obj;
            return Number.Equals(tgt.Number) && CallOutNumber.Equals(tgt.CallOutNumber) && Platform == tgt.Platform;
        }

        public override int GetHashCode()
        {
            return (Number + "_" + CallOutNumber + "_" + Platform).GetHashCode();
        }

        public override string ToString()
        {
            Type MyObjectType = this.GetType();
            PropertyInfo[] propertyInfoList = MyObjectType.GetProperties();
            string result = "";
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                result += string.Format("{0}={1} ", propertyInfo.Name, propertyInfo.GetValue(this, null));
            }

            return result;
        }
    }
}