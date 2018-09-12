using System.Runtime.Serialization;

namespace YW.Model.Entity
{
    [DataContract]
    public class SetBindNum
    {
        private SetBindNumHeader _header = new SetBindNumHeader();
        private SetBindNumBody _body = new SetBindNumBody();

        [DataMember]
        public SetBindNumHeader header
        {
            get => _header;
            set => _header = value;
        }

        [DataMember]
        public SetBindNumBody body
        {
            get => _body;
            set => _body = value;
        }
    }

    [DataContract]
    public class SetBindNumHeader
    {
        private string _SERVICENAME;
        private int _OPERATE;
        private string _TOKEN;
        private int _VCCID;

        [DataMember]
        public string SERVICENAME
        {
            get => _SERVICENAME;
            set => _SERVICENAME = value;
        }

        [DataMember]
        public int OPERATE
        {
            get => _OPERATE;
            set => _OPERATE = value;
        }

        [DataMember]
        public string TOKEN
        {
            get => _TOKEN;
            set => _TOKEN = value;
        }

        [DataMember]
        public int VCCID
        {
            get => _VCCID;
            set => _VCCID = value;
        }
    }

    [DataContract]
    public class SetBindNumBody
    {
        private int _TYPE;
        private string _STREAMNUMBER;
        private string _CALLERNUM;
        private string _WAYBILLNUM;
        private string _MIDDLEINNUM;
        private string _MESSAGEID;
        private string _MIDDLEOUTNUM;
        private string _CALLEDNUM;
        private int _MAXDURATION;
        private int _ISRECORD;
        private int _STATE;
        private string _VALIDTIME;

        [DataMember]
        public int TYPE
        {
            get => _TYPE;
            set => _TYPE = value;
        }

        [DataMember]
        public string STREAMNUMBER
        {
            get => _STREAMNUMBER;
            set => _STREAMNUMBER = value;
        }

        [DataMember]
        public string CALLERNUM
        {
            get => _CALLERNUM;
            set => _CALLERNUM = value;
        }

        [DataMember]
        public string WAYBILLNUM
        {
            get => _WAYBILLNUM;
            set => _WAYBILLNUM = value;
        }

        [DataMember]
        public string MIDDLEINNUM
        {
            get => _MIDDLEINNUM;
            set => _MIDDLEINNUM = value;
        }

        [DataMember]
        public string MESSAGEID
        {
            get => _MESSAGEID;
            set => _MESSAGEID = value;
        }

        [DataMember]
        public string MIDDLEOUTNUM
        {
            get => _MIDDLEOUTNUM;
            set => _MIDDLEOUTNUM = value;
        }

        [DataMember]
        public string CALLEDNUM
        {
            get => _CALLEDNUM;
            set => _CALLEDNUM = value;
        }

        [DataMember]
        public int MAXDURATION
        {
            get => _MAXDURATION;
            set => _MAXDURATION = value;
        }

        [DataMember]
        public int ISRECORD
        {
            get => _ISRECORD;
            set => _ISRECORD = value;
        }

        [DataMember]
        public int STATE
        {
            get => _STATE;
            set => _STATE = value;
        }

        [DataMember]
        public string VALIDTIME
        {
            get => _VALIDTIME;
            set => _VALIDTIME = value;
        }
    }
}