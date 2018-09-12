using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YW.Manage.BLL;

namespace YW.Manage.Controllers
{
    public class DealerController : BaseController
    {
        //
        // GET: /Dealer/

        [CheckAuthorize]
        public ActionResult Index()
        {
            Models.User user = BLL.Authorize.GetUser();
            ViewBag.Count = BLL.Client.Get(user.Server).ManageGetCount(user.LoginId);
            return View(user);
        }
        [CheckAuthorize]
        public JsonResult GetList(int pageindex, int pagesize, string name)
        {
            var user = Authorize.GetUser();
            var list = BLL.Client.Get(user.Server).ManageGetDealerList(user.LoginId);
            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(w => w.Name.IndexOf(name, System.StringComparison.Ordinal) >= 0).ToList();
            }
            int total = list.Count();
            int totalNum = (pageindex - 1) * pagesize;
            list = list.Skip(totalNum).Take(pagesize).ToList();
            return Json(new { Result = 1, Count = total, List = list });
        }

        [CheckAuthorize]
        public JsonResult Del(int id)
        {
            var user = Authorize.GetUser();
            try
            {
                BLL.Client.Get(user.Server).ManageDelDealer(user.LoginId, id);
                return Json(new { Result = 1 });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
        }

        [CheckAuthorize]
        public ActionResult Edit(int id)
        {
            Models.User user = BLL.Authorize.GetUser();
            Model.Entity.Dealer dUser;
            if (id == 0)
            {
                ViewBag.NameType = 0;
                dUser = new Model.Entity.Dealer { ParentId = user.Dealer.DealerId };
            }
            else
            {
                ViewBag.NameType = 1;
                dUser = BLL.Client.Get(user.Server).ManageGetDealer(user.LoginId, id);
            }
            ViewBag.User = user;
            return View(dUser);

        }
        [HttpPost]
        [CheckAuthorize]
        public JsonResult Edit(Model.Entity.Dealer dealer)
        {
            Models.User user = BLL.Authorize.GetUser();
            try
            {
                if (dealer.DealerId == 0)
                {
                    BLL.Client.Get(user.Server).ManageAddDealer(user.LoginId, dealer);
                    return Json(new { Result = 1, Message = "添加成功" });
                }
                else
                {
                    BLL.Client.Get(user.Server).ManageSaveDealer(user.LoginId, dealer);
                    return Json(new { Result = 1, Message = "保存成功" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
        }
        [CheckAuthorize]
        public ActionResult User(int id)
        {
            ViewBag.DealerId = id;
            return View();
        }
    }
}
