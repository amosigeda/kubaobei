using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YW.Model.Entity
{
    public class Count
    {
        public DateTime Date { get; set; }
        public int Address { get; set; }
        public int LbsAndWifi { get; set; }
        public int LbsAndWifiFail { get; set; }
        public int LbsAndWifiCache { get; set; }
        public int AddressTotal { get; set; }
        public int LbsAndWifiTotal { get; set; }
        public int SMS { get; set; }
    }
}
