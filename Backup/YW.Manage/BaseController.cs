using System.ServiceModel;
using System.Web.Mvc;

namespace YW.Manage
{
    public class BaseController : Controller
    {
        /// <summary>
        /// 异常方法处理
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception.GetType() != typeof (FaultException<int>))
            {
                Data.Logger.Error(filterContext.Exception);
            }
            else
            {
                var ex = (FaultException<int>) filterContext.Exception;
                if (ex.Detail == 0)
                    BLL.Authorize.Clern();
            }
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new {Result = 0, Message = filterContext.Exception.Message},
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 200; //设置回正常返回，让前端报告错误
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
            else
            {
                filterContext.HttpContext.Response.Redirect("/Home/Error?Error=" + System.Web.HttpUtility.UrlEncode(filterContext.Exception.Message, System.Text.Encoding.GetEncoding("UTF-8")));
            }
        }
    }
}