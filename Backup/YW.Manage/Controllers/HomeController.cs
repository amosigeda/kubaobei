using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using YW.Manage.BLL;

namespace YW.Manage.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/
        [CheckAuthorize]
        public ActionResult Index(string project)
        {
            Models.User user = BLL.Authorize.GetUser();
            ViewBag.Count = BLL.Client.Get(user.Server).ManageGetCount(user.LoginId);
            return View(user);
        }
        
        public ActionResult Login(string project)
        {
            Models.Login model = new Models.Login();
            ViewBag.ShowServer = true;
            if (!string.IsNullOrEmpty(project))
            {
                //StreamReader sr = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Server.txt"), Encoding.Default);
                //String line;
                //while ((line = sr.ReadLine()) != null)
                //{
                //    string[] item = line.Split(',');
                //    if (item[0].ToLower().Equals(project.ToLower()))
                //    {
                //        model.Server = item[1].Replace(":6699","");
                //        //ViewBag.ShowServer = false;
                //        break;
                //    }
                //}
                //sr.Close();
                //sr.Dispose();
                var server=BLL.Project.GetInstance().GetById(project);
                if (server != null)
                    model.Server = server.Server.Replace(":6699", "");
            }
            else
            {
                var server = System.Web.HttpContext.Current.Request.Cookies["Server"];
                model.Server = server != null ? server.Value : "";
            }
            var userName = System.Web.HttpContext.Current.Request.Cookies["UserName"];
            model.UserName = userName != null ? userName.Value : "";
            //model.PassWord = "123456";
            return View(model);
        }

        [HttpPost]
        public JsonResult Login(string server,string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(server))
                {
                    return Json(new {Result = 0, Message = "服务器不能为空"});
                }
                if (server.IndexOf(":", System.StringComparison.Ordinal) < 0)
                {
                    if (!IsIpAddress(server))
                        return Json(new {Result = 0, Message = "服务器格式错误"});
                    else
                        server = server + ":6699";
                }
                else
                {
                    string[] ipAndPort = server.Split(':');
                    if (ipAndPort.Length != 2 || !IsIpAddress(ipAndPort[0]) || !IsIpPort(ipAndPort[1]))
                    {
                        return Json(new {Result = 0, Message = "服务器格式错误"});
                    }
                }
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new {Result = 0, Message = "用户名不能为空"});
                }
                if (string.IsNullOrEmpty(password))
                {
                    return Json(new {Result = 0, Message = "密码不能为空"});
                }
                var project = BLL.Project.GetInstance().GetByServer(server);
                if (project==null)
                {
                    return Json(new {Result = 0, Message = "无法连接该服务器，请检查服务器地址是否有误"});
                }
                Guid loginId = BLL.Client.Get(server).ManageLogin(username, password);
                Models.User user = new Models.User(project.Server, loginId, BLL.Client.Get(server).ManageGetLoginDealer(loginId), BLL.Client.Get(server).ManageGetLoginUser(loginId));
                Authorize.SetLogin(project.Id, loginId, user);
                return Json(new {Result = 1, Message = "登录成功"});
            }
            catch (System.ServiceModel.EndpointNotFoundException ex)
            {
                return Json(new { Result = 0, Message = "无法连接该服务器,请稍后再试" });
            }
            catch (Exception ex)
            {
                return Json(new {Result = 0, Message = ex.Message});
            }
        }
       
        public ActionResult Logout()
        {
            var user = Authorize.GetUser();
            if (user != null)
            {
                BLL.Client.Get(user.Server).ManageLogOut(user.LoginId);
                BLL.Authorize.Clern();
            }
            return RedirectToAction("Index");
        }
        public static bool IsIpAddress(string ip)
        {
            if (string.IsNullOrEmpty(ip) || ip.Length < 7 || ip.Length > 15) return false;
            const string regformat = @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);
            return regex.IsMatch(ip);

        }
        public ActionResult Error(string error)
        {
            ViewBag.Error = error;
            return View();
        }
        public static bool IsIpPort(string port)
        {
            bool isPort = false;
            int portNum;
            isPort = Int32.TryParse(port, out portNum);
            if (isPort && portNum >= 0 && portNum <= 65535)
            {
                isPort = true;
            }
            else
            {
                isPort = false;
            }
            return isPort;
        }

    }
}
