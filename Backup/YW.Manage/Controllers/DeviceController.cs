using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YW.Manage.BLL;

namespace YW.Manage.Controllers
{
    public class DeviceController : BaseController
    {
        [CheckAuthorize]
        public ActionResult Edit(int deviceId)
        {
            Models.User user = BLL.Authorize.GetUser();
            Model.Entity.Device device =BLL.Client.Get(user.Server).ManageGetDevice(user.LoginId, deviceId);
            if (user.Dealer.DealerType < 3)
                ViewBag.EditEnable = 0;
            else
                ViewBag.EditEnable = 1;
            
            ViewBag.User = user;
            return View(device);

        }
        [HttpPost]
        [CheckAuthorize]
        public JsonResult Edit(int deviceId, string bindNumber, string[] model)
        {
            Models.User user = BLL.Authorize.GetUser();
            int iModel = 0;
            if (model != null)
            {
                iModel += model.Sum(item => int.Parse(item));
            }
            try
            {

                BLL.Client.Get(user.Server).ManageUpdateDevice(user.LoginId, deviceId, iModel, bindNumber);
                return Json(new {Result = 1, Message = "保存成功"});
            }
            catch (Exception ex)
            {
                return Json(new {Result = 0, Message = ex.Message});
            }
        }

        [CheckAuthorize]
        public ActionResult Add()
        {
            var user = Authorize.GetUser();
            ViewBag.Count = BLL.Client.Get(user.Server).ManageGetCount(user.LoginId);
            return View();
        }

        [CheckAuthorize]
        [HttpPost]
        public JsonResult Add(string devices, string[] model)
        {
            int iModel = 0;
            if (model != null)
            {
                foreach (var item in model)
                {
                    iModel += int.Parse(item);
                }
            }
            var user = Authorize.GetUser();
            BLL.Client.Get(user.Server).ManageAddDeviceList(user.LoginId, devices, iModel);
            return Json(new {Result = 1, Message = "导入成功"});
        }

        [CheckAuthorize]
        [HttpGet]
        public ActionResult Stock()
        {
            var user = Authorize.GetUser();
            ViewBag.Count = BLL.Client.Get(user.Server).ManageGetCount(user.LoginId);
            return View();
        }
        [CheckAuthorize]
        [HttpPost]
        public JsonResult Stock(string devices)
        {
            var user = Authorize.GetUser();
            BLL.Client.Get(user.Server).ManageStockDeviceList(user.LoginId, devices);
            return Json(new { Result = 1, Message = "入库成功" });;
        }
        [CheckAuthorize]
        [HttpPost]
        public JsonResult StockSingle(int deviceId)
        {
            var user = Authorize.GetUser();
            try
            {
                BLL.Client.Get(user.Server).ManageStockDevice(user.LoginId, deviceId);
                return Json(new { Result = 1 });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
        }
        [CheckAuthorize]
        [HttpGet]
        public ActionResult Sales(string serialNumber)
        {
            var user = Authorize.GetUser();
            var listDealer = BLL.Client.Get(user.Server).ManageGetDealerList(user.LoginId);
            ViewBag.Dealer = listDealer;
            ViewBag.SerialNumber = serialNumber;
            ViewBag.Count = BLL.Client.Get(user.Server).ManageGetCount(user.LoginId);
            return View();
        }
        [CheckAuthorize]
        [HttpPost]
        public JsonResult Sales(int dealerId,string devices)
        {
            var user = Authorize.GetUser();
            BLL.Client.Get(user.Server).ManageSalesDeviceList(user.LoginId, dealerId, devices);
            return Json(new { Result = 1, Message = "销售成功" }); ;
        }
        [CheckAuthorize]
        public JsonResult GetList(int pageindex, int pagesize, string serialNumber,
            int? model, int? status, bool? active)
        {
            var user = Authorize.GetUser();
            var list = BLL.Client.Get(user.Server).ManageGetDealerDeviceList(user.LoginId, pageindex, pagesize, serialNumber, model, status, active);
            return Json(new { Result = 1, Count = list.Total, List = list.List });
        }
        [CheckAuthorize]
        public JsonResult Del(int deviceId)
        {
            var user = Authorize.GetUser();
            try
            {
                BLL.Client.Get(user.Server).ManageDelDevice(user.LoginId, deviceId);
                return Json(new { Result = 1 });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
        }
        [CheckAuthorize]
        public JsonResult Reset(int deviceId)
        {
            var user = Authorize.GetUser();
            try
            {
                BLL.Client.Get(user.Server).ManageResetDevice(user.LoginId, deviceId);
                return Json(new { Result = 1 });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = ex.Message });
            }
        }
        
    }
}
