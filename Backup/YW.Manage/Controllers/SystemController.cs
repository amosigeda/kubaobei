using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YW.Manage.BLL;

namespace YW.Manage.Controllers
{
    public class SystemController : BaseController
    {
        [CheckAuthorize]
        public ActionResult Index()
        {
            Models.User user = BLL.Authorize.GetUser();
            ViewBag.Count = BLL.Client.Get(user.Server).ManageGetCount(user.LoginId);
            var count = BLL.Client.Get(user.Server).ManageGetSystemCount(user.LoginId);
            return View(count);
        }
        [CheckAuthorize]
        public ActionResult Notification()
        {
            Models.User user = BLL.Authorize.GetUser();
            ViewBag.Count = BLL.Client.Get(user.Server).ManageGetCount(user.LoginId);
            return View(user);
        }
        [CheckAuthorize]
        public JsonResult GetNotification(int pageindex, int pagesize)
        {
            Models.User user = BLL.Authorize.GetUser();
            var list = BLL.Client.Get(user.Server).ManageGetNotificationList(user.LoginId, pageindex, pagesize);
            return Json(new { Result = 1, Count = list.Total, List = list.List });
        }
        [CheckAuthorize]
        public ActionResult EditNotification()
        {
            Models.User user = BLL.Authorize.GetUser();
            ViewBag.User = user;
            return View();

        }
        [HttpPost]
        [CheckAuthorize]
        public JsonResult EditNotification(string content)
        {
            Models.User user = BLL.Authorize.GetUser();
            try
            {
                BLL.Client.Get(user.Server).ManageSendNotification(user.LoginId, content);
                return Json(new { Result = 1, Message = "发送成功" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
        }

        [CheckAuthorize]
        public ActionResult Project()
        {
            Models.User user = BLL.Authorize.GetUser();
            ViewBag.Count = BLL.Client.Get(user.Server).ManageGetCount(user.LoginId);
            return View(user);
        }
        [CheckAuthorize]
        public JsonResult GetProject(int pageindex, int pagesize)
        {
            Models.User user = BLL.Authorize.GetUser();
            var list = BLL.Client.Get(user.Server).ManageGetProjectList(user.LoginId);
            var total = list.Count;
            int totalNum = (pageindex - 1) * pagesize;
            list = list.Skip(totalNum).Take(pagesize).ToList();
            return Json(new { Result = 1, Count = total, List = list });
        }
        [CheckAuthorize]
        public JsonResult DelProject(string projectId)
        {
            var user = Authorize.GetUser();
            try
            {
                BLL.Client.Get(user.Server).ManageDelProject(user.LoginId, projectId);
                return Json(new { Result = 1 });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
        }

        [CheckAuthorize]
        public ActionResult EditProject(string projectId)
        {
            Models.User user = BLL.Authorize.GetUser();
            Model.Entity.Project project;
            if (string.IsNullOrEmpty(projectId))
            {
                ViewBag.NameType = 0;
                project = new Model.Entity.Project();
            }
            else
            {
                ViewBag.NameType = 1;
                project = BLL.Client.Get(user.Server).ManageGetProject(user.LoginId, projectId);
            }
            ViewBag.User = user;
            return View(project);

        }
        [HttpPost]
        [CheckAuthorize]
        public JsonResult EditProject(Model.Entity.Project project)
        {
            Models.User user = BLL.Authorize.GetUser();
            try
            {
                BLL.Client.Get(user.Server).ManageSaveProject(user.LoginId, project);
                    return Json(new { Result = 1, Message = "保存成功" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
        }
        [CheckAuthorize]
        public ActionResult Manage()
        {
            Models.User user = BLL.Authorize.GetUser();
            ViewBag.Count = BLL.Client.Get(user.Server).ManageGetCount(user.LoginId);
            ViewBag.Version = BLL.Client.Get(user.Server).ManageGetVersion(user.LoginId);
            return View();

        }

        [CheckAuthorize]
        public ActionResult Upgrade()
        {
            Models.User user = BLL.Authorize.GetUser();
            return View();
        }

        [HttpPost]
        [CheckAuthorize]
        public JsonResult Upgrade(string url)
        {
            Models.User user = BLL.Authorize.GetUser();
            try
            {
                BLL.Client.Get(user.Server).ManageUpgrade(user.LoginId, url);
                return Json(new { Result = 1, Message = "正在远程升级系统" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
        }
    }
}
