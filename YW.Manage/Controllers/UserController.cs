using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YW.Manage.BLL;

namespace YW.Manage.Controllers
{
    public class UserController : BaseController
    {
        //
        // GET: /User/

        [CheckAuthorize]
        public ActionResult Index()
        {
            Models.User user = BLL.Authorize.GetUser();
            ViewBag.Count = BLL.Client.Get(user.Server).ManageGetCount(user.LoginId);
            return View(user);
        }
        [CheckAuthorize]
        public JsonResult GetList(int dealerId, int pageindex, int pagesize, string name)
        {
            var user = Authorize.GetUser();
            if (dealerId == 0)
                dealerId = user.Dealer.DealerId;
            var result = BLL.Client.Get(user.Server).ManageGetDealerUserList(user.LoginId,dealerId, pageindex, pagesize, name);
            return Json(new { Result = 1, Count = result.Total, List = result.List });
        }

        [CheckAuthorize]
        public JsonResult Del(int id)
        {
            var user = Authorize.GetUser();
            try
            {
                BLL.Client.Get(user.Server).ManageDelDealerUser(user.LoginId, id);
                return Json(new { Result = 1 });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
        }

        [CheckAuthorize]
        public ActionResult Edit(int dealerId,int dealerUserId)
        {
            Models.User user = BLL.Authorize.GetUser();
            Model.Entity.DealerUser dUser;
            if (dealerId == 0)
                dealerId = user.Dealer.DealerId;
            if (dealerUserId == 0)
            {
                ViewBag.NameType = 0;
                
                dUser = new Model.Entity.DealerUser {DealerId =dealerId };
            }
            else
            {
                ViewBag.NameType = 1;
                dUser = BLL.Client.Get(user.Server).ManageGetDealerUser(user.LoginId, dealerUserId);
            }
            ViewBag.User = user;
            return View(dUser);

        }
        [HttpPost]
        [CheckAuthorize]
        public JsonResult Edit(Model.Entity.DealerUser dealerUser)
        {
            Models.User user = BLL.Authorize.GetUser();
            try
            {
                if (dealerUser.DealerUserId == 0)
                {
                    BLL.Client.Get(user.Server).ManageAddDealerUser(user.LoginId, dealerUser, dealerUser.Password);
                    return Json(new { Result = 1, Message = "添加成功" });
                }
                else
                {
                    BLL.Client.Get(user.Server).ManageSaveDealerUser(user.LoginId, dealerUser, dealerUser.Password);
                    return Json(new { Result = 1, Message="保存成功" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
        }
        [CheckAuthorize]
        public ActionResult MyInfo()
        {
            Models.User user = BLL.Authorize.GetUser();
            return View(user.DealerUser);
        }
        [CheckAuthorize]
        [HttpPost]
        public JsonResult MyInfo(Model.Entity.DealerUser dUser)
        {
            Models.User user = BLL.Authorize.GetUser();
            Client.Get(user.Server).ManageUpdataUser(user.LoginId, dUser.Name, dUser.PhoneNumber);
            user.DealerUser.Name = dUser.Name;
            user.DealerUser.PhoneNumber = dUser.PhoneNumber;
            return Json(new { Result = 1, Message = "保存成功" });
        }
        [CheckAuthorize]
        public ActionResult MyPassword()
        {
            Models.User user = BLL.Authorize.GetUser();
            return View();
        }
        [CheckAuthorize]
        [HttpPost]
        public JsonResult MyPassword(string password, string newPassword)
        {
            Models.User user = BLL.Authorize.GetUser();
            Client.Get(user.Server).ManageUpdataUserPassword(user.LoginId, password, newPassword);
            return Json(new { Result = 1, Message = "修改成功" });
        }
    }
}
