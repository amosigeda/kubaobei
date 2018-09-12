using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YW.Model
{
    public class NotificationCount
    {
        public int DeviceID { get;set;}
        public int Message { get; set; }
        public int Voice { get; set; }
        public int SMS { get; set; }
        public int Photo { get; set; }
    }
}
