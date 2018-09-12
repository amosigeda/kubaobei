using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using NSoup;
using NSoup.Nodes;
using RestSharp;
using YW.Contracts;
using YW.Data;
using YW.Logic;
using YW.Model;
using YW.Model.Entity;
using YW.Model.Manage;
using YW.Utility;
using YW.Utility.Security;
using YW.Utility.Stat;
using AgentNumber = YW.Model.Entity.AgentNumber;
using Convert = System.Convert;
using Count = YW.Model.Manage.Count;
using Dealer = YW.Model.Entity.Dealer;
using DealerDevice = YW.Logic.DealerDevice;
using DealerNotification = YW.Model.Entity.DealerNotification;
using DealerUser = YW.Model.Entity.DealerUser;
using Device = YW.Model.Entity.Device;
using DeviceContact = YW.Logic.DeviceContact;
using DeviceException = YW.Logic.DeviceException;
using DeviceFriend = YW.Logic.DeviceFriend;
using DevicePhoto = YW.Logic.DevicePhoto;
using DeviceSet = YW.Logic.DeviceSet;
using DeviceSMS = YW.Logic.DeviceSMS;
using DeviceState = YW.Logic.DeviceState;
using DeviceVoice = YW.Logic.DeviceVoice;
using GeoFence = YW.Model.Entity.GeoFence;
using Method = RestSharp.Method;
using Notification = YW.Logic.Notification;
using Project = YW.Model.Entity.Project;
using SchoolGuardian = YW.Model.Entity.SchoolGuardian;
using Timer = System.Timers.Timer;
using User = YW.Model.Entity.User;
using UserDevice = YW.Logic.UserDevice;

namespace YW.WCF
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Any)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Client : IClient
    {
        public delegate bool SendHandler(Device device, SendType commandType, string paramter);

        public static event SendHandler OnSend;

        public delegate void DisConnectHandler(int deviceId);

        public static event DisConnectHandler OnDisConnect;

        public delegate int GetOnlineHandler();

        public static event GetOnlineHandler OnGetOnlineHandler;

        public delegate int GetDoLocationQueueCount();

        public static event GetDoLocationQueueCount OnGetDoLocationQueueCount;
        public static event GetDoLocationQueueCount OnGetDoLocationQueueLbsWifiCount;

        public delegate int GetInsertHistoryQueueCount();

        public static event GetInsertHistoryQueueCount OnGetInsertHistoryQueueCount;

        public delegate void CollectLbsAndWifiHandler(LBSWIFI lbswifi);

        public static event CollectLbsAndWifiHandler OnCollectLbsAndWifi;

        private const string SystemVersion = "1.0.4";
        private const string TimeFormat = "yyyy/MM/dd HH:mm:ss";
        private const string DateFormat = "yyyy/MM/dd";
        private readonly string _apiKey;

        private readonly bool _history;

        public static readonly ConcurrentDictionary<string, User> LoginUserTemp = new ConcurrentDictionary<string, User>();
        private static readonly Dictionary<string, CheckNumber> CheckNumberTemp = new Dictionary<string, CheckNumber>();

        // Code ：大于2接口自定义错误 2取不到数据  1正常返回 0登录异常 -1输入错误 -2系统错误 -3越权操作
        // Code ：大于2接口自定义错误 7不能删除自己 6非管理员，无权操作,8通讯录已满
        public Client()
        {
            try
            {
                _apiKey = AppConfig.GetValue("ApiKey");
            }
            catch (Exception)
            {
            }

            try
            {
                if (AppConfig.GetValue("MongoDBIp").Length > 0 &&
                    AppConfig.GetValue("MongoDBPort").Length > 0)
                {
                    _history = true;
                }
                else
                {
                    _history = false;
                }
            }
            catch (Exception)
            {
                _history = false;
            }
        }

        #region 登录注册

        public string Login(int loginType, string phoneNumber, string passWord, string appleId, string project)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();
            try
            {
                if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(passWord) ||
                    loginType == 2 && string.IsNullOrEmpty(appleId))
                {
                    info.set('1', "Login", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"-1\",\"Message\":\"输入参数错误\" }";
                }

                var user = Logic.User.GetInstance().Get(phoneNumber);
                if (user == null)
                {
                    user = Logic.User.GetInstance().GetByBindNumber(phoneNumber);
                }

                string salt = user?.Salt;
                string passWordEncoded = MD5Helper.MD5Encrypt(passWord, salt);

                var usingApi = false;
                var keys = AppConfig.GetValue("CUSTOMER_KEYS").Split(',');
                if (keys.Contains(passWord))
                {
                    usingApi = true;
                }

                if (user == null || !user.Password.Equals(passWordEncoded))
                {
                    if (user == null)
                    {
                        Device device = Logic.Device.GetInstance().GetByBindNum(phoneNumber);

                        if (device == null || device.Deleted == true)
                        {
                            return "{ \"Code\":\"0\",\"Message\":\"用户不存在\" }";
                        }

                        if (!passWordEncoded.Equals(MD5Helper.MD5Encrypt(Constants.DEFAULT_PASSWORD, null)) && !usingApi)
                        {
                            return "{ \"Code\":\"0\",\"Message\":\"设备密码错误，默认密码：" + Constants.DEFAULT_PASSWORD + "\" }";
                        }

                        salt = Utils.createNewSalt();

                        if (usingApi)
                        {
                            passWord = Constants.DEFAULT_PASSWORD;
                        }

                        passWordEncoded = MD5Helper.MD5Encrypt(passWord, salt);
                        user = new User
                        {
                            BindNumber = phoneNumber,
                            Password = passWordEncoded,
                            Salt = salt,
                            UserType = 2,
                            LoginID = Guid.NewGuid().ToString(),
                            AppID = appleId,
                            Project = project,
                            Notification = true,
                            NotificationSound = true,
                            NotificationVibration = true
                        };

                        Logic.User.GetInstance().Save(user);
                        if (user.UserID != 0)
                        {
                            device.UserId = user.UserID;
                            Logic.Device.GetInstance().Save(device);
                        }

                        try
                        {
                            if (!UserDevice.GetInstance().IsUserDeviceExists(user.UserID, device.DeviceID))
                            {
                                var ud = new Model.Entity.UserDevice()
                                {
                                    DeviceID = device.DeviceID,
                                    UserID = user.UserID,
                                    Status = 1,
                                    CreateTime = DateTime.Now,
                                    UpdateTime = DateTime.Now
                                };
                                // 保存 ud 到数据库
                                UserDevice.GetInstance().New(ud);
                            }
                        }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
                        catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
                        {
                            //解决旧数据问题
                        }


                        // 生成初始化DeviceSet
                        DeviceSet.GetInstance().CreateDefaultDeviceSet(device.DeviceID);
                        // 绑定图灵接口
                        APIClient.GetInstance().ProcessBind(device.DeviceID);
                    }
                    else if (!usingApi)
                    {
                        info.set('1', "Login", "", 0, Environment.TickCount - ticker, 0);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{ \"Code\":\"0\",\"Message\":\"用户名或密码错误\" }";
                    }
                }

                if (!usingApi)
                {
                    lock (user)
                    {
                        if (!string.IsNullOrEmpty(user.LoginID))
                        {
                            LoginUserTemp.TryRemove(user.LoginID, out var res);
                        }

                        user.LoginID = Guid.NewGuid().ToString();
                        if (!string.IsNullOrEmpty(appleId))
                        {
                            appleId = appleId.Replace(" ", "");
                            var list = Logic.User.GetInstance().GetDictionary().Where(w => !string.IsNullOrEmpty(w.Value.AppID) && w.Value.AppID.Equals(appleId)).ToList().ConvertAll(x=>x.Value);
                            foreach (var item in list)
                            {
                                item.AppID = null;
                                Logic.User.GetInstance().Save(item);
                            }

                            user.AppID = appleId;
                        }
                        else
                        {
                            user.AppID = null;
                        }

                        user.Project = project;
                        user.LoginType = loginType;
                        Logic.User.GetInstance().Save(user);
                        LoginUserTemp.TryAdd(user.LoginID, user);
                    }
                }
                else if (string.IsNullOrEmpty(user.LoginID))
                {
                    user.LoginID = Guid.NewGuid().ToString();
                    user.Project = project;
                    user.LoginType = loginType;
                    lock (user)
                    {
                        Logic.User.GetInstance().Save(user);
                        LoginUserTemp.TryAdd(user.LoginID, user);
                    }
                }
                else
                {
                    user.LoginType = loginType;
                    lock (user)
                    {
                        Logic.User.GetInstance().Save(user);
                        if (!LoginUserTemp.ContainsKey(user.LoginID))
                        {
                            LoginUserTemp.TryAdd(user.LoginID, user);
                        }
                    }
                }

                Logic.Notification.GetInstance().CleanRenewNotification(user.UserID);

                info.set('1', "Login", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"1\",\"LoginId\":\"" + user.LoginID +
                       "\",\"UserId\":\"" + user.UserID + "\",\"PhoneNumber\":\"" + user.PhoneNumber + "\",\"BindNumber\":\"" + user.BindNumber +
                       "\",\"UserType\":\"" + user.UserType + "\",\"Name\":\"" +
                       user.Name + "\",\"Notification\":\"" + user.Notification + "\",\"NotificationSound\":\"" +
                       user.NotificationSound + "\",\"NotificationVibration\":\"" + user.NotificationVibration +
                       "\" }";
            }
            catch (Exception ex)
            {
                info.set('1', "Login", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        private User CheckLogin(string loginId)
        {
            if (string.IsNullOrEmpty(loginId))
            {
                throw new FaultException<ServiceError>(new ServiceError {Code = -1, Message = "输入参数错误!"});
            }

            LoginUserTemp.TryGetValue(loginId, out var user);
            if (user == null)
            {
                var res = Logic.User.GetInstance().GetDictionary().FirstOrDefault(w => w.Value.Deleted == false && w.Value.LoginID != null && w.Value.LoginID.Equals(loginId));
                user = default(KeyValuePair<int,User>).Equals(res) ? null : res.Value;
                if (user != null)
                {
                    LoginUserTemp.TryAdd(loginId, user);
                }
            }

            if (user == null)
            {
                throw new FaultException<ServiceError>(new ServiceError {Code = 0, Message = "登录信息已过期，请重新登录!"});
            }

            return user;
        }

        private User CheckLogin(string loginId, int deviceId)
        {
            if (string.IsNullOrEmpty(loginId))
            {
                throw new FaultException<ServiceError>(new ServiceError {Code = -1, Message = "输入参数错误!"});
            }

            LoginUserTemp.TryGetValue(loginId, out var user);
            if (user == null)
            {
                var res = Logic.User.GetInstance().GetDictionary().FirstOrDefault(w => w.Value.Deleted == false && w.Value.LoginID != null && w.Value.LoginID.Equals(loginId));
                user = default(KeyValuePair<int, User>).Equals(res) ? null : res.Value;
            }

            if (user == null)
            {
                throw new FaultException<ServiceError>(new ServiceError {Code = 0, Message = "登录信息已过期，请重新登录!"});
            }
            if (!UserDevice.GetInstance().GetByUserId(user.UserID).ContainsKey(deviceId) ||
                UserDevice.GetInstance().GetByUserId(user.UserID)[deviceId].Status == 0)
            {
                throw new FaultException<ServiceError>(new ServiceError {Code = -3, Message = "无权操作该设备!"});
            }

            return user;
        }

        public string LoginOut(string loginId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
//                lock (user)
//                {
//                    if (user != null)
//                    {
//                        user.LoginID = null;
//                        user.AppID = null;
//                        user.LoginType = 0;
//                        user.Project = null;
//                        Logic.User.GetInstance().Save(user);
//                    }

//                if (LoginUserTemp.ContainsKey(loginId))
//                {
                LoginUserTemp.TryRemove(loginId, out var res);
//                }
//                }

                int ticker2 = Environment.TickCount;
                info.set('1', "LoginOut", "", 0, ticker2 - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"1\" }";
            }
            catch (FaultException<ServiceError> ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "LoginOut", "", 0, ticker2 - ticker, -1);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "LoginOut", "", 0, ticker2 - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string ChangePassword(string loginId, string passWord, string newPassword)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (string.IsNullOrEmpty(passWord) || string.IsNullOrEmpty(newPassword))
                {
                    info.set('1', "ChangePassword", "", 0, 0, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"-1\",\"Message\":\"输入参数错误\" }";
                }

                var user = CheckLogin(loginId);

                if (user != null && string.IsNullOrEmpty(user.PhoneNumber))
                {
                    return "{\"Code\":\"-2\",\"Message\":\"用户没有设置管理员手机号，不能修改密码！\"}";
                }

                passWord = MD5Helper.MD5Encrypt(passWord, user.Salt);
                int ticker2 = Environment.TickCount;
                if (user.Password.Equals(passWord))
                {
                    lock (user)
                    {
                        if (string.IsNullOrEmpty(user.Salt))
                        {
                            user.Salt = Utils.createNewSalt();
                        }

                        newPassword = MD5Helper.MD5Encrypt(newPassword, user.Salt);
                        user.Password = newPassword;
                        Logic.User.GetInstance().Save(user);
                    }
                }
                else
                {
                    info.set('1', "ChangePassword", "", 0, ticker2 - ticker, 0);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"3\",\"Message\":\"原密码错误\" }";
                }

                info.set('1', "ChangePassword", "", 0, ticker2 - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"1\",\"Message\":\"密码修改成功\" }";
            }
            catch (FaultException<ServiceError> ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "ChangePassword", "", 0, ticker2 - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"-99\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "ChangePassword", "", 0, ticker2 - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"系统出错\" }";
            }
        }

        public string RegisterCheck(string phoneNumber, string phoneCode, string project)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    info.set('1', "RegisterCheck", "", 0, 1, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"-1\",\"Message\":\"输入参数错误\" }";
                }

                var user = Logic.User.GetInstance().Get(phoneNumber);
                if (user != null)
                {
                    info.set('1', "RegisterCheck", "", 0, Environment.TickCount - ticker, -2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"3\",\"Message\":\"该用户已经存在\" }";
                }

                /* cheny changed at 20161117 for old version sms reg
                var ran = new Random();
                string checkCode = ran.NextDouble().ToString().Substring(2, 6);
                lock (CheckNumberTemp)
                {
                    if (CheckNumberTemp.ContainsKey(phoneNumber))
                    {
                        if (CheckNumberTemp[phoneNumber].Black >= 3)
                        {
                            return "{ \"Code\":\"4\",\"Message\":\"尝试次数太多，请稍候再试\" }";
                        }

                        if (CheckNumberTemp[phoneNumber].Count >= 3)
                        {
                            if (CheckNumberTemp[phoneNumber].Time.AddHours(24) < DateTime.Now)
                            {
                                CheckNumberTemp[phoneNumber].Count = 0;
                                CheckNumberTemp[phoneNumber].Black++;
                                CheckNumberTemp[phoneNumber].Time = DateTime.Now;
                            }
                            else
                            {
                                //return "{ \"Code\":\"4\",\"Message\":\"尝试次数较多，请稍候再试\" }";
                                return "{ \"Code\":\"1\",\"Message\":\"验证码已经发送成功\" }";
                            }

                        }

                        if (CheckNumberTemp[phoneNumber].Time.AddMinutes(10) > DateTime.Now)
                        {
                            //return "{ \"Code\":\"4\",\"Message\":\"请等待三分钟后再获取手机验证码\" }";
                            return "{ \"Code\":\"1\",\"Message\":\"验证码已经发送成功\" }";
                        }
                        else
                        {
                            CheckNumberTemp[phoneNumber].CheckCode = checkCode;
                            CheckNumberTemp[phoneNumber].Time = DateTime.Now;
                            CheckNumberTemp[phoneNumber].Count++;
                        }
                    }
                    else
                        CheckNumberTemp.Add(phoneNumber, new CheckNumber
                        {
                            PhoneNumber = phoneNumber,
                            CheckCode = checkCode,
                            Time = DateTime.Now,
                            Count = 0,
                            Black = 0
                        });


                }

                if (!string.IsNullOrEmpty(phoneCode))
                {
                    return "{ \"Code\":\"1\",\"Message\":\"验证码已经发送成功\" }";
                }

                if (SMS.GetInstance().SendRegCheckNumber(project, phoneNumber, checkCode))
                    return "{ \"Code\":\"1\",\"Message\":\"验证码已经发送成功\" }";
                else
                {
                    return "{ \"Code\":\"2\",\"Message\":\"验证码发送失败,请稍后再试\" }";
                }*/
                int ticker2 = Environment.TickCount;
                info.set('1', "RegisterCheck", "", 0, ticker2 - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"1\",\"Message\":\"验证码已经发送成功\" }";
            }
            catch (Exception ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "RegisterCheck", "", 0, ticker2 - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string Register(string phoneNumber, string phoneCode, string checkNumber, string passWord, string appleId, string project)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (string.IsNullOrEmpty(phoneNumber) ||
                    string.IsNullOrEmpty(passWord))
                {
                    info.set('1', "Register", "", 0, 1, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"-1\",\"Message\":\"输入参数错误\" }";
                }
                /*rm this as sms reg
                if ((string.IsNullOrEmpty(checkNumber) || checkNumber.Length != 6)&&(string.IsNullOrEmpty(phoneCode)))
                {
                    return "{ \"Code\":\"-1\",\"Message\":\"输入参数错误\" }";
                }*/


                lock (CheckNumberTemp)
                {
                    var user = Logic.User.GetInstance().Get(phoneNumber);
                    if (user != null)
                    {
                        info.set('1', "Register", "", 0, Environment.TickCount - ticker, 3);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{ \"Code\":\"3\",\"Message\":\"该用户已经存在\" }";
                    }

                    /// rm this as sms reg CheckNumberTemp.ContainsKey(phoneNumber) (CheckNumberTemp[phoneNumber].CheckCode.Equals(checkNumber)||(!string.IsNullOrEmpty(phoneCode))))

                    //if (CheckNumberTemp[phoneNumber].Time.AddMinutes(30) < DateTime.Now)
                    //    return "{ \"Code\":\"5\",\"Message\":\"验证码超过有效期\" }";
                    if (!string.IsNullOrEmpty(appleId))
                    {
                        appleId = appleId.Replace(" ", "");
                        var list =
                            Logic.User.GetInstance()
                                .GetDictionary()
                                .Where(w => !string.IsNullOrEmpty(w.Value.AppID) && w.Value.AppID.Equals(appleId))
                                .Select(x=>x.Value);
                        foreach (var item in list)
                        {
                            item.AppID = null;
                            Logic.User.GetInstance().Save(item);
                        }
                    }

                    string salt = Utils.createNewSalt();
                    user = new User
                    {
                        PhoneNumber = phoneNumber,
                        Password = MD5Helper.MD5Encrypt(passWord, salt),
                        Salt = salt,
                        UserType = 1,
                        LoginID = Guid.NewGuid().ToString(),
                        AppID = appleId,
                        Project = project,
                        Notification = true,
                        NotificationSound = true,
                        NotificationVibration = true
                    };

                    Logic.User.GetInstance().Save(user);
                    LoginUserTemp.TryAdd(user.LoginID, user);

                    info.set('1', "Register", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);
                    //CheckNumberTemp.Remove(phoneNumber);
                    return "{ \"Code\":\"1\",\"LoginId\":\"" +
                           user.LoginID +
                           "\",\"UserId\":\"" + user.UserID + "\",\"UserType\":\"" + user.UserType +
                           "\",\"Name\":\"" +
                           user.Name + "\",\"Notification\":\"" + user.Notification + "\",\"NotificationSound\":\"" +
                           user.NotificationSound + "\",\"NotificationVibration\":\"" + user.NotificationVibration +
                           "\" }";
                }

#pragma warning disable CS0162 // 检测到无法访问的代码
                int ticker2 = Environment.TickCount;
#pragma warning restore CS0162 // 检测到无法访问的代码
                info.set('1', "Register", "", 0, ticker2 - ticker, 4);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"4\",\"Message\":\"验证码错误\" }";
            }
            catch (Exception ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "Register", "", 0, ticker2 - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string ForgotCheck(string phoneNumber, string project, string SerialNumber)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    int ticker2 = Environment.TickCount;
                    info.set('1', "ForgotCheck", "", 0, ticker2 - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"-1\",\"Message\":\"输入参数错误\" }";
                }

                var user = Logic.User.GetInstance().Get(phoneNumber);
                if (user == null)
                {
                    int ticker2 = Environment.TickCount;
                    info.set('1', "ForgotCheck", "", 0, ticker2 - ticker, 3);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"3\",\"Message\":\"用户并不存在\" }";
                }

                if (!string.IsNullOrEmpty(SerialNumber))
                {
                    var device = Logic.Device.GetInstance().GetByBindNum(SerialNumber);
                    if (device == null || device.Deleted)
                    {
                        int ticker2 = Environment.TickCount;
                        info.set('1', "ForgotCheck", "", 0, ticker2 - ticker, -2);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"-2\",\"Message\":\"绑定号出错\"}";
                    }

                    User usr = Logic.User.GetInstance().GetByBindNumber(SerialNumber);
                    if (usr != null && string.IsNullOrEmpty(usr.PhoneNumber))
                    {
                        return "{\"Code\":\"-2\",\"Message\":\"绑定号对应的用户没有设置管理员手机号，不能修改密码！\"}";
                    }

                    //else
                    {
                        List<User> userDevicelist = UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID).ToList();

                        foreach (var tUser in userDevicelist)
                        {
                            if (tUser.PhoneNumber.Equals(phoneNumber))
                            {
                                info.set('1', "ForgotCheck", "", 0, Environment.TickCount - ticker, 1);
                                CPhotoStat.GetInstance().add_info(info);

                                return "{ \"Code\":\"1\",\"Message\":\"验证通过\" }";
                            }
                        }

                        if (userDevicelist.Count == 0)
                        {
                            info.set('1', "ForgotCheck", "", 0, Environment.TickCount - ticker, 1);
                            CPhotoStat.GetInstance().add_info(info);

                            return "{ \"Code\":\"1\",\"Message\":\"验证通过\" }";
                        }

                        int ticker2 = Environment.TickCount;
                        info.set('1', "ForgotCheck", "", 0, ticker2 - ticker, -2);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"-2\",\"Message\":\"手机号与设备不匹配\"}";
                    }
                }

                Random ran = new Random();
                string checkCode = ran.NextDouble().ToString().Substring(2, 6);
                lock (CheckNumberTemp)
                {
                    if (CheckNumberTemp.ContainsKey(phoneNumber))
                    {
                        if (CheckNumberTemp[phoneNumber].Black >= 3)
                        {
                            int ticker2 = Environment.TickCount;
                            info.set('1', "ForgotCheck", "", 0, ticker2 - ticker, 4);
                            CPhotoStat.GetInstance().add_info(info);

                            return "{ \"Code\":\"4\",\"Message\":\"尝试次数太多，请稍候再试\" }";
                        }

                        if (CheckNumberTemp[phoneNumber].Count >= 3)
                        {
                            if (CheckNumberTemp[phoneNumber].Time.AddHours(24) < DateTime.Now)
                            {
                                CheckNumberTemp[phoneNumber].Count = 0;
                                CheckNumberTemp[phoneNumber].Black++;
                                CheckNumberTemp[phoneNumber].Time = DateTime.Now;
                            }
                            else
                            {
                                int ticker2 = Environment.TickCount;
                                info.set('1', "ForgotCheck", "", 0, ticker2 - ticker, 1);
                                CPhotoStat.GetInstance().add_info(info);
                                //return "{ \"Code\":\"4\",\"Message\":\"尝试次数较多，请稍候再试\" }";
                                return "{ \"Code\":\"1\",\"Message\":\"验证码已经发送成功\" }";
                            }
                        }

                        if (CheckNumberTemp[phoneNumber].Time.AddMinutes(10) > DateTime.Now)
                        {
                            int ticker2 = Environment.TickCount;
                            info.set('1', "ForgotCheck", "", 0, ticker2 - ticker, 1);
                            CPhotoStat.GetInstance().add_info(info);

                            //return "{ \"Code\":\"4\",\"Message\":\"请等待三分钟后再获取手机验证码\" }";
                            return "{ \"Code\":\"1\",\"Message\":\"验证码已经发送成功\" }";
                        }
                        else
                        {
                            CheckNumberTemp[phoneNumber].CheckCode = checkCode;
                            CheckNumberTemp[phoneNumber].Time = DateTime.Now;
                            CheckNumberTemp[phoneNumber].Count++;
                        }
                    }
                    else
                        CheckNumberTemp.Add(phoneNumber, new CheckNumber
                        {
                            PhoneNumber = phoneNumber,
                            CheckCode = checkCode,
                            Time = DateTime.Now
                        });
                }

                if (SMS.GetInstance().SendForgotCheckNumber(project, phoneNumber, checkCode))
                {
                    int ticker2 = Environment.TickCount;
                    info.set('1', "ForgotCheck", "", 0, ticker2 - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);
                    return "{ \"Code\":\"1\",\"Message\":\"验证码已经发送成功\" }";
                }
                else
                {
                    int ticker2 = Environment.TickCount;
                    info.set('1', "ForgotCheck", "", 0, ticker2 - ticker, -2);
                    CPhotoStat.GetInstance().add_info(info);
                    return "{ \"Code\":\"-2\",\"Message\":\"验证码发送失败,请稍后再试\" }";
                }
            }
            catch (Exception ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "ForgotCheck", "", 0, ticker2 - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-99\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string Forgot(string phoneNumber, string checkNumber, string passWord, string appleId, string project, string bindNumber)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (string.IsNullOrEmpty(phoneNumber) ||
                    string.IsNullOrEmpty(passWord))
                {
                    info.set('1', "Forgot", "", 0, 1, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"-1\",\"Message\":\"输入参数错误\" }";
                }

                #region 两种验证方式条件的初步判断

                if ((string.IsNullOrEmpty(checkNumber) || checkNumber.Length != 6) &&
                    (string.IsNullOrEmpty(bindNumber)))
                {
                    int ticker2 = Environment.TickCount;
                    info.set('1', "Forgot", "", 0, ticker2 - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"-1\",\"Message\":\"输入参数错误\" }";
                }

                #endregion

                var user = Logic.User.GetInstance().Get(phoneNumber);
                if (user == null)
                {
                    int ticker2 = Environment.TickCount;
                    info.set('1', "Forgot", "", 0, ticker2 - ticker, 3);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"3\",\"Message\":\"该用户并不存在\" }";
                }

                #region 如果APP是利用扫描设备绑定号来修改密码

                if (!string.IsNullOrEmpty(bindNumber))
                {
                    var device = Logic.Device.GetInstance().GetByBindNum(bindNumber);
                    if (device == null || device.Deleted)
                    {
                        int ticker2 = Environment.TickCount;
                        info.set('1', "Forgot", "", 0, ticker2 - ticker, -2);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"-2\",\"Message\":\"绑定号出错\"}";
                    }
                    else
                    {
                        List<User> userDevicelist = UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID).ToList();

                        int check_valid = 0;

                        foreach (var tUser in userDevicelist)
                        {
                            if (tUser.PhoneNumber.Equals(phoneNumber))
                            {
                                check_valid = 1;
                            }
                        }

                        if (userDevicelist.Count == 0)
                            check_valid = 1;

                        if (check_valid == 1)
                        {
                            #region 通过了用户与设备的绑定关系验证，重置用户密码

                            lock (user)
                            {
                                if (!string.IsNullOrEmpty(user.LoginID) && LoginUserTemp.ContainsKey(user.LoginID))
                                {
                                    LoginUserTemp.TryRemove(user.LoginID, out var res);
                                }

                                user.LoginID = Guid.NewGuid().ToString();
                                if (string.IsNullOrEmpty(user.Salt))
                                {
                                    user.Salt = Utils.createNewSalt();
                                }

                                user.Password = MD5Helper.MD5Encrypt(passWord, user.Salt);
                                if (!string.IsNullOrEmpty(appleId))
                                {
                                    appleId = appleId.Replace(" ", "");
                                    var list =
                                        Logic.User.GetInstance()
                                            .GetDictionary()
                                            .Where(w => !string.IsNullOrEmpty(w.Value.AppID) && w.Value.AppID.Equals(appleId))
                                            .Select(x=>x.Value);
                                    foreach (var item in list)
                                    {
                                        item.AppID = null;
                                        Logic.User.GetInstance().Save(item);
                                    }

                                    user.AppID = appleId;
                                }
                                else
                                {
                                    user.AppID = null;
                                }

                                user.Project = project;
                                Logic.User.GetInstance().Save(user);
                                LoginUserTemp.TryAdd(user.LoginID, user);
                            }

                            #endregion

                            Notification.GetInstance().CleanRenewNotification(user.UserID);

                            info.set('1', "Forgot", "", 0, Environment.TickCount - ticker, 1);
                            CPhotoStat.GetInstance().add_info(info);

                            return "{ \"Code\":\"1\",\"LoginId\":\"" +
                                   user.LoginID +
                                   "\",\"UserId\":\"" + user.UserID + "\",\"UserType\":\"" + user.UserType +
                                   "\",\"Name\":\"" +
                                   user.Name + "\",\"Notification\":\"" + user.Notification + "\",\"NotificationSound\":\"" +
                                   user.NotificationSound + "\",\"NotificationVibration\":\"" + user.NotificationVibration +
                                   "\" }";
                            ;
                        }

                        int ticker2 = Environment.TickCount;
                        info.set('1', "Forgot", "", 0, ticker2 - ticker, -2);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"-2\",\"Message\":\"手机号与设备不匹配\"}";
                    }
                }

                #endregion

                #region 如果APP是利用短信验证码来修改密码的

                else
                {
                    lock (CheckNumberTemp)
                    {
                        if (CheckNumberTemp.ContainsKey(phoneNumber) &&
                            CheckNumberTemp[phoneNumber].CheckCode.Equals(checkNumber))
                        {
                            if (CheckNumberTemp[phoneNumber].Time.AddMinutes(30) < DateTime.Now)
                            {
                                info.set('1', "Forgot", "", 0, Environment.TickCount - ticker, 5);
                                CPhotoStat.GetInstance().add_info(info);

                                return "{ \"Code\":\"5\",\"Message\":\"验证码超过有效期\" }";
                            }

                            #region 通过了短信验证码验证，重置用户密码

                            lock (user)
                            {
                                if (!string.IsNullOrEmpty(user.LoginID) && LoginUserTemp.ContainsKey(user.LoginID))
                                {
                                    LoginUserTemp.TryRemove(user.LoginID, out var res);
                                }

                                user.LoginID = Guid.NewGuid().ToString();
                                if (string.IsNullOrEmpty(user.Salt))
                                {
                                    user.Salt = Utils.createNewSalt();
                                }

                                user.Password = MD5Helper.MD5Encrypt(passWord, user.Salt);
                                if (!string.IsNullOrEmpty(appleId))
                                {
                                    appleId = appleId.Replace(" ", "");
                                    var list =
                                        Logic.User.GetInstance()
                                            .GetDictionary()
                                            .Where(w => !string.IsNullOrEmpty(w.Value.AppID) && w.Value.AppID.Equals(appleId))
                                            .ToList().ConvertAll(x=>x.Value);
                                    foreach (var item in list)
                                    {
                                        item.AppID = null;
                                        Logic.User.GetInstance().Save(item);
                                    }

                                    user.AppID = appleId;
                                }
                                else
                                {
                                    user.AppID = null;
                                }

                                user.Project = project;
                                Logic.User.GetInstance().Save(user);
                                LoginUserTemp.TryAdd(user.LoginID, user);
                            }

                            #endregion

                            CheckNumberTemp.Remove(phoneNumber);
                            Notification.GetInstance().CleanRenewNotification(user.UserID);


                            info.set('1', "Forgot", "", 0, Environment.TickCount - ticker, 1);
                            CPhotoStat.GetInstance().add_info(info);

                            return "{ \"Code\":\"1\",\"LoginId\":\"" +
                                   user.LoginID +
                                   "\",\"UserId\":\"" + user.UserID + "\",\"UserType\":\"" + user.UserType +
                                   "\",\"Name\":\"" +
                                   user.Name + "\",\"Notification\":\"" + user.Notification + "\",\"NotificationSound\":\"" +
                                   user.NotificationSound + "\",\"NotificationVibration\":\"" + user.NotificationVibration +
                                   "\" }";
                        }
                    }

                    int ticker2 = Environment.TickCount;
                    info.set('1', "Forgot", "", 0, ticker2 - ticker, 4);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"4\",\"Message\":\"验证码错误\" }";
                }

                #endregion
            }


            catch (Exception ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "Forgot", "", 0, ticker2 - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        #endregion

        #region 消息系统

        public string GetNotification(string loginId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                var list = Notification.GetInstance().GetNotification(user.UserID);
                var getList = Notification.GetInstance().GetNotificationCount(user.UserID);
                var getTotal = getList.Sum(s => s.Message + s.Voice + s.SMS);
                StringBuilder strGet = new StringBuilder();
                foreach (var item in getList)
                {
                    if (strGet.Length > 0)
                        strGet.Append(",");
                    strGet.Append("{ \"DeviceID\":\"" + item.DeviceID + "\",\"Message\":\"" + item.Message +
                                  "\",\"Voice\":\"" + item.Voice + "\",\"SMS\":\"" + item.SMS + "\",\"Photo\":\"" + item.Photo + "\" }");
                }

                StringBuilder strState = new StringBuilder();
                List<Device> userDevice = UserDevice.GetInstance().GetDeviceByUserId(user.UserID);
                var time = DateTime.Now;
                int totalDeviceState = 0;
                foreach (var item in userDevice)
                {
                    var deviceState = DeviceState.GetInstance().Get(item.DeviceID);
                    if (deviceState.UpdateTime < user.ActivityTime)
                        continue;

                    if (totalDeviceState > 0)
                        strState.Append(",");
                    totalDeviceState++;
                    strState.Append("{");
                    strState.Append("\"DeviceID\":\"" + deviceState.DeviceID + "\"");
                    strState.Append(",\"Altitude\":\"" + deviceState.Altitude + "\"");
                    strState.Append(",\"Course\":\"" + deviceState.Course + "\"");
                    strState.Append(",\"LocationType\":\"" + deviceState.LocationType + "\"");
                    strState.Append(",\"CreateTime\":\"" +
                                    deviceState.CreateTime.ToString(TimeFormat,
                                        DateTimeFormatInfo.InvariantInfo) + "\"");
                    strState.Append(",\"DeviceTime\":\"" +
                                    (deviceState.DeviceTime == null
                                        ? ""
                                        : deviceState.DeviceTime.Value.ToString(TimeFormat,
                                            DateTimeFormatInfo.InvariantInfo)) + "\"");
                    strState.Append(",\"Electricity\":\"" + deviceState.Electricity + "\"");
                    strState.Append(",\"GSM\":\"" + deviceState.GSM + "\"");
                    strState.Append(",\"Step\":\"" + deviceState.Step + "\"");
                    strState.Append(",\"Health\":\"" + deviceState.Health + "\"");
                    if (deviceState.Latitude != null && deviceState.Longitude != null)
                    {
                        double lat;
                        double lng;
                        LocationHelper.WGS84ToGCJ((double) deviceState.Latitude.Value, (double) deviceState.Longitude.Value, out lat, out lng);
                        strState.Append(",\"Latitude\":\"" + lat.ToString("F6") + "\"");
                        strState.Append(",\"Longitude\":\"" + lng.ToString("F6") + "\"");
                    }
                    else
                    {
                        strState.Append(",\"Latitude\":\"0\"");
                        strState.Append(",\"Longitude\":\"0\"");
                    }

                    strState.Append(",\"Online\":\"" + (deviceState.Online ? "1" : "0") + "\"");
                    strState.Append(",\"SatelliteNumber\":\"" + deviceState.SatelliteNumber + "\"");
                    strState.Append(",\"ServerTime\":\"" +
                                    (deviceState.ServerTime == null
                                        ? ""
                                        : deviceState.ServerTime.Value.ToString(TimeFormat,
                                            DateTimeFormatInfo.InvariantInfo)) + "\"");
                    strState.Append(",\"Speed\":\"" + deviceState.Speed + "\"");
                    strState.Append(",\"UpdateTime\":\"" +
                                    deviceState.UpdateTime.ToString(TimeFormat,
                                        DateTimeFormatInfo.InvariantInfo) + "\"");
                    strState.Append("}");
                }

                user.ActivityTime = time;


                StringBuilder str = new StringBuilder();
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        if (str.Length > 0)
                            str.Append(",");
                        if (item.Type == 1)
                        {
                            var notification = Notification.GetInstance().Get(item.ObjectId);
                            str.Append("{ \"Id\":\"" +
                                       item.UserNotificationId +
                                       "\",\"Type\":\"" +
                                       notification.Type +
                                       "\",\"DeviceID\":\"" + item.DeviceID + "\",\"Content\":\"" + notification.Content +
                                       "\",\"Message\":\"" +
                                       Notification.GetInstance()
                                           .GetNotificationDescription(notification.Type, notification) +
                                       "\",\"CreateTime\":\"" +
                                       item.CreateTime.ToString(TimeFormat,
                                           DateTimeFormatInfo.InvariantInfo) + "\" }");
                        }
                        else if (item.Type == 2)
                        {
                            var voice = DeviceVoice.GetInstance().Get(item.ObjectId);
                            str.Append("{ \"Id\":\"" +
                                       item.UserNotificationId +
                                       "\",\"Type\":\"1\",\"DeviceID\":\"" + item.DeviceID +
                                       "\",\"Content\":\"\",\"Message\":\"" +
                                       Notification.GetInstance().GetNotificationDescription(1, voice) +
                                       "\",\"CreateTime\":\"" +
                                       item.CreateTime.ToString(TimeFormat,
                                           DateTimeFormatInfo.InvariantInfo) + "\" }");
                        }
                        else if (item.Type == 3)
                        {
                            var sms = DeviceSMS.GetInstance().Get(item.ObjectId);
                            str.Append("{ \"Id\":\"" +
                                       item.UserNotificationId +
                                       "\",\"Type\":\"8\",\"DeviceID\":\"" + item.DeviceID +
                                       "\",\"Content\":\"\",\"Message\":\"" +
                                       Notification.GetInstance().GetNotificationDescription(8, sms) +
                                       "\",\"CreateTime\":\"" +
                                       item.CreateTime.ToString(TimeFormat,
                                           DateTimeFormatInfo.InvariantInfo) + "\" }");
                        }
                        else if (item.Type == 4)
                        {
                            var exception = DeviceException.GetInstance().Get(item.ObjectId);
                            str.Append("{ \"Id\":\"" +
                                       item.UserNotificationId +
                                       "\",\"Type\":\"" +
                                       exception.Type +
                                       "\",\"DeviceID\":\"" + item.DeviceID + "\",\"Content\":\"\",\"Message\":\"" +
                                       Notification.GetInstance().GetNotificationDescription(exception.Type, exception) +
                                       "\",\"CreateTime\":\"" +
                                       item.CreateTime.ToString(TimeFormat,
                                           DateTimeFormatInfo.InvariantInfo) + "\" }");
                        }
                        else if (item.Type == 5)
                        {
                            var notification = Notification.GetInstance().Get(item.ObjectId);
                            str.Append("{ \"Id\":\"" +
                                       item.UserNotificationId +
                                       "\",\"Type\":\"" +
                                       notification.Type +
                                       "\",\"DeviceID\":\"" + item.DeviceID + "\",\"Content\":\"" + notification.Content +
                                       "\",\"Message\":\"" +
                                       Notification.GetInstance()
                                           .GetNotificationDescription(notification.Type, notification) +
                                       "\",\"CreateTime\":\"" +
                                       item.CreateTime.ToString(TimeFormat,
                                           DateTimeFormatInfo.InvariantInfo) + "\" }");
                        }
                        else if (item.Type == 6)
                        {
                            str.Append("{ \"Id\":\"" +
                                       item.UserNotificationId +
                                       "\",\"Type\":\"" +
                                       item.ObjectId +
                                       "\",\"DeviceID\":\"" + item.DeviceID +
                                       "\",\"Content\":\"\",\"Message\":\"\",\"CreateTime\":\"" +
                                       item.CreateTime.ToString(TimeFormat,
                                           DateTimeFormatInfo.InvariantInfo) + "\" }");
                        }
                        else if (item.Type == 7)
                        {
                            var photo = DevicePhoto.GetInstance().Get(item.ObjectId);
                            str.Append("{ \"Id\":\"" +
                                       item.UserNotificationId +
                                       "\",\"Type\":\"11\",\"DeviceID\":\"" + item.DeviceID + "\",\"Content\":\"\",\"Message\":\"" +
                                       Notification.GetInstance()
                                           .GetNotificationDescription(11, photo) +
                                       "\",\"CreateTime\":\"" +
                                       item.CreateTime.ToString(TimeFormat,
                                           DateTimeFormatInfo.InvariantInfo) + "\" }");
                        }
                    }
                }

                int ticker2 = Environment.TickCount;
                info.set('1', "GetNotificatin", "", 0, ticker2 - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"1\",\"New\":\"" + getTotal + "\",\"NewList\":[" + strGet +
                       "],\"DeviceState\":[" + strState + "],\"Notification\":[" + str + "] }";
            }
            catch (FaultException<ServiceError> ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "GetNotificatin", "", 0, ticker2 - ticker, -1);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "GetNotificatin", "", 0, ticker2 - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string GetMessage(string loginId, int deviceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId, deviceId);
                var list = Notification.GetInstance().GetList(user.UserID, deviceId);
                if (list.Count == 0)
                {
                    int ticker2 = Environment.TickCount;
                    info.set('1', "GetMessage", "", 0, ticker2 - ticker, 0);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"2\",\"Message\":\"取不到数据\" }";
                }
                else
                {
                    StringBuilder str = new StringBuilder();
                    foreach (var item in list)
                    {
                        if (str.Length > 0)
                            str.Append(",");
                        if (item.Type == 1)
                        {
                            var notification = Notification.GetInstance().Get(item.ObjectId);
                            str.Append("{ \"Type\":\"" +
                                       notification.Type +
                                       "\",\"DeviceID\":\"" + item.DeviceID + "\",\"Content\":\"" + notification.Content +
                                       "\",\"Message\":\"" +
                                       Notification.GetInstance()
                                           .GetNotificationDescription(notification.Type, notification) +
                                       "\",\"CreateTime\":\"" +
                                       item.CreateTime.ToString(TimeFormat,
                                           DateTimeFormatInfo.InvariantInfo) + "\" }");
                        }
                        else if (item.Type == 4)
                        {
                            var exception = DeviceException.GetInstance().Get(item.ObjectId);
                            double lat;
                            double lng;
                            LocationHelper.WGS84ToGCJ((double) exception.Latitude, (double) exception.Longitude,
                                out lat, out lng);
                            str.Append("{ \"Type\":\"" +
                                       exception.Type +
                                       "\",\"DeviceID\":\"" + item.DeviceID + "\",\"Content\":\"" + lat + "-" + lng +
                                       "\",\"Message\":\"" +
                                       Notification.GetInstance().GetNotificationDescription(exception.Type, exception) +
                                       "\",\"CreateTime\":\"" +
                                       item.CreateTime.ToString(TimeFormat,
                                           DateTimeFormatInfo.InvariantInfo) + "\" }");
                        }
                        else if (item.Type == 5)
                        {
                            var notification = Notification.GetInstance().Get(item.ObjectId);
                            str.Append("{ \"Type\":\"" +
                                       notification.Type +
                                       "\",\"DeviceID\":\"" + item.DeviceID + "\",\"Content\":\"" + notification.Content +
                                       "\",\"Message\":\"" +
                                       Notification.GetInstance()
                                           .GetNotificationDescription(notification.Type, notification) +
                                       "\",\"CreateTime\":\"" +
                                       item.CreateTime.ToString(TimeFormat,
                                           DateTimeFormatInfo.InvariantInfo) + "\" }");
                        }
                    }

                    int ticker2 = Environment.TickCount;
                    info.set('1', "GetMessage", "", 0, ticker2 - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{ \"Code\":\"1\",\"List\":[" + str + "] }";
                }
            }
            catch (FaultException<ServiceError> ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "GetMessage", "", 0, ticker2 - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "GetMessage", "", 0, ticker2 - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        #endregion

        #region 关联设备

        public string LinkDeviceCheck(string loginId, string bindNumber)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (string.IsNullOrEmpty(bindNumber))
                {
                    info.set('1', "LinkDeviceChek", "", 0, 1, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                var user = CheckLogin(loginId);
                var device = Logic.Device.GetInstance().GetByBindNum(bindNumber);
                if (device == null || device.Deleted)
                {
                    int ticker2 = Environment.TickCount;
                    info.set('1', "LinkDeviceChek", "", 0, ticker2 - ticker, 3);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"3\",\"Message\":\"设备并不存在\"}";
                }
                else
                {
                    if (DeviceContact.GetInstance().GetByDeviceId(device.DeviceID).Count >= 50)
                    {
                        int ticker2 = Environment.TickCount;
                        info.set('1', "LinkDeviceChek", "", 0, ticker2 - ticker, 0);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"8\",\"Message\":\"通讯录已满\"}";
                    }

                    if (UserDevice.GetInstance().GetByUserId(user.UserID).ContainsKey(device.DeviceID))
                    {
                        var ud = UserDevice.GetInstance().GetByUserId(user.UserID)[device.DeviceID];
                        if (ud.Status == 1)
                        {
                            int ticker2 = Environment.TickCount;
                            info.set('1', "LinkDeviceChek", "", 0, ticker2 - ticker, 0);
                            CPhotoStat.GetInstance().add_info(info);

                            return "{\"Code\":\"4\",\"Message\":\"您已经与该设备关联\"}";
                        }
                        else
                        {
                            var notification = new Model.Entity.Notification()
                            {
                                DeviceID = device.DeviceID,
                                Type = 2,
                                Content = user.UserID + "," + user.PhoneNumber,
                                CreateTime = DateTime.Now
                            };
                            User us = Logic.User.GetInstance().Get(device.UserId);
                            if (us != null)
                            {
                                Notification.GetInstance().Send(notification, us);
                                //此处不需要通知客户端更新通讯录
                                int ticker2 = Environment.TickCount;
                                info.set('1', "LinkDeviceChek", "", 0, ticker2 - ticker, 1);
                                CPhotoStat.GetInstance().add_info(info);
                            }

                            return "{\"Code\":\"5\",\"Message\":\"请待设备管理员确认\"}";
                        }
                    }
                    else
                    {
                        int ticker2 = Environment.TickCount;
                        info.set('1', "LinkDeviceChek", "", 0, ticker2 - ticker, 2);
                        CPhotoStat.GetInstance().add_info(info);

                        if (device.UserId == 0)
                            return "{\"Code\":\"1\",\"Message\":\"设备未关联\"}";
                        else
                            return "{\"Code\":\"2\",\"Message\":\"设备已经被别人关联，需要请求对方同意\"}";
                    }
                }
            }
            catch (FaultException<ServiceError> ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "LinkDeviceChek", "", 0, ticker2 - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "LinkDeviceChek", "", 0, ticker2 - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string LinkDevice(string loginId, int photo, string name, string bindNumber)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (string.IsNullOrEmpty(bindNumber) || string.IsNullOrEmpty(name))
                {
                    int ticker2 = Environment.TickCount;
                    info.set('1', "LinkDevice", "", 0, ticker2 - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                var user = CheckLogin(loginId);
                var device = Logic.Device.GetInstance().GetByBindNum(bindNumber);
                if (device == null || device.Deleted)
                {
                    int ticker2 = Environment.TickCount;
                    info.set('1', "LinkDevice", "", 0, ticker2 - ticker, 3);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"3\",\"Message\":\"设备并不存在\"}";
                }
                else
                {
                    if (UserDevice.GetInstance().GetByUserId(user.UserID).ContainsKey(device.DeviceID))
                    {
                        var ud = UserDevice.GetInstance().GetByUserId(user.UserID)[device.DeviceID];
                        if (ud.Status == 1)
                        {
                            info.set('1', "LinkDevice", "", 0, Environment.TickCount - ticker, 4);
                            CPhotoStat.GetInstance().add_info(info);

                            return "{\"Code\":\"4\",\"Message\":\"您已经与该设备关联\"}";
                        }
                        else if (!device.BindNumber.Equals(user.BindNumber))
                        {
                            var notification = new Model.Entity.Notification()
                            {
                                DeviceID = device.DeviceID,
                                Type = 2,
                                Content = user.UserID + "," + name,
                                CreateTime = DateTime.Now
                            };
                            Notification.GetInstance()
                                .Send(notification, Logic.User.GetInstance().Get(device.UserId));
                            //此处不需要通知客户端更新通讯录

                            info.set('1', "LinkDevice", "", 0, Environment.TickCount - ticker, 5);
                            CPhotoStat.GetInstance().add_info(info);

                            return "{\"Code\":\"5\",\"Message\":\"正在等待设备管理员确认\"}";
                        }
                    }

                    if (device.UserId != 0 && !device.BindNumber.Equals(user.BindNumber))
                    {
                        var ud = new Model.Entity.UserDevice()
                        {
                            DeviceID = device.DeviceID,
                            UserID = user.UserID,
                            Status = 0,
                            CreateTime = DateTime.Now,
                            UpdateTime = DateTime.Now
                        };
                        // 保存 ud 到数据库
                        UserDevice.GetInstance().New(ud);
                        //发消息给 device.UserId 确认是否让该用户关联
                        var notification = new Model.Entity.Notification()
                        {
                            DeviceID = device.DeviceID,
                            Type = 2,
                            Content = user.UserID + "," + name,
                            CreateTime = DateTime.Now
                        };
                        Notification.GetInstance().Send(notification, Logic.User.GetInstance().Get(device.UserId));
                        //通知客户端更新通讯录
                        List<User> userList = UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID);
                        Notification.GetInstance().Send(device.DeviceID, 232, userList);

                        info.set('1', "LinkDevice", "", 0, Environment.TickCount - ticker, 1);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"1\",\"Message\":\"等待管理员确认\",\"DeviceID\":\"-1\"}";
                    }
                    else
                    {
                        device.UserId = user.UserID;
                        var ud = new Model.Entity.UserDevice()
                        {
                            DeviceID = device.DeviceID,
                            UserID = user.UserID,
                            Status = 1,
                            CreateTime = DateTime.Now,
                            UpdateTime = DateTime.Now
                        };
                        // 保存 ud 到数据库
                        UserDevice.GetInstance().New(ud);
                        //修改Device表中的UserID的状态

                        //添加到通讯录
                        if (!string.IsNullOrEmpty(user.PhoneNumber))
                        {
                            var contact = new Model.Entity.DeviceContact()
                            {
                                DeviceID = device.DeviceID,
                                Type = 2,
                                ObjectId = user.UserID,
                                Name = name,
                                PhoneNumber = user.PhoneNumber,
                                Photo = photo,
                                CreateTime = DateTime.Now,
                                UpdateTime = DateTime.Now,
                                HeadImgVersion = 0
                            };
                            DeviceContact.GetInstance().Save(contact);
                        }

                        device.BabyName = null;
                        device.Birthday = null;
                        device.ContactVersionNO = 1;
                        device.SetVersionNO = 1;
                        device.Firmware = null;
                        device.Gender = false;
                        device.Grade = 0;
                        device.HomeAddress = null;
                        device.HomeLat = null;
                        device.HomeLng = null;
                        device.IsGuard = false;
                        device.PhoneCornet = null;
                        device.PhoneNumber = null;
                        device.Photo = null;
                        device.SchoolAddress = null;
                        device.SchoolLat = null;
                        device.SchoolLng = null;
                        device.SmsBalanceKey = null;
                        device.SmsFlowKey = null;
                        device.SmsNumber = null;
                        device.LatestTime = "18:00";
                        Logic.Device.GetInstance().Save(device);

                        // 生成初始化DeviceSet
                        DeviceSet.GetInstance().CreateDefaultDeviceSet(device.DeviceID);
                        var deviceState = DeviceState.GetInstance().Get(device.DeviceID);
                        lock (deviceState)
                        {
                            deviceState.Altitude = 0;
                            deviceState.Course = 0;
                            deviceState.DeviceTime = null;
                            deviceState.Electricity = 0;
                            deviceState.GSM = 0;
                            deviceState.Latitude = null;
                            deviceState.LBS = null;
                            deviceState.LocationType = 0;
                            deviceState.Longitude = null;
                            deviceState.Radius = 0;
                            deviceState.SatelliteNumber = 0;
                            deviceState.ServerTime = null;
                            deviceState.Speed = 0;
                            deviceState.Wifi = null;
                        }

                        //通知网关更新设备配置
                        if (OnSend != null)
                            OnSend(device, SendType.Set, "");
                        //通知网关更新设备通讯录
                        if (OnSend != null)
                            OnSend(device, SendType.Contact, "");
                        //此处不需要通知客户端更新通讯录
                        if (OnSend != null)
                            OnSend(device, SendType.Init, "1");
                    }

                    int ticker2 = Environment.TickCount;
                    info.set('1', "LinkDevice", "", 0, ticker2 - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"1\",\"Message\":\"设备关联成功\",\"DeviceID\":\"" + device.DeviceID + "\"}";
                }
            }
            catch (FaultException<ServiceError> ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "LinkDevice", "", 0, ticker2 - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "LinkDevice", "", 0, ticker2 - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string LinkDeviceConfirm(string loginId, int deviceId, int userId, string name, int photo, int confirm)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId == 0 || userId == 0)
                {
                    info.set('1', "LinkDeviceConf", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                var device = Logic.Device.GetInstance().Get(deviceId);
                if (device == null || device.Deleted)
                {
                    info.set('1', "LinkDeviceConf", "", 0, Environment.TickCount - ticker, 3);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"3\",\"Message\":\"设备并不存在\"}";
                }
                else
                {
                    if (device.UserId != user.UserID)
                    {
                        info.set('1', "LinkDeviceConf", "", 0, Environment.TickCount - ticker, -3);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"-3\",\"Message\":\"无权操作\"}";
                    }
                    else
                    {
                        if (confirm == 1)
                        {
                            if (DeviceContact.GetInstance().GetByDeviceId(deviceId).Count >= 50)
                            {
                                //Logic.UserDevice.GetInstance().Del(userId, deviceId);
                                //var notification = new Model.Entity.Notification()
                                //{
                                //    DeviceID = device.DeviceID,
                                //    Type = 4,
                                //    Content = null,
                                //    CreateTime = DateTime.Now

                                //};
                                //Logic.Notification.GetInstance()
                                //    .Send(notification, Logic.User.GetInstance().Get(userId));
                                ////通知客户端更新通讯录
                                //List<Model.Entity.User> userList =
                                //    UserDevice.GetInstance()
                                //        .GetUserByDeviceId(device.DeviceID);
                                //Notification.GetInstance().Send(device.DeviceID, 232, userList);
                                info.set('1', "LinkDeviceConf", "", 0, Environment.TickCount - ticker, 8);
                                CPhotoStat.GetInstance().add_info(info);

                                return "{\"Code\":\"8\",\"Message\":\"通讯录已满,请删除部分联系人后再试\"}";
                            }

                            var udList = UserDevice.GetInstance().GetByUserId(userId);
                            Model.Entity.UserDevice ud = null;
                            if (udList != null)
                            {
                                if (!udList.TryGetValue(device.DeviceID, out ud))
                                {
                                    info.set('1', "LinkDeviceConf", "", 0, Environment.TickCount - ticker, 2);
                                    CPhotoStat.GetInstance().add_info(info);

                                    return "{\"Code\":\"2\",\"Message\":\"消息已过期\"}";
                                }
                            }
                            else
                            {
                                info.set('1', "LinkDeviceConf", "", 0, Environment.TickCount - ticker, 2);
                                CPhotoStat.GetInstance().add_info(info);

                                return "{\"Code\":\"2\",\"Message\":\"未取到数据\"}";
                            }

                            if (ud.Status == 1)
                            {
                                info.set('1', "LinkDeviceConf", "", 0, Environment.TickCount - ticker, 4);
                                CPhotoStat.GetInstance().add_info(info);

                                return "{\"Code\":\"4\",\"Message\":\"该用户已经与该设备关联\"}";
                            }
                            else
                            {
                                //添加到通讯录
                                User _user = Logic.User.GetInstance().Get(userId);
                                bool checkDeviceContact = false;
                                List<Model.Entity.DeviceContact> contactList = DeviceContact.GetInstance().GetByDeviceId(deviceId);
                                if (contactList.Exists(p => p.Name == name))
                                {
                                    info.set('1', "LinkDeviceConf", "", 0, Environment.TickCount - ticker, 8);
                                    CPhotoStat.GetInstance().add_info(info);

                                    return "{\"Code\":\"8\",\"Message\":\"通讯录中已有该名称\"}";
                                }

                                foreach (var item in contactList)
                                {
                                    if (item.PhoneNumber == _user.PhoneNumber)
                                    {
                                        item.Type = 2;
                                        item.ObjectId = _user.UserID;
                                        item.Name = name;
                                        item.Photo = photo;
                                        DeviceContact.GetInstance().Save(item);
                                        checkDeviceContact = true;
                                        break;
                                    }
                                }

                                if (!checkDeviceContact && !string.IsNullOrEmpty(_user.PhoneNumber))
                                {
                                    var contact = new Model.Entity.DeviceContact()
                                    {
                                        DeviceID = deviceId,
                                        Type = 2,
                                        ObjectId = _user.UserID,
                                        Name = name,
                                        PhoneNumber = _user.PhoneNumber,
                                        Photo = photo,
                                        CreateTime = DateTime.UtcNow,
                                        UpdateTime = DateTime.UtcNow,
                                        HeadImgVersion = 0
                                    };
                                    DeviceContact.GetInstance().Save(contact);
                                }

                                UserDevice.GetInstance().Update(userId, deviceId, 1);
                                var notification = new Model.Entity.Notification()
                                {
                                    DeviceID = device.DeviceID,
                                    Type = 3,
                                    Content = null,
                                    CreateTime = DateTime.Now
                                };
                                device.ContactVersionNO++;
                                Logic.Device.GetInstance().Save(device);
                                Notification.GetInstance()
                                    .Send(notification, Logic.User.GetInstance().Get(userId));
                                //通知网关更新设备通讯录
                                if (OnSend != null)
                                    OnSend(device, SendType.Contact, "");
                                //通知客户端更新通讯录
                                List<User> userList =
                                    UserDevice.GetInstance()
                                        .GetUserByDeviceId(device.DeviceID).Where(w => w.UserID != user.UserID).ToList();
                                Notification.GetInstance().Send(device.DeviceID, 232, userList);
                            }
                        }
                        else
                        {
                            var udList = UserDevice.GetInstance().GetByUserId(userId);
                            Model.Entity.UserDevice ud = null;
                            if (udList != null)
                            {
                                if (!udList.TryGetValue(device.DeviceID, out ud) || ud.Status == 1)
                                {
                                    info.set('1', "LinkDeviceConfirm", "", 0, Environment.TickCount - ticker, 2);
                                    CPhotoStat.GetInstance().add_info(info);

                                    return "{\"Code\":\"2\",\"Message\":\"消息已过期\"}";
                                }
                            }

                            UserDevice.GetInstance().Del(userId, deviceId);
                            var notification = new Model.Entity.Notification()
                            {
                                DeviceID = device.DeviceID,
                                Type = 4,
                                Content = null,
                                CreateTime = DateTime.Now
                            };
                            Notification.GetInstance().Send(notification, Logic.User.GetInstance().Get(userId));
                            //通知客户端更新通讯录
                            List<User> userList =
                                UserDevice.GetInstance()
                                    .GetUserByDeviceId(device.DeviceID).Where(w => w.UserID != user.UserID).ToList();
                            Notification.GetInstance().Send(device.DeviceID, 232, userList);
                        }
                    }
                }

                info.set('1', "LinkDeviceConf", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "LinkDeviceConf", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                info.set('1', "LinkDeviceConf", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        /// <summary>
        /// 解除绑定
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public string ReleaseBound(string loginId, int deviceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                if (deviceId <= 0)
                {
                    info.set('1', "ReleaseBound", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"参数错误\"}";
                }

                Device device = Logic.Device.GetInstance().Get(deviceId);
                /*if (device!=null&&device.BindNumber.Equals(user.BindNumber))
                {
                    info.set('1', "ReleaseBound", "", 0, System.Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-99\",\"Message\":\"您不能解绑此设备！\"}";
                }*/
                if (user.UserID == device.UserId || device.BindNumber.Equals(user.BindNumber))
                {
                    User usr = Logic.User.GetInstance().Get(device.UserId);
                    if (usr == null)
                    {
                        usr = Logic.User.GetInstance().GetByBindNumber(device.BindNumber);
                    }

                    if (usr != null)
                    {
                        LoginUserTemp.TryRemove(LoginUserTemp.FirstOrDefault(x => x.Value.UserID == usr.UserID).Value.LoginID, out var res);
                    }

                    Logic.Device.GetInstance().Save(device);
                    OnSend?.Invoke(device, SendType.Init, "0");
                    var notification = new Model.Entity.Notification()
                    {
                        DeviceID = device.DeviceID,
                        Type = 9,
                        Content = null,
                        CreateTime = DateTime.Now
                    };
                    List<User> userList = UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID).Where(w => w.UserID != user.UserID).ToList();
                    Notification.GetInstance().Send(notification, userList);
                    //无需客户端更新通讯录

                    APIClient.GetInstance().ProcessUnBind(deviceId);
                    Task.Run(() =>
                    {
                        Thread.Sleep(2000);
                        OnDisConnect?.Invoke(deviceId);
                    });
                    Task.Run(() =>
                    {
                        Thread.Sleep(3000);
                        ClearDeviceHistory(loginId, deviceId);
                        userList.ForEach(x =>
                        {
                            LoginUserTemp.TryRemove(x.LoginID, out var res);
                            Logic.User.GetInstance().DelReal(x.UserID);
                        });
                        Logic.Device.GetInstance().CleanRelation(deviceId);
                        Logic.Device.GetInstance().ResetDevice(device);
                        Logic.Device.GetInstance().Save(device);
                    });
                }
                else
                {
                    UserDevice.GetInstance().Del(user.UserID, deviceId);
                    List<Model.Entity.DeviceContact> contactList = DeviceContact.GetInstance().GetByDeviceId(deviceId);
                    Model.Entity.DeviceContact deviceContact = contactList.Find(p => p.PhoneNumber == user.PhoneNumber);
                    if (deviceContact != null)
                    {
                        DeviceContact.GetInstance().Del(deviceContact.DeviceContactId);
                        device.ContactVersionNO++;
                        Logic.Device.GetInstance().Save(device);
                        //通知网关更新设备通讯录
                        if (OnSend != null)
                            OnSend(device, SendType.Contact, "");
                        //通知其他客户端更新通讯录
                        List<User> userList = UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID).Where(w => w.UserID != user.UserID).ToList();
                        Notification.GetInstance().Send(device.DeviceID, 232, userList);
                    }
                }

                info.set('1', "ReleaseBound", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"已解除关联\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "ReleaseBound", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "ReleaseBound", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 设备信息

        public string GetDeviceDetail(string loginId, int deviceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0)
                {
                    info.set('1', "GetDeviceDeta", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"0\",\"Message\":\"输入参数错误\"}";
                }

                StringBuilder sb = new StringBuilder();
                var user = CheckLogin(loginId, deviceId);
                var device = Logic.Device.GetInstance().Get(deviceId);

                if (device == null)
                {
                    info.set('1', "GetDeviceDeta", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"取不到数据\"}";
                }
                else
                {
                    double homeLat = 0, homeLng = 0, schoolLat = 0, schoolLng = 0;
                    if (device.HomeLat != null && device.HomeLng != null)
                    {
                        LocationHelper.WGS84ToGCJ((double) device.HomeLat, (double) device.HomeLng, out homeLat,
                            out homeLng);
                    }

                    if (device.SchoolLat != null && device.SchoolLng != null)
                    {
                        LocationHelper.WGS84ToGCJ((double) device.SchoolLat, (double) device.SchoolLng,
                            out schoolLat, out schoolLng);
                    }

                    sb.Append("{");
                    sb.Append("\"Code\":\"1\"");
                    sb.Append(",\"ActiveDate\":\"" +
                              (device.ActiveDate == null
                                  ? ""
                                  : device.ActiveDate.Value.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo)) + "\"");
                    sb.Append(",\"BabyName\":\"" + GetStr(device.BabyName) + "\"");
                    sb.Append(",\"BindNumber\":\"" + device.BindNumber + "\"");
                    sb.Append(",\"Birthday\":\"" +
                              (device.Birthday == null
                                  ? ""
                                  : device.Birthday.Value.ToString(DateFormat,
                                      DateTimeFormatInfo.InvariantInfo)) + "\"");
                    sb.Append(",\"CreateTime\":\"" +
                              device.CreateTime.ToString(TimeFormat,
                                  DateTimeFormatInfo.InvariantInfo) + "\"");
                    sb.Append(",\"CurrentFirmware\":\"" + device.CurrentFirmware + "\"");
                    sb.Append(",\"SetVersionNO\":\"" + device.SetVersionNO + "\"");
                    sb.Append(",\"ContactVersionNO\":\"" + device.ContactVersionNO + "\"");
                    sb.Append(",\"OperatorType\":\"" + device.OperatorType + "\"");
                    sb.Append(",\"SmsNumber\":\"" + GetStr(device.SmsNumber) + "\"");
                    sb.Append(",\"SmsBalanceKey\":\"" + GetStr(device.SmsBalanceKey) + "\"");
                    sb.Append(",\"SmsFlowKey\":\"" + GetStr(device.SmsFlowKey) + "\"");
                    sb.Append(",\"DeviceID\":\"" + device.DeviceID + "\"");
                    sb.Append(",\"UserId\":\"" + device.UserId + "\"");
                    sb.Append(",\"DeviceModelID\":\"" + Convert.ToString(device.DeviceModelID + (_history ? 128 : 0), 2).PadLeft(8, '0') + "\"");
                    sb.Append(",\"Firmware\":\"" + device.Firmware + "\"");
                    sb.Append(",\"Gender\":\"" + (device.Gender ? "1" : "0") + "\"");
                    sb.Append(",\"Grade\":\"" + device.Grade + "\"");
                    sb.Append(",\"HireExpireDate\":\"" +
                              (device.HireExpireDate == null
                                  ? ""
                                  : device.HireExpireDate.Value.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo)) + "\"");
                    sb.Append(",\"HireStartDate\":\"" +
                              (device.HireStartDate == null
                                  ? ""
                                  : device.HireStartDate.Value.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo)) + "\"");
                    sb.Append(",\"HomeAddress\":\"" + GetStr(device.HomeAddress) + "\"");
                    sb.Append(",\"HomeLat\":\"" + homeLat.ToString("F6") + "\"");
                    sb.Append(",\"HomeLng\":\"" + homeLng.ToString("F6") + "\"");
                    sb.Append(",\"IsGuard\":\"" + (device.IsGuard ? "1" : "0") + "\"");
                    sb.Append(",\"Password\":\"\"");
                    sb.Append(",\"PhoneCornet\":\"" + GetStr(device.PhoneCornet) + "\"");
                    sb.Append(",\"PhoneNumber\":\"" + GetStr(device.PhoneNumber) + "\"");
                    sb.Append(",\"Photo\":\"" + device.Photo + "\"");
                    sb.Append(",\"SchoolAddress\":\"" + GetStr(device.SchoolAddress) + "\"");
                    sb.Append(",\"SchoolLat\":\"" + schoolLat.ToString("F6") + "\"");
                    sb.Append(",\"SchoolLng\":\"" + schoolLng.ToString("F6") + "\"");
                    sb.Append(",\"SerialNumber\":\"" + device.SerialNumber + "\"");
                    sb.Append(",\"UpdateTime\":\"" +
                              device.UpdateTime.ToString(TimeFormat,
                                  DateTimeFormatInfo.InvariantInfo) + "\"");
                    sb.Append(",\"LatestTime\":\"" + device.LatestTime + "\"");
                    sb.Append("}");
                }

                info.set('1', "GetDeviceDeta", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetDeviceDeta", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                info.set('1', "GetDeviceDeta", "", 0, Environment.TickCount - ticker, -1);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public static string GetStr(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            else
            {
                str = str.Replace("'", "");
                str = str.Replace("\"", "");
                str = str.Replace("\r\n", "");
                str = str.Replace("\r", "");
                str = str.Replace("\n", "");
                return str;
            }
        }

        public string UpdateDeviceSet(string loginId, int deviceId, string setInfo, string classDisable1,
            string classDisable2, string weekDisable, string timeClose, string timeOpen, int brightScreen, string weekAlarm1, string weekAlarm2, string weekAlarm3, string alarm1, string alarm2,
            string alarm3,
            string locationMode, string locationTime, string flowerNumber, string sleepCalculate, string stepCalculate, string hrCalculate, string sosMsgswitch)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0)
                {
                    info.set('1', "UpdateDeviceS", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                Model.Entity.DeviceSet deviceSet = DeviceSet.GetInstance().Get(deviceId);
                var device = Logic.Device.GetInstance().Get(deviceId);
                if (device == null || deviceSet == null)
                {
                    info.set('1', "UpdateDeviceS", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"取不到数据\"}";
                }
                else
                {
                    if (!string.IsNullOrEmpty(setInfo))
                    {
                        if (setInfo.Length != 23)
                        {
                            info.set('1', "UpdateDeviceS", "", 0, Environment.TickCount - ticker, -1);
                            CPhotoStat.GetInstance().add_info(info);

                            return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                        }

                        deviceSet.SetInfo = setInfo;
                    }

                    if (!string.IsNullOrEmpty(classDisable1))
                    {
                        deviceSet.ClassDisabled1 = classDisable1;
                    }

                    if (!string.IsNullOrEmpty(classDisable2))
                    {
                        deviceSet.ClassDisabled2 = classDisable2;
                    }

                    if (!string.IsNullOrEmpty(weekDisable))
                    {
                        deviceSet.WeekDisabled = weekDisable;
                    }

                    if (!string.IsNullOrEmpty(timeClose))
                    {
                        deviceSet.TimerClose = timeClose;
                    }

                    if (!string.IsNullOrEmpty(timeOpen))
                    {
                        deviceSet.TimerOpen = timeOpen;
                    }

                    if (brightScreen != 0)
                    {
                        deviceSet.BrightScreen = brightScreen;
                    }

                    if (!string.IsNullOrEmpty(weekAlarm1))
                    {
                        deviceSet.WeekAlarm1 = weekAlarm1;
                    }

                    if (!string.IsNullOrEmpty(weekAlarm2))
                    {
                        deviceSet.WeekAlarm2 = weekAlarm2;
                    }

                    if (!string.IsNullOrEmpty(weekAlarm3))
                    {
                        deviceSet.WeekAlarm3 = weekAlarm3;
                    }

                    if (!string.IsNullOrEmpty(alarm1))
                    {
                        deviceSet.Alarm1 = alarm1;
                    }

                    if (!string.IsNullOrEmpty(alarm2))
                    {
                        deviceSet.Alarm2 = alarm2;
                    }

                    if (!string.IsNullOrEmpty(alarm3))
                    {
                        deviceSet.Alarm3 = alarm3;
                    }

                    if (!string.IsNullOrEmpty(locationMode))
                    {
                        deviceSet.LocationMode = locationMode;
                    }

                    if (!string.IsNullOrEmpty(locationTime))
                    {
                        deviceSet.LocationTime = locationTime;
                    }

                    if (!string.IsNullOrEmpty(flowerNumber))
                    {
                        deviceSet.FlowerNumber = flowerNumber;
                    }

                    if (!string.IsNullOrEmpty(sleepCalculate))
                    {
                        deviceSet.SleepCalculate = sleepCalculate;
                    }

                    if (!string.IsNullOrEmpty(stepCalculate))
                    {
                        deviceSet.StepCalculate = stepCalculate;
                    }

                    if (!string.IsNullOrEmpty(hrCalculate))
                    {
                        deviceSet.HrCalculate = hrCalculate;
                    }

                    if (!string.IsNullOrEmpty(sosMsgswitch))
                    {
                        deviceSet.SosMsgswitch = sosMsgswitch;
                    }

                    DeviceSet.GetInstance().Save(deviceSet);
                    device.SetVersionNO++;
                    Logic.Device.GetInstance().Save(device);
                    //通知网关更新设备配置
                    if (OnSend != null)
                        OnSend(device, SendType.Set, "");

                    //修改成功后推送消息，报告设备设置改动情况
                    List<User> userList = UserDevice.GetInstance()
                        .GetUserByDeviceId(device.DeviceID).Where(w => w.UserID != user.UserID).ToList();
                    if (userList.Count > 0)
                        Notification.GetInstance().Send(device.DeviceID, 231, userList);

                    info.set('1', "UpdateDeviceS", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"1\",\"Message\":\"修改成功\"}";
                }
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "UpdateDeviceS", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                info.set('1', "UpdateDeviceS", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string GetDeviceSet(string loginId, int deviceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0)
                {
                    info.set('1', "GetDeviceSet", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"设备ID错误\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                StringBuilder sb = new StringBuilder();
                Model.Entity.DeviceSet deviceSet = DeviceSet.GetInstance().Get(deviceId);

                if (deviceSet == null)
                {
                    // 生成初始化DeviceSet
                    deviceSet = DeviceSet.GetInstance().CreateDefaultDeviceSet(deviceId);
                }

                if (deviceSet == null)
                {
                    info.set('1', "GetDeviceSet", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"取不到数据\"}";
                }
                else
                {
                    sb.Append("{");
                    sb.Append("\"Code\":\"1\"");
                    sb.Append(",\"SetInfo\":\"" + deviceSet.SetInfo + "\"");
                    sb.Append(",\"ClassDisabled1\":\"" + deviceSet.ClassDisabled1 + "\"");
                    sb.Append(",\"ClassDisabled2\":\"" + deviceSet.ClassDisabled2 + "\"");
                    sb.Append(",\"WeekDisabled\":\"" + deviceSet.WeekDisabled + "\"");
                    sb.Append(",\"TimerOpen\":\"" + deviceSet.TimerOpen + "\"");
                    sb.Append(",\"TimerClose\":\"" + deviceSet.TimerClose + "\"");
                    sb.Append(",\"BrightScreen\":\"" + deviceSet.BrightScreen + "\"");
                    sb.Append(",\"WeekAlarm1\":\"" + deviceSet.WeekAlarm1 + "\"");
                    sb.Append(",\"WeekAlarm2\":\"" + deviceSet.WeekAlarm2 + "\"");
                    sb.Append(",\"WeekAlarm3\":\"" + deviceSet.WeekAlarm3 + "\"");
                    sb.Append(",\"Alarm1\":\"" + deviceSet.Alarm1 + "\"");
                    sb.Append(",\"Alarm2\":\"" + deviceSet.Alarm2 + "\"");
                    sb.Append(",\"Alarm3\":\"" + deviceSet.Alarm3 + "\"");
                    sb.Append(",\"LocationMode\":\"" + deviceSet.LocationMode + "\"");
                    sb.Append(",\"LocationTime\":\"" + deviceSet.LocationTime + "\"");
                    sb.Append(",\"FlowerNumber\":\"" + deviceSet.FlowerNumber + "\"");
                    sb.Append(",\"SleepCalculate\":\"" + deviceSet.SleepCalculate + "\"");
                    sb.Append(",\"StepCalculate\":\"" + deviceSet.StepCalculate + "\"");
                    sb.Append(",\"HrCalculate\":\"" + deviceSet.HrCalculate + "\"");
                    sb.Append(",\"SosMsgswitch\":\"" + deviceSet.SosMsgswitch + "\"");

                    sb.Append(",\"CreateTime\":\"" +
                              deviceSet.CreateTime.ToString(TimeFormat,
                                  DateTimeFormatInfo.InvariantInfo) + "\"");
                    sb.Append(",\"UpdateTime\":\"" +
                              deviceSet.UpdateTime.ToString(TimeFormat,
                                  DateTimeFormatInfo.InvariantInfo) + "\"");
                    sb.Append("}");
                }

                info.set('1', "GetDeviceSet", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetDeviceSet", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                info.set('1', "GetDeviceSet", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string GetDeviceList(string loginId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                List<Device> deviceList = UserDevice.GetInstance().GetDeviceByUserId(user.UserID);
                //解决使用绑定号登录时没有设备的问题
                if ((deviceList == null || deviceList.Count == 0) && !string.IsNullOrEmpty(user.BindNumber))
                {
                    Device device = Logic.Device.GetInstance().GetByBindNum(user.BindNumber);
                    if (device != null)
                    {
                        try
                        {
                            if (!UserDevice.GetInstance().IsUserDeviceExists(user.UserID, device.DeviceID))
                            {
                                Model.Entity.UserDevice ud = new Model.Entity.UserDevice
                                {
                                    DeviceID = device.DeviceID,
                                    UserID = user.UserID,
                                    Status = 1,
                                    CreateTime = DateTime.Now,
                                    UpdateTime = DateTime.Now
                                };
                                // 保存 ud 到数据库
                                UserDevice.GetInstance().New(ud);
                            }
                        }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
                        catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
                        {
                            //
                        }


                        if (deviceList == null)
                        {
                            deviceList = new List<Device>();
                        }

                        deviceList.Add(device);
                    }
                }

                if (deviceList == null || deviceList.Count == 0)
                {
                    info.set('1', "GetDeviceList", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"未取到数据\"}";
                }

                Notification.GetInstance().CleanRenewNotification(user.UserID);
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\"Code\":\"1\"");
                sb.Append(",\"deviceList\":[");
                var time = DateTime.Now;
                foreach (var item in deviceList)
                {
                    double gcjHomeLat = 0, gcjHomeLng = 0, gcjSchoolLat = 0, gcjSchoolLng = 0;
                    if (item.HomeLat != null && item.HomeLng != null)
                    {
                        LocationHelper.WGS84ToGCJ((double) item.HomeLat, (double) item.HomeLng, out gcjHomeLat,
                            out gcjHomeLng);
                    }

                    if (item.SchoolLat != null && item.SchoolLng != null)
                    {
                        LocationHelper.WGS84ToGCJ((double) item.SchoolLat, (double) item.SchoolLng,
                            out gcjSchoolLat, out gcjSchoolLng);
                    }

                    sb.Append("{");
                    sb.Append("\"ActiveDate\":\"" +
                              (item.ActiveDate == null
                                  ? ""
                                  : item.ActiveDate.Value.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo)) + "\"");
                    sb.Append(",\"BabyName\":\"" + GetStr(item.BabyName) + "\"");
                    sb.Append(",\"BindNumber\":\"" + item.BindNumber + "\"");
                    sb.Append(",\"DeviceType\":\"" + item.DeviceType + "\"");
                    sb.Append(",\"Birthday\":\"" +
                              (item.Birthday == null
                                  ? ""
                                  : item.Birthday.Value.ToString(DateFormat,
                                      DateTimeFormatInfo.InvariantInfo)) + "\"");
                    sb.Append(",\"CreateTime\":\"" +
                              item.CreateTime.ToString(TimeFormat, DateTimeFormatInfo.InvariantInfo) +
                              "\"");
                    sb.Append(",\"CurrentFirmware\":\"" + item.CurrentFirmware + "\"");
                    sb.Append(",\"SetVersionNO\":\"" + item.SetVersionNO + "\"");
                    sb.Append(",\"ContactVersionNO\":\"" + item.ContactVersionNO + "\"");
                    sb.Append(",\"OperatorType\":\"" + item.OperatorType + "\"");
                    sb.Append(",\"SmsNumber\":\"" + GetStr(item.SmsNumber) + "\"");
                    sb.Append(",\"SmsBalanceKey\":\"" + GetStr(item.SmsBalanceKey) + "\"");
                    sb.Append(",\"SmsFlowKey\":\"" + GetStr(item.SmsFlowKey) + "\"");
                    sb.Append(",\"DeviceID\":\"" + item.DeviceID + "\"");
                    sb.Append(",\"UserId\":\"" + item.UserId + "\"");
                    sb.Append(",\"DeviceModelID\":\"" + Convert.ToString(item.DeviceModelID + (_history ? 128 : 0), 2).PadLeft(8, '0') + "\"");
                    sb.Append(",\"Firmware\":\"" + item.Firmware + "\"");
                    sb.Append(",\"Gender\":\"" + (item.Gender ? "1" : "0") + "\"");
                    sb.Append(",\"Grade\":\"" + item.Grade + "\"");
                    sb.Append(",\"HireExpireDate\":\"" +
                              (item.HireExpireDate == null
                                  ? ""
                                  : item.HireExpireDate.Value.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo)) + "\"");
                    sb.Append(",\"HireStartDate\":\"" +
                              (item.HireStartDate == null
                                  ? ""
                                  : item.HireStartDate.Value.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo)) + "\"");
                    sb.Append(",\"HomeAddress\":\"" + GetStr(item.HomeAddress) + "\"");
                    sb.Append(",\"HomeLat\":\"" + gcjHomeLat.ToString("F6") + "\"");
                    sb.Append(",\"HomeLng\":\"" + gcjHomeLng.ToString("F6") + "\"");
                    sb.Append(",\"IsGuard\":\"" + (item.IsGuard ? "1" : "0") + "\"");
                    sb.Append(",\"Password\":\"\"");
                    sb.Append(",\"PhoneCornet\":\"" + GetStr(item.PhoneCornet) + "\"");
                    sb.Append(",\"PhoneNumber\":\"" + GetStr(item.PhoneNumber) + "\"");
                    sb.Append(",\"Photo\":\"" + item.Photo + "\"");
                    sb.Append(",\"SchoolAddress\":\"" + GetStr(item.SchoolAddress) + "\"");
                    sb.Append(",\"SchoolLat\":\"" + gcjSchoolLat.ToString("F6") + "\"");
                    sb.Append(",\"SchoolLng\":\"" + gcjSchoolLng.ToString("F6") + "\"");
                    sb.Append(",\"SerialNumber\":\"" + item.SerialNumber + "\"");
                    sb.Append(",\"UpdateTime\":\"" +
                              item.UpdateTime.ToString(TimeFormat, DateTimeFormatInfo.InvariantInfo) +
                              "\"");
                    sb.Append(",\"LatestTime\":\"" + item.LatestTime + "\"");
                    sb.Append(",\"CloudPlatform\":\"" + item.CloudPlatform + "\"");

                    Model.Entity.DeviceSet deviceSet = DeviceSet.GetInstance().Get(item.DeviceID);
                    sb.Append(",\"DeviceSet\":{");
                    if (deviceSet != null)
                    {
                        sb.Append("\"SetInfo\":\"" + deviceSet.SetInfo + "\"");
                        sb.Append(",\"ClassDisabled1\":\"" + deviceSet.ClassDisabled1 + "\"");
                        sb.Append(",\"ClassDisabled2\":\"" + deviceSet.ClassDisabled2 + "\"");
                        sb.Append(",\"WeekDisabled\":\"" + deviceSet.WeekDisabled + "\"");
                        sb.Append(",\"TimerOpen\":\"" + deviceSet.TimerOpen + "\"");
                        sb.Append(",\"TimerClose\":\"" + deviceSet.TimerClose + "\"");
                        sb.Append(",\"BrightScreen\":\"" + deviceSet.BrightScreen + "\"");
                        sb.Append(",\"WeekAlarm1\":\"" + deviceSet.WeekAlarm1 + "\"");
                        sb.Append(",\"WeekAlarm2\":\"" + deviceSet.WeekAlarm2 + "\"");
                        sb.Append(",\"WeekAlarm3\":\"" + deviceSet.WeekAlarm3 + "\"");
                        sb.Append(",\"Alarm1\":\"" + deviceSet.Alarm1 + "\"");
                        sb.Append(",\"Alarm2\":\"" + deviceSet.Alarm2 + "\"");
                        sb.Append(",\"Alarm3\":\"" + deviceSet.Alarm3 + "\"");
                        sb.Append(",\"LocationMode\":\"" + deviceSet.LocationMode + "\"");
                        sb.Append(",\"LocationTime\":\"" + deviceSet.LocationTime + "\"");
                        sb.Append(",\"FlowerNumber\":\"" + deviceSet.FlowerNumber + "\"");
                        sb.Append(",\"SleepCalculate\":\"" + deviceSet.SleepCalculate + "\"");
                        sb.Append(",\"StepCalculate\":\"" + deviceSet.StepCalculate + "\"");
                        sb.Append(",\"HrCalculate\":\"" + deviceSet.HrCalculate + "\"");
                        sb.Append(",\"SosMsgswitch\":\"" + deviceSet.SosMsgswitch + "\"");

                        sb.Append(",\"CreateTime\":\"" +
                                  deviceSet.CreateTime.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo) + "\"");
                        sb.Append(",\"UpdateTime\":\"" +
                                  deviceSet.UpdateTime.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo) + "\"");
                    }

                    sb.Append("}");

                    Model.Entity.DeviceState deviceState = DeviceState.GetInstance().Get(item.DeviceID);
                    sb.Append(",\"DeviceState\":{");
                    sb.Append("\"Altitude\":\"" + deviceState.Altitude + "\"");
                    sb.Append(",\"Course\":\"" + deviceState.Course + "\"");
                    sb.Append(",\"LocationType\":\"" + deviceState.LocationType + "\"");
                    sb.Append(",\"CreateTime\":\"" +
                              deviceState.CreateTime.ToString(TimeFormat,
                                  DateTimeFormatInfo.InvariantInfo) + "\"");
                    sb.Append(",\"DeviceTime\":\"" +
                              (deviceState.DeviceTime == null
                                  ? ""
                                  : deviceState.DeviceTime.Value.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo)) + "\"");
                    sb.Append(",\"Electricity\":\"" + deviceState.Electricity + "\"");
                    sb.Append(",\"GSM\":\"" + deviceState.GSM + "\"");
                    sb.Append(",\"Step\":\"" + deviceState.Step + "\"");
                    sb.Append(",\"Health\":\"" + deviceState.Health + "\"");

                    if (deviceState.Latitude != null && deviceState.Longitude != null)
                    {
                        double lat;
                        double lng;
                        LocationHelper.WGS84ToGCJ((double) deviceState.Latitude.Value,
                            (double) deviceState.Longitude.Value, out lat, out lng);
                        sb.Append(",\"Latitude\":\"" + lat.ToString("F6") + "\"");
                        sb.Append(",\"Longitude\":\"" + lng.ToString("F6") + "\"");
                    }
                    else
                    {
                        sb.Append(",\"Latitude\":\"0\"");
                        sb.Append(",\"Longitude\":\"0\"");
                    }

                    sb.Append(",\"Online\":\"" + (deviceState.Online ? "1" : "0") + "\"");
                    sb.Append(",\"SatelliteNumber\":\"" + deviceState.SatelliteNumber + "\"");
                    sb.Append(",\"ServerTime\":\"" +
                              (deviceState.ServerTime == null
                                  ? ""
                                  : deviceState.ServerTime.Value.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo)) + "\"");
                    sb.Append(",\"Speed\":\"" + deviceState.Speed + "\"");
                    sb.Append(",\"UpdateTime\":\"" +
                              deviceState.UpdateTime.ToString(TimeFormat,
                                  DateTimeFormatInfo.InvariantInfo) + "\"");
                    sb.Append("}");

                    List<Model.Entity.DeviceContact> deviceContacts =
                        DeviceContact.GetInstance().GetByDeviceId(item.DeviceID);
                    sb.Append(",\"ContactArr\":[");
                    StringBuilder sbNew = new StringBuilder();
                    foreach (var contact in deviceContacts)
                    {
                        sbNew.Append("{");
                        sbNew.Append("\"DeviceContactId\":\"" + contact.DeviceContactId + "\"");
                        sbNew.Append(",\"Relationship\":\"" + GetStr(contact.Name) + "\"");
                        sbNew.Append(",\"ObjectId\":\"" + contact.ObjectId + "\"");
                        sbNew.Append(",\"Photo\":\"" + contact.Photo + "\"");
                        sbNew.Append(",\"PhoneNumber\":\"" + GetStr(contact.PhoneNumber) + "\"");
                        sbNew.Append(",\"PhoneShort\":\"" + GetStr(contact.PhoneShort) + "\"");
                        sbNew.Append(",\"Type\":\"" + contact.Type + "\"");
                        sbNew.Append(",\"HeadImg\":\"" + contact.HeadImg + "\"");
                        sbNew.Append("},");
                    }

                    string strNew = sbNew.ToString().TrimEnd(',');
                    sbNew = new StringBuilder();
                    sbNew.Append(strNew);
                    sb.Append(sbNew.ToString());
                    sb.Append("]");
                    sb.Append("},");
                }

                user.ActivityTime = time;
                string str = sb.ToString().TrimEnd(',');
                sb = new StringBuilder();
                sb.Append(str);
                sb.Append("]}");

                info.set('1', "GetDeviceList", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetDeviceList", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                info.set('1', "GetDeviceList", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }


        public string UpdateDevice(string loginId, int deviceId, string babyName, string photo, string phoneNumber,
            string phoneCornet, int gender, string birthday, int grade, string homeAddress, string schoolAddress,
            string homeLat, string homeLng, string schoolLat, string schoolLng, string latestTime)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0)
                {
                    info.set('1', "UpdateDevice", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"设备参数错误\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                Device device = Logic.Device.GetInstance().Get(deviceId);
                if (!string.IsNullOrEmpty(babyName))
                {
                    device.BabyName = babyName;
                }

                if (!string.IsNullOrEmpty(photo))
                {
                    string filePath = "";
                    byte[] imgByte = Utility.Convert.StrToHexByte(photo);
                    string imgName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                    filePath = Config.GetInstance().Path + "\\Upload" + "\\Photo\\" + deviceId + "\\";
                    var di = new DirectoryInfo(filePath);
                    if (!di.Exists)
                        di.Create();
                    var fs = new FileStream(filePath + imgName,
                        FileMode.OpenOrCreate);
                    try
                    {
                        fs.Write(imgByte, 0, imgByte.Length);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (fs != null)
                        {
                            fs.Close();
                        }
                    }

                    device.Photo = "Photo/" + deviceId + "/" + imgName;
                }

                //云平台终端不能更改手机号
                if (!string.IsNullOrEmpty(phoneNumber) && (device.CloudPlatform == 0 || user.LoginType == 4))
                {
                    if (phoneNumber == "-1")
                        device.PhoneNumber = "";
                    else
                        device.PhoneNumber = phoneNumber;
                    device.SetVersionNO++;
                    //通知网关更新设备配置
                    if (OnSend != null)
                    {
                        OnSend(device, SendType.FriendsListNotify, "1");
                        OnSend(device, SendType.Set, "");
                    }
                }

                if (!string.IsNullOrEmpty(phoneCornet))
                {
                    if (phoneCornet == "-1")
                    {
                        device.PhoneCornet = "";
                    }
                    else
                    {
                        device.PhoneCornet = phoneCornet;
                    }
                }

                if (gender != 0)
                {
                    device.Gender = gender == 1 ? true : false;
                }

                if (!string.IsNullOrEmpty(birthday))
                {
                    device.Birthday = DateTime.Parse(birthday);
                }

                if (grade != 0)
                {
                    device.Grade = grade - 1;
                }

                if (!string.IsNullOrEmpty(homeAddress))
                {
                    device.HomeAddress = homeAddress;
                }

                if (!string.IsNullOrEmpty(homeLat) && !string.IsNullOrEmpty(homeLng))
                {
                    try
                    {
                        double wgsHomeLat = 0;
                        double wgsHomeLng = 0;
                        LocationHelper.GCJToWGS84(Convert.ToDouble(homeLat), Convert.ToDouble(homeLng),
                            out wgsHomeLat,
                            out wgsHomeLng);
                        device.HomeLat = Convert.ToDecimal(wgsHomeLat);
                        device.HomeLng = Convert.ToDecimal(wgsHomeLng);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(homeLat + "," + homeLng);
                        Logger.Error(ex);
                    }
                }

                if (!string.IsNullOrEmpty(schoolAddress))
                {
                    device.SchoolAddress = schoolAddress;
                }

                if (!string.IsNullOrEmpty(schoolLat) && !string.IsNullOrEmpty(schoolLng))
                {
                    try
                    {
                        double wgsSchoolLat = 0;
                        double wgsSchoolLng = 0;
                        LocationHelper.GCJToWGS84(Convert.ToDouble(schoolLat), Convert.ToDouble(schoolLng),
                            out wgsSchoolLat, out wgsSchoolLng);
                        device.SchoolLat = Convert.ToDecimal(wgsSchoolLat);
                        device.SchoolLng = Convert.ToDecimal(wgsSchoolLng);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(schoolLat + "," + schoolLng);
                        Logger.Error(ex);
                    }
                }

                if (!string.IsNullOrEmpty(latestTime) && latestTime.IndexOf(":", StringComparison.Ordinal) > 0)
                {
                    device.LatestTime = latestTime;
                }

                Logic.Device.GetInstance().Save(device);

                List<User> userList =
                    UserDevice.GetInstance()
                        .GetUserByDeviceId(device.DeviceID)
                        .Where(item => item.UserID != user.UserID)
                        .ToList();
                Notification.GetInstance().Send(device.DeviceID, 230, userList);

                info.set('1', "UpdateDevice", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Photo\":\"" + device.Photo + "\",\"Message\":\"修改成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "UpdateDevice", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                info.set('1', "UpdateDevice", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string UpdateSmsOrder(string loginId, int deviceId, string smsNumber, string smsBalanceKey,
            string smsFlowKey)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId, deviceId);
                if (string.IsNullOrEmpty(smsBalanceKey) || string.IsNullOrEmpty(smsFlowKey) ||
                    string.IsNullOrEmpty(smsNumber) || deviceId <= 0)
                {
                    info.set('1', "UpdateSmsOrder", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                Device device = Logic.Device.GetInstance().Get(deviceId);
                if (device == null)
                {
                    info.set('1', "UpdateSmsOrder", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"未取到数据\"}";
                }

                device.SmsBalanceKey = smsBalanceKey;
                device.SmsFlowKey = smsFlowKey;
                device.SmsNumber = smsNumber;
                Logic.Device.GetInstance().Save(device);
                List<User> userList =
                    UserDevice.GetInstance()
                        .GetUserByDeviceId(device.DeviceID)
                        .Where(item => item.UserID != user.UserID)
                        .ToList();
                Notification.GetInstance().Send(device.DeviceID, 230, userList);

                info.set('1', "UpdateSmsOrder", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"更新成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "UpdateSmsOrder", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                info.set('1', "UpdateSmsOrder", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        #endregion

        #region 通讯录

        public string EditRelation(string loginId, string name, int photo, int deviceContactId, string phoneShort,
            string phoneNumber)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();
            Device device = null;
            try
            {
                if (deviceContactId <= 0)
                {
                    info.set('1', "EditRelation", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                Model.Entity.DeviceContact contact = DeviceContact.GetInstance().Get(deviceContactId);
                if (contact == null)
                {
                    info.set('1', "EditRelation", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"该项目已经被删除\"}";
                }

                List<Model.Entity.DeviceContact> contactList =
                    DeviceContact.GetInstance().GetByDeviceId(contact.DeviceID);
                var user = CheckLogin(loginId, contact.DeviceID);
                if (!string.IsNullOrEmpty(name))
                {
                    if (contact.Name != name)
                    {
                        if (contactList.Exists(p => p.Name == name))
                        {
                            info.set('1', "EditRelation", "", 0, Environment.TickCount - ticker, 8);
                            CPhotoStat.GetInstance().add_info(info);

                            return "{\"Code\":\"8\",\"Message\":\"通讯录中已有该名称\"}";
                        }

                        contact.Name = name;
                    }
                }

                if (photo > 0)
                {
                    contact.Photo = photo;
                    contact.HeadImg = "";
                    if (contact.HeadImgVersion >= 65535)
                    {
                        contact.HeadImgVersion = 0;
                    }
                    else
                    {
                        contact.HeadImgVersion += 1;
                    }
                }

                if (!string.IsNullOrEmpty(phoneShort))
                {
                    if (phoneShort == "-1")
                    {
                        contact.PhoneShort = "";
                    }
                    else
                    {
                        contact.PhoneShort = phoneShort;
                    }
                }

                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    if (phoneNumber == "-1")
                    {
                        contact.PhoneNumber = "";
                    }
                    else
                    {
                        //同步修改用户登录表内容
                        if (!string.IsNullOrEmpty(contact.PhoneNumber) && !contact.PhoneNumber.Equals(phoneNumber) && contact.UserId > 0)
                        {
                            User u = Logic.User.GetInstance().Get(contact.UserId);
                            if (u != null)
                            {
                                u.PhoneNumber = phoneNumber;
                                Logic.User.GetInstance().Save(u);
                            }
                        }

                        contact.PhoneNumber = phoneNumber;
                    }
                }

                //如果是管理员，可以改所有人的关系
                device = Logic.Device.GetInstance().Get(contact.DeviceID);
                if (contact.Type == 1 ||
                    (contact.Type == 2 && (contact.ObjectId == user.UserID || device.UserId == user.UserID)) ||
                    contact.Type == 3)
                {
                    DeviceContact.GetInstance().Save(contact);
                    device.ContactVersionNO++;
                    Logic.Device.GetInstance().Save(device);
                    //通知网关更新设备通讯录
                    if (OnSend != null)
                        OnSend(device, SendType.Contact, "");
                    List<User> userList =
                        UserDevice.GetInstance()
                            .GetUserByDeviceId(device.DeviceID)
                            .Where(item => item.UserID != user.UserID)
                            .ToList();
                    Notification.GetInstance().Send(device.DeviceID, 232, userList);
                }
                //如果不是，则不能修改管理员的关系
                else
                {
                    info.set('1', "EditRelation", "", 0, Environment.TickCount - ticker, 6);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"6\",\"Message\":\"无权修改该信息\"}";
                }
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "EditRelation", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                info.set('1', "EditRelation", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(device == null ? "device==null" : device.ToString(), ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }

            info.set('1', "EditRelation", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);

            return "{\"Code\":\"1\",\"Message\":\"编辑成功\"}";
        }

        public string AddContact(string loginId, int deviceId, string name, int photo, string phoneNum,
            string phoneShort, string bindNumber)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();
            var needReLogin = false;

            try
            {
                if (deviceId <= 0 || (string.IsNullOrEmpty(phoneNum) && string.IsNullOrEmpty(phoneShort)))
                {
                    info.set('1', "AddContact", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                var device = Logic.Device.GetInstance().Get(deviceId);
                var uds = DeviceContact.GetInstance().GetByDeviceId(device.DeviceID);
                if (uds.Count >= 50 || (device.CloudPlatform == 2 && uds.Count >= 5) || ((device.CloudPlatform == 1 || device.CloudPlatform > 2) && uds.Count >= 10))
                {
                    info.set('1', "AddContact", "", 0, Environment.TickCount - ticker, 8);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"8\",\"Message\":\"通讯录已满\"}";
                }

                List<Model.Entity.DeviceContact> contactList = DeviceContact.GetInstance().GetByDeviceId(deviceId);
                foreach (var item in contactList)
                {
                    if (item.Name.Equals(name))
                    {
                        info.set('1', "AddContact", "", 0, Environment.TickCount - ticker, 8);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"8\",\"Message\":\"通讯录中已有该名称\"}";
                    }

                    if (item.PhoneNumber.Equals(phoneNum))
                    {
                        info.set('1', "AddContact", "", 0, Environment.TickCount - ticker, 8);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"8\",\"Message\":\"通讯录中已有该号码\"}";
                        //item.Name = name;
                        //item.PhoneShort = phoneShort;
                        //item.Photo = photo;
                        //DeviceContact.GetInstance().Save(item);
                        //device.ContactVersionNO++;
                        //Device.GetInstance().Save(device);
                        ////通知网关更新设备通讯录
                        //if (OnSend != null)
                        //    OnSend(device, SendType.Contact, "");
                        //return "{\"Code\":\"1\",\"Message\":\"添加成功\",\"DeviceContactId\":\"" + item.DeviceContactId + "\"}";
                    }
                }

                //让新增的通讯录对象也能登录，并绑定设备
                User userLogin = null;
                if (!string.IsNullOrEmpty(phoneNum))
                {
                    User userPhone = null;
                    if (!string.IsNullOrEmpty(bindNumber))
                    {
                        userPhone = Logic.User.GetInstance().Get(phoneNum);
                        var dv = Logic.Device.GetInstance().GetDic().FirstOrDefault(x => x.Value.UserId == userPhone?.UserID).Value;
                        if (userPhone != null && dv != null && dv.DeviceID != 0)
                        {
                            return "{ \"Code\":\"9\",\"Message\":\"此号码已被设置为其它手表的管理员账户，请更换其它手机号码作为此设备的管理员账户\" }";
                        }

                        userLogin = Logic.User.GetInstance().GetByBindNumber(bindNumber);
                        if (userLogin != null && userPhone != null && userLogin.UserID != userPhone.UserID)
                        {
                            //如果手机号用户和绑定号用户同时存在，则使用手机号用户，删除绑定号用户，因为绑定号用户是程序自动生成的。
                            Logic.User.GetInstance().DelReal(userLogin.UserID);
                        }
                    }

                    if (userPhone != null)
                    {
                        userLogin = userPhone;
                    }

                    if (userLogin == null)
                    {
                        string salt = Utils.createNewSalt();
                        string passWordEncoded = MD5Helper.MD5Encrypt(Constants.DEFAULT_PASSWORD, salt);
                        userLogin = new User
                        {
                            PhoneNumber = phoneNum,
                            Password = passWordEncoded,
                            Salt = salt,
                            UserType = 2,
                            LoginID = Guid.NewGuid().ToString(),
                            AppID = null,
                            Project = null,
                            Notification = true,
                            NotificationSound = true,
                            NotificationVibration = true,
                            BindNumber = bindNumber
                        };
                    }
                    else
                    {
                        userLogin.PhoneNumber = phoneNum;
                        userLogin.BindNumber = bindNumber;
                    }

                    if (device.UserId != userLogin.UserID && !string.IsNullOrEmpty(bindNumber))
                    {
                        LoginUserTemp.TryRemove(user.LoginID, out var res);
                        userLogin.LoginID = null;
                        needReLogin = true;
                        device.UserId = userLogin.UserID;
                    }

                    Logic.User.GetInstance().Save(userLogin);

                    try
                    {
                        if (!UserDevice.GetInstance().IsUserDeviceExists(userLogin.UserID, device.DeviceID))
                        {
                            var ud = new Model.Entity.UserDevice()
                            {
                                DeviceID = device.DeviceID,
                                UserID = userLogin.UserID,
                                Status = 1,
                                CreateTime = DateTime.Now,
                                UpdateTime = DateTime.Now
                            };
                            // 保存 ud 到数据库
                            UserDevice.GetInstance().New(ud);
                        }
                    }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
                    catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
                    {
                        //旧数据问题
                    }
                }

                Model.Entity.DeviceContact deviceContact = new Model.Entity.DeviceContact
                {
                    Type = 1,
                    DeviceID = deviceId,
                    Name = name,
                    PhoneNumber = phoneNum,
                    PhoneShort = phoneShort,
                    Photo = photo,
                    UserId = userLogin == null ? 0 : userLogin.UserID
                };
                if (!string.IsNullOrEmpty(bindNumber) && !"NULL".Equals(bindNumber.ToUpper()) && userLogin != null)
                {
                    //管理员
                    deviceContact.Type = 2;
                    //只有通过本逻辑添加的新用户才能同步更新，以防止更改非相关账户
                    deviceContact.ObjectId = userLogin.UserID;
                }

                DeviceContact.GetInstance().Save(deviceContact);
                device.ContactVersionNO++;
                Logic.Device.GetInstance().Save(device);

                //通知网关更新设备通讯录
                OnSend?.Invoke(device, SendType.Contact, "");
                List<User> userList = UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID).Where(item => item.UserID != user.UserID).ToList();
                Notification.GetInstance().Send(device.DeviceID, 232, userList);

                info.set('1', "AddContact", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                var msgTit = string.IsNullOrEmpty(bindNumber) ? "通讯录" : "管理员";
                var msg = needReLogin ? "因管理员用户已改变，可能需要重新登录！" : (msgTit + "添加成功");
                return "{\"Code\":\"1\",\"Message\":\"" + msg + "\",\"DeviceContactId\":\"" + deviceContact.DeviceContactId + "\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "AddContact", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                info.set('1', "AddContact", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string GetDeviceContact(string loginId, int deviceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0)
                {
                    info.set('1', "GetDeviceConta", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"设备ID参数无效\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                StringBuilder sb = new StringBuilder();
                List<Model.Entity.DeviceContact> deviceContacts = DeviceContact.GetInstance().GetByDeviceId(deviceId);
                if (deviceContacts == null)
                {
                    info.set('1', "GetDeviceConta", "", 0, Environment.TickCount - ticker, -2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"取不到数据\"}";
                }
                else
                {
                    sb.Append("{");
                    sb.Append("\"Code\":\"1\"");
                    sb.Append(",\"ContactArr\":[");
                    foreach (var item in deviceContacts)
                    {
                        sb.Append("{");
                        sb.Append("\"DeviceContactId\":\"" + item.DeviceContactId + "\"");
                        sb.Append(",\"Relationship\":\"" + GetStr(item.Name) + "\"");
                        sb.Append(",\"ObjectId\":\"" + item.ObjectId + "\"");
                        sb.Append(",\"Photo\":\"" + item.Photo + "\"");
                        sb.Append(",\"PhoneNumber\":\"" + GetStr(item.PhoneNumber) + "\"");
                        sb.Append(",\"PhoneShort\":\"" + GetStr(item.PhoneShort) + "\"");
                        sb.Append(",\"Type\":\"" + item.Type + "\"");
                        sb.Append(",\"HeadImg\":\"" + item.HeadImg + "\"");
                        sb.Append("},");
                    }

                    string str = sb.ToString().TrimEnd(',');
                    sb = new StringBuilder();
                    sb.Append(str);
                    sb.Append("]}");
                }

                info.set('1', "GetDeviceConta", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetDeviceConta", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                info.set('1', "GetDeviceConta", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string DeleteContact(string loginId, int deviceContactId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceContactId <= 0)
                {
                    info.set('1', "DeleteContact", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                Model.Entity.DeviceContact contact = DeviceContact.GetInstance().Get(deviceContactId);
                if (contact == null)
                {
                    info.set('1', "DeleteContact", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"取不到数据\"}";
                }

                var user = CheckLogin(loginId, contact.DeviceID);
                Device device = Logic.Device.GetInstance().Get(contact.DeviceID);

                if (!((user.UserType == 1) || (user.UserType == 2)))
                {
                    return "{\"Code\":\"6\",\"Message\":\"用户类型出错\"}";
                }

                if (contact.Type == 1 || contact.Type == 3)
                {
                    //普通用户删除自己
                    if ((user.UserType == 1) && (device.UserId != user.UserID))
                    {
                        info.set('1', "DeleteContact", "", 0, Environment.TickCount - ticker, 6);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"6\",\"Message\":\"不是管理员无权操作\"}";
                    }

                    DeviceContact.GetInstance().Del(deviceContactId);
                    UserDevice.GetInstance().Del(contact.UserId, contact.DeviceID);
                    var us = Logic.User.GetInstance().Get(contact.UserId);
                    DelLoginUser(us, device);
                }
                else if (contact.Type == 2)
                {
                    //管理员删除
                    if ((user.UserType == 1) && (device.UserId != user.UserID))
                    {
                        info.set('1', "DeleteContact", "", 0, Environment.TickCount - ticker, 6);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"6\",\"Message\":\"不是管理员无权操作\"}";
                    }

                    if ((user.UserType == 2) && (device.UserId == contact.ObjectId))
                    {
                        info.set('1', "DeleteContact", "", 0, Environment.TickCount - ticker, 6);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"6\",\"Message\":\"不能删除管理员\"}";
                    }

                    if (user.UserID == contact.ObjectId)
                    {
                        info.set('1', "DeleteContact", "", 0, Environment.TickCount - ticker, 7);
                        CPhotoStat.GetInstance().add_info(info);

                        return "{\"Code\":\"7\",\"Message\":\"不能删除自己\"}";
                    }

                    User usr = Logic.User.GetInstance().Get(contact.ObjectId);
                    DeviceContact.GetInstance().Del(deviceContactId);
                    UserDevice.GetInstance().Del(usr.UserID, contact.DeviceID);
                    DelLoginUser(usr, device);


                    //删除联系人后通知用户
                    Model.Entity.Notification notification = new Model.Entity.Notification()
                    {
                        DeviceID = contact.DeviceID,
                        Type = 9,
                        CreateTime = DateTime.Now
                    };
                    Notification.GetInstance().Send(notification, usr);
                    device.ContactVersionNO++;
                    Logic.Device.GetInstance().Save(device);
                }

                //通知网关更新设备通讯录
                if (OnSend != null)
                    OnSend(device, SendType.Contact, "");
                List<User> userList =
                    UserDevice.GetInstance()
                        .GetUserByDeviceId(device.DeviceID).Where(w => w.UserID != user.UserID).ToList();
                Notification.GetInstance().Send(device.DeviceID, 232, userList);

                info.set('1', "DeleteContact", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"删除成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "DeleteContact", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "DeleteContact", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"" + ex.Message + "\"}";
            }
        }

        private void DelLoginUser(User user, Device device)
        {
            if (user == null || device == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(user.BindNumber) || user.UserID == device.UserId)
            {
                Logic.User.GetInstance().DelReal(user.UserID);
            }
        }

        public string EditHeadImg(string loginId, int deviceContactId, string headImg)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                if (headImg == "")
                {
                    info.set('1', "EditHeadImg", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"参数错误\"}";
                }

                Model.Entity.DeviceContact deviceContact = DeviceContact.GetInstance().Get(deviceContactId);
                if (deviceContact == null || user.UserID != deviceContact.ObjectId)
                {
                    info.set('1', "EditHeadImg", "", 0, Environment.TickCount - ticker, -3);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-3\",\"Message\":\"无权操作\"}";
                }

                string filePath = "";
                byte[] imgByte = Utility.Convert.StrToHexByte(headImg);
                string imgName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                filePath = Config.GetInstance().Path + "\\Upload" + "\\HeadImg\\" + deviceContact.DeviceID + "\\";
                DirectoryInfo di = new DirectoryInfo(filePath);
                if (!di.Exists)
                    di.Create();
                //FileStream fs = new FileStream(filePath + imgName,
                //    FileMode.OpenOrCreate);
                try
                {
                    //fs.Write(imgByte, 0, imgByte.Length);
                    using (MemoryStream ms = new MemoryStream(imgByte))
                    {
                        ms.Write(imgByte, 0, imgByte.Length);
                        int w = 64, h = 64;
                        //Image outputImg = Image.FromStream(ms);
                        Bitmap originBmp = new Bitmap(ms);
                        Bitmap resizedBmp = new Bitmap(64, 64);
                        Graphics g = Graphics.FromImage(resizedBmp);
                        //设置高质量插值法
                        g.InterpolationMode = InterpolationMode.High;
                        //设置高质量,低速度呈现平滑程度
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        //消除锯齿
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.DrawImage(originBmp, new Rectangle(0, 0, w, h),
                            new Rectangle(0, 0, originBmp.Width, originBmp.Height), GraphicsUnit.Pixel);
                        resizedBmp.Save(filePath + imgName, ImageFormat.Jpeg);
                        g.Dispose();
                        resizedBmp.Dispose();
                        originBmp.Dispose();

                        //Image image = new System.Drawing.Image(outputImg, 64, 64);
                        //outputImg.Save(filePath + imgName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        //Bitmap image = new System.Drawing.Bitmap(outputImg, 64, 64);
                        ////查找JPEG这种编码
                        //ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                        //ImageCodecInfo codec = null;
                        //foreach (ImageCodecInfo c in codecs)
                        //{
                        //    if (c.FormatDescription == "JPEG")
                        //        codec = c;
                        //} EncoderParameters param = new EncoderParameters();
                        //param.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100);
                        //image.Save(filePath + imgName, codec, param);
                        //image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    //if (fs != null)
                    //{
                    //    fs.Close();
                    //}
                }

                deviceContact.HeadImg = "HeadImg/" + deviceContact.DeviceID + "/" + imgName;
                if (deviceContact.HeadImgVersion >= 65535)
                {
                    deviceContact.HeadImgVersion = 0;
                }
                else
                {
                    deviceContact.HeadImgVersion += 1;
                }

                DeviceContact.GetInstance().Save(deviceContact);
                Device device = Logic.Device.GetInstance().Get(deviceContact.DeviceID);
                device.ContactVersionNO++;
                Logic.Device.GetInstance().Save(device);
                //通知网关更新设备通讯录
                if (OnSend != null)
                    OnSend(device, SendType.Contact, "");
                List<User> userList =
                    UserDevice.GetInstance()
                        .GetUserByDeviceId(device.DeviceID)
                        .Where(item => item.UserID != user.UserID)
                        .ToList();
                Notification.GetInstance().Send(device.DeviceID, 232, userList);

                info.set('1', "EditHeadImg", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"保存成功\",\"HeadImg\":\"" + deviceContact.HeadImg + "\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "EditHeadImg", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "EditHeadImg", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 指令下发

        public string RefreshDeviceState(string loginId, int deviceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0)
                {
                    info.set('1', "RefreshDevice", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"设备ID输入错误\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                var device = Logic.Device.GetInstance().Get(deviceId);
                //通知网关更新设备状态
                if (OnSend != null)
                {
                    OnSend(device, SendType.Location, "");
                }

                info.set('1', "RefreshDevice", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "RefreshDevice", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "RefreshDevice", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"" + ex + "\"}";
            }
        }

        public string SendDeviceCommand(string loginId, int deviceId, string commandType, string paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0)
                {
                    info.set('1', "SendDeviceCmd", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"设备ID参数无效\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                var device = Logic.Device.GetInstance().Get(deviceId);
                if (device == null)
                {
                    info.set('1', "SendDeviceCmd", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"取不到数据\"}";
                }
                else
                {
                    bool isSuccess = false;
                    if (OnSend != null)
                    {
                        switch (commandType)
                        {
                            case "Monitor":
                                isSuccess = !string.IsNullOrEmpty(user.PhoneNumber) && OnSend(device, SendType.Monitor, user.PhoneNumber);
                                break;
                            case "Find":
                                isSuccess = OnSend(device, SendType.Find, "");
                                break;
                            case "PowerOff":
                                isSuccess = OnSend(device, SendType.PowerOff, "");
                                break;
                            case "TakePhoto":
                                isSuccess = OnSend(device, SendType.TakePhoto, user.PhoneNumber);
                                break;
                            case "SleepCalculate":
                                isSuccess = OnSend(device, SendType.SleepCalculate, paramter);
                                break;
                            case "StepCalculate":
                                isSuccess = OnSend(device, SendType.StepCalculate, paramter);
                                break;
                            case "HrCalculate":
                                isSuccess = OnSend(device, SendType.HrCalculate, paramter);
                                break;
                            case "DeviceRecovery":
                                isSuccess = OnSend(device, SendType.DeviceRecovery, paramter);
                                break;
                            case "DeviceReset":
                                isSuccess = OnSend(device, SendType.DeviceReset, paramter);
                                break;
                            case "GUARD":
                                isSuccess = OnSend(device, SendType.GUARD, paramter);
                                break;
                        }
                    }

                    info.set('1', "SendDeviceCmd", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);
                    if (isSuccess)
                    {
                        return "{\"Code\":\"1\",\"Message\":\"指令发送成功\"}";
                    }
                    else if (string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        return "{\"Code\":\"1\",\"Message\":\"本机手机号为空时，不能执行此操作！\"}";
                    }
                    else
                    {
                        return "{\"Code\":\"1\",\"Message\":\"手表不在线，操作失败\"}";
                    }
                }
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "SendDeviceCmd", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                info.set('1', "SendDeviceCmd", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        #endregion

        #region 短信

        public string GetDeviceSMS(string loginId, int deviceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0)
                {
                    info.set('1', "GetDeviceSMS", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                List<Model.Entity.DeviceSMS> smsList =
                    DeviceSMS.GetInstance().GetList(user.UserID, deviceId).OrderBy(o => o.DeviceSMSID).ToList();
                if (smsList.Count == 0)
                {
                    info.set('1', "GetDeviceSMS", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"未取到数据\"}";
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\"Code\":\"1\"");
                sb.Append(",\"SMSList\":[");
                foreach (var item in smsList)
                {
                    sb.Append("{");
                    sb.Append("\"DeviceSMSID\":\"" + item.DeviceSMSID + "\"");
                    sb.Append(",\"DeviceID\":\"" + item.DeviceID + "\"");
                    sb.Append(",\"Type\":\"" + item.Type + "\"");
                    sb.Append(",\"Phone\":\"" + item.Phone + "\"");
                    sb.Append(",\"SMS\":\"" + item.SMS.Replace("\r\n", "").Replace("\r", "").Replace("\n", "") + "\"");
                    sb.Append(",\"CreateTime\":\"" +
                              item.CreateTime.ToString(TimeFormat, DateTimeFormatInfo.InvariantInfo) +
                              "\"");
                    sb.Append("},");
                }

                string str = sb.ToString().TrimEnd(',');
                sb = new StringBuilder();
                sb.Append(str);
                sb.Append("]}");

                info.set('1', "GetDeviceSMS", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetDeviceSMS", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "GetDeviceSMS", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }


        public string SaveDeviceSMS(string loginId, int deviceId, string phone, string content)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0 || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(content))
                {
                    info.set('1', "SaveDeviceSMS", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                var device = Logic.Device.GetInstance().Get(deviceId);
                Model.Entity.DeviceSMS deviceSms = new Model.Entity.DeviceSMS()
                {
                    DeviceID = deviceId,
                    Type = 1,
                    Phone = phone,
                    SMS = content.Replace("\r\n", "").Replace("\r", "").Replace("\n", ""),
                    CreateTime = DateTime.Now
                };
                DeviceSMS.GetInstance().Save(deviceSms);
                //通知网关发送新的短信
                if (OnSend != null)
                    OnSend(device, SendType.SMS, "");
                info.set('1', "SaveDeviceSMS", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"保存成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "SaveDeviceSMS", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "SaveDeviceSMS", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 拍照

        public string GetDevicePhoto(string loginId, int deviceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0)
                {
                    info.set('1', "GetDevicePhoto", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                List<Model.Entity.DevicePhoto> list =
                    DevicePhoto.GetInstance().GetList(user.UserID, deviceId).OrderBy(o => o.DevicePhotoId).ToList();
                if (list.Count == 0)
                {
                    info.set('1', "GetDevicePhoto", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"未取到数据\"}";
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\"Code\":\"1\"");
                sb.Append(",\"List\":[");
                foreach (var item in list)
                {
                    sb.Append("{");
                    sb.Append("\"DevicePhotoId\":\"" + item.DevicePhotoId + "\"");
                    sb.Append(",\"DeviceID\":\"" + item.DeviceID + "\"");
                    sb.Append(",\"Source\":\"" + item.Source + "\"");
                    sb.Append(",\"DeviceTime\":\"" + (item.DeviceTime == null ? "" : item.DeviceTime.Value.ToString(TimeFormat, DateTimeFormatInfo.InvariantInfo)) + "\"");
                    if (item.Latitude != null && item.Longitude != null)
                    {
                        double lat;
                        double lng;
                        LocationHelper.WGS84ToGCJ((double) item.Latitude.Value,
                            (double) item.Longitude.Value, out lat, out lng);
                        sb.Append(",\"Latitude\":\"" + lat.ToString("F6") + "\"");
                        sb.Append(",\"Longitude\":\"" + lng.ToString("F6") + "\"");
                    }
                    else
                    {
                        sb.Append(",\"Latitude\":\"0\"");
                        sb.Append(",\"Longitude\":\"0\"");
                    }

                    sb.Append(",\"Mark\":\"" + item.Mark + "\"");
                    sb.Append(",\"Path\":\"" + item.Path + "\"");
                    sb.Append(",\"CreateTime\":\"" +
                              item.CreateTime.ToString(TimeFormat, DateTimeFormatInfo.InvariantInfo) +
                              "\"");
                    sb.Append(",\"UpdateTime\":\"" +
                              item.UpdateTime.ToString(TimeFormat, DateTimeFormatInfo.InvariantInfo) +
                              "\"");
                    sb.Append("},");
                }

                string str = sb.ToString().TrimEnd(',');
                sb = new StringBuilder();
                sb.Append(str);
                sb.Append("]}");

                info.set('1', "GetDevicePhoto", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetDevicePhoto", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "GetDevicePhoto", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 删除图片

        public string DeleteDevicePhoto(string loginId, int DeviceID, string DevicePhotoId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (DeviceID <= 0)
                {
                    info.set('1', "DelDevicePhoto", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                var user = CheckLogin(loginId, DeviceID);

                DevicePhoto.GetInstance().DelByDevicePhotoId(int.Parse(DevicePhotoId));


                info.set('1', "DelDevicePhoto", "", 1, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"删除成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "DelDevicePhoto", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "DelDevicePhoto", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-1\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 语音

        public string GetDeviceVoice(string loginId, int deviceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0)
                {
                    info.set('1', "GetDeviceVoice", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                List<Model.Entity.DeviceVoice> deviceVoice = DeviceVoice.GetInstance().GetList(user.UserID, deviceId);
                if (deviceVoice == null || deviceVoice.Count == 0)
                {
                    return "{\"Code\":\"-2\",\"Message\":\"无语音记录！\"}";
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\"Code\":\"1\"");
                sb.Append(",\"VoiceList\":[");
                foreach (var item in deviceVoice)
                {
                    sb.Append("{");
                    sb.Append("\"DeviceVoiceId\":\"" + item.DeviceVoiceId + "\"");
                    sb.Append(",\"DeviceID\":\"" + item.DeviceID + "\"");
                    sb.Append(",\"State\":\"" + item.State + "\"");
                    sb.Append(",\"Type\":\"" + item.Type + "\"");
                    sb.Append(",\"MsgType\":\"" + item.MsgType + "\"");
                    sb.Append(",\"ObjectId\":\"" + item.ObjectId + "\"");
                    sb.Append(",\"Mark\":\"" + item.Mark + "\"");
                    sb.Append(",\"Path\":\"" + item.Path + "\"");
                    sb.Append(",\"Length\":\"" + item.Length + "\"");
                    sb.Append(",\"CreateTime\":\"" +
                              item.CreateTime.ToString(TimeFormat, DateTimeFormatInfo.InvariantInfo) +
                              "\"");
                    sb.Append(",\"UpdateTime\":\"" +
                              item.UpdateTime.ToString(TimeFormat, DateTimeFormatInfo.InvariantInfo) +
                              "\"");
                    sb.Append("},");
                }

                string str = sb.ToString().TrimEnd(',');
                sb = new StringBuilder();
                sb.Append(str);
                sb.Append("]}");

                info.set('1', "GetDeviceVoice", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetDeviceVoice", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "GetDeviceVoice", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        public string SendDeviceVoice(string loginId, int deviceId, string voice, int length, int msgtype)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0 || string.IsNullOrEmpty(voice))
                {
                    info.set('1', "SendDeviceVoi", "", 0, Environment.TickCount - ticker, -2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                var devcie = Logic.Device.GetInstance().Get(deviceId);
                var mark = user.PhoneNumber + "_" + length + "_" + DateTime.Now.ToString("yyyyMMddhhmmss");
                var fileName = mark + ".amr";
                if (msgtype == 1)
                {
                    fileName = mark + ".txt";
                }
                else
                {
                    fileName = mark + ".amr";
                }

                string filePath = Config.GetInstance().Path + "\\AMR\\" + deviceId + "\\";
                DirectoryInfo di = new DirectoryInfo(filePath);
                if (!di.Exists)
                    di.Create();
                FileStream fs = new FileStream(filePath + fileName, FileMode.OpenOrCreate, FileAccess.Write)
                {
                    Position = 0
                };

                byte[] bytes;
                if (msgtype == 1)
                {
                    bytes = Encoding.UTF8.GetBytes(voice);
                }
                else
                {
                    bytes = Utility.Convert.StrToHexByte(voice);
                }

                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
                fs.Dispose();
                Model.Entity.DeviceVoice deviceVoide = new Model.Entity.DeviceVoice()
                {
                    DeviceID = deviceId,
                    State = 1,
                    TotalPackage = 0,
                    CurrentPackage = 0,
                    Type = 3,
                    ObjectId = user.UserID,
                    Mark = mark,
                    Path = deviceId + "/" + fileName,
                    Length = length,
                    MsgType = msgtype
                };
                DeviceVoice.GetInstance().Save(deviceVoide);

                //给所有关联该设备的用户发消息
                List<User> userList = UserDevice.GetInstance().GetUserByDeviceId(deviceId);
                Notification.GetInstance().Send(deviceVoide, userList);
                //通知网关发送新的语音给设备
                if (OnSend != null)
                    OnSend(devcie, SendType.Voice, "");
                StringBuilder sb = new StringBuilder();
                sb.Append("{\"Code\":\"1\"");
                sb.Append(",\"DeviceID\":\"" + deviceVoide.DeviceID + "\"");
                sb.Append(",\"DeviceVoiceId\":\"" + deviceVoide.DeviceVoiceId + "\"");
                sb.Append(",\"Length\":\"" + deviceVoide.Length + "\"");
                sb.Append(",\"Mark\":\"" + deviceVoide.Mark + "\"");
                sb.Append(",\"ObjectId\":\"" + deviceVoide.ObjectId + "\"");
                sb.Append(",\"Path\":\"" + deviceVoide.Path + "\"");
                sb.Append(",\"State\":\"" + deviceVoide.State + "\"");
                sb.Append(",\"Type\":\"" + deviceVoide.Type + "\"");
                sb.Append(",\"MsgType\":\"" + deviceVoide.MsgType + "\"");
                sb.Append(",\"CreateTime\":\"" +
                          deviceVoide.CreateTime.ToString(TimeFormat,
                              DateTimeFormatInfo.InvariantInfo) + "\"");
                sb.Append(",\"UpdateTime\":\"" +
                          deviceVoide.UpdateTime.ToString(TimeFormat,
                              DateTimeFormatInfo.InvariantInfo) + "\"");
                sb.Append("}");

                info.set('1', "SendDeviceVoi", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "SendDeviceVoi", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "SendDeviceVoi", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 历史轨迹

        private readonly Dictionary<string, TempLocationModel> _tempLocation = new Dictionary<string, TempLocationModel>();

        private class TempLocationModel
        {
            public List<Location> List;
            public DateTime Time;
        }

        private Timer _tempLocationTimer;

        public void ClearDeviceHistory(string loginId, int deviceId)
        {
            try
            {
                CheckLogin(loginId, deviceId);
            }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
            catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
            {
                return;
            }


            var res = _tempLocation.Where(x => x.Key.StartsWith(deviceId.ToString()) && x.Key.Length > 16 && deviceId.ToString().Equals(x.Key.Substring(0, x.Key.Length - 16)));
            foreach (var re in res)
            {
                _tempLocation.Remove(re.Key);
            }

            Task.Run(() =>
            {
                DateTime mEnd = DateTime.Now;
                DateTime mStart = DateTime.Now.AddMonths(-60);

                var startId = Location.GenerateId(deviceId, mStart, null);
                var endId = Location.GenerateId(deviceId, mEnd, null);
                string conn = "mongodb://" + AppConfig.GetValue("MongoDBIp") + ":" + AppConfig.GetValue("MongoDBPort");
                MongoServer mserver = null;
                try
                {
                    var mc = new MongoClient(conn);
                    const string tbName = "Location";
                    mserver = mc.GetServer();
                    while (mStart < mEnd)
                    {
                        string dbName = "History" + mStart.ToString("yyyyMM");
                        MongoDatabase db = mserver.GetDatabase(dbName);
                        MongoCollection col = db.GetCollection(tbName);
                        col.Remove(Query.And(
                            Query.GTE("_id", BsonBinaryData.Create(startId)),
                            Query.LTE("_id", BsonBinaryData.Create(endId))
                        ));
                        mStart = mStart.AddMonths(1);
                    }
                }
                finally
                {
                    if (mserver != null)
                    {
                        mserver.Disconnect();
                    }
                }
            });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="deviceId"></param>
        /// <param name="startTime">格式：yyyy/MM/dd 00:00</param>
        /// <param name="endTime">格式：yyyy/MM/dd 23:59</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public string GetDeviceHistory(string loginId, int deviceId, string startTime, string endTime, int pageIndex,
            int pageSize)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                if (deviceId <= 0 || string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
                {
                    info.set('1', "GetDeviceHist", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
                }

                var user = CheckLogin(loginId, deviceId);
                if (_tempLocationTimer == null)
                {
                    lock (this)
                    {
                        if (_tempLocationTimer == null)
                        {
                            _tempLocationTimer = new Timer(600000) {AutoReset = true};
                            _tempLocationTimer.Elapsed +=
                                new ElapsedEventHandler(tempLocationTimer_Elapsed);
                            _tempLocationTimer.Enabled = true;
                        }
                    }
                }

                string key = deviceId + startTime + endTime;
                DateTime start = DateTime.Parse(startTime);
                DateTime end = DateTime.Parse(endTime);
                List<Location> list = null;
                TempLocationModel temp = null;
                if (_tempLocation.TryGetValue(key, out temp))
                {
                    list = temp.List;
                    temp.Time = DateTime.Now;
                }
                else
                {
                    list = new List<Location>();
                    DateTime mStart = start.Date;
                    mStart = mStart.AddDays(1 - start.Day); //获得当月一号的时间

                    var startId = Location.GenerateId(deviceId, start, null);
                    var endId = Location.GenerateId(deviceId, end, null);
                    string conn = "mongodb://" + AppConfig.GetValue("MongoDBIp") + ":" + AppConfig.GetValue("MongoDBPort");
                    var mc = new MongoClient(conn);
                    const string tbName = "Location";
                    MongoServer mserver = null;
                    try
                    {
                        mserver = mc.GetServer();
                        while (mStart < end)
                        {
                            string dbName = "History" + mStart.ToString("yyyyMM");
                            MongoDatabase db = mserver.GetDatabase(dbName);
                            MongoCollection col = db.GetCollection(tbName);
                            var result = col.FindAs<Location>(Query.And(
                                Query.GTE("_id", BsonBinaryData.Create(startId)),
                                Query.LTE("_id", BsonBinaryData.Create(endId))
                            )).SetSortOrder(SortBy.Ascending("_id")).ToList();
                            list.AddRange(result);
                            mStart = mStart.AddMonths(1);
                        }
                    }
                    finally
                    {
                        if (mserver != null)
                        {
                            mserver.Disconnect();
                        }
                    }


                    if (list.Count > 2000)
                    {
                        lock (_tempLocation)
                        {
                            _tempLocation.Add(key, new TempLocationModel {List = list, Time = DateTime.Now});
                        }
                    }
                }

                int total = list.Count();
                int totalNum = (pageIndex - 1) * pageSize;
                if (total == 0 || total <= totalNum)
                {
                    info.set('1', "GetDeviceHist", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"未找到任何记录\"}";
                }
                else
                {
                    list = list.Skip(totalNum).Take(pageSize).ToList();
                    var sb = new StringBuilder();
                    sb.Append("{");
                    sb.Append("\"Code\":\"1\"");
                    sb.Append(",\"Total\":\"" + total + "\"");
                    sb.Append(",\"List\":[");
                    foreach (var item in list)
                    {
                        sb.Append("{");
                        sb.Append("\"Time\":\"" + item.Time.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo) + "\"");
                        sb.Append(",\"Status\":\"" + item.Status + "\"");
                        double lat = 0;
                        double lng = 0;
                        if (item.Lat != null && item.Lng != null)
                            LocationHelper.WGS84ToGCJ(item.Lat.Value, item.Lng.Value, out lat, out lng);
                        sb.Append(",\"Latitude\":\"" + lat.ToString("F6") + "\"");
                        sb.Append(",\"Longitude\":\"" + lng.ToString("F6") + "\"");
                        sb.Append(",\"LocationType\":\"" + item.LocationType + "\"");
                        sb.Append(",\"CreateTime\":\"" +
                                  item.CreateTime.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo) +
                                  "\"");
                        sb.Append(",\"UpdateTime\":\"" +
                                  item.UpdateTime.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo) +
                                  "\"");
                        sb.Append("},");
                    }

                    string str = sb.ToString().TrimEnd(',');
                    sb = new StringBuilder();
                    sb.Append(str);
                    sb.Append("]}");

                    info.set('1', "GetDeviceHist", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    return sb.ToString();
                }
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetDeviceHist", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "GetDeviceHist", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        private void tempLocationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int ticker = Environment.TickCount;
#pragma warning disable CS0219 // 变量“info”已被赋值，但从未使用过它的值
            RECORD_INFO info = new RECORD_INFO();
#pragma warning restore CS0219 // 变量“info”已被赋值，但从未使用过它的值

            try
            {
                var time = DateTime.Now.AddMinutes(-5);
                lock (_tempLocation)
                {
                    var list = _tempLocation.Keys.ToList();
                    foreach (var key in list)
                    {
                        if (_tempLocation[key].Time < time)
                        {
                            _tempLocation.Remove(key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion

        #region 意见反馈

        public string Feedback(string loginId, string content)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);

                Feedback feedBack = new Feedback()
                {
                    QuestionContent = content,
                    QuestionType = 0,
                    QuestionUserID = user.UserID,
                    CreateTime = DateTime.Now,
                    FeedbackState = 0,
                    Deleted = false
                };
                FeedBack.GetInstance().Save(feedBack);

                info.set('1', "Feedback", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"保存成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "Feedback", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "Feedback", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }


        public string GetFeedback(string loginId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                List<Feedback> feedBack = FeedBack.GetInstance().GetList(user.UserID);
                if (feedBack == null)
                {
                    info.set('1', "GetFeedback", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"取不到数据\"}";
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\"Code\":\"1\"");
                sb.Append(",\"Arr\":[");
                foreach (var item in feedBack)
                {
                    sb.Append("{");
                    sb.Append("\"AnswerContent\":\"" + item.AnswerContent + "\"");
                    sb.Append(",\"AnswerUserID\":\"" + item.AnswerUserID + "\"");
                    sb.Append(",\"CreateTime\":\"" +
                              item.CreateTime.ToString(TimeFormat, DateTimeFormatInfo.InvariantInfo) +
                              "\"");
                    sb.Append(",\"FeedbackID\":\"" + item.FeedbackID + "\"");
                    sb.Append(",\"FeedbackState\":\"" + item.FeedbackState + "\"");
                    sb.Append(",\"HandleTime\":\"" +
                              (item.HandleTime == null
                                  ? ""
                                  : item.HandleTime.Value.ToString(TimeFormat,
                                      DateTimeFormatInfo.InvariantInfo)) + "\"");
                    sb.Append(",\"HandleUserID\":\"" + item.HandleUserID + "\"");
                    sb.Append(",\"QuestionContent\":\"" + item.QuestionContent + "\"");
                    sb.Append(",\"QuestionImg\":\"" + item.QuestionImg + "\"");
                    sb.Append(",\"QuestionType\":\"" + item.QuestionType + "\"");
                    sb.Append(",\"QuestionUserID\":\"" + item.QuestionUserID + "\"");
                    sb.Append("},");
                }

                string str = sb.ToString().TrimEnd(',');
                sb = new StringBuilder();
                sb.Append(str);
                sb.Append("]}");

                info.set('1', "GetFeedback", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetFeedback", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "GetFeedback", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        public string DeleteFeedback(string loginId, int feedbackId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                if (feedbackId <= 0)
                {
                    info.set('1', "DeleteFeedback", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"参数错误\"}";
                }

                FeedBack.GetInstance().Delete(feedbackId);

                info.set('1', "DeleteFeedback", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"删除成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "DeleteFeedback", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "DeleteFeedback", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        public string UpLoadImg(string loginId, int feedBackId, string img)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                string imgUrl = "";
                if (string.IsNullOrEmpty(img))
                {
                    string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                    string filePath = Config.GetInstance().Path + "\\Upload" + "\\FeedBack\\" + user.UserID + "\\";
                    DirectoryInfo di = new DirectoryInfo(filePath);
                    if (!di.Exists)
                        di.Create();
                    byte[] by = Utility.Convert.StrToHexByte(img);
                    //MemoryStream ms = new MemoryStream();
                    //ms.Write(by, 0, by.Length);
                    //Image image = Image.FromStream(ms);
                    //Bitmap bitmap = new Bitmap(image, 64, 64);
                    //bitmap.Save("", System.Drawing.Imaging.ImageFormat.Jpeg);
                    FileStream fs = new FileStream(filePath + fileName, FileMode.OpenOrCreate);
                    fs.Position = 0;
                    fs.Write(by, 0, by.Length);
                    fs.Close();
                    fs.Dispose();
                    imgUrl = "FeedBack/" + user.UserID + "/" + fileName;
                }

                Feedback feedBack = FeedBack.GetInstance().GetByFeedbackId(feedBackId);
                if (feedBack.QuestionUserID == user.UserID)
                {
                    if (string.IsNullOrEmpty(feedBack.QuestionImg))
                    {
                        feedBack.QuestionImg = imgUrl;
                    }
                    else
                    {
                        feedBack.QuestionImg = feedBack.QuestionImg + "|" + imgUrl;
                    }

                    FeedBack.GetInstance().Save(feedBack);

                    info.set('1', "UpLoadImg", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"1\",\"Message\":\"上传成功\"}";
                }
                else
                {
                    info.set('1', "UpLoadImg", "", 0, Environment.TickCount - ticker, -4);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-3\",\"Message\":\"越权操作\"}";
                }
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "UpLoadImg", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "UpLoadImg", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 用户设置

        public string UpdateNotification(string loginId, int notification, int notificationSound,
            int notificationVibration)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                user.Notification = notification == 1 ? true : false;
                user.NotificationSound = notificationSound == 1 ? true : false;
                user.NotificationVibration = notificationVibration == 1 ? true : false;
                Logic.User.GetInstance().Save(user);

                info.set('1', "UpdateNotific", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"修改成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "UpdateNotific", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "UpdateNotific", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 取宝贝好友列表

        public string GetBabyFriendList(string loginId, int deviceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                var friends = DeviceFriend.GetInstance().GetByDeviceId(deviceId);

                var sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\"Code\":\"1\"");
                sb.Append(",\"friendList\":[");
                foreach (var item in friends)
                {
                    Device friend = Logic.Device.GetInstance().Get(item.ObjectId);

                    sb.Append("{");
                    sb.Append("\"DeviceFriendId\":\"" + item.DeviceFriendId + "\"");
                    sb.Append(",\"Relationship\":\"" + item.Type + "\"");
                    sb.Append(",\"FriendDeviceId\":\"" + item.ObjectId + "\"");

                    var name = item.Name;
                    if (string.IsNullOrEmpty(name))
                    {
                        var device = Logic.Device.GetInstance().Get(item.ObjectId);
                        name = device.BabyName;
                        if (string.IsNullOrEmpty(name))
                        {
                            name = string.IsNullOrEmpty(device.PhoneNumber) ? device.SerialNumber : device.PhoneNumber;
                        }
                    }

                    sb.Append(",\"Name\":\"" + name + "\"");

                    if (friend.PhoneNumber != "")
                    {
                        sb.Append(",\"Phone\":\"" + friend.PhoneNumber + "\"");
                    }
                    else
                    {
                        sb.Append(",\"Phone\":\"\"");
                    }

                    sb.Append("},");
                }

                string str = sb.ToString().TrimEnd(',');
                sb = new StringBuilder();
                sb.Append(str);
                sb.Append("]}");

                info.set('1', "GetBabyFriendL", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetBabyFriendL", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "GetBabyFriendL", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 删除宝贝好友

        public string DeleteBabyFriend(string loginId, int DeviceFriendId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                var friends = DeviceFriend.GetInstance().Get(DeviceFriendId);

                if (friends != null)
                {
                    //delete the record
                    DeviceFriend.GetInstance().CleanByDeviceId(friends.DeviceID, friends.ObjectId);

                    var device = Logic.Device.GetInstance().Get(friends.DeviceID);
                    OnSend(device, SendType.FriendsListNotify, "1");

                    var device2 = Logic.Device.GetInstance().Get(friends.ObjectId);
                    OnSend(device2, SendType.FriendsListNotify, "1");
                }

                info.set('1', "DeleteBabyF", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"删除成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "DeleteBabyF", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "DeleteBabyF", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 修改宝贝好友昵称

        public string UpdateBabyFriendName(string loginId, int DeviceFriendId, string new_name)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                var friends = DeviceFriend.GetInstance().Get(DeviceFriendId);

                friends.Name = new_name;
                //update the record
                DeviceFriend.GetInstance().Save(friends);

                var device = Logic.Device.GetInstance().Get(friends.DeviceID);
                OnSend(device, SendType.FriendsListNotify, "1");

                info.set('1', "UpdateBabyFN", "", 1, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"修改成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "UpdateBabyFN", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);
                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "UpdateBabyFN", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);
                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 电子围栏

        public string GetGeoFenceList(string loginId, int deviceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId, deviceId);
                List<GeoFence> geofenceList = Logic.GeoFence.GetInstence().GetGeoFenceByDeviceId(deviceId);
                if (geofenceList == null || geofenceList.Count <= 0)
                {
                    info.set('1', " GetGeoFenL", "", 0, Environment.TickCount - ticker, -2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"未取到数据\"}";
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\"Code\":\"1\"");
                sb.Append(",\"GeoFenceList\":[");
                foreach (var item in geofenceList)
                {
                    double gcjLat = 0, gcjLng = 0;
                    int radii = 0;
                    if (!string.IsNullOrEmpty(item.LatAndLng))
                    {
                        string[] latAndLng = item.LatAndLng.Split('-');
                        string[] latLng = latAndLng[0].Split(',');
                        double lat = double.Parse(latLng[0]);
                        double lng = double.Parse(latLng[1]);
                        LocationHelper.WGS84ToGCJ(lat, lng, out gcjLat, out gcjLng);
                        radii = int.Parse(latAndLng[1]);
                    }

                    sb.Append("{");
                    sb.Append("\"GeofenceID\":\"" + item.GeofenceID + "\"");
                    sb.Append(",\"FenceName\":\"" + item.FenceName + "\"");
                    sb.Append(",\"Entry\":\"" + (item.Entry ? 1 : 0) + "\"");
                    sb.Append(",\"Exit\":\"" + (item.Exit ? 1 : 0) + "\"");
                    sb.Append(",\"CreateTime\":\"" +
                              item.CreateTime.ToString(TimeFormat, DateTimeFormatInfo.InvariantInfo) +
                              "\"");
                    sb.Append(",\"UpdateTime\":\"" +
                              item.UpdateTime.ToString(TimeFormat, DateTimeFormatInfo.InvariantInfo) +
                              "\"");
                    sb.Append(",\"Enable\":\"" + (item.Enable ? 1 : 0) + "\"");
                    sb.Append(",\"Description\":\"" + item.Description + "\"");
                    sb.Append(",\"Lat\":\"" + gcjLat + "\"");
                    sb.Append(",\"Lng\":\"" + gcjLng + "\"");
                    sb.Append(",\"Radii\":\"" + radii + "\"");
                    sb.Append("},");
                }

                string str = sb.ToString().TrimEnd(',');
                sb = new StringBuilder();
                sb.Append(str);
                sb.Append("]}");

                info.set('1', "GetGeoFenL", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetGeoFenL", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "GetGeoFenL", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        public string SaveGeoFence(string loginId, int geoFenceId, string fenceName, int entry, int exit, int deviceId,
            string latAndLng, int enable)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId, deviceId);
                if (string.IsNullOrEmpty(fenceName))
                {
                    info.set('1', "SaveGeoFence", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"围栏名称不能为空\"}";
                }

                if (string.IsNullOrEmpty(latAndLng))
                {
                    info.set('1', "SaveGeoFence", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"经纬度不能为空\"}";
                }

                double lat = 0, lng = 0;
                string[] temp = latAndLng.Split('-');
                string[] latLng = temp[0].Split(',');
                double gcjLat = double.Parse(latLng[0]);
                double gcjLng = double.Parse(latLng[1]);
                LocationHelper.GCJToWGS84(gcjLat, gcjLng, out lat, out lng);
                GeoFence geoFenceSave = new GeoFence
                {
                    GeofenceID = geoFenceId,
                    FenceName = fenceName,
                    Entry = entry == 1 ? true : false,
                    Exit = exit == 1 ? true : false,
                    Enable = enable == 1 ? true : false,
                    DeviceID = deviceId,
                    LatAndLng = lat + "," + lng + "-" + temp[1]
                };
                Logic.GeoFence.GetInstence().SaveGeoFence(geoFenceSave);

                info.set('1', "SaveGeoFence", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"保存成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "SaveGeoFence", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "SaveGeoFence", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        public string DeleteGeoFence(string loginId, int geoFenceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                if (geoFenceId <= 0)
                {
                    info.set('1', "DeleteGeoFence", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"参数错误\"}";
                }

                Logic.GeoFence.GetInstence().DleteGeoFenceById(geoFenceId);

                info.set('1', "DeleteGeoFence", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"删除成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "DeleteGeoFence", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "DeleteGeoFence", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 上学守护

        public string SchoolGuardian(string loginId, int deviceId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                if (deviceId <= 0)
                {
                    info.set('1', "SchoolGuard", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"-1\",\"Message\":\"参数错误\"}";
                }

                SchoolGuardian sg = Logic.SchoolGuardian.GetInstance()
                    .Get(deviceId, DateTime.Now.ToString("yyyy-MM-dd"));
                if (sg == null)
                {
                    info.set('1', "SchoolGuard", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"未取到数据\"}";
                }

                var sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\"Code\":\"1\"");
                sb.Append(",\"SchoolID\":\"" + sg.SchoolGuardianID + "\"");
                sb.Append(",\"SchoolDay\":\"" + sg.SchoolDay + "\"");
                sb.Append(",\"SchoolArriveContent\":\"" + sg.SchoolArriveContent + "\"");
                sb.Append(",\"SchoolArriveTime\":\"" + (sg.SchoolArriveTime ?? "") + "\"");
                sb.Append(",\"SchoolLeaveContent\":\"" + sg.SchoolLeaveContent + "\"");
                sb.Append(",\"SchoolLeaveTime\":\"" + (sg.SchoolLeaveTime ?? "") + "\"");
                sb.Append(",\"RoadStayContent\":\"" + sg.RoadStayContent + "\"");
                sb.Append(",\"RoadStayTime\":\"" + sg.RoadStayTime + "\"");
                sb.Append(",\"HomeBackContent\":\"" + sg.HomeBackContent + "\"");
                sb.Append(",\"HomeBackTime\":\"" + (sg.HomeBackTime ?? "") + "\"");
                sb.Append("}");

                info.set('1', "SchoolGuard", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "SchoolGuard", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);
                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "SchoolGuard", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        public string UpdateGuard(string loginId, int deviceId, int offOn)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId, deviceId);
                Device device = Logic.Device.GetInstance().Get(deviceId);
                if (device == null)
                {
                    info.set('1', "UpdateGuard", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"未取到数据\"}";
                }

                if (offOn != 0)
                {
                    info.set('1', "UpdateGuard", "", 0, Environment.TickCount - ticker, 3);
                    CPhotoStat.GetInstance().add_info(info);

                    if (device.SchoolLat == 0 || device.SchoolLng == 0)
                        return "{\"Code\":\"3\",\"Message\":\"请先设置学校位置\"}";
                    if (device.HomeLat == 0 || device.HomeLng == 0)
                        return "{\"Code\":\"3\",\"Message\":\"请先设置家的位置\"}";
                    if (string.IsNullOrEmpty(device.LatestTime))
                        return "{\"Code\":\"3\",\"Message\":\"请先设置最晚到家时间\"}";
                    Model.Entity.DeviceSet deviceSet =
                        DeviceSet.GetInstance().Get(deviceId);
                    if (string.IsNullOrEmpty(deviceSet.WeekDisabled))
                        return "{\"Code\":\"3\",\"Message\":\"请先设置上课周期\"}";
                    if (string.IsNullOrEmpty(deviceSet.ClassDisabled1))
                        return "{\"Code\":\"3\",\"Message\":\"请先设置上课时间段\"}";
                    if (string.IsNullOrEmpty(deviceSet.ClassDisabled2))
                        return "{\"Code\":\"3\",\"Message\":\"请先设置上课时间段\"}";
                }

                device.IsGuard = offOn != 0;
                Logic.Device.GetInstance().Save(device);
                List<User> userList = UserDevice.GetInstance()
                    .GetUserByDeviceId(device.DeviceID).Where(w => w.UserID != user.UserID).ToList();
                Notification.GetInstance().Send(device.DeviceID, 230, userList);

                info.set('1', "UpdateGuard", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"1\",\"Message\":\"修改成功\"}";
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "UpdateGuard", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);
                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "UpdateGuard", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        #endregion

        #region 其他

        public string GetAD(string loginId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                var project = Logic.Project.GetInstance().Get(user.Project);
                if (project == null)
                {
                    info.set('1', "GetAD", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"未取到数据\"}";
                }

                var ad = new StringBuilder();
                int adTotal = 0;
                if (!string.IsNullOrEmpty(project.AD))
                {
                    string[] list = project.AD.Split('|');
                    adTotal = list.Count();
                    foreach (var item in list)
                    {
                        string[] imageAndUrl = item.Split(',');
                        if (ad.Length != 0)
                        {
                            ad.Append(",");
                        }

                        ad.Append("{\"Image\":\"" + imageAndUrl[0] + "\",\"Url\":\"" + imageAndUrl[1] + "\"}");
                    }
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\"Code\":\"1\"");
                sb.Append(",\"Total\":\"" + adTotal + "\"");
                sb.Append(",\"List\":[" + ad + "]");
                sb.Append("}");

                info.set('1', "GetAD", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetAD", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "GetAD", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        public string ExceptionError(string error)
        {
            Logger.Fatal(error);
            return "{ \"Code\":\"1\"}";
        }

        public string CheckAppVersion(string loginId)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                var user = CheckLogin(loginId);
                Project project = Logic.Project.GetInstance().Get(user.Project);
                if (project == null)
                {
                    info.set('1', "CheckAppVersio", "", 0, Environment.TickCount - ticker, 2);
                    CPhotoStat.GetInstance().add_info(info);

                    return "{\"Code\":\"2\",\"Message\":\"未取到数据\"}";
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\"Code\":\"1\"");
                sb.Append(",\"AppVersionId\":\"0\"");
                if (user.LoginType == 2 && !string.IsNullOrEmpty(user.AppID))
                {
                    sb.Append(",\"AppleVersion\":\"" + project.AppleVersion + "\"");
                    sb.Append(",\"AppleUrl\":\"" + project.AppleUrl + "\"");
                    sb.Append(",\"AppleDescription\":\"" + project.AppleDescription + "\"");
                }
                else
                {
                    sb.Append(",\"AndroidVersion\":\"" + project.AndroidVersion + "\"");
                    sb.Append(",\"AndroidUrl\":\"" + project.AndroidUrl + "\"");
                    sb.Append(",\"AndroidDescription\":\"" + project.AndroidDescription + "\"");
                }

                sb.Append("}");

                info.set('1', "CheckAppVersio", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return sb.ToString();
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "CheckAppVersio", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return "{\"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\"}";
            }
            catch (Exception ex)
            {
                info.set('1', "CheckAppVersio", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{\"Code\":\"-2\",\"Message\":\"系统错误\"}";
            }
        }

        public Task<string> GetAddressAsync(string loginId, int mapType, double lat, double lng)
        {
            return GetAddressService(loginId, mapType, lat, lng);
        }

        public Task<string> GetAddressService(string loginId, int mapType, double lat, double lng)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                CheckLogin(loginId);
                double wgslat = lat;
                double wgslng = lng;
                int DoLocationQueueLbsWifiCount = 0;
                if (mapType == 2)
                {
                    LocationHelper.BD09ToGCJ(lat, lng, out wgslat, out wgslng);
                }

                if (mapType == 1 || mapType == 2)
                {
                    LocationHelper.GCJToWGS84(wgslat, wgslng, out wgslat, out wgslng);
                }

                Logic.Count.GetInstance().AddressTotal();

                GetDoLocationQueueCount dolocationqueuelbswifihandler = OnGetDoLocationQueueLbsWifiCount;

                if (dolocationqueuelbswifihandler != null)
                    DoLocationQueueLbsWifiCount = dolocationqueuelbswifihandler();

                if (DoLocationQueueLbsWifiCount < 20)
                {
//                    var address = LBSWIFIClient.Get().GetAddress(_apiKey, wgslat, wgslng);
                    var address = APIClient.GetInstance().GetAddress(wgslat, wgslng);
                    if (address != null && address.Code == 1)
                    {
                        Logic.Count.GetInstance().Address();
                        StringBuilder nearby = new StringBuilder();
                        StringBuilder strAddress = new StringBuilder();
                        strAddress.Append(address.Province + address.City + address.District + address.Road);
                        foreach (var item in address.Nearby)
                        {
                            if (nearby.Length == 0)
                            {
                                nearby.Append("{\"POI\":\"" + GetStr(item) + "\"}");
                            }
                            else
                            {
                                nearby.Append(",{\"POI\":\"" + GetStr(item) + "\"}");
                            }

                            strAddress.Append(",");
                            strAddress.Append(item);
                        }

                        info.set('1', "GetAddressSvc", "", 0, Environment.TickCount - ticker, 1);
                        CPhotoStat.GetInstance().add_info(info);

                        return Task.FromResult("{ \"Code\":\"1\",\"Province\":\"" + GetStr(address.Province) + "\",\"City\":\"" + GetStr(address.City) +
                                               "\",\"District\":\"" + GetStr(address.District) + "\",\"Road\":\"" + GetStr(address.Road) + "\",\"Nearby\":[" +
                                               nearby.ToString() + "],\"Address\":\"" + GetStr(strAddress.ToString()) + "\" }");
                        //this.GetStr(nearby.ToString()) + "]}";
                    }
                    else
                    {
                        info.set('1', "GetAddressSvc", "", 0, Environment.TickCount - ticker, 0);
                        CPhotoStat.GetInstance().add_info(info);

                        return Task.FromResult("{ \"Code\":\"0\",\"Message\":\"无法解析该的位置\" }");
                    }
                }
                else
                {
                    info.set('1', "GetAddressSvc", "", 0, Environment.TickCount - ticker, 0);
                    CPhotoStat.GetInstance().add_info(info);

                    return Task.FromResult("{ \"Code\":\"0\",\"Message\":\"解析接口无法响应请求\" }");
                }
            }
            catch (FaultException<ServiceError> ex)
            {
                info.set('1', "GetAddressSvc", "", 0, Environment.TickCount - ticker, -3);
                CPhotoStat.GetInstance().add_info(info);

                return Task.FromResult("{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }");
            }
            catch (Exception ex)
            {
                info.set('1', "GetAddressSvc", "", 0, Environment.TickCount - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return Task.FromResult("{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }");
            }
        }

        public string WIFILBS(string loginId, int mapType, string bts, string wifis, double lat, double lng, int radius)
        {
            return "{ \"Code\":\"1\" }";

#pragma warning disable CS0162 // 检测到无法访问的代码
            try
#pragma warning restore CS0162 // 检测到无法访问的代码
            {
                CheckLogin(loginId);
                if (!string.IsNullOrEmpty(bts) && bts.IndexOf(',') < 0)
                    bts = null;
                if (!string.IsNullOrEmpty(wifis) && wifis.IndexOf(',') < 0)
                    wifis = null;
                if (lat == 0 && lng == 0 || string.IsNullOrEmpty(bts) && string.IsNullOrEmpty(wifis))
                {
                    return "{ \"Code\":\"-1\",\"Message\":\"输入参数错误\" }";
                }

                double wgslat = lat;
                double wgslng = lng;
                if (mapType == 2)
                {
                    LocationHelper.BD09ToGCJ(lat, lng, out wgslat, out wgslng);
                }

                if (mapType == 1 || mapType == 2)
                {
                    LocationHelper.GCJToWGS84(wgslat, wgslng, out wgslat, out wgslng);
                }

                if (OnCollectLbsAndWifi != null)
                    OnCollectLbsAndWifi(new LBSWIFI()
                    {
                        bts = bts,
                        wifis = wifis,
                        lat = wgslat,
                        lng = wgslng,
                        radius = radius
                    });
                //Thread t = new Thread(delegate()
                //{
                //    LBSWIFI.Get().CollectLbsAndWifiAsync(_apiKey, 2, bts, wifis, wgslat, wgslng, radius);
                //}) {IsBackground = true};
                //t.Start();
                return "{ \"Code\":\"1\" }";
            }
            catch (FaultException<ServiceError> ex)
            {
                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        #endregion


        #region 后台管理

        private static readonly Dictionary<Guid, DealerUser> LoginDealerTemp =
            new Dictionary<Guid, DealerUser>();

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="passWord">密码</param>
        /// <returns></returns>
        public Guid ManageLogin(string userName, string passWord)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passWord))
            {
                throw new FaultException<int>(-1, "输入参数错误!");
            }

            var user = Logic.DealerUser.GetInstance().Get(userName);
            string passWordEncoded = MD5Helper.MD5Encrypt(passWord, user == null ? null : user.Salt);
            if (user == null || !user.Password.Equals(passWordEncoded))
            {
                throw new FaultException<int>(0, "用户名或密码错误!");
            }
            else
            {
                lock (user)
                {
                    if (user.LoginID != null && LoginDealerTemp.ContainsKey(user.LoginID.Value))
                    {
                        //LoginDealerTemp.Remove(user.LoginID.Value);
                    }
                    else
                    {
                        user.LoginID = Guid.NewGuid();
                        if (string.IsNullOrEmpty(user.Salt))
                        {
                            user.Salt = Utils.createNewSalt();
                            user.Password = MD5Helper.MD5Encrypt(passWord, user.Salt);
                        }

                        Logic.DealerUser.GetInstance().Save(user);
                        LoginDealerTemp.Add(user.LoginID.Value, user);
                    }
                }

                return user.LoginID.Value;
            }
        }

        public void ManageLogOut(Guid loginId)
        {
            var user = ManageCheckLogin(loginId);
            user.LoginID = null;
            Logic.DealerUser.GetInstance().Save(user);
            if (LoginDealerTemp.ContainsKey(loginId))
            {
                //LoginDealerTemp.Remove(loginId);
            }
        }

        public void ManageUpdataUser(Guid loginId, string name, string phoneNumber)
        {
            var user = ManageCheckLogin(loginId);
            if (string.IsNullOrEmpty(name))
            {
                throw new FaultException<int>(-1, "输入参数错误!");
            }

            user.Name = name;
            user.PhoneNumber = phoneNumber;
            Logic.DealerUser.GetInstance().Save(user);
        }

        public void ManageUpdataUserPassword(Guid loginId, string password, string newPassword)
        {
            var user = ManageCheckLogin(loginId);
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(newPassword))
            {
                throw new FaultException<int>(-1, "输入参数错误!");
            }

            if (!user.Password.Equals(MD5Helper.MD5Encrypt(password, user.Salt)))
            {
                throw new FaultException<int>(-1, "输入原密码错误!");
            }

            if (string.IsNullOrEmpty(user.Salt))
            {
                user.Salt = Utils.createNewSalt();
            }

            user.Password = MD5Helper.MD5Encrypt(newPassword, user.Salt);
            Logic.DealerUser.GetInstance().Save(user);


            LoginDealerTemp.Remove(user.LoginID.Value);
            user.LoginID = Guid.NewGuid();
            Logic.DealerUser.GetInstance().Save(user);
            LoginDealerTemp.Add(user.LoginID.Value, user);
        }

        private DealerUser ManageCheckLogin(Guid loginId)
        {
            DealerUser user;
            lock (LoginDealerTemp)
            {
                if (LoginDealerTemp.ContainsKey(loginId))
                    user = LoginDealerTemp[loginId];
                else
                {
                    user = Logic.DealerUser.GetInstance()
                        .GetList()
                        .FirstOrDefault(
                            w => w.LoginID != null && w.LoginID == loginId);
                    if (user != null)
                    {
                        LoginDealerTemp.Add(loginId, user);
                    }
                }

            }

            if (user == null || user.Status == 0)
                throw new FaultException<int>(0, "登录信息已过期，请重新登录!");
            return user;
        }

        /// <summary>
        /// 获取登陆用户信息
        /// </summary>
        /// <param name="loginId">登陆编号</param>
        /// <returns></returns>
        public DealerUser ManageGetLoginUser(Guid loginId)
        {
            var user = ManageCheckLogin(loginId);
            return user;
        }

        /// <summary>
        /// 获取登陆用户代理商信息
        /// </summary>
        /// <param name="loginId">登陆编号</param>
        /// <returns></returns>
        public Dealer ManageGetLoginDealer(Guid loginId)
        {
            var user = ManageCheckLogin(loginId);
            return Logic.Dealer.GetInstance().Get(user.DealerId);
        }

        public List<Dealer> ManageGetDealerList(Guid loginId)
        {
            var user = ManageCheckLogin(loginId);
            var list = Logic.Dealer.GetInstance()
                .GetList()
                .Where(
                    w => w.ParentId == user.DealerId && w.Status == 1).ToList();
            return list;
        }

        public Dealer ManageGetDealer(Guid loginId, int dealerId)
        {
            var user = ManageCheckLogin(loginId);
            var dealer = Logic.Dealer.GetInstance().Get(dealerId);
            if (dealer == null || dealer.ParentId != user.DealerId)
            {
                throw new FaultException<int>(-1, "该经销商并不存在!");
            }

            return dealer;
        }

        public int ManageAddDealer(Guid loginId, Dealer ndealer)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 2)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            if (string.IsNullOrEmpty(ndealer.Name) ||
                ndealer.DealerType > Logic.Dealer.GetInstance().Get(user.DealerId).DealerType ||
                Logic.Dealer.GetInstance().Get(user.DealerId).DealerType == 1)
            {
                throw new FaultException<int>(-1, "输入参数错误!");
            }

            ndealer.ParentId = user.DealerId;
            ndealer.Status = 1;
            Logic.Dealer.GetInstance().Save(ndealer);
            return ndealer.DealerId;
        }

        public int ManageSaveDealer(Guid loginId, Dealer ndealer)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 2)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            var odealer = Logic.Dealer.GetInstance().Get(ndealer.DealerId);
            if (odealer == null || odealer.ParentId != user.DealerId)
            {
                throw new FaultException<int>(-1, "该经销商并不存在!");
            }

            odealer.Name = ndealer.Name;
            odealer.PhoneNumber = ndealer.PhoneNumber;
            odealer.Address = ndealer.Address;
            odealer.Remark = ndealer.Remark;
            Logic.Dealer.GetInstance().Save(odealer);
            return odealer.DealerId;
        }

        public void ManageDelDealer(Guid loginId, int dealerId)
        {
            var user = ManageCheckLogin(loginId);
            if (dealerId == 0)
            {
                throw new FaultException<int>(-1, "输入参数错误!");
            }

            var oDealer = Logic.Dealer.GetInstance().Get(dealerId);
            if (user.UserType < 2 || oDealer == null || oDealer.ParentId != user.DealerId)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            if (DealerDevice.GetInstance().GetByDealerId(dealerId).Count > 0)
            {
                throw new FaultException<int>(-1, "无法删除已出售设备的经销商!");
            }

            //删除经销商的登陆账号
            var list = Logic.DealerUser.GetInstance().GetList().Where(w => w.DealerId == dealerId).ToList();
            foreach (var item in list)
            {
                Logic.DealerUser.GetInstance().Del(item.DealerUserId);
            }

            Logic.Dealer.GetInstance().Del(dealerId);
        }

        public Count ManageGetCount(Guid loginId)
        {
            var user = ManageCheckLogin(loginId);
            return DealerDevice.GetInstance().GetCount(user.DealerId);
        }

        public DealerUserList ManageGetDealerUserList(Guid loginId, int dealerId, int pageindex, int pagesize,
            string name)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 2)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            var result = new DealerUserList {PageIndex = pageindex, PageSize = pagesize};
            if (dealerId == 0)
            {
                dealerId = user.DealerId;
            }
            else if (dealerId != user.DealerId)
            {
                var dealer = Logic.Dealer.GetInstance().Get(dealerId);
                if (dealer == null || dealer.ParentId != user.DealerId)
                {
                    dealerId = user.DealerId;
                }
            }

            var list =
                Logic.DealerUser.GetInstance()
                    .GetList()
                    .Where(
                        w =>
                            w.DealerId == dealerId &&
                            (string.IsNullOrEmpty(name) || w.Name.IndexOf(name, StringComparison.Ordinal) >= 0) &&
                            w.UserType != 3)
                    .ToList();
            result.Total = list.Count();
            int totalNum = (pageindex - 1) * pagesize;
            result.List = list.Skip(totalNum).Take(pagesize).ToList();
            return result;
        }

        public DealerUser ManageGetDealerUser(Guid loginId, int dealerUserId)
        {
            var user = ManageCheckLogin(loginId);
            var dealerUser = Logic.DealerUser.GetInstance().Get(dealerUserId);
            if (dealerUser == null)
            {
                throw new FaultException<int>(-1, "该用户并不存在!");
            }

            if (dealerUser.DealerId != user.DealerId)
            {
                var dealer = Logic.Dealer.GetInstance().Get(dealerUser.DealerId);
                if (dealer == null || dealer.ParentId != user.DealerId)
                {
                    throw new FaultException<int>(-1, "该用户并不存在!");
                }
            }

            return dealerUser;
        }

        public int ManageAddDealerUser(Guid loginId, DealerUser dealerUser, string passsword)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 2)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            if (string.IsNullOrEmpty(dealerUser.UserName) || string.IsNullOrEmpty(passsword) ||
                string.IsNullOrEmpty(dealerUser.Name) || dealerUser.UserType > user.UserType)
            {
                throw new FaultException<int>(-1, "输入参数错误!");
            }

            if (Logic.DealerUser.GetInstance().Get(dealerUser.UserName) != null)
            {
                throw new FaultException<int>(-1, "用户名已经被使用，请使用别的用户名!");
            }

            if (dealerUser.DealerId == 0)
                dealerUser.DealerId = user.DealerId;
            else if (dealerUser.DealerId != user.DealerId)
            {
                var dealer = Logic.Dealer.GetInstance().Get(dealerUser.DealerId);
                if (dealer == null || dealer.ParentId != user.DealerId)
                {
                    dealerUser.DealerId = user.DealerId;
                }
            }

            if (string.IsNullOrEmpty(dealerUser.Salt))
            {
                dealerUser.Salt = Utils.createNewSalt();
            }

            dealerUser.Password = MD5Helper.MD5Encrypt(passsword, dealerUser.Salt);
            dealerUser.Status = 1;
            Logic.DealerUser.GetInstance().Save(dealerUser);
            return dealerUser.DealerUserId;
        }

        public int ManageSaveDealerUser(Guid loginId, DealerUser dealerUser, string passsword)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 2)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            if (string.IsNullOrEmpty(dealerUser.Name) || dealerUser.DealerUserId == 0)
            {
                throw new FaultException<int>(-1, "输入参数错误!");
            }

            var oDealerUser = Logic.DealerUser.GetInstance().Get(dealerUser.DealerUserId);
            if (oDealerUser.DealerId != user.DealerId)
            {
                var dealer = Logic.Dealer.GetInstance().Get(dealerUser.DealerId);
                if (dealer == null || dealer.ParentId != user.DealerId)
                {
                    throw new FaultException<int>(-1, "没有权限执行该操作");
                }
            }

            if (string.IsNullOrEmpty(oDealerUser.Salt))
            {
                oDealerUser.Salt = Utils.createNewSalt();
            }

            if (!string.IsNullOrEmpty(passsword))
            {
                oDealerUser.Password = MD5Helper.MD5Encrypt(passsword, oDealerUser.Salt);
            }

            oDealerUser.Name = dealerUser.Name;
            oDealerUser.PhoneNumber = dealerUser.PhoneNumber;
            oDealerUser.Remark = dealerUser.Remark;
            Logic.DealerUser.GetInstance().Save(oDealerUser);
            return dealerUser.DealerUserId;
        }

        public void ManageDelDealerUser(Guid loginId, int dealerUserId)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 2)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            if (dealerUserId == 0)
            {
                throw new FaultException<int>(-1, "输入参数错误!");
            }

            if (dealerUserId == user.DealerUserId)
            {
                throw new FaultException<int>(-1, "无法删除自己!");
            }

            var oDealerUser = Logic.DealerUser.GetInstance().Get(dealerUserId);
            if (oDealerUser.DealerId != user.DealerId)
            {
                var dealer = Logic.Dealer.GetInstance().Get(oDealerUser.DealerId);
                if (dealer == null || dealer.ParentId != user.DealerId)
                {
                    throw new FaultException<int>(-1, "没有权限执行该操作!");
                }
            }

            Logic.DealerUser.GetInstance().Del(dealerUserId);
        }

        public DealerDeviceList ManageGetDealerDeviceList(Guid loginId, int pageindex, int pagesize, string serialNumber,
            int? model, int? status, bool? active)
        {
            var user = ManageCheckLogin(loginId);
            var result = new DealerDeviceList
            {
                Total = 0,
                PageIndex = pageindex,
                PageSize = pagesize,
                List = new List<Model.Manage.DealerDevice>()
            };
            var list = DealerDevice.GetInstance()
                .GetList(user.DealerId, pageindex, pagesize, serialNumber, model, status, active, out result.Total);
            foreach (var item in list)
            {
                var device = Logic.Device.GetInstance().Get(item.DeviceId);
                var state = DeviceState.GetInstance().Get(item.DeviceId);
                var obj = new Model.Manage.DealerDevice
                {
                    CreateTime = item.CreateTime,
                    DealerId = item.DealerId,
                    DeviceId = item.DeviceId,
                    Purchaser = item.Purchaser,
                    PurchaserName =
                        (item.Purchaser != null && item.Purchaser.Value != 0)
                            ? Logic.Dealer.GetInstance().Get(item.Purchaser.Value).Name
                            : "",
                    Remark = item.Remark,
                    ReworkTime = item.ReworkTime,
                    SalesTime = item.SalesTime,
                    Status = item.Status,
                    StockTime = item.StockTime,
                    UpdateTime = item.UpdateTime,
                    SerialNumber = device.SerialNumber,
                    DeviceModelID = device.DeviceModelID,
                    BindNumber = device.BindNumber,
                    State = device.State,
                    ActiveDate = device.ActiveDate,
                    Online = state.Online,
                    LocationType = state.LocationType,
                    DeviceNote = device.DeviceNote,
                    DeviceType = device.DeviceType,
                    UserCount = UserDevice.GetInstance().GetByDeviceId(item.DeviceId).Count(),
                    DeviceTime = state.DeviceTime
                };
                result.List.Add(obj);
            }

            return result;
        }

        public void ManageAgentNumberEdit(Guid loginId, int platform, string agentNumbers)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 2)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            if (string.IsNullOrEmpty(agentNumbers))
            {
                throw new FaultException<int>(-1, "中间号不能为空!");
            }

            string[] items = Regex.Split(agentNumbers, "\r?\n");
            Regex regex = new Regex("^\\d{5,20},\\d{5,20}$");
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < items.Length; i++)
            {
                string line = items[i];
                if (!regex.Match(line).Success)
                {
                    sb.Append("第").Append(i + 1).Append("行数据格式错误");
                    if (i < items.Length - 1)
                    {
                        sb.Append("<br>");
                    }
                }
            }

            if (sb.Length > 0)
            {
                throw new FaultException<int>(-1, sb.ToString());
            }

            var createTime = DateTime.Now;
            foreach (string item in items)
            {
                if (regex.Match(item).Success)
                {
                    string[] line = item.Split(',');
                    AgentNumber agentNumber = new AgentNumber
                    {
                        Number = line[0],
                        CallOutNumber = line[1],
                        Platform = platform,
                        CreateTime = createTime
                    };
                    Logic.AgentNumber.GetInstance().Save(agentNumber);
                }
            }
        }

        public PageList<AgentNumber> ManageAgentNumberList(Guid loginId, int pageindex, int pagesize, string midNumber, int? platform)
        {
            var user = ManageCheckLogin(loginId);
            var result = new PageList<AgentNumber>()
            {
                Total = 0,
                PageIndex = pageindex,
                PageSize = pagesize,
                List = new List<AgentNumber>()
            };
            var list = Logic.AgentNumber.GetInstance().GetList(pageindex, pagesize, midNumber, platform, out result.Total);
            result.List.AddRange(list);
            return result;
        }

        public void ManageAgentNumberDel(Guid loginId, string number)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 2)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            Logic.AgentNumber.GetInstance().Del(number);
        }

        public Device ManageGetDevice(Guid loginId, int deviceId)
        {
            var user = ManageCheckLogin(loginId);
            var dealer = Logic.Dealer.GetInstance().Get(user.DealerId);
            var dealerDevice = DealerDevice.GetInstance().Get(user.DealerId, deviceId);
            if (dealer == null || dealer.Status == 0 ||
                (dealer.DealerType != 4 && dealerDevice == null))
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            var device = Logic.Device.GetInstance().Get(deviceId);
            if (device == null)
            {
                throw new FaultException<int>(-1, "设备并不存在!");
            }

            return device;
        }

        public void ManageUpdateDevice(Guid loginId, int deviceId, int model, string bindNumber, string phoneNumber, string deviceNote, int deviceType)
        {
            var user = ManageCheckLogin(loginId);
            var dealer = Logic.Dealer.GetInstance().Get(user.DealerId);
            var dealerDevice = DealerDevice.GetInstance().Get(user.DealerId, deviceId);
            if (user.UserType == 1 || dealer == null || dealer.DealerType < 3 || dealer.Status == 0 ||
                (dealer.DealerType != 4 && dealerDevice == null))
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            var device = Logic.Device.GetInstance().Get(deviceId);
            if (device == null)
            {
                throw new FaultException<int>(-1, "设备并不存在!");
            }

            /*string oldBindNumber = null;
            if (!string.IsNullOrEmpty(bindNumber) && !bindNumber.Equals(device.BindNumber))
            {
                if (Logic.Device.GetInstance().GetByBindNum(bindNumber) != null)
                    throw new FaultException<int>(-1, "绑定号已经存在!");
                oldBindNumber = device.BindNumber;
                device.BindNumber = bindNumber;
            }*/

            device.DeviceModelID = model;

            var deviceinfo = deviceNote.Split(',');
            if (deviceinfo.Length < 2 || string.IsNullOrEmpty(deviceinfo[0]) || string.IsNullOrEmpty(deviceinfo[1]))
            {
                throw new FaultException<int>(-1, "错误，备注格式：客户名称，项目名称，功能说明");
            }

            device.PhoneNumber = phoneNumber;
            device.DeviceNote = deviceNote;
            device.DeviceType = deviceType;

            Logic.Device.GetInstance().Save(device);
//            if (!string.IsNullOrEmpty(oldBindNumber))
//                Logic.Device.GetInstance().ChangeBindNumber(oldBindNumber, bindNumber);
            List<User> userList = UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID).ToList();
            Notification.GetInstance().Send(device.DeviceID, 230, userList);
        }

        public void ManageDelDevice(Guid loginId, int deviceId)
        {
            var user = ManageCheckLogin(loginId);
            var dealer = Logic.Dealer.GetInstance().Get(user.DealerId);
            var dealerDevice = DealerDevice.GetInstance().Get(user.DealerId, deviceId);
            if (user.UserType == 1 || dealer == null || dealer.DealerType < 3 || dealer.Status == 0 || (dealer.DealerType != 4 && dealerDevice == null))
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            Logic.Device.GetInstance().Del(deviceId);
            List<User> userList = UserDevice.GetInstance()
                .GetUserByDeviceId(deviceId);
            var notification = new Model.Entity.Notification()
            {
                DeviceID = deviceId,
                Type = 9,
                Content = null,
                CreateTime = DateTime.Now
            };
            Notification.GetInstance().Send(notification, userList);
            if (OnDisConnect != null)
                OnDisConnect(deviceId);
        }

        public void ManageResetDevice(Guid loginId, int deviceId)
        {
            var user = ManageCheckLogin(loginId);
            var dealer = Logic.Dealer.GetInstance().Get(user.DealerId);
            var dealerDevice = DealerDevice.GetInstance().Get(user.DealerId, deviceId);
            if (user.UserType == 1 || dealer == null || dealer.Status == 0 ||
                (dealer.DealerType != 4 && dealerDevice == null))
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            Device device = Logic.Device.GetInstance().Get(deviceId);
            User usr = Logic.User.GetInstance().Get(device.UserId);
            if (usr == null)
            {
                usr = Logic.User.GetInstance().GetByBindNumber(device.BindNumber);
            }

            if (usr != null && usr.LoginID != null)
            {
                LoginUserTemp.TryRemove(usr.LoginID, out var res);

                ClearDeviceHistory(usr.LoginID, deviceId);
            }

            Logic.Device.GetInstance().ResetDevice(device);
            device.Deviceflag = 1;
            Logic.Device.GetInstance().Save(device);

            var notification = new Model.Entity.Notification()
            {
                DeviceID = deviceId,
                Type = 9,
                Content = null,
                CreateTime = DateTime.Now
            };
            List<User> userList = UserDevice.GetInstance().GetUserByDeviceId(deviceId);
            Notification.GetInstance().Send(notification, userList);


            //通知网关更新设备配置
            //OnSend(device, SendType.Set, "");
            //通知网关更新设备通讯录
            //OnSend(device, SendType.Contact, "");
            //设备恢复出厂设置
            OnSend?.Invoke(device, SendType.DeviceRecovery, "1");

            Task.Run(() =>
            {
                Thread.Sleep(2000);
                OnDisConnect?.Invoke(deviceId);
            });


            Task.Run(() =>
            {
                Thread.Sleep(3000);
                Logic.Device.GetInstance().CleanRelation(deviceId);
            });
        }

        public void ManageStockDevice(Guid loginId, int deviceId)
        {
            var user = ManageCheckLogin(loginId);
            var dealer = Logic.Dealer.GetInstance().Get(user.DealerId);
            var dealerDevice = DealerDevice.GetInstance().Get(user.DealerId, deviceId);
            if (user.UserType == 1 || dealer == null || dealer.Status == 0 || dealerDevice == null)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            dealerDevice.Status = 1;
            dealerDevice.StockTime = DateTime.Now;
            DealerDevice.GetInstance().Update(dealerDevice);
        }

        public void ManageAddDeviceList(Guid loginId, string devices, int model, string deviceNote, int deviceType, int cloudPlatform)
        {
            var user = ManageCheckLogin(loginId);
            var dealer = Logic.Dealer.GetInstance().Get(user.DealerId);
            if (string.IsNullOrEmpty(devices))
                throw new FaultException<int>(-1, "输入参数错误!");
            if (user.UserType == 1 || dealer == null || dealer.DealerType < 3 || dealer.Status == 0)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            devices = devices.Replace("\r\n", "|");
            devices = devices.Replace("\r", "|");
            devices = devices.Replace("\n", "|");
            devices = devices.Replace("，", ",");
            string[] list = devices.Split('|');
            int row = 0;
            foreach (var item in list)
            {
                row++;
                if (!string.IsNullOrEmpty(item))
                {
                    string strDevice = item.Trim();
                    var nDevice = strDevice.Split(',');
                    if ((cloudPlatform > 0 && nDevice.Length != 3) || (cloudPlatform == 0 && nDevice.Length != 2) || string.IsNullOrEmpty(nDevice[0]) || nDevice[0].Length != 15 || //导入设备号15位
                        string.IsNullOrEmpty(nDevice[1]))
                    {
                        throw new FaultException<int>(-1, "第[" + row + "]行验证不通过");
                    }
                    else
                    {
                        if (Logic.Device.GetInstance().Get(nDevice[0]) != null)
                            throw new FaultException<int>(-1, "第[" + row + "]行验证不通过,设备编号[" + nDevice[0] + "]已经存在");
                        if (Logic.Device.GetInstance().GetByBindNum(nDevice[1]) != null)
                            throw new FaultException<int>(-1, "第[" + row + "]行验证不通过,设备绑定号[" + nDevice[1] + "]已经存在");
                    }
                }
                else
                {
                    throw new FaultException<int>(-1, "第[" + row + "]行验证不通过");
                }
            }

            if (deviceNote == null)
            {
                throw new FaultException<int>(-1, "设备备注信息不能为空!");
            }

            if (deviceType == 0)
            {
                throw new FaultException<int>(-1, "设备类型信息不能为空!");
            }

            var deviceinfo = deviceNote.Split(',');
            if (deviceinfo.Length < 2 || string.IsNullOrEmpty(deviceinfo[0]) || string.IsNullOrEmpty(deviceinfo[1]))
            {
                throw new FaultException<int>(-1, "错误，备注格式：客户名称，项目名称，功能说明");
            }

            row = 0;
            StringBuilder strError = new StringBuilder();
            foreach (var item in list)
            {
                row++;
                if (!string.IsNullOrEmpty(item))
                {
                    string strDevice = item.Trim();
                    var nDevice = strDevice.Split(',');
                    if ((cloudPlatform > 0 && nDevice.Length != 3) || (cloudPlatform == 0 && nDevice.Length != 2) || string.IsNullOrEmpty(nDevice[0]) || nDevice[0].Length != 15 || //导入设备号15位
                        string.IsNullOrEmpty(nDevice[1]))
                    {
                        strError.Append("第[" + row + "]行验证不通过\n");
                    }
                    else
                    {
                        if (Logic.Device.GetInstance().Get(nDevice[0]) != null)
                        {
                            strError.Append("第[" + row + "]行验证不通过,设备编号[" + nDevice[0] + "]已经存在\n");
                            continue;
                        }

                        if (Logic.Device.GetInstance().GetByBindNum(nDevice[1]) != null)
                        {
                            strError.Append("第[" + row + "]行验证不通过,设备绑定号[" + nDevice[1] + "]已经存在\n");
                            continue;
                        }

                        Device device = new Device
                        {
                            SerialNumber = nDevice[0],
                            State = 0,
                            Deleted = false,
                            UserId = 0,
                            BindNumber = nDevice[1],
                            DeviceModelID = model,
                            DeviceNote = deviceNote,
                            DeviceCustomer = deviceinfo[0],
                            DeviceProject = deviceinfo[1],
                            DeviceType = deviceType,
                            CloudPlatform = cloudPlatform
                        };
                        if (nDevice.Length > 2)
                        {
                            device.PhoneNumber = nDevice[2];
                        }

                        Logic.Device.GetInstance().Save(device);
                        Model.Entity.DealerDevice dd = new Model.Entity.DealerDevice
                        {
                            DeviceId = device.DeviceID,
                            DealerId = user.DealerId,
                            Status = 1,
                            StockTime = DateTime.Now,
                            CreateTime = DateTime.Now,
                            UpdateTime = DateTime.Now
                        };

                        try
                        {
                            DealerDevice.GetInstance().New(dd);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                }
                else
                {
                    strError.Append("第[" + row + "]行验证不通过\n");
                }
            }

            if (strError.Length > 0)
            {
                throw new FaultException<int>(-1, strError.ToString());
            }
        }

        public void ManageStockDeviceList(Guid loginId, string devices)
        {
            var user = ManageCheckLogin(loginId);
            var dealer = Logic.Dealer.GetInstance().Get(user.DealerId);
            if (string.IsNullOrEmpty(devices))
                throw new FaultException<int>(-1, "输入参数错误!");
            if (user.UserType == 1 || dealer == null || dealer.Status == 0)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            devices = devices.Replace("\r\n", "|");
            devices = devices.Replace("\r", "|");
            devices = devices.Replace("\n", "|");
            devices = devices.Replace("，", ",");
            string[] list = devices.Split('|');
            int row = 0;
            foreach (var item in list)
            {
                row++;
                if (!string.IsNullOrEmpty(item))
                {
                    var serialNumbr = item.Trim();
                    if (serialNumbr.Length != 15) //ManageStockDevice设备号15位
                    {
                        throw new FaultException<int>(-1, "第[" + row + "]行验证不通过");
                    }
                    else
                    {
                        var device = Logic.Device.GetInstance().Get(serialNumbr);
                        if (device == null || device.Deleted)
                            throw new FaultException<int>(-1, "第[" + row + "]行验证不通过,设备编号[" + serialNumbr + "]并不存在");
                        var dd = DealerDevice.GetInstance().Get(user.DealerId, device.DeviceID);
                        if (dd == null || dd.Status != 0)
                            throw new FaultException<int>(-1, "第[" + row + "]行验证不通过,设备编号[" + serialNumbr + "]并不存在");
                    }
                }
                else
                {
                    throw new FaultException<int>(-1, "第[" + row + "]行验证不通过");
                }
            }

            StringBuilder strError = new StringBuilder();
            row = 0;
            foreach (var item in list)
            {
                row++;
                if (!string.IsNullOrEmpty(item))
                {
                    var serialNumbr = item.Trim();
                    if (serialNumbr.Length != 15) //ManageStockDevice设备号15位
                    {
                        strError.Append("第[" + row + "]行验证不通过\n");
                    }
                    else
                    {
                        var device = Logic.Device.GetInstance().Get(serialNumbr);
                        if (device == null || device.Deleted)
                        {
                            strError.Append("第[" + row + "]行验证不通过,设备编号[" + serialNumbr + "]并不存在\n");
                            continue;
                        }

                        var dd = DealerDevice.GetInstance().Get(user.DealerId, device.DeviceID);
                        if (dd == null || dd.Status != 0)
                        {
                            strError.Append("第[" + row + "]行验证不通过,设备编号[" + serialNumbr + "]并不存在\n");
                            continue;
                        }

                        dd.Status = 1;
                        dd.StockTime = DateTime.Now;
                        DealerDevice.GetInstance().Update(dd);
                    }
                }
                else
                {
                    strError.Append("第[" + row + "]行验证不通过\n");
                }
            }

            if (strError.Length > 0)
            {
                throw new FaultException<int>(-1, strError.ToString());
            }
        }

        public void ManageSalesDeviceList(Guid loginId, int dealerId, string devices)
        {
            var user = ManageCheckLogin(loginId);
            var dealer = Logic.Dealer.GetInstance().Get(user.DealerId);

            if (string.IsNullOrEmpty(devices))
                throw new FaultException<int>(-1, "输入参数错误!");
            if (user.UserType == 1 || dealer == null || dealer.Status == 0)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            if (dealerId != 0)
            {
                var toDealer = Logic.Dealer.GetInstance().Get(dealerId);
                if (toDealer == null ||
                    toDealer.ParentId != dealer.DealerId)
                {
                    throw new FaultException<int>(-1, "没有权限执行该操作!");
                }
            }

            devices = devices.Replace("\r\n", "|");
            devices = devices.Replace("\r", "|");
            devices = devices.Replace("\n", "|");
            devices = devices.Replace("，", ",");
            string[] list = devices.Split('|');
            int row = 0;
            foreach (var item in list)
            {
                row++;
                if (!string.IsNullOrEmpty(item))
                {
                    var serialNumbr = item.Trim();
                    if (serialNumbr.Length != 15) //销售设备号15位
                    {
                        throw new FaultException<int>(-1, "第[" + row + "]行验证不通过");
                    }
                    else
                    {
                        var device = Logic.Device.GetInstance().Get(serialNumbr);
                        if (device == null || device.Deleted)
                            throw new FaultException<int>(-1, "第[" + row + "]行验证不通过,设备编号[" + serialNumbr + "]并不存在");
                        var dd = DealerDevice.GetInstance().Get(user.DealerId, device.DeviceID);
                        if (dd == null || dd.Status > 1)
                            throw new FaultException<int>(-1, "第[" + row + "]行验证不通过,设备编号[" + serialNumbr + "]并不存在");
                    }
                }
                else
                {
                    throw new FaultException<int>(-1, "第[" + row + "]行验证不通过");
                }
            }

            StringBuilder strError = new StringBuilder();
            row = 0;
            foreach (var item in list)
            {
                row++;
                if (!string.IsNullOrEmpty(item))
                {
                    var serialNumbr = item.Trim();
                    if (serialNumbr.Length != 15) //销售设备号15位
                    {
                        strError.Append("第[" + row + "]行验证不通过\n");
                    }
                    else
                    {
                        var device = Logic.Device.GetInstance().Get(serialNumbr);
                        if (device == null || device.Deleted)
                        {
                            strError.Append("第[" + row + "]行验证不通过,设备编号[" + serialNumbr + "]并不存在\n");
                            continue;
                        }

                        var dd = DealerDevice.GetInstance().Get(user.DealerId, device.DeviceID);
                        if (dd == null || dd.Status > 1)
                        {
                            strError.Append("第[" + row + "]行验证不通过,设备编号[" + serialNumbr + "]并不存在\n");
                            continue;
                        }

                        dd.Status = 2;
                        dd.SalesTime = DateTime.Now;
                        dd.Purchaser = dealerId;
                        DealerDevice.GetInstance().Update(dd);
                        if (dealerId != 0)
                        {
                            Model.Entity.DealerDevice ndd = new Model.Entity.DealerDevice
                            {
                                DeviceId = device.DeviceID,
                                DealerId = dealerId,
                                Status = 0,
                                CreateTime = DateTime.Now,
                                UpdateTime = DateTime.Now
                            };
                            DealerDevice.GetInstance().New(ndd);
                        }
                    }
                }
                else
                {
                    strError.Append("第[" + row + "]行验证不通过\n");
                }
            }

            if (strError.Length > 0)
            {
                throw new FaultException<int>(-1, strError.ToString());
            }
        }

        public void ManageSendNotification(Guid loginId, string content)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType == 1)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            if (string.IsNullOrEmpty(content))
                throw new FaultException<int>(-1, "输入参数错误!");

            Model.Entity.Notification notification = new Model.Entity.Notification()
            {
                DeviceID = 0,
                Type = 210,
                Content = content,
                CreateTime = DateTime.Now
            };
            var deviceList = DealerDevice.GetInstance().GetByDealerId(user.DealerId).Values.ToList();
            var dealer = Logic.Dealer.GetInstance().Get(user.DealerId);
            List<User> userList;
            if (dealer.DealerType == 4)
            {
                userList = Logic.User.GetInstance().GetDictionary().Values.ToList();
            }
            else
            {
                var dictUser = new Dictionary<int, User>();
                foreach (var item in deviceList)
                {
                    var userDevicelist = UserDevice.GetInstance().GetUserByDeviceId(item.DeviceId).ToList();
                    foreach (var tUser in userDevicelist)
                    {
                        if (!dictUser.ContainsKey(tUser.UserID))
                        {
                            dictUser.Add(tUser.UserID, tUser);
                        }
                    }
                }

                userList = dictUser.Values.ToList();
            }

            Notification.GetInstance().Save(notification);
            var dealerNotification = new DealerNotification
            {
                DealerId = user.DealerId,
                NotificationID = notification.NotificationID,
                Content = content,
                UserCount = userList.Count
            };
            Logic.DealerNotification.GetInstance().Save(dealerNotification);
            Thread t = new Thread(delegate()
            {
                try
                {
                    Notification.GetInstance().Send(notification, userList);
                    dealerNotification.Status = 1;
                    Logic.DealerNotification.GetInstance().Save(dealerNotification);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    dealerNotification.Status = 2;
                    Logic.DealerNotification.GetInstance().Save(dealerNotification);
                }
            }) {IsBackground = true};
            t.Start();
        }

        public DealerNotificationList ManageGetNotificationList(Guid loginId, int pageindex, int pagesize)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType == 1)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            var result = new DealerNotificationList {PageIndex = pageindex, PageSize = pagesize};
            List<DealerNotification> list;
            var dealer = Logic.Dealer.GetInstance().Get(user.DealerId);
            if (dealer.DealerType == 4)
            {
                list = Logic.DealerNotification.GetInstance().GetList();
            }
            else
            {
                list = Logic.DealerNotification.GetInstance().GetList().Where(w => w.DealerId == user.DealerId).ToList();
            }

            result.Total = list.Count();
            int totalNum = (pageindex - 1) * pagesize;
            result.List = list.Skip(totalNum).Take(pagesize).ToList();
            return result;
        }

        public SystemCount ManageGetSystemCount(Guid loginId)
        {
            var user = ManageCheckLogin(loginId);
            var dealer = Logic.Dealer.GetInstance().Get(user.DealerId);
            if (dealer.DealerType < 3)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            SystemCount count = new SystemCount {List = Logic.Count.GetInstance().GetList()};
            foreach (var item in count.List)
            {
                count.SMS += item.SMS;
                count.LbsAndWifi += item.LbsAndWifi;
                count.Address += item.Address;
                count.LbsAndWifiCache += item.LbsAndWifiCache;
                count.LbsAndWifiFail += item.LbsAndWifiFail;
                count.AddressTotal += item.AddressTotal;
                count.LbsAndWifiTotal += item.LbsAndWifiTotal;
            }

            GetOnlineHandler onlinehandler = OnGetOnlineHandler;
            if (onlinehandler != null)
                count.Online = onlinehandler();

            GetDoLocationQueueCount dolocationqueuehandler = OnGetDoLocationQueueCount;
            GetDoLocationQueueCount dolocationqueuelbswifihandler = OnGetDoLocationQueueLbsWifiCount;

            if (dolocationqueuehandler != null)
                count.DoLocationQueueCount = dolocationqueuehandler();

            if (dolocationqueuelbswifihandler != null)
                count.DoLocationQueueLbsWifiCount = dolocationqueuelbswifihandler();

            GetInsertHistoryQueueCount inserthistoryqueuehandler = OnGetInsertHistoryQueueCount;
            if (inserthistoryqueuehandler != null)
                count.InsertHistoryQueueCount = inserthistoryqueuehandler();

            return count;
        }


        public Project ManageGetProject(Guid loginId, string projectId)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 3)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            var project = Logic.Project.GetInstance().Get(projectId);
            if (project == null)
            {
                throw new FaultException<int>(-1, "该项目并不存在!");
            }

            return project;
        }

        public List<Project> ManageGetProjectList(Guid loginId)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 3)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            return Logic.Project.GetInstance().GetList();
        }

        public void ManageSaveProject(Guid loginId, Project project)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 3)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            if (string.IsNullOrEmpty(project.ProjectId) || string.IsNullOrEmpty(project.Name) || string.IsNullOrEmpty(project.AndroidUrl) || string.IsNullOrEmpty(project.AndroidDescription))
                throw new FaultException<int>(-1, "输入参数错误!");
            Logic.Project.GetInstance().Save(project);
        }

        public void ManageDelProject(Guid loginId, string projectId)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 3)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            Logic.Project.GetInstance().Del(projectId);
        }

        public void ManageUpgrade(Guid loginId, string url)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 3)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            var proc = new Process
            {
                StartInfo = {FileName = Config.GetInstance().Path + "\\YW.Upgrade.exe", Arguments = url},
                EnableRaisingEvents = true
            };
            proc.Start();
            Logger.Debug("启动更新");
        }

        public string ManageGetVersion(Guid loginId)
        {
            var user = ManageCheckLogin(loginId);
            if (user.UserType < 3)
            {
                throw new FaultException<int>(-1, "没有权限执行该操作!");
            }

            return SystemVersion;
        }

        #endregion

        ///自己web后台访问
        public string GetDeviceState(int UserID)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            try
            {
                User user = Logic.User.GetInstance().Get(UserID);

                StringBuilder strState = new StringBuilder();
                List<Device> userDevice = UserDevice.GetInstance().GetDeviceByUserId(user.UserID);
                var time = DateTime.Now;
                int totalDeviceState = 0;
                foreach (var item in userDevice)
                {
                    var deviceState = DeviceState.GetInstance().Get(item.DeviceID);

                    if (totalDeviceState > 0)
                        strState.Append(",");
                    totalDeviceState++;
                    strState.Append("{");
                    strState.Append("\"DeviceID\":\"" + deviceState.DeviceID + "\"");
                    strState.Append(",\"Altitude\":\"" + deviceState.Altitude + "\"");
                    strState.Append(",\"Course\":\"" + deviceState.Course + "\"");
                    strState.Append(",\"LocationType\":\"" + deviceState.LocationType + "\"");
                    strState.Append(",\"CreateTime\":\"" +
                                    deviceState.CreateTime.ToString(TimeFormat,
                                        DateTimeFormatInfo.InvariantInfo) + "\"");
                    strState.Append(",\"DeviceTime\":\"" +
                                    (deviceState.DeviceTime == null
                                        ? ""
                                        : deviceState.DeviceTime.Value.ToString(TimeFormat,
                                            DateTimeFormatInfo.InvariantInfo)) + "\"");
                    strState.Append(",\"Electricity\":\"" + deviceState.Electricity + "\"");
                    strState.Append(",\"GSM\":\"" + deviceState.GSM + "\"");
                    strState.Append(",\"Step\":\"" + deviceState.Step + "\"");
                    strState.Append(",\"Health\":\"" + deviceState.Health + "\"");
                    if (deviceState.Latitude != null && deviceState.Longitude != null)
                    {
                        double lat;
                        double lng;
                        LocationHelper.WGS84ToGCJ((double) deviceState.Latitude.Value,
                            (double) deviceState.Longitude.Value, out lat, out lng);
                        strState.Append(",\"Latitude\":\"" + lat.ToString("F6") + "\"");
                        strState.Append(",\"Longitude\":\"" + lng.ToString("F6") + "\"");
                    }
                    else
                    {
                        strState.Append(",\"Latitude\":\"0\"");
                        strState.Append(",\"Longitude\":\"0\"");
                    }

                    strState.Append(",\"Online\":\"" + (deviceState.Online ? "1" : "0") + "\"");
                    strState.Append(",\"SatelliteNumber\":\"" + deviceState.SatelliteNumber + "\"");
                    strState.Append(",\"ServerTime\":\"" +
                                    (deviceState.ServerTime == null
                                        ? ""
                                        : deviceState.ServerTime.Value.ToString(TimeFormat,
                                            DateTimeFormatInfo.InvariantInfo)) + "\"");
                    strState.Append(",\"Speed\":\"" + deviceState.Speed + "\"");
                    strState.Append(",\"UpdateTime\":\"" +
                                    deviceState.UpdateTime.ToString(TimeFormat,
                                        DateTimeFormatInfo.InvariantInfo) + "\"");
                    strState.Append("}");
                }


                int ticker2 = Environment.TickCount;
                info.set('1', "GetDeviceState", "", 0, ticker2 - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"1\",\"DeviceState\":[" + strState + "]}";
            }
            catch (FaultException<ServiceError> ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "GetDeviceState", "", 0, ticker2 - ticker, -1);
                CPhotoStat.GetInstance().add_info(info);

                return "{ \"Code\":\"" + ex.Detail.Code + "\",\"Message\":\"" + ex.Detail.Message + "\" }";
            }
            catch (Exception ex)
            {
                int ticker2 = Environment.TickCount;
                info.set('1', "GetDeviceState", "", 0, ticker2 - ticker, -2);
                CPhotoStat.GetInstance().add_info(info);

                Logger.Error(ex);
                return "{ \"Code\":\"-2\",\"Message\":\"" + ex.Message + "\" }";
            }
        }

        public string CallDevice(string loginId, int deviceId)
        {
            if (deviceId <= 0)
            {
                return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
            }

            User user = CheckLogin(loginId, deviceId);
            Device device = Logic.Device.GetInstance().Get(deviceId);
            if (device == null)
            {
                return "{\"Code\":\"-1\",\"Message\":\"设备不存在！\"}";
            }
            else if (device.CloudPlatform < 1)
            {
                return JsonConvert.SerializeObject(new RespResult(-1, "非云平台设备，不允许通过此方式拨打电话！"));
            }

            CloudPlatformInfo cpi = Logic.AgentNumber.GetInstance().GetCloudPlatformInfo(device.CloudPlatform);
            if (string.IsNullOrEmpty(cpi.VccId) || string.IsNullOrEmpty(cpi.UrlDial))
            {
                return JsonConvert.SerializeObject(new RespResult(-1, "拨号平台出错！"));
            }

            Model.Entity.DeviceContact dc = DeviceContact.GetInstance().GetByDeviceId(deviceId).First(x => user.UserID == x.ObjectId);
            if (dc == null)
            {
                return JsonConvert.SerializeObject(new RespResult(-1, "请先添加通讯录号码！"));
            }

            CallOutService cr = new CallOutService();
            Service serv = new Service();
            cr.service = serv;
            serv.name = "callRequest";
            serv.messageID = DateTime.Now.Ticks.ToString();
            serv.callingNumber = dc.PhoneNumber;
            serv.calledNumber = device.PhoneNumber;
            serv.callerAreaID = "";
            serv.callerDisplayNum = dc.CallOutNumber;
            serv.calledDisplayNum = dc.AgentNumber;
            serv.vccID = cpi.VccId;
            serv.isRecord = "0";
            serv.chargeNumber = "";

            string time = DateTime.Now.ToString("yyyyMMddHHmmss");
            string token = MD5Helper.MD5Encrypt(cpi.Key + time, null);

            var client = new RestClient(cpi.UrlDial + "?token=" + token.ToLower());
            var request = new RestRequest(Method.POST);
            ITHXmlSerializer xms = new ITHXmlSerializer();
            xms.ContentType = "text/xml; charset=utf-8";
            request.XmlSerializer = xms;
            request.AddXmlBody(cr);
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("正在拨号，向接口提交的数据为：" + xms.Serialize(cr));
            }

            var response = client.Execute(request);

            Logger.Debug("拨号收到的数据为：" + response.Content + ", Exception:" + response.ErrorException);
            Document doc = NSoupClient.Parse(response.Content);
            string messageID = doc.Select("messageID").Text;
            string callID = doc.Select("callID").Text;
            int resultCode = -1;
            try
            {
                resultCode = int.Parse(doc.Select("result").Text);
            }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
            catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
            {
                //
            }

            string reason = doc.Select("reason").Text;
            if (string.IsNullOrEmpty(reason))
            {
                reason = response.Content;
            }

            if (!string.IsNullOrEmpty(reason) && reason.Length > 100)
            {
                reason = reason.Substring(0, 100);
            }

            if (resultCode == 1000)
            {
                RespResult resp = new RespResult(1, "正在拨号中，请等待！");
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("messageID", messageID);
                dic.Add("callID", callID);
                resp.Body = dic;
                return JsonConvert.SerializeObject(resp);
            }
            else
            {
                Logger.Error("拨号失败，MessageID：" + messageID + " ResultCode:" + resultCode + " Reason:" + reason);
            }

            RespResult respResult = new RespResult(-1, "拨号失败！");
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("resultCode", resultCode);
            dict.Add("reason", reason);
            respResult.Body = dict;
            return JsonConvert.SerializeObject(respResult);
        }

        public string CallDeviceCancel(string loginId, int deviceId, string messageID, string callID)
        {
            if (deviceId <= 0)
            {
                return "{\"Code\":\"-1\",\"Message\":\"输入参数错误\"}";
            }

            User user = CheckLogin(loginId, deviceId);
            Device device = Logic.Device.GetInstance().Get(deviceId);
            if (device == null)
            {
                return "{\"Code\":\"-1\",\"Message\":\"设备不存在！\"}";
            }
            else if (device.CloudPlatform < 1)
            {
                return JsonConvert.SerializeObject(new RespResult(-1, "非云平台设备，不允许通过此方式拨打电话！"));
            }

            CloudPlatformInfo cpi = Logic.AgentNumber.GetInstance().GetCloudPlatformInfo(device.CloudPlatform);
            if (string.IsNullOrEmpty(cpi.VccId) || string.IsNullOrEmpty(cpi.UrlDial))
            {
                return JsonConvert.SerializeObject(new RespResult(-1, "拨号平台出错！"));
            }

            CancelRequest cr = new CancelRequest();
            CancelRequestService serv = new CancelRequestService();
            cr.service = serv;
            serv.callID = callID;
            serv.messageID = messageID;
            serv.vccID = cpi.VccId;

            string time = DateTime.Now.ToString("yyyyMMddHHmmss");
            string token = MD5Helper.MD5Encrypt(cpi.Key + time, null);

            var client = new RestClient(cpi.UrlDial + "?token=" + token.ToLower());
            var request = new RestRequest(Method.POST);
            ITHXmlSerializer xms = new ITHXmlSerializer();
            xms.ContentType = "text/xml; charset=utf-8";
            request.XmlSerializer = xms;
            request.AddXmlBody(cr);
            Logger.Debug("正在取消拨号，向接口提交的数据为：" + xms.Serialize(cr));
            var response = client.Execute(request);

            Logger.Debug("取消拨号收到的数据为：" + response.Content + ", Exception:" + response.ErrorException);
            Document doc = NSoupClient.Parse(response.Content);
            string msgId = doc.Select("messageID").Text;
            string caId = doc.Select("callID").Text;
            int resultCode = int.Parse(doc.Select("result").Text);
            string reason = doc.Select("reason").Text;
            if (string.IsNullOrEmpty(reason))
            {
                reason = response.Content;
            }

            if (!string.IsNullOrEmpty(reason) && reason.Length > 100)
            {
                reason = reason.Substring(0, 100);
            }

            if (resultCode == 1000)
            {
                RespResult resp = new RespResult(1, "正在取消拨号");
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("messageID", msgId);
                dic.Add("callID", caId);
                resp.Body = dic;
                return JsonConvert.SerializeObject(resp);
            }
            else
            {
                Logger.Error("取消拨号失败，MessageID：" + msgId + " ResultCode:" + resultCode + " Reason:" + reason);
            }

            RespResult respResult = new RespResult(-1, "取消拨号失败！");
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("resultCode", resultCode);
            dict.Add("reason", reason);
            respResult.Body = dict;
            return JsonConvert.SerializeObject(respResult);
        }

        public string SyncAgentNumberData()
        {
            Logic.AgentNumber.GetInstance().SyncAgentNumbers();
            DeviceContact.GetInstance().SyncAgentNumberData();
            RespResult respResult = new RespResult(1, "同步成功！");
            return JsonConvert.SerializeObject(respResult);
        }

        public string SyncContactsForce()
        {
            var dcs = DeviceContact.GetInstance().GetDic();
            if (dcs == null || dcs.Count == 0)
            {
                return "";
            }

            var itor = dcs.Where(x => x.Value != null && x.Value.AgentNumber != null);
            foreach (var dc in itor)
            {
                var device = Logic.Device.GetInstance().Get(dc.Value.DeviceID);
                if (device == null)
                {
                    continue;
                }

                Logger.Info("正在同步通讯录：" + dc);

                try
                {
                    OnSend?.Invoke(device, SendType.Contact, "");
                    List<User> userList = UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID).ToList();
                    Notification.GetInstance().Send(device.DeviceID, 232, userList);
                }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
                catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
                {
                    //
                }
            }

            return "";
        }

        public string Error(string code)
        {
            return "{ \"Code\":\"-2\",\"Message\":\"" + code + "\" }";
        }
    }
}