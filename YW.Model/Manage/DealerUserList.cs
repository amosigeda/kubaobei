using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YW.Model.Entity;

namespace YW.Model.Manage
{
    public class DealerUserList
    {
        public int Total;
        public int PageIndex;
        public int PageSize;
        public List<DealerUser> List;
    }
}
