using System.Web;
using System.Web.Mvc;

namespace YW.Manage.BLL
{
    public class CheckAuthorize : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool Pass = false;
            if (Authorize.GetUser() == null)
            {
                httpContext.Response.StatusCode = 401;//无权限状态码
                Pass = false;
            }
            else
            {
                Pass = true;
            }
            return Pass;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                if (Authorize.GetUser() == null)
                {
                    filterContext.HttpContext.Response.StatusCode = 200; //设置回正常返回，让前端报告错误
                    filterContext.Result = new JsonResult
                    {
                        Data = new { Result = 401, Message = "登录超时或在其他地方登陆,请重新登录再操作!" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    return;
                }
            }
            if (Authorize.GetUser() != null)
            {
                return;
            }
            base.HandleUnauthorizedRequest(filterContext);
            if (filterContext.HttpContext.Response.StatusCode == 401)
            {
                if (string.IsNullOrEmpty(filterContext.HttpContext.Request["project"]))
                    filterContext.Result = new RedirectResult("/Home/Login");
                else
                    filterContext.Result =
                        new RedirectResult("/Home/Login?Project=" + filterContext.HttpContext.Request["project"]);

            }
        }
    }
}