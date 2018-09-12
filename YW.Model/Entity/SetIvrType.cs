using System.Runtime.Serialization;

namespace YW.Model.Entity
{
    [DataContract]
    public class SetIvrType
    {
        private string _SERVICENAME;
        private int _OPERATE;
        private string _STREAMNUMBER;
        private string _TOKEN;
        private int _VCCID;
        private string _MIDDLEINNUM;
        private int _TYPE;
        private int _SOURCEDATA;
        private int _WELCOMEIVRSTATE;
        private string _WELCOMEIVRNAME;
        private string _WELCOMEIVRFILE;
        private int _EXCEPTIONIVRSTATE;
        private string _EXCEPTIONIVRNAME;
        private string _EXCEPTIONWELCOMEIVRFILE;
        private int _STATE;
        private string _VALIDTIME;

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
        public string STREAMNUMBER
        {
            get => _STREAMNUMBER;
            set => _STREAMNUMBER = value;
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

        [DataMember]
        public string MIDDLEINNUM
        {
            get => _MIDDLEINNUM;
            set => _MIDDLEINNUM = value;
        }

        [DataMember]
        public int TYPE
        {
            get => _TYPE;
            set => _TYPE = value;
        }

        [DataMember]
        public int SOURCEDATA
        {
            get => _SOURCEDATA;
            set => _SOURCEDATA = value;
        }

        [DataMember]
        public int WELCOMEIVRSTATE
        {
            get => _WELCOMEIVRSTATE;
            set => _WELCOMEIVRSTATE = value;
        }

        [DataMember]
        public string WELCOMEIVRNAME
        {
            get => _WELCOMEIVRNAME;
            set => _WELCOMEIVRNAME = value;
        }

        [DataMember]
        public string WELCOMEIVRFILE
        {
            get => _WELCOMEIVRFILE;
            set => _WELCOMEIVRFILE = value;
        }

        [DataMember]
        public int EXCEPTIONIVRSTATE
        {
            get => _EXCEPTIONIVRSTATE;
            set => _EXCEPTIONIVRSTATE = value;
        }

        [DataMember]
        public string EXCEPTIONIVRNAME
        {
            get => _EXCEPTIONIVRNAME;
            set => _EXCEPTIONIVRNAME = value;
        }

        [DataMember]
        public string EXCEPTIONWELCOMEIVRFILE
        {
            get => _EXCEPTIONWELCOMEIVRFILE;
            set => _EXCEPTIONWELCOMEIVRFILE = value;
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