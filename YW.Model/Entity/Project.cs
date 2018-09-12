using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YW.Model.Entity
{
    public class Project
    {
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string SMSKey { get; set; }
        public string SMSReg { get; set; }
        public string SMSForgot { get; set; }

        public int AppleVersion { get; set; }
        public string AppleUrl { get; set; }
        public string AppleDescription { get; set; }
        public int AndroidVersion { get; set; }
        public string AndroidUrl { get; set; }
        public string AndroidDescription { get; set; }
        public string AD { get; set; }

    }
}
