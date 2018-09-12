using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YW.Manage.Models
{
    public class User
    {
        public User(string server, Guid loginId, Model.Entity.Dealer dealer, Model.Entity.DealerUser dealerUser)
        {
            this.Server = server;
            this.LoginId = loginId;
            this.Dealer = dealer;
            this.DealerUser = dealerUser;
        }
        public Guid LoginId { set; get; }
        public Model.Entity.Dealer Dealer { set; get; }
        public Model.Entity.DealerUser DealerUser { set; get; }
        public string Server { set; get; }
    }
}