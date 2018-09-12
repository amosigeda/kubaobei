using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using YW.Model.Entity;
using YW.Model.Manage;

namespace YW.Contracts
{
    [ServiceContract]
    public interface IClient
    {
        //Code：大于2接口自定义错误 2取不到数据  1正常返回 0登录异常 -1输入错误 -2系统错误 -3越权操作
        //通知类型 Type:1 语音信息，2发信息给管理员关联确认，3管理员确认关联，4管理员拒绝关联, 5设备升级成功 6设备配置已经同步 7设备通讯录已经同步 8设备收到短信 9解除关联 10更新设备信息 100以上的都是报警信息

        #region 登录注册

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name="loginType">登录方式,0,未登录,1 Android,2 IOS,3 Web</param>
        /// <param name="phoneNumber">手机号码</param>
        /// <param name="passWord">密码</param>
        /// <param name="appleId">苹果设备编号</param>
        /// <param name="project">项目标识</param>
        /// <returns></returns>
        [OperationContract]
        string Login(int loginType, string phoneNumber, string passWord, string appleId, string project);

        /// <summary>
        /// 注销登录
        /// </summary>
        /// <param name="loginId">登录编号</param>
        /// <returns></returns>
        [OperationContract]
        string LoginOut(string loginId);

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="loginId">登录编号</param>
        /// <param name="passWord">原密码</param>
        /// <param name="newPassword">新密码</param>
        /// <returns>Code:3表示愿密码错误</returns>
        [OperationContract]
        string ChangePassword(string loginId, string passWord, string newPassword);

        /// <summary>
        /// 注册发送验证码，检查号码是否存在，不存在发送验证码
        /// </summary>
        /// <param name="phoneNumber">手机号码</param>
        /// <param name="project">项目标识</param>
        /// <returns>Code:3表示手机已经存在,4验证码请求过于平凡，需要等待1分钟</returns>
        [OperationContract]
        string RegisterCheck(string phoneNumber, string phoneCode, string project);

        /// <summary>
        /// 注册用户，并登录
        /// </summary>
        /// <param name="phoneNumber">手机号码</param>
        /// <param name="checkNumber">手机收到的验证码</param>
        /// <param name="passWord">密码</param>
        /// <param name="appleId">苹果设备编号</param>
        /// <param name="project">项目标识</param>
        /// <returns>Code:3表示手机已经存在，4表示验证码错误,5验证码超过10分钟有效期</returns>
        [OperationContract]
        string Register(string phoneNumber, string phoneCode, string checkNumber, string passWord, string appleId, string project);

        /// <summary>
        /// 找回密码发送短信验证码
        /// </summary>
        /// <param name="phoneNumber">手机号码</param>
        /// <param name="project">项目标识</param>
        /// <returns>Code:3表示用户并不存在,4验证码请求过于平凡，需要等待1分钟</returns>
        [OperationContract]
        string ForgotCheck(string phoneNumber, string project, string SerialNumber);

        /// <summary>
        /// 重新设置密码，并登录
        /// </summary>
        /// <param name="phoneNumber">手机号码</param>
        /// <param name="checkNumber">手机收到的验证码</param>
        /// <param name="passWord">密码</param>
        /// <param name="appleId">苹果设备编号</param>
        /// <param name="project">项目标识</param>
        /// <returns>Code:3表示手机已经存在，4表示验证码错误,5验证码超过10分钟有效期</returns>
        [OperationContract]
        string Forgot(string phoneNumber, string checkNumber, string passWord, string appleId, string project, string SerialNumber);

        #endregion

        #region 关联设备

        /// <summary>
        /// 检查设备是否被关联
        /// </summary>
        /// <param name="loginId">登录编号</param>
        /// <param name="bindNumber">设备序列号</param>
        /// <returns>Code 1表示未关联，2表示已经被关联</returns>
        [OperationContract]
        string LinkDeviceCheck(string loginId, string bindNumber);

        /// <summary>
        /// 关联设备
        /// </summary>
        /// <param name="loginId">登录编号</param>
        /// <param name="photo">头像</param>
        /// <param name="name">称呼</param>
        /// <param name="bindNumber">设备序列号</param>
        /// <returns></returns>
        [OperationContract]
        string LinkDevice(string loginId, int photo, string name, string bindNumber);

        /// <summary>
        /// 同意用户关联该设备
        /// </summary>
        /// <param name="loginId">登录编号</param>
        /// <param name="deviceId">设备编号</param>
        /// <param name="userId">对方用户编号</param>
        /// <param name="name">名称</param>
        /// <param name="photo">头像</param>
        /// <param name="confirm">是否同意 0表示拒绝,1表示同意</param>
        /// <returns></returns>
        [OperationContract]
        string LinkDeviceConfirm(string loginId, int deviceId, int userId, string name, int photo, int confirm);

        /// <summary>
        /// 解除关联
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [OperationContract]
        string ReleaseBound(string loginId, int deviceId);

        #endregion

        #region 设备信息

        /// <summary>
        /// 获取宝贝资料
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [OperationContract]
        string GetDeviceDetail(string loginId, int deviceId);

        /// <summary>
        /// 获取登录用户的所有设备信息列表
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <returns></returns>
        [OperationContract]
        string GetDeviceList(string loginId);

        /// <summary>
        /// 更新宝贝资料
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="babyName">宝贝名称</param>
        /// <param name="photo">头像</param>
        /// <param name="phoneNumber">电话号码</param>
        /// <param name="phoneCornet">亲情号</param>
        /// <param name="gender">性别</param>
        /// <param name="birthday">生日</param>
        /// <param name="grade">年级</param>
        /// <param name="homeAddress">家庭地址</param>
        /// <param name="schoolAddress">学校地址</param>
        /// <param name="homeLat">家庭维度</param>
        ///
        /// <param name="homeLng">家庭经度</param>
        /// <param name="schoolLat">学校纬度</param>
        /// <param name="schoolLng">学校经度</param>
        /// <param name="latestTime">最晚到家时间</param>
        /// <returns></returns>
        [OperationContract]
        string UpdateDevice(string loginId, int deviceId, string babyName, string photo, string phoneNumber,
            string phoneCornet, int gender, string birthday, int grade, string homeAddress, string schoolAddress,
            string homeLat, string homeLng, string schoolLat, string schoolLng, string latestTime);

        /// <summary>
        /// 更新设备设置
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备编号</param>
        /// <param name="setInfo">设置信息</param>
        /// <param name="classDisable1">上课禁用时间</param>
        /// <param name="classDisable2">下课禁用时间</param>
        /// <param name="weekDisable">禁用星期说明</param>
        /// <param name="timeClose">关机时间</param>
        /// <param name="timeOpen">开机时间</param>
        /// <param name="brightScreen">亮屏时间</param>
        /// <returns></returns>
        [OperationContract]
        string UpdateDeviceSet(string loginId, int deviceId, string setInfo, string classDisable1, string classDisable2,
            string weekDisable, string timeClose, string timeOpen, int brightScreen, string weekAlarm1, string weekAlarm2, string weekAlarm3, string alarm1, string alarm2, string alarm3,
            string locationMode, string locationTime, string flowerNumber, string sleepCalculate, string stepCalculate, string hrCalculate, string sosMsgswitch);

        /// <summary>
        /// 获取设备设置
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [OperationContract]
        string GetDeviceSet(string loginId, int deviceId);

        /// <summary>
        /// 更新短信查询指令
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [OperationContract]
        string UpdateSmsOrder(string loginId, int deviceId, string smsNumber, string smsBalanceKey, string smsFlowKey);

        #endregion

        #region 指令下发

        /// <summary>
        /// 请求更新设备状态
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [OperationContract]
        string RefreshDeviceState(string loginId, int deviceId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="deviceId"></param>
        /// <param name="commandType"></param>
        /// <param name="paramter"></param>
        /// <returns></returns>
        [OperationContract]
        string SendDeviceCommand(string loginId, int deviceId, string commandType, string paramter);

        #endregion

        #region 通讯录

        /// <summary>
        /// 获取设备通讯录
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [OperationContract]
        string GetDeviceContact(string loginId, int deviceId);

        /// <summary>
        /// 删除联系人
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceContactId">通讯录ID</param>
        /// <returns></returns>
        [OperationContract]
        string DeleteContact(string loginId, int deviceContactId);

        /// <summary>
        /// 添加联系人
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="name">与宝贝关系名称</param>
        /// <param name="photo">头像编号</param>
        /// <param name="phoneNum">电话号码</param>
        /// <param name="phoneShort">短号</param>
        /// /// <param name="bindNumber">用户绑定号</param>
        /// <returns></returns>
        [OperationContract]
        string AddContact(string loginId, int deviceId, string name, int photo, string phoneNum, string phoneShort, string bindNumber);

        /// <summary>
        /// 编辑关系和添加亲情号
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="name">与宝贝关系名称</param>
        /// <param name="photo">头像编号</param>
        /// <param name="deviceContactId">通讯录ID</param>
        /// <param name="phoneShort">亲情号</param>
        /// <returns></returns>
        [OperationContract]
        string EditRelation(string loginId, string name, int photo, int deviceContactId, string phoneShort,
            string phoneNumber);

        [OperationContract]
        string EditHeadImg(string loginId, int deviceContactId, string headImg);

        #endregion

        #region 短信

        /// <summary>
        /// 获取短信
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [OperationContract]
        string GetDeviceSMS(string loginId, int deviceId);

        /// <summary>
        /// 保存短信
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="phone">电话号码</param>
        /// <param name="content">短信内容</param>
        /// <returns></returns>
        [OperationContract]
        string SaveDeviceSMS(string loginId, int deviceId, string phone, string content);

        #endregion

        #region 拍照

        /// <summary>
        /// 获取照片
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [OperationContract]
        string GetDevicePhoto(string loginId, int deviceId);

        #endregion

        #region 删除照片

        [OperationContract]
        string DeleteDevicePhoto(string loginId, int DeviceID, string DevicePhotoId);

        #endregion

        #region 语音

        /// <summary>
        /// 获取语音
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备ID</param>
        /// <returns></returns>
        [OperationContract]
        string GetDeviceVoice(string loginId, int deviceId);

        /// <summary>
        /// 发送语音
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="voice">语音数据</param>
        /// <param name="length">录音时间</param>
        /// <returns></returns>
        [OperationContract]
        string SendDeviceVoice(string loginId, int deviceId, string voice, int length, int msgtype);

        #endregion

        #region 历史轨迹

        /// <summary>
        /// 获取历史轨迹
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="deviceId">设备编号</param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [OperationContract]
        string GetDeviceHistory(string loginId, int deviceId, string startTime, string endTime, int pageIndex,
            int pageSize);

        #endregion

        #region 意见反馈

        /// <summary>
        /// 意见反馈
        /// </summary>
        /// <param name="loginId">登录id</param>
        /// <param name="content">内容</param>
        /// <returns></returns>
        [OperationContract]
        string Feedback(string loginId, string content);

        /// <summary>
        /// 获取意见反馈
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <returns></returns>
        [OperationContract]
        string GetFeedback(string loginId);

        /// <summary>
        /// 删除意见反馈
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="feedbackId"></param>
        /// <returns></returns>
        [OperationContract]
        string DeleteFeedback(string loginId, int feedbackId);

        /// <summary>
        /// 意见反馈上传图片
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="feedBackId">意见反馈ID</param>
        /// <param name="img">图片数据</param>
        /// <returns></returns>
        [OperationContract]
        string UpLoadImg(string loginId, int feedBackId, string img);

        #endregion

        #region 用户设置

        /// <summary>
        /// 更新APP消息推送
        /// </summary>
        /// <param name="loginId">登录ID</param>
        /// <param name="notification">是否开启推送（1开，0关）</param>
        /// <param name="notificationSound">是否开启声音（1开，0关）</param>
        /// <param name="notificationVibration">是否开启震动（1开，0关）</param>
        /// <returns></returns>
        [OperationContract]
        string UpdateNotification(string loginId, int notification, int notificationSound, int notificationVibration);

        #endregion

        #region 宝贝好友

        /// <summary>
        /// 去宝贝好友列表
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="deviceId">登录用户绑定的设备ID</param>
        /// <returns></returns>
        [OperationContract]
        string GetBabyFriendList(string loginId, int deviceId);

        [OperationContract]
        string DeleteBabyFriend(string loginId, int DeviceFriendId);

        [OperationContract]
        string UpdateBabyFriendName(string loginId, int DeviceFriendId, string new_name);

        #endregion

        #region 电子围栏

        [OperationContract]
        string GetGeoFenceList(string loginId, int deviceId);

        [OperationContract]
        string SaveGeoFence(string loginId, int geoFenceId, string fenceName, int entry, int exit, int deviceId,
            string latAndLng, int enable);

        [OperationContract]
        string DeleteGeoFence(string loginId, int geoFenceId);

        #endregion

        #region 上学守护

        [OperationContract]
        string SchoolGuardian(string loginId, int deviceId);

        [OperationContract]
        string UpdateGuard(string loginId, int deviceId, int offOn);

        #endregion


        #region 消息系统

        /// <summary>
        /// Android 获取新通知消息接口
        /// </summary>
        /// <param name="loginId">登录编号</param>
        /// <returns></returns>
        [OperationContract]
        string GetNotification(string loginId);

        /// <summary>
        /// 获取新消息记录接口
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [OperationContract]
        string GetMessage(string loginId, int deviceId);

        #endregion

        #region 其他

        /// <summary>
        /// 获取广告
        /// </summary>
        /// <param name="loginId"></param>
        /// <returns></returns>
        [OperationContract]
        string GetAD(string loginId);

        /// <summary>
        /// 收集基站,WIFI位置数据
        /// </summary>
        /// <param name="loginId">登录编号</param>
        /// <param name="mapType">地图类型,1 苹果自带地图，高德地图，谷歌地图,2百度地图</param>
        /// <param name="bts">连接的基站:非CDMA格式(mcc,mnc,lac,cellid,signal),CDMA格式(sid,nid,bid,lon,lat,signal 其中lon,lat可为空)，多个基站用“|”分隔</param>
        /// <param name="wifis">热点信息:mac,signal,ssid，热点信息之间使用 “|”分隔, 请不要包 含移动 wifi 信息.</param>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <param name="radius">定位精度单位米</param>
        /// <returns></returns>
        [OperationContract]
        string WIFILBS(string loginId, int mapType, string bts, string wifis, double lat, double lng, int radius);


        /// <summary>
        /// 检查app的最新版本
        /// </summary>
        /// <param name="loginId"></param>
        /// <returns></returns>
        [OperationContract]
        string CheckAppVersion(string loginId);

        /// <summary>
        /// 异常信息上传
        /// </summary>
        /// <param name="error">异常信息</param>
        /// <returns></returns>
        [OperationContract]
        string ExceptionError(string error);

        /// <summary>
        ///
        /// </summary>
        /// <param name="loginId">登陆编号</param>
        /// <param name="mapType">地图类型,1 苹果自带地图，高德地图，谷歌地图,2百度地图</param>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <returns></returns>
        [OperationContract]
        Task<string> GetAddressAsync(string loginId, int mapType, double lat, double lng);

        #endregion

        #region 后台管理

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="passWord">密码</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        Guid ManageLogin(string userName, string passWord);

        /// <summary>
        /// 注销登录
        /// </summary>
        /// <param name="loginId">登录编号</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageLogOut(Guid loginId);

        /// <summary>
        /// 编辑个人信息
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="name"></param>
        /// <param name="phoneNumber"></param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageUpdataUser(Guid loginId, string name, string phoneNumber);

        /// <summary>
        /// 修改个人密码
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="password"></param>
        /// <param name="newPassword"></param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageUpdataUserPassword(Guid loginId, string password, string newPassword);

        /// <summary>
        /// 获取登陆用户信息
        /// </summary>
        /// <param name="loginId">登陆编号</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        Model.Entity.DealerUser ManageGetLoginUser(Guid loginId);

        /// <summary>
        /// 获取登陆用户代理商信息
        /// </summary>
        /// <param name="loginId">登陆编号</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        Model.Entity.Dealer ManageGetLoginDealer(Guid loginId);

        /// <summary>
        /// 获取代理商列表
        /// </summary>
        /// <param name="loginId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        List<Model.Entity.Dealer> ManageGetDealerList(Guid loginId);

        /// <summary>
        /// 获取单个代理商
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="dealerId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        Model.Entity.Dealer ManageGetDealer(Guid loginId, int dealerId);

        /// <summary>
        /// 添加代理商
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="dealer"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        int ManageAddDealer(Guid loginId, Model.Entity.Dealer dealer);

        /// <summary>
        /// 保存代理商
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="dealer"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        int ManageSaveDealer(Guid loginId, Model.Entity.Dealer dealer);

        /// <summary>
        /// 删除代理商
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="dealerId"></param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageDelDealer(Guid loginId, int dealerId);

        /// <summary>
        /// 获取统计
        /// </summary>
        /// <param name="loginId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        Model.Manage.Count ManageGetCount(Guid loginId);

        /// <summary>
        /// 获取管理用户列表
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="dealerId"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        DealerUserList ManageGetDealerUserList(Guid loginId, int dealerId, int pageindex, int pagesize, string name);

        /// <summary>
        /// 获取单个管理用户
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="dealerUserId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        Model.Entity.DealerUser ManageGetDealerUser(Guid loginId, int dealerUserId);

        /// <summary>
        /// 添加管理用户
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="dealerUser"></param>
        /// <param name="passsword"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        int ManageAddDealerUser(Guid loginId, Model.Entity.DealerUser dealerUser, string passsword);

        /// <summary>
        /// 保存单个管理用户
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="dealerUser"></param>
        /// <param name="passsword"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        int ManageSaveDealerUser(Guid loginId, Model.Entity.DealerUser dealerUser, string passsword);

        /// <summary>
        /// 删除管理用户
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="dealerUserId"></param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageDelDealerUser(Guid loginId, int dealerUserId);

        /// <summary>
        /// 获取管理的设备列表
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <param name="serialNumber"></param>
        /// <param name="model"></param>
        /// <param name="status"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        DealerDeviceList ManageGetDealerDeviceList(Guid loginId, int pageindex, int pagesize, string serialNumber, int? model, int? status, bool? active);

        /// <summary>
        /// 添加中间号
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="platform"></param>
        /// <param name="agentNumbers"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageAgentNumberEdit(Guid loginId, int platform, string agentNumbers);

        /// <summary>
        /// 中间号列表
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <param name="midNumber"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        PageList<AgentNumber> ManageAgentNumberList(Guid loginId, int pageindex, int pagesize, string midNumber, int? platform);

        /// <summary>
        /// 删除中间号
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="number"></param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageAgentNumberDel(Guid loginId, string number);

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="deviceId"></param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageDelDevice(Guid loginId, int deviceId);

        /// <summary>
        /// 重置设置
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="deviceId"></param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageResetDevice(Guid loginId, int deviceId);

        /// <summary>
        /// 入库设备
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="deviceId"></param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageStockDevice(Guid loginId, int deviceId);

        /// <summary>
        /// 批量添加设备
        /// </summary>
        /// <param name="loginId">登陆编号</param>
        /// <param name="devices">设备列表，一行一个，格式：“设备号,绑定号”</param>
        /// <param name="model">功能</param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageAddDeviceList(Guid loginId, string devices, int model, string deviceNote, int deviceType, int cloudPlatform);

        /// <summary>
        /// 批量入库设备
        /// </summary>
        /// <param name="loginId">登陆编号</param>
        /// <param name="devices">设备列表，一行一个，格式：“设备号”</param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageStockDeviceList(Guid loginId, string devices);

        /// <summary>
        /// 批量销售设备
        /// </summary>
        /// <param name="loginId">登陆编号</param>
        /// <param name="dealerId">销售对象</param>
        /// <param name="devices">设备列表，一行一个，格式：“设备号”</param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageSalesDeviceList(Guid loginId, int dealerId, string devices);

        /// <summary>
        /// 获取单个设备信息
        /// </summary>
        /// <param name="loginId">登陆编号</param>
        /// <param name="deviceId">设备编号</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        Model.Entity.Device ManageGetDevice(Guid loginId, int deviceId);

        /// <summary>
        /// 更新单个设备信息
        /// </summary>
        /// <param name="loginId">登陆编号</param>
        /// <param name="deviceId">设备编号</param>
        /// <param name="model">功能</param>
        /// <param name="bindNumber">绑定号</param>
        /// <param name="phoneNumber">手机号</param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageUpdateDevice(Guid loginId, int deviceId, int model, string bindNumber, string phoneNumber, string deviceNote, int deviceType);

        /// <summary>
        /// 推送消息
        /// </summary>
        /// <param name="loginId">登陆编号</param>
        /// <param name="content">消息内容</param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageSendNotification(Guid loginId, string content);

        /// <summary>
        /// 获取发送记录
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        DealerNotificationList ManageGetNotificationList(Guid loginId, int pageindex, int pagesize);

        /// <summary>
        /// 获取项目列表
        /// </summary>
        /// <param name="loginId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        List<Model.Entity.Project> ManageGetProjectList(Guid loginId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        Model.Entity.Project ManageGetProject(Guid loginId, string projectId);

        /// <summary>
        /// 保存项目
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="project"></param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageSaveProject(Guid loginId, Model.Entity.Project project);

        /// <summary>
        /// 删除项目
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="projectId"></param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageDelProject(Guid loginId, string projectId);

        /// <summary>
        /// 获取系统统计数据
        /// </summary>
        /// <param name="loginId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        SystemCount ManageGetSystemCount(Guid loginId);

        /// <summary>
        /// 升级本系统
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="url"></param>
        [OperationContract]
        [FaultContract(typeof(int))]
        void ManageUpgrade(Guid loginId, string url);

        /// <summary>
        /// 获取系统版本
        /// </summary>
        /// <param name="loginId"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(int))]
        string ManageGetVersion(Guid loginId);

        #endregion

        [OperationContract]
        string GetDeviceState(int UserID);

        /// <summary>
        /// 拨号
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [OperationContract]
        string CallDevice(string loginId,int deviceId);

        /// <summary>
        /// 取消拨号
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="callID"></param>
        /// <returns></returns>
        [OperationContract]
        string CallDeviceCancel(string loginId, int deviceId, string messageID,string callID);

        /// <summary>
        /// 同步中间号
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string SyncAgentNumberData();

        /// <summary>
        /// 同步通讯录
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string SyncContactsForce();

        /// <summary>
        /// 出错处理
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [OperationContract]
        string Error(string code);
    }
}