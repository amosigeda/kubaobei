using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YW.Model.Entity;

namespace YW.Model.Manage
{
    public class DealerNotificationList
    {
        public int Total;
        public int PageIndex;
        public int PageSize;
        public List<DealerNotification> List;
    }
}
