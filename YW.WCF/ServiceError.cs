using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace YW.WCF
{
     [DataContract(Name = "ServiceError", Namespace = "YW.WCF")]
    public class ServiceError
    {
         /// <summary>
         /// 错误编号，系统异常小于0，常规异常大于0
         /// </summary>
         [DataMember(Name = "Code")]
         public int Code { get; set; }
         /// <summary>
         /// 异常信息
         /// </summary>
         [DataMember(Name = "Message")]
         public string Message { get; set; }
    }
}
