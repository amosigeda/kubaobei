using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YW.Model.Manage
{
    public class SystemCount:Model.Entity.Count
    {
        public int Online;
        public int DoLocationQueueCount;
        public int DoLocationQueueLbsWifiCount;
        public int InsertHistoryQueueCount;
        public List<Model.Entity.Count> List;
    }
}
