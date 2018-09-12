using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Web;
using System.Xml;

namespace YW.Manage.BLL
{
    public class Authorize
    {
        private static readonly Dictionary<Guid, Models.User> LoginUser = new Dictionary<Guid, Models.User>();

        private static readonly Dictionary<string, Models.User> LoginUserByServer =
            new Dictionary<string, Models.User>();

        /// <summary>
        /// 设置用户登录信息
        /// </summary>
        /// <param name="project">服务器</param>
        /// <param name="loginId">登录授权码</param>
        /// <param name="user">用户实体</param>
        public static void SetLogin(string project, Guid loginId, Models.User user)
        {
            var cookiesUserName = new HttpCookie("UserName", user.DealerUser.UserName)
            {
                Expires = DateTime.Now.Date.AddYears(1)
            };
            HttpContext.Current.Response.Cookies.Add(cookiesUserName);
            var cookiesAuthorize = new HttpCookie("LoginId", loginId.ToString())
            {
                Expires = DateTime.Now.Date.AddYears(1)
            };
            HttpContext.Current.Response.Cookies.Add(cookiesAuthorize);
            HttpContext.Current.Session["LoginId"] = loginId;
            var cookiesServer = new HttpCookie("Project", project) { Expires = DateTime.Now.Date.AddYears(1) };
            HttpContext.Current.Response.Cookies.Add(cookiesServer);
            HttpContext.Current.Session["Project"] = project;

            lock (LoginUser)
            {
                string checkKey = user.Server + "_" + user.DealerUser.UserName;
                if (LoginUserByServer.ContainsKey(checkKey))
                {
                    LoginUser.Remove(LoginUserByServer[checkKey].LoginId);
                    LoginUserByServer[checkKey] = user;
                }
                else
                {
                    LoginUserByServer.Add(checkKey, user);
                }

                if (LoginUser.ContainsKey(loginId))
                {
                    LoginUser[loginId] = user;
                }
                else
                {
                    LoginUser.Add(loginId, user);
                }
            }
        }
        /// <summary>
        /// 获得用户授权码
        /// </summary>
        /// <returns>用户登录授权</returns>
        public static Guid GetLoginId()
        {
            var loginIdS = HttpContext.Current.Session["LoginId"];
            if (loginIdS == null)
            {
                var loginIdC = HttpContext.Current.Request.Cookies["LoginId"];
                if (loginIdC == null)
                    throw new Exception("登录超时,请重新登录!");
                HttpContext.Current.Session["LoginId"] = Guid.Parse(loginIdC.Value);
                return Guid.Parse(loginIdC.Value);
            }
            else
            {
                return (Guid)loginIdS;
            }
        }
        public static Models.Project GetProject()
        {
            string strProject;
            var projectS = HttpContext.Current.Session["Project"];
            if (projectS == null)
            {
                var projectC = HttpContext.Current.Request.Cookies["Project"];
                if (projectC == null)
                    throw new Exception("登录超时,请重新登录!");
                strProject = projectC.Value;
                HttpContext.Current.Session["Project"] = strProject;
            }
            else
            {
                strProject = (string)projectS;
            }
            var project = BLL.Project.GetInstance().GetById(strProject);
            if (project == null)
                throw new Exception("登录超时,请重新登录!");
            return project;
        }
        public static string GetServer()
        {
            return GetProject().Server;
        }
        /// <summary>
        /// 获得登录用户实体
        /// </summary>
        /// <returns>用户实体</returns>
        public static Models.User GetUser()
        {
            try
            {
                var loginId = GetLoginId();
                
                lock (LoginUser)
                {
                    if (LoginUser.ContainsKey(loginId))
                    {
                        return LoginUser[loginId];
                    }
                    else
                    {
                        var server = GetServer();
                        try
                        {
                            Models.User user = new Models.User(server, loginId,
                                BLL.Client.Get(server).ManageGetLoginDealer(loginId),
                                BLL.Client.Get(server).ManageGetLoginUser(loginId));
                            string checkKey = user.Server + "_" + user.DealerUser.UserName;
                            if (LoginUserByServer.ContainsKey(checkKey))
                            {
                                LoginUser.Remove(LoginUserByServer[checkKey].LoginId);
                                LoginUserByServer[checkKey] = user;
                            }
                            else
                            {
                                LoginUserByServer.Add(checkKey, user);
                            }
                            LoginUser.Add(loginId, user);
                            return user;
                        }
                        catch (FaultException<int> fe)
                        {
                            Clern();
                            return null;
                        }


                    }
                }
            }
            catch { return null; }
        }
        public static void Clern()
        {
            //object authorize = HttpContext.Current.Session["Authorize"];
            Guid? authorize = null;
            var authorizeS = HttpContext.Current.Session["LoginId"];
            if (authorizeS == null)
            {
                var authorizeC = HttpContext.Current.Request.Cookies["LoginId"];
                if (authorizeC != null)
                    authorize = Guid.Parse(authorizeC.Value);
            }
            else
            {
                authorize = (Guid)authorizeS;
            }

            if (authorize != null && LoginUser.ContainsKey(authorize.Value))
                LoginUser.Remove(authorize.Value);
            HttpContext.Current.Session["LoginId"] = null;
            var cookies = new HttpCookie("LoginId", null) {Expires = DateTime.Now.Date.AddDays(-1)};
            HttpContext.Current.Response.Cookies.Add(cookies);
        }
        
    }
}