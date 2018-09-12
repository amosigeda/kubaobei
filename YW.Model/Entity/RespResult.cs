using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;

namespace YW.Model.Entity
{
    [Serializable]
    [DataContract]
    public class RespResult : DynamicObject
    {
        private int _code;
        private string _message;
        private Dictionary<string, object> body;

        protected RespResult()
        {
        }

        public RespResult(int code, string message)
        {
            _code = code;
            _message = message;
        }

        [DataMember]
        public int Code
        {
            get => _code;
            set => _code = value;
        }
        [DataMember]
        public string Message
        {
            get => _message;
            set => _message = value;
        }
        [DataMember]
        public Dictionary<string, object> Body
        {
            get => body;
            set => body = value;
        }
    }
}