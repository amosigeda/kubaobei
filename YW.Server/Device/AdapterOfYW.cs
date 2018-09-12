using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using YW.Data;
using YW.Logic;
using YW.Model;
using YW.Model.Entity;
using YW.Server.Socket;
using YW.Utility;
using YW.Utility.Stat;
using YW.WCF;
using Convert = System.Convert;
using Count = YW.Logic.Count;
using DeviceContact = YW.Model.Entity.DeviceContact;
using DeviceException = YW.Model.Entity.DeviceException;
using DeviceFriend = YW.Model.Entity.DeviceFriend;
using DevicePhoto = YW.Model.Entity.DevicePhoto;
using DeviceSet = YW.Model.Entity.DeviceSet;
using DeviceSMS = YW.Model.Entity.DeviceSMS;
using DeviceState = YW.Logic.DeviceState;
using DeviceVoice = YW.Model.Entity.DeviceVoice;
using Notification = YW.Logic.Notification;
using User = YW.Model.Entity.User;
using UserDevice = YW.Logic.UserDevice;

namespace YW.Server.Device
{
    public class AdapterOfYW
    {
        public AdapterFactory.LocationHandler OnLocation;
        private readonly AdapterFactory.SendTcpHandler _sendTcpHandler;

        ///send data to himself
        public static event Client.SendHandler OnSend;

        public AdapterFactory.LocationHandler OnLocationLbsWifi;
        public AdapterFactory.LocationHandler OnLocationGaode;

        private int PackageSize = int.Parse(AppConfig.GetValue("PackageSize"));
        private readonly Dictionary<int, MySAE> _dictDevice;
        private readonly string _apiKey;
        private int _timeZone = 480;

        public AdapterOfYW(AdapterFactory.SendTcpHandler sendTcpHandler, Client.SendHandler send)
        {
            _apiKey = AppConfig.GetValue("ApiKey");
            _sendTcpHandler = sendTcpHandler;
            OnSend = send;
            _dictDevice = new Dictionary<int, MySAE>();
        }

        public bool Initialize(MySAE mySae, byte[] bytes, int startOffset, int length, ref int msgLength)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();
            Model.Entity.Device device = null;
            try
            {
                if (length < 30 ||
                    bytes[startOffset] != 0x5B ||
                    bytes[startOffset + 1] != 0x59 ||
                    bytes[startOffset + 2] != 0x57 ||
                    bytes[startOffset + 3] != 0x2A)
                {
                    info.set('1', "Initialize", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return false;
                }

                int contentlength = Convert.ToInt32(Encoding.UTF8.GetString(bytes, startOffset + 25, 4), 16);
                msgLength = contentlength + 30 + 1;
                if (msgLength > length || bytes[startOffset + msgLength - 1] != 0x5D)
                {
                    msgLength = 0;
                    info.set('1', "Initialize", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);
                    return false;
                }

                string data = "";
                string message = "";
                //TK
                if (contentlength > 7 && bytes[startOffset + 30] == 0x54 && bytes[startOffset + 31] == 0x4B)
                {
                    int endOffset = startOffset + 31;
                    int remainder = 5;
                    while (remainder > 0 && endOffset < (startOffset + msgLength - 1))
                    {
                        endOffset++;
                        if (bytes[endOffset] == 0x2C)
                        {
                            remainder--;
                        }
                    }

                    //设备发送语音包，5个逗号；设备发送语音确认包4个逗号
                    if (remainder > 0)
                    {
                        data = Encoding.UTF8.GetString(bytes, startOffset, msgLength);
                    }
                    else
                    {
                        data = Encoding.UTF8.GetString(bytes, startOffset, endOffset - startOffset + 1);
                    }
                }
                //TPBK 图片
                else if (contentlength > 7 && bytes[startOffset + 30] == 0x54 && bytes[startOffset + 31] == 0x50 && bytes[startOffset + 32] == 0x42 && bytes[startOffset + 33] == 0x4B)
                {
                    int endOffset = startOffset + 33;
                    int remainder = 5;
                    bool location = false;
                    while (remainder > 0 && endOffset < (startOffset + msgLength))
                    {
                        endOffset++;
                        if (bytes[endOffset] == 0x2C)
                        {
                            remainder--;
                        }
                        else if (remainder == 2 && bytes[endOffset] == 0x30 && bytes[endOffset - 1] == 0x2C && bytes[endOffset + 1] == 0x2C)
                        {
                            location = true;
                        }
                    }

                    if (remainder > 0)
                    {
                        info.set('1', "Initialize", "", 0, Environment.TickCount - ticker, -1);
                        CPhotoStat.GetInstance().add_info(info);
                        return false;
                    }

                    if (!location)
                        data = Encoding.UTF8.GetString(bytes, startOffset, endOffset - startOffset + 1);
                    else
                    {
                        endOffset = startOffset + msgLength;
                        int index = 28 + startOffset;
                        int comma = 25;
                        int commaIndex = 0;
                        int lbsLength = 0;
                        int wifiLength = 0;
                        while (commaIndex <= comma && index < endOffset)
                        {
                            index++;
                            if (bytes[index] == 0x2C)
                            {
                                commaIndex++;
                                if (commaIndex == 22)
                                {
                                    comma += lbsLength * 3;
                                }
                            }
                            else if (commaIndex == 21)
                            {
                                lbsLength = lbsLength * 10 + int.Parse(Encoding.UTF8.GetString(bytes, index, 1));
                            }
                            else if (commaIndex == comma)
                            {
                                if (bytes[index] != 0x5D)
                                    wifiLength = wifiLength * 10 + int.Parse(Encoding.UTF8.GetString(bytes, index, 1));
                                else
                                    break;
                            }
                        }

                        StringBuilder dataStr = new StringBuilder();
                        dataStr.Append(Encoding.UTF8.GetString(bytes, startOffset, index - startOffset));
                        if (wifiLength > 0)
                        {
                            while (wifiLength > 0)
                            {
                                wifiLength--;
                                comma = 3;
                                int beginIndex = index;
                                int ssidEnd = 0;
                                while (comma > 0 && index < endOffset)
                                {
                                    index++;
                                    if (bytes[index] == 0x2C) //如果SSID 中包含逗号，会导致系统异常
                                    {
                                        comma--;
                                        if (comma == 2)
                                            ssidEnd = index;
                                    }
                                }

                                if (EncodingType.BeUtf8(bytes, beginIndex + 1, ssidEnd - beginIndex - 1))
                                    dataStr.Append(Encoding.UTF8.GetString(bytes, beginIndex, index - beginIndex));
                                else
                                    dataStr.Append(Encoding.Default.GetString(bytes, beginIndex, index - beginIndex));
                            }
                        }

                        data = dataStr.ToString();
                    }
                }
                else
                {
                    int endOffset = startOffset + msgLength;
                    //如果是UD/AL 且Wifi数量不等于0
                    if (contentlength > 7 && ((bytes[startOffset + 30] == 0x55 && bytes[startOffset + 31] == 0x44) ||
                                              (bytes[startOffset + 30] == 0x41 && bytes[startOffset + 31] == 0x4C)) &&
                        bytes[startOffset + msgLength - 2] != 0x30)
                    {
                        int index = 26 + startOffset;
                        int comma = 21;
                        int commaIndex = 0;
                        int lbsLength = 0;
                        int wifiLength = 0;
                        while (commaIndex <= comma && index < endOffset)
                        {
                            index++;
                            if (bytes[index] == 0x2C)
                            {
                                commaIndex++;
                                if (commaIndex == 18)
                                {
                                    comma += lbsLength * 3;
                                }
                            }
                            else if (commaIndex == 17)
                            {
                                lbsLength = lbsLength * 10 + int.Parse(Encoding.UTF8.GetString(bytes, index, 1));
                            }
                            else if (commaIndex == comma)
                            {
                                if (bytes[index] != 0x5D)
                                    wifiLength = wifiLength * 10 + int.Parse(Encoding.UTF8.GetString(bytes, index, 1));
                                else
                                    break;
                            }
                        }

                        StringBuilder dataStr = new StringBuilder();
                        dataStr.Append(Encoding.UTF8.GetString(bytes, startOffset, index - startOffset));
                        if (wifiLength > 0)
                        {
                            while (wifiLength > 0)
                            {
                                wifiLength--;
                                comma = 3;
                                int beginIndex = index;
                                int ssidEnd = 0;
                                while (comma > 0 && index < endOffset)
                                {
                                    index++;
                                    if (bytes[index] == 0x2C) //如果SSID 中包含逗号，会导致系统异常
                                    {
                                        comma--;
                                        if (comma == 2)
                                            ssidEnd = index;
                                    }
                                }

                                if (EncodingType.BeUtf8(bytes, beginIndex + 1, ssidEnd - beginIndex - 1))
                                    dataStr.Append(Encoding.UTF8.GetString(bytes, beginIndex, index - beginIndex));
                                else
                                    dataStr.Append(Encoding.Default.GetString(bytes, beginIndex, index - beginIndex));
                            }
                        }

                        data = dataStr.ToString();
                    }
                    else
                    {
                        data = Encoding.UTF8.GetString(bytes, startOffset, msgLength);
                    }
                }

                if (data[data.Length - 1] == ']')
                {
                    data = data.Remove(data.Length - 1, 1);
                }

                string[] items = data.Split(new char[] {'*'}, 5);
                string indexNo = items[2];
                string dataContent = items[4];
                string sn = items[1];

                string commandType = dataContent;
                string[] contentArr = null;
                if (commandType.IndexOf(",", StringComparison.Ordinal) > 0)
                {
                    contentArr = dataContent.Split(',');
                    commandType = contentArr[0];
                }

                device = Logic.Device.GetInstance().Get(sn);
                if (device != null)
                {
                    mySae.DeviceState = DeviceState.GetInstance().Get(device.DeviceID);
                    mySae.DeviceType = DeviceType.YW;
                    lock (mySae.DeviceState)
                    {
                        mySae.DeviceState.SocketId = mySae.SocketId;
                        mySae.DeviceState.Online = true;
                        mySae.DeviceState.CreateTime = DateTime.Now;
                    }
                }

                //var addr = APIClient.GetInstance().GetAddress(31.9335420332784, 104.585046605214);

                if (device == null || device.Deleted)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        if (device == null)
                        {
                            Logger.Debug("设备未注册：" + sn);
                        }

                        Logger.Debug("移除设备后，断开已连接设备！");
                    }

//                    message = "[YW*" + sn + "*" + indexNo + "*0006*INIT,0]";
//                    byte[] bts = Encoding.UTF8.GetBytes(message);
//                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

                    mySae.DisconnectHandler(mySae);

                    info.set('1', "Initialize", "", 0, Environment.TickCount - ticker, 0);
                    CPhotoStat.GetInstance().add_info(info);

                    return true;
                }

                if (mySae.DeviceState == null)
                {
                    lock (_dictDevice)
                    {
                        if (_dictDevice.ContainsKey(device.DeviceID))
                        {
                            try
                            {
                                if (_dictDevice[device.DeviceID].DeviceState == mySae.DeviceState)
                                {
                                    if (Logger.IsDebugEnabled)
                                    {
                                        Logger.Debug("同用户登录，踢掉上一处登录！");
                                    }

                                    _dictDevice[device.DeviceID].DisconnectHandler(_dictDevice[device.DeviceID]);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);
                            }

                            _dictDevice.Remove(device.DeviceID);
                        }

                        _dictDevice.Add(device.DeviceID, mySae);
                    }
                }

                if ((device.State != 1) && (device.UserId > 0))
                {
                    if (device.State == 0)
                    {
                        device.ActiveDate = DateTime.Now;
                    }

                    device.State = 1;
                    Logic.Device.GetInstance().Save(device);
                }

                if (device.Deviceflag == 1)
                {
                    device.Deviceflag = 0;
                    Logic.Device.GetInstance().Save(device);

                    //this.SendSet(mySae, device);
                    //this.SendContact(mySae, device);
                    this.SendDeviceRecovery(mySae, device, "1");
                }

                mySae.DeviceState.UpdateTime = DateTime.Now;

                if (commandType.Equals("INIT"))
                {
                    #region 初始化包

                    //[YW*8800000015*0001*0002*INIT,13256122653,0,G29_BASE_V1.00_2015.04,0002,2300]运营商类型:0表示移动、1表示联通、2表示电信
                    int replyState = 0;
                    string devicePhone = "";
                    int operatorType = 0;
                    string currentFirmware = "";
                    int setVersionNo = 0;
                    int contactVersionNo = 0;
                    bool deviceSave = false;
                    try
                    {
                        devicePhone = contentArr[1]; //设备电话
                        operatorType = Convert.ToInt32(contentArr[2]); //运营商类型
                        currentFirmware = contentArr[3]; //设备固件版本号
                        setVersionNo = Utility.Convert.HexToInt(contentArr[4]);
                        contactVersionNo = Utility.Convert.HexToInt(contentArr[5]);
                        if (contentArr.Length > 6 && contentArr[6] != null && contentArr[6].Length == 1)
                        {
                            try
                            {
                                device.LengthCountType = int.Parse(contentArr[6]);
                            }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
                            catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
                            {
                                //
                            }
                        }

                        if (device.Firmware == null || (currentFirmware != null && !currentFirmware.Equals(device.CurrentFirmware)))
                        {
                            device.CurrentFirmware = currentFirmware;
                            deviceSave = true;
                        }

                        //if (!string.IsNullOrEmpty(device.PhoneNumber))//设备暂时不会传回电话号码
                        //{
                        //    device.PhoneNumber = devicePhone;
                        //    deviceSave = true;
                        //}
                        //if (device.UserId > 0) //必须要被绑定的设备
                        replyState = 1;
                    }
                    catch (Exception ex)
                    {
                        info.set('1', "Initialize", "", 0, Environment.TickCount - ticker, -2);
                        CPhotoStat.GetInstance().add_info(info);

                        Logger.Error(ex);
                    }

                    try
                    {
                        message = "[YW*" + sn + "*" + indexNo + "*0006*INIT," + replyState + "]";
                        byte[] bts = Encoding.UTF8.GetBytes(message);
                        _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);
                        DateTime now = DateTime.Now.AddMinutes(-_timeZone);
                        string returnStr = "LK," + now.ToString("yyyy-MM-dd") + "," + now.ToString("HH:mm:ss");
                        int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(returnStr).Length : returnStr.Length;
                        message = "[YW*" + sn + "*" + indexNo + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + returnStr + "]";
                        bts = Encoding.UTF8.GetBytes(message);
                        _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);
                        if (replyState == 1)
                        {
                            if (device.OperatorType != operatorType)
                            {
                                device.OperatorType = operatorType;
                                deviceSave = true;
                            }

                            if (device.Firmware != currentFirmware)
                            {
                                this.SendFirmware(mySae, device);
                                //下发版本升级
                            }

                            if (device.SetVersionNO != setVersionNo)
                            {
                                //下发配置信息[YW*YYYYYYYYYY*NNNN*LEN*SET,设置次数流水号,设置项,上课禁用时间段,定时开机时间,定时关机时间,亮屏时间]
                                this.SendSet(mySae, device);
                            }

                            if (device.ContactVersionNO != contactVersionNo)
                            {
                                this.SendContact(mySae, device);
                                //下发联系人
                            }

                            this.SendSMS(mySae, device);
                            //this.SendVoice(mySae, device);
                            if (string.IsNullOrEmpty(device.SmsNumber) && string.IsNullOrEmpty(device.SmsBalanceKey) && string.IsNullOrEmpty(device.SmsFlowKey))
                            {
                                switch (operatorType)
                                {
                                    case 1:
                                        device.SmsNumber = "10086";
                                        device.SmsBalanceKey = "101";
                                        device.SmsFlowKey = "401";
                                        deviceSave = true;
                                        break;
                                    case 2:
                                        device.SmsNumber = "10010";
                                        device.SmsBalanceKey = "102";
                                        device.SmsFlowKey = "CXLLB";
                                        deviceSave = true;
                                        break;
                                    case 3:
                                        device.SmsNumber = "";
                                        device.SmsBalanceKey = "";
                                        device.SmsFlowKey = "";
                                        break;
                                }
                            }

                            if (deviceSave)
                            {
                                Logic.Device.GetInstance().Save(device);
                                List<User> userList = UserDevice.GetInstance() .GetUserByDeviceId(device.DeviceID) .ToList();
                                Notification.GetInstance().Send(device.DeviceID, 230, userList);
                            }

                            this.SendFriendListNotity(mySae, device, "0");
                        }

                        info.set('1', "init", "", 0, Environment.TickCount - ticker, 1);
                        CPhotoStat.GetInstance().add_info(info);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        info.set('1', "init", "", 0, Environment.TickCount - ticker, -3);
                        CPhotoStat.GetInstance().add_info(info);

                        Logger.Error(ex);
                    }

                    #endregion
                }
                else if (commandType.Equals("LK"))
                {
                    #region 心跳包

                    //[YW*8800000015*0002*LK]
                    //device.el = contactVersionNO;
                    //,2015-08-18,14:30:30
                    var ds = mySae.DeviceState;
                    if (contentArr != null)
                    {
                        ds.Electricity = int.Parse(contentArr[1]);
                    }

                    DateTime now = DateTime.Now.AddMinutes(-_timeZone);
                    string returnStr = "LK," + now.ToString("yyyy-MM-dd") + "," + now.ToString("HH:mm:ss");
                    int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(returnStr).Length : returnStr.Length;
                    message = "[YW*" + sn + "*" + indexNo + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + returnStr + "]";
                    byte[] bts = Encoding.UTF8.GetBytes(message);

                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

                    if (!ds.IsLowPowerAlarmed && ds.Electricity < 20)
                    {
                        ds.IsLowPowerAlarmed = true;
                        var sendUserList = UserDevice.GetInstance().GetUserByDeviceId(ds.DeviceID);
                        var exception = new DeviceException
                        {
                            Type = 104,
                            DeviceID = ds.DeviceID,
                            Content = "",
                            Latitude = ds.Latitude ?? 0,
                            Longitude = ds.Longitude ?? 0,
                            CreateTime = ds.CreateTime
                        };
                        Logic.DeviceException.GetInstance().Save(exception);
                        Notification.GetInstance().Send(exception, sendUserList);
                    }
                    else if (ds.Electricity >= 20)
                    {
                        ds.IsLowPowerAlarmed = false;
                    }

                    info.set('1', "LK", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    #endregion
                }
                else if (commandType.Equals("IPREQ"))
                {
                    #region IPADDR

                    string returnStr;
                    string serverIP = AppConfig.GetValue("ServerIP");
                    string serverPort = AppConfig.GetValue("ServerPort");
                    if (!string.IsNullOrEmpty(serverIP))
                    {
                        returnStr = "IPREQ," + "1," + serverIP + "," + serverPort;
                    }
                    else
                    {
                        returnStr = "IPREQ," + "1," + "47.91.149.94," + "8899";
                    }

                    //string returnStr = "IPREQ," + "1," + "192.168.10.34," + "8899";
                    int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(returnStr).Length : returnStr.Length;
                    message = "[YW*" + sn + "*" + indexNo + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + returnStr + "]";
                    byte[] bts = Encoding.UTF8.GetBytes(message);

                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

                    info.set('1', "IPREQ", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    #endregion
                }
                else if (commandType.Equals("UD") || commandType.Equals("AL") || commandType.Equals("UD2") || commandType.Equals("TQ") || commandType.Equals("UD3"))
                {
                    #region GPS+LBS+报警+回复

                    if (commandType.Equals("AL"))
                    {
                        int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(commandType).Length : commandType.Length;
                        message = "[YW*" + sn + "*" + indexNo + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + commandType + "]";
                        byte[] bts = Encoding.UTF8.GetBytes(message);
                        _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

                        info.set('1', "AL", "", 0, Environment.TickCount - ticker, 1);
                        CPhotoStat.GetInstance().add_info(info);
                    }

                    var location = GetLocation(mySae, contentArr, 0);

                    //定位请求源,0表示未定义源，1表示协议命令UD正常定位请求源，2表示协议命令UD2补偿定位请求源，3表示协议命令TQ天气预报定位请求源，4表示协议命令TPBK远程拍照相片定位请求源, 5表示协议命令AL报警数据定位请求源

                    if (commandType.Equals("TQ"))
                    {
                        mySae.DeviceState.LocationSource = 3;
                    }


                    mySae.DeviceState.SatelliteNumber = int.Parse(contentArr[11]);
                    mySae.DeviceState.GSM = int.Parse(contentArr[12]);
                    mySae.DeviceState.Electricity = int.Parse(contentArr[13]);
                    mySae.DeviceState.Step = contentArr[14];
                    mySae.DeviceState.Health = contentArr[15];

                    if (commandType.Equals("UD3"))
                    {
                        this.OnLocationGaode(location);
                    }
                    else
                        this.OnLocation(location);

                    info.set('1', commandType, "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);
                    return true;

                    #endregion
                }
                else if (commandType.Equals("DWREQ"))
                {
                    #region 设备短信取位置

                    var location = GetLocation(mySae, contentArr, 0);
                    //6，定位来源，短信定位源
                    mySae.DeviceState.LocationSource = 6;
                    mySae.DeviceState.SatelliteNumber = int.Parse(contentArr[11]);
                    mySae.DeviceState.GSM = int.Parse(contentArr[12]);
                    mySae.DeviceState.Electricity = int.Parse(contentArr[13]);
                    mySae.DeviceState.Step = contentArr[14];
                    mySae.DeviceState.Health = contentArr[15];
                    this.OnLocationLbsWifi(location);

                    info.set('1', "DWREQ", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);
                    return true;

                    #endregion
                }
                else if (commandType.Equals("COST2"))
                {
                    #region 设备返回短信查询内容

                    string[] itemArr = dataContent.Split(',');
                    string phoneNum = itemArr[1];
                    int douHao = 32 + itemArr[0].Length + itemArr[1].Length;

                    string smsContent = Encoding.UTF8.GetString(bytes, startOffset + douHao, contentlength + 31 - douHao);
                    DeviceSMS deviceSms = new DeviceSMS
                    {
                        DeviceID = mySae.DeviceState.DeviceID,
                        Type = 2,
                        State = 2,
                        Phone = phoneNum,
                        SMS = smsContent,
                        CreateTime = DateTime.Now
                    };
                    Logic.DeviceSMS.GetInstance().Save(deviceSms);
                    message = "[YW*" + sn + "*" + indexNo + "*0007*COST2,1]";
                    byte[] bts = Encoding.UTF8.GetBytes(message);
                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);
                    List<User> userList = UserDevice.GetInstance()
                        .GetUserByDeviceId(device.DeviceID);
                    Notification.GetInstance().Send(deviceSms, userList);

                    info.set('1', "COST2", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);
                    return true;

                    #endregion
                }
                else if (commandType.Equals("TPBK"))
                {
                    #region TPBK

                    var fileName = contentArr[2];
                    int currentPackage = int.Parse(contentArr[3]);
                    int totalPackage = int.Parse(contentArr[4]);
                    string filePath = Config.GetInstance().Path + "\\Upload\\DCIM\\" + device.DeviceID + "\\";
                    DirectoryInfo di = new DirectoryInfo(filePath);
                    if (!di.Exists)
                    {
                        di.Create();
                    }

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        int extNameIdx = fileName.LastIndexOf(".");
                        if (extNameIdx > 0)
                        {
                            fileName = fileName.Substring(0, extNameIdx + 1) + "jpg";
                        }
                    }

                    if (currentPackage == 0)
                    {
                        var location = GetLocation(mySae, contentArr, 4);
                        try
                        {
//                            var lbswifi = Logic.LBSWIFIClient.Get() .GetPosition(_apiKey, location.LBS, location.WIFI);
                            var lbswifi = APIClient.GetInstance().GetPosition(location.LBS, location.WIFI);
                            //-1 验证失败,0 表示未获取到信息,1 wifi 数据库,2 wifi接口,11 lbs数据库 12 lbs接口
                            if (lbswifi != null && lbswifi.Code > 0)
                            {
                                location.Lat = lbswifi.Lat;
                                location.Lng = lbswifi.Lng;
                                location.Radius = lbswifi.Radius;
                                if (lbswifi.Code == 1 || lbswifi.Code == 2)
                                    location.LocationType = 3;
                                else if (lbswifi.Code == 11 || lbswifi.Code == 12)
                                    location.LocationType = 2;
                                Count.GetInstance().LbsAndWifi();
                            }
                        }
                        catch (Exception ex)
                        {
                            info.set('1', "TPBK", "", 0, Environment.TickCount - ticker, -1);
                            CPhotoStat.GetInstance().add_info(info);

                            Logger.Error(ex);
                            Logger.Error("LBS:" + location.LBS + "     WIFI:" + location.WIFI);
                        }

                        mySae.DeviceState.SatelliteNumber = int.Parse(contentArr[15]);
                        mySae.DeviceState.GSM = int.Parse(contentArr[16]);
                        mySae.DeviceState.Electricity = int.Parse(contentArr[17]);
                        this.OnLocation(location);
                        DevicePhoto photo = Logic.DevicePhoto.GetInstance().GetByDeviceIdAndMark(device.DeviceID, fileName);
                        if (photo == null)
                        {
                            photo = new DevicePhoto
                            {
                                DeviceID = device.DeviceID,
                                Source =
                                    string.IsNullOrEmpty(contentArr[1]) || contentArr[1].Equals("0")
                                        ? null
                                        : contentArr[1],
                                DeviceTime = location.Time,
                                Latitude = location.Lat,
                                Longitude = location.Lng,
                                State = 0,
                                Path = "DCIM/" + device.DeviceID + "/" + fileName,
                                Mark = fileName,
                                TotalPackage = totalPackage,
                                CurrentPackage = 0,
                                CreateTime = DateTime.Now,
                                UpdateTime = DateTime.Now
                            };
                            Logic.DevicePhoto.GetInstance().Save(photo);
                        }
                        else
                        {
                            photo.DeviceTime = location.Time;
                            photo.Latitude = location.Lat;
                            photo.Longitude = location.Lng;
                            photo.UpdateTime = DateTime.Now;
                            Logic.DevicePhoto.GetInstance().Save(photo);
                            if (photo.CurrentPackage == photo.TotalPackage)
                            {
                                List<User> userList =
                                    UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID);
                                Notification.GetInstance().Send(photo, userList);
                            }
                        }
                    }
                    else
                    {
                        DevicePhoto photo = Logic.DevicePhoto.GetInstance().GetByDeviceIdAndMark(device.DeviceID, fileName);
                        FileStream fs = new FileStream(filePath + fileName, FileMode.OpenOrCreate, FileAccess.Write);
                        int startByte = (currentPackage - 1) * PackageSize;
                        fs.Seek(startByte, SeekOrigin.Begin);
                        fs.Write(bytes, startOffset + data.Length, msgLength - data.Length - 1);
                        fs.Close();
                        fs.Dispose();
                        if (photo == null)
                        {
                            photo = new DevicePhoto
                            {
                                DeviceID = device.DeviceID,
                                Source = string.IsNullOrEmpty(contentArr[1]) || contentArr[1].Equals("0") ? null : contentArr[1],
                                State = 0,
                                Path = "DCIM/" + device.DeviceID + "/" + fileName,
                                Mark = fileName,
                                TotalPackage = totalPackage,
                                CurrentPackage = 0,
                                CreateTime = DateTime.Now,
                                UpdateTime = DateTime.Now
                            };
                        }

                        if (totalPackage > currentPackage)
                        {
                            photo.TotalPackage = totalPackage;
                            photo.CurrentPackage = currentPackage;
                            photo.State = 0;
                            Logic.DevicePhoto.GetInstance().Save(photo);
                        }
                        else
                        {
                            photo.TotalPackage = totalPackage;
                            photo.CurrentPackage = currentPackage;
                            photo.State = 1;
                            Logic.DevicePhoto.GetInstance().Save(photo);
                            if (photo.DeviceTime != null)
                            {
                                List<User> userList = UserDevice.GetInstance()
                                    .GetUserByDeviceId(device.DeviceID);
                                Notification.GetInstance().Send(photo, userList);
                            }
                        }
                    }

                    string str = "TPCF," + fileName + "," + currentPackage + "," + totalPackage + ",1";
                    int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(str).Length : str.Length;
                    message = "[YW*" + sn + "*" + indexNo + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + str + "]";
                    byte[] bts = Encoding.UTF8.GetBytes(message);
                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

                    info.set('1', "TPBK", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    #endregion
                }
                else if (commandType.Equals("GUARD"))
                {
                    #region GUARD

                    var opt = int.Parse(contentArr[1]);
                    var fileName = contentArr[2];
                    int totalByteLength = int.Parse(contentArr[3]);
                    int currentPackage = int.Parse(contentArr[4]);
                    int totalPackage = int.Parse(contentArr[5]);
                    string filePath = Config.GetInstance().Path + "\\Upload\\GUARD\\" + device.DeviceID + "\\";
                    DirectoryInfo di = new DirectoryInfo(filePath);
                    if (!di.Exists)
                    {
                        di.Create();
                    }

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        int extNameIdx = fileName.LastIndexOf(".");
                        if (extNameIdx > 0)
                        {
                            string ext;
                            if (opt == 1)
                            {
                                ext = "amr";
                            }
                            else if (opt == 2)
                            {
                                ext = "mp4";
                            }
                            else
                            {
                                ext = "jpg";
                            }

                            fileName = fileName.Substring(0, extNameIdx + 1) + ext;
                        }
                    }

                    DevicePhoto media = Logic.DevicePhoto.GetInstance().GetByDeviceIdAndMark(device.DeviceID, fileName);
                    if (media == null)
                    {
                        media = new DevicePhoto
                        {
                            DeviceID = device.DeviceID,
                            DeviceTime = DateTime.Now,
                            Latitude = 0,
                            Longitude = 0,
                            State = 0,
                            Path = "GUARD/" + device.DeviceID + "/" + fileName,
                            Mark = fileName,
                            TotalPackage = totalPackage,
                            CurrentPackage = 0,
                            CreateTime = DateTime.Now,
                            UpdateTime = DateTime.Now
                        };
                        Logic.DevicePhoto.GetInstance().Save(media);
                    }

                    if (currentPackage == 0)
                    {
                        var location = GetLocation(mySae, contentArr, 5);
                        try
                        {
                            var lbswifi = APIClient.GetInstance().GetPosition(location.LBS, location.WIFI);
                            //-1 验证失败,0 表示未获取到信息,1 wifi 数据库,2 wifi接口,11 lbs数据库 12 lbs接口
                            if (lbswifi != null && lbswifi.Code > 0)
                            {
                                location.Lat = lbswifi.Lat;
                                location.Lng = lbswifi.Lng;
                                location.Radius = lbswifi.Radius;
                                if (lbswifi.Code == 1 || lbswifi.Code == 2)
                                    location.LocationType = 3;
                                else if (lbswifi.Code == 11 || lbswifi.Code == 12)
                                    location.LocationType = 2;
                                Count.GetInstance().LbsAndWifi();
                            }
                        }
                        catch (Exception ex)
                        {
                            info.set('1', "GUARD", "", 0, Environment.TickCount - ticker, -1);
                            CPhotoStat.GetInstance().add_info(info);
                            Logger.Error(ex);
                            Logger.Error("LBS:" + location.LBS + "     WIFI:" + location.WIFI);
                        }

                        mySae.DeviceState.SatelliteNumber = int.Parse(contentArr[16]);
                        mySae.DeviceState.GSM = int.Parse(contentArr[17]);
                        mySae.DeviceState.Electricity = int.Parse(contentArr[18]);
                        this.OnLocation(location);
                        media.Latitude = location.Lat;
                        media.Longitude = location.Lng;
                        media.DeviceTime = location.Time;
                    }
                    else
                    {
                        int endOffset = startOffset + 33;
                        int remainder = 6;
                        while (remainder > 0 && endOffset < (startOffset + msgLength))
                        {
                            endOffset++;
                            if (bytes[endOffset] == 0x2C)
                            {
                                remainder--;
                            }
                        }

                        data = Encoding.UTF8.GetString(bytes, startOffset, endOffset - startOffset + 1);

                        FileStream fs = new FileStream(filePath + fileName, FileMode.OpenOrCreate, FileAccess.Write);
                        int startByte = (currentPackage - 1) * PackageSize;
                        fs.Seek(startByte, SeekOrigin.Begin);
                        fs.Write(bytes, startOffset + data.Length, msgLength - data.Length - 1);
                        fs.Close();
                        fs.Dispose();
                        if (currentPackage < totalPackage)
                        {
                            media.TotalPackage = totalPackage;
                            media.CurrentPackage = currentPackage;
                            media.State = 0;
                            Logic.DevicePhoto.GetInstance().Save(media);
                        }
                        else
                        {
                            media.TotalPackage = totalPackage;
                            media.CurrentPackage = currentPackage;
                            media.State = 1;

                            if (fileName.EndsWith("mp4"))
                            {
                                string thumbName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".jpg";
                                media.Thumb = thumbName;
                                Utils.CaptureFromVideo(filePath + fileName, filePath + thumbName, 1);
                            }

                            Logic.DevicePhoto.GetInstance().Save(media);
                            List<User> userList = UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID);
                            Notification.GetInstance().Send(media, userList);
                        }
                    }

                    string str = "GUARD," + opt + "," + fileName + "," + totalByteLength + "," + currentPackage + "," + totalPackage + ",1";
                    int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(str).Length : str.Length;
                    message = "[YW*" + sn + "*" + indexNo + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + str + "]";
                    byte[] bts = Encoding.UTF8.GetBytes(message);
                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

                    info.set('1', "GUARD", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    #endregion
                }
                else if (commandType.Equals("FILE"))
                {
                    #region FILE

                    int currentPackage = 1;
                    int totalPackage = 1;
                    int pkgSize = PackageSize;
                    try
                    {
                        pkgSize = int.Parse(contentArr[1]);
                    }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
                    catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
                    {
                        //
                    }

                    string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    string str = "FILE," + fileName + "," + currentPackage + "," + totalPackage + ",]";
                    int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(str).Length : str.Length;
                    message = "[YW*" + sn + "*" + indexNo + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + str;
                    byte[] bts = Encoding.UTF8.GetBytes(message);
//                    byte[] fBytes = new byte[bts.Length + 1 + 1];
//                    Buffer.BlockCopy(bts, 0, fBytes, 0, bts.Length);
//                    fBytes[fBytes.Length - 1] = (byte) ']';
                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);
                    info.set('1', "FILR", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    #endregion
                }
                else if (commandType.Equals("TKQ"))
                {
                    #region TKQ

                    mySae.DeviceState.StopSendVoice = false;
                    SendVoice(mySae, device);

                    info.set('1', "TKQ", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    return true;

                    #endregion
                }
                else if (commandType.Equals("TK") || commandType.Equals("TK2"))
                {
                    #region 接收语音对讲

                    if (contentArr.Length <= 5)
                    {
                        if (contentArr[1] == "1")
                        {
                            mySae.DictSend.TryRemove(indexNo, out var res);
                            if (mySae.DeviceVoiceId != 0)
                            {
                                //Logic.DeviceVoice.GetInstance().UpdateResponse(mySae.DeviceVoiceId);
                                DeviceVoice deviceVoice =
                                    Logic.DeviceVoice.GetInstance().Get(mySae.DeviceVoiceId);
                                if (deviceVoice.TotalPackage == deviceVoice.CurrentPackage)
                                {
                                    deviceVoice.State = 3;
                                    Logic.DeviceVoice.GetInstance().Save(deviceVoice);
                                    mySae.DeviceVoiceId = 0;
                                    SendVoice(mySae, device);
                                }
                                else
                                {
                                    deviceVoice.CurrentPackage += 1;
                                    Logic.DeviceVoice.GetInstance().Save(deviceVoice);
                                    SendVoice(mySae, device, deviceVoice);
                                }
                            }
                            else
                            {
                                SendVoice(mySae, device);
                            }
                        }
                        else if (contentArr[1] == "0")
                        {
                            mySae.DictSend.TryRemove(indexNo, out var res);
                            SendVoice(mySae, device);
                        }
                        else if (contentArr[1] == "2")
                        {
                            mySae.DeviceState.StopSendVoice = true;

                            #region 加了TK1的功能

                            mySae.DictSend.TryRemove(indexNo, out var res);
                            if (mySae.DeviceVoiceId != 0)
                            {
                                //Logic.DeviceVoice.GetInstance().UpdateResponse(mySae.DeviceVoiceId);
                                DeviceVoice deviceVoice =
                                    Logic.DeviceVoice.GetInstance().Get(mySae.DeviceVoiceId);
                                if (deviceVoice.TotalPackage == deviceVoice.CurrentPackage)
                                {
                                    deviceVoice.State = 4;
                                    Logic.DeviceVoice.GetInstance().Save(deviceVoice);
                                    mySae.DeviceVoiceId = 0;
                                }
                                else
                                {
                                    deviceVoice.State = 4;
                                    Logic.DeviceVoice.GetInstance().Save(deviceVoice);
                                    mySae.DeviceVoiceId = 0;
                                }
                            }

                            #endregion
                        }
                        else if (contentArr[1] == "3")
                        {
                            mySae.DictSend.TryRemove(indexNo, out var res);
                            if (mySae.DeviceVoiceId != 0)
                            {
                                //Logic.DeviceVoice.GetInstance().UpdateResponse(mySae.DeviceVoiceId);
                                DeviceVoice deviceVoice = Logic.DeviceVoice.GetInstance().Get(mySae.DeviceVoiceId);
                                if (contentArr.Length > 2)
                                {
                                    if (deviceVoice.Mark == contentArr[2])
                                    {
                                        //Data.Logger.Info("repeat  tp:" + deviceVoice.TotalPackage + " cp:" + deviceVoice.CurrentPackage + " ca3:" + contentArr[3] + " ca4:" + contentArr[4]);
                                        if (deviceVoice.TotalPackage == deviceVoice.CurrentPackage)
                                        {
                                            deviceVoice.State = 3;
                                            Logic.DeviceVoice.GetInstance().Save(deviceVoice);
                                            mySae.DeviceVoiceId = 0;
                                            Logger.Info("repeat mid:" + mySae.DeviceVoiceId + " Did:" + deviceVoice.DeviceVoiceId);
                                            SendVoice(mySae, device);
                                        }
                                        else
                                        {
                                            deviceVoice.CurrentPackage += 1;
                                            Logic.DeviceVoice.GetInstance().Save(deviceVoice);
                                            SendVoice(mySae, device, deviceVoice);
                                        }
                                    }
                                    else
                                    {
                                        Logger.Info("diff voice DeviceID:" + device.DeviceID + "name:" + deviceVoice.Mark + "," + contentArr[2]);
                                        SendVoice(mySae, device);
                                    }
                                }
                            }
                            else
                            {
                                SendVoice(mySae, device);
                            }
                        }
                    }
                    else
                    {
                        //Data.Logger.Error("content len:" + contentArr.Length + " ca0:" + contentArr[0]+" ca1:" + contentArr[1] + " ca2:" + contentArr[2]+" ca3:" + contentArr[3] + " ca4:" + contentArr[4] + " ca5:" + contentArr[5]);
                        int currentPackage = int.Parse(contentArr[3]);
                        int totalPackage = int.Parse(contentArr[4]);

                        var fileName = contentArr[2];
                        var playLength = 0;
                        if (fileName.Substring(0, 5).ToLower() == "watch")
                        {
                            //fileName = fileName.Split('_')[2];
                            playLength = Convert.ToInt32(fileName.Split('_')[1]);
                        }

                        string filePath = Config.GetInstance().Path + "\\AMR\\" + device.DeviceID + "\\";
                        DirectoryInfo di = new DirectoryInfo(filePath);
                        if (!di.Exists)
                            di.Create();
                        FileStream fs = new FileStream(filePath + fileName + ".amr", FileMode.OpenOrCreate,
                            FileAccess.Write);
                        int startByte = (currentPackage - 1) * PackageSize;
                        fs.Seek(startByte, SeekOrigin.Begin);
                        fs.Write(bytes, startOffset + data.Length, msgLength - data.Length - 1);
                        int filelength = (int) fs.Length;
                        fs.Close();
                        fs.Dispose();
                        byte[] bts;


                        if (currentPackage == 1)
                        {
                            DeviceVoice deviceVoice = new DeviceVoice
                            {
                                DeviceID = device.DeviceID
                            };
                            if (commandType.Equals("TK2"))
                            {
                                var tarDevice = Logic.Device.GetInstance().Get(contentArr[1]);
                                if (tarDevice == null)
                                {
                                    message = "[YW*" + sn + "*" + indexNo + "*0005*TK2,0]";
                                    bts = Encoding.UTF8.GetBytes(message);
                                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);
                                    return true;
                                }

                                deviceVoice.DeviceID = tarDevice.DeviceID;
                                deviceVoice.ObjectId = device.DeviceID;
                            }
                            else
                            {
                                deviceVoice.ObjectId = 0;
                                deviceVoice.Length = playLength;
                            }

                            if (currentPackage == totalPackage)
                            {
                                deviceVoice.State = 1;
                                deviceVoice.Length = 1;
                            }
                            else
                            {
                                deviceVoice.State = 0;
                            }

                            deviceVoice.Path = device.DeviceID + "/" + fileName + ".amr";
                            deviceVoice.Mark = fileName;
                            deviceVoice.TotalPackage = totalPackage;
                            deviceVoice.CurrentPackage = 1;
                            if (commandType.Equals("TK2"))
                                deviceVoice.Type = 4;
                            else
                                deviceVoice.Type = 2;
                            Logic.DeviceVoice.GetInstance().Save(deviceVoice);
                            if (deviceVoice.State == 1)
                            {
                                if (commandType.Equals("TK2"))
                                {
                                    var tarDevice = Logic.Device.GetInstance().Get(contentArr[1]);
                                    if (tarDevice != null)
                                    {
                                        OnSend(tarDevice, SendType.Voice, null);
                                    }
                                }
                                else
                                {
                                    List<User> userList = UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID);
                                    Notification.GetInstance().Send(deviceVoice, userList);
                                }
                            }
                        }
                        else if (totalPackage > currentPackage)
                        {
                            DeviceVoice deviceVoice = null;

                            if (commandType.Equals("TK2"))
                            {
                                var tarDevice = Logic.Device.GetInstance().Get(contentArr[1]);
                                deviceVoice = Logic.DeviceVoice.GetInstance().GetByDeviceIdAndMark(tarDevice.DeviceID, fileName);
                                if (deviceVoice == null)
                                {
                                    message = "[YW*" + sn + "*" + indexNo + "*0005*TK2,0]";
                                    bts = Encoding.UTF8.GetBytes(message);
                                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);
                                    return true;
                                }
                            }
                            else
                            {
                                deviceVoice = Logic.DeviceVoice.GetInstance().GetByDeviceIdAndMark(device.DeviceID, fileName);

                                if (deviceVoice == null)
                                {
                                    message = "[YW*" + sn + "*" + indexNo + "*0004*TK,0]";
                                    bts = Encoding.UTF8.GetBytes(message);
                                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);
                                    return true;
                                }
                            }

                            deviceVoice.TotalPackage = totalPackage;
                            deviceVoice.CurrentPackage = currentPackage;
                            deviceVoice.State = 0;
                            Logic.DeviceVoice.GetInstance().Save(deviceVoice);
                            //, totalPackage, currentPackage, 0
                        }
                        else
                        {
                            DeviceVoice deviceVoice = null;

                            if (commandType.Equals("TK2"))
                            {
                                var tarDevice = Logic.Device.GetInstance().Get(contentArr[1]);
                                deviceVoice = Logic.DeviceVoice.GetInstance().GetByDeviceIdAndMark(tarDevice.DeviceID, fileName);
                                if (deviceVoice == null)
                                {
                                    message = "[YW*" + sn + "*" + indexNo + "*0005*TK2,0]";
                                    bts = Encoding.UTF8.GetBytes(message);
                                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);
                                    return true;
                                }
                            }
                            else
                            {
                                deviceVoice = Logic.DeviceVoice.GetInstance().GetByDeviceIdAndMark(device.DeviceID, fileName);

                                if (deviceVoice == null)
                                {
                                    message = "[YW*" + sn + "*" + indexNo + "*0004*TK,0]";
                                    bts = Encoding.UTF8.GetBytes(message);
                                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);
                                    return true;
                                }
                            }

                            deviceVoice.TotalPackage = totalPackage;
                            deviceVoice.CurrentPackage = currentPackage;
                            deviceVoice.Length = deviceVoice.Length == 0 ? (filelength - 6) / 1000 : deviceVoice.Length;
                            deviceVoice.State = 1;
                            Logic.DeviceVoice.GetInstance().Save(deviceVoice);

                            if (commandType.Equals("TK"))
                            {
                                List<User> userList = UserDevice.GetInstance() .GetUserByDeviceId(device.DeviceID);
                                Notification.GetInstance().Send(deviceVoice, userList);
                            }
                            else if (commandType.Equals("TK2"))
                            {
                                var tarDevice = Logic.Device.GetInstance().Get(contentArr[1]);
                                if (tarDevice != null)
                                {
                                    OnSend(tarDevice, SendType.Voice, null);
                                }
                            }
                        }

                        if (commandType.Equals("TK2"))
                        {
                            message = "[YW*" + sn + "*" + indexNo + "*0005*TK2,1]";
                            bts = Encoding.UTF8.GetBytes(message);
                        }
                        else
                        {
                            message = "[YW*" + sn + "*" + indexNo + "*0004*TK,1]";
                            bts = Encoding.UTF8.GetBytes(message);
                        }

                        _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);
                    }

                    info.set('1', "TK", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    return true;

                    #endregion
                }
                else if (commandType.Equals("MFD"))
                {
                    #region MFD

                    //[YW*YYYYYYYYYY*NNNN*LEN*MFD,对方设备号码]
                    var targerDevice = Logic.Device.GetInstance().Get(contentArr[1]);
                    int ret = 1;
                    if (device != null && device.Deleted != true && targerDevice != null && targerDevice.Deleted != true) ///check ok
                    {
                        ///check the friend list
                        List<DeviceFriend> friends = Logic.DeviceFriend.GetInstance().GetByDeviceId(device.DeviceID);

                        if (friends.Count > 0)
                        {
                            foreach (DeviceFriend friend in friends)
                            {
                                if (friend.ObjectId == targerDevice.DeviceID)
                                {
                                    ret = 2;
                                }
                            }
                        }

                        if (ret == 1)
                        {
                            DeviceFriend friendContact = new DeviceFriend
                            {
                                DeviceID = device.DeviceID,
                                Type = 3,
                                ObjectId = targerDevice.DeviceID,
                                Name = targerDevice.BabyName,
                                CreateTime = DateTime.Now
                            };
                            Logic.DeviceFriend.GetInstance().Save(friendContact);

                            DeviceFriend friendContact2 = new DeviceFriend
                            {
                                DeviceID = targerDevice.DeviceID,
                                Type = 3,
                                ObjectId = device.DeviceID,
                                Name = device.BabyName,
                                CreateTime = DateTime.Now
                            };
                            Logic.DeviceFriend.GetInstance().Save(friendContact2);
                        }
                    }
                    else
                    {
                        ret = 0;
                    }

                    if (ret > 0)
                    {
                        //通知其他客户端更新通讯录
                        Notification.GetInstance().Send(device.DeviceID, 233, UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID).ToList());
                        Notification.GetInstance().Send(device.DeviceID, 233, UserDevice.GetInstance().GetUserByDeviceId(targerDevice.DeviceID).ToList());
                    }

                    //[YW*8800000015*0001*0020*MFD,8800000013,1]
                    string otherContent = "MFD," + targerDevice.SerialNumber + "," + Convert.ToString(ret);
                    int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
                    message = "[YW*" + sn + "*" + mySae.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";

                    byte[] bts = Encoding.UTF8.GetBytes(message);

                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

                    OnSend(targerDevice, SendType.FriendsListNotify, "1");


                    info.set('1', "MFD", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);
                    return true;

                    #endregion
                }

                else if (commandType.Equals("FDL"))
                {
                    #region FDL

                    //[YW*YYYYYYYYYY*NNNN*LEN*FDL]
                    List<DeviceFriend> friends = Logic.DeviceFriend.GetInstance().GetByDeviceId(device.DeviceID);

                    string otherContent = "FDL," + Convert.ToString(friends.Count);

                    foreach (var item in friends)
                    {
                        Model.Entity.Device friend = Logic.Device.GetInstance().Get(item.ObjectId);
                        otherContent += "," + friend.SerialNumber + "," + (item.Name != "" ? item.Name : friend.BabyName) + "," + friend.PhoneNumber + ",,,,";
                    }

                    //[YW*8800000015*0001*0020*FDL,8800000013,XIAO,15729292929,,,8880000014,SSSS,189222229,,,,]
                    int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
                    message = "[YW*" + device.SerialNumber + "*" + mySae.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";

                    byte[] bts = Encoding.UTF8.GetBytes(message);
                    _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

                    info.set('1', "FDL", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    #endregion
                }
                else if ("DISMISS".Equals(commandType))
                {
                    #region DISMISS 解绑操作  copy from Client.cs 1866行

                    Logic.Device.GetInstance().CleanRelation(device.DeviceID);

                    device.UserId = 0;
                    device.ContactVersionNO = 0;
                    device.SetVersionNO = 0;

                    device.UserId = 0;
                    device.SetVersionNO = 0;
                    device.ContactVersionNO = 0;
                    device.OperatorType = 0;
                    device.State = 2;
                    device.SmsNumber = null;
                    device.SmsBalanceKey = null;
                    device.SmsFlowKey = null;
                    device.Photo = null;
                    device.BabyName = null;
                    device.PhoneNumber = null;
                    device.PhoneCornet = null;
                    device.Gender = false;
                    device.Birthday = null;
                    device.Grade = 0;
                    device.SchoolAddress = null;
                    device.SchoolLng = null;
                    device.SchoolLat = null;
                    device.HomeAddress = null;
                    device.HomeLng = null;
                    device.HomeLat = null;
                    device.IsGuard = false;
                    device.Password = null;
                    device.Deviceflag = 1;
                    Logic.Device.GetInstance().Save(device);

                    var notification = new Model.Entity.Notification()
                    {
                        DeviceID = device.DeviceID,
                        Type = 9,
                        Content = null,
                        CreateTime = DateTime.Now
                    };
                    List<User> userList = UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID);
                    Notification.GetInstance().Send(notification, userList);

                    Logic.DeviceFriend.GetInstance().DelDevice(device.DeviceID);

                    OnSend(device, SendType.DeviceRecovery, "1");
                    //OnSend?.Invoke(device, SendType.Init, "0");
                    Task.Run(() =>
                    {
                        Thread.Sleep(5000);
                        mySae.DisconnectHandler(mySae);
                    });

                    #endregion
                }
                else //设备回复
                {
                    if (commandType.Equals("UPGRADE"))
                    {
                        #region UPGRADE

                        device.CurrentFirmware = contentArr[1];
                        Logic.Device.GetInstance().Save(device);
                        var nf = new Model.Entity.Notification
                        {
                            DeviceID = mySae.DeviceState.DeviceID,
                            Type = 5
                        };
                        List<User> userList = UserDevice.GetInstance()
                            .GetUserByDeviceId(device.DeviceID);
                        Notification.GetInstance().Send(nf, userList);

                        info.set('1', "UPGRADE", "", 0, Environment.TickCount - ticker, 1);
                        CPhotoStat.GetInstance().add_info(info);

                        #endregion
                    }
                    else if (commandType.Equals("COST1"))
                    {
                        #region 发送查询余额流量短信

                        if (mySae.DictSend.ContainsKey(indexNo))
                        {
                            var deviceSms = (DeviceSMS) mySae.DictSend[indexNo].Object;
                            deviceSms.State = contentArr[1] == "1" ? 2 : 3;
                            Logic.DeviceSMS.GetInstance().Save(deviceSms);
                            mySae.DictSend.TryRemove(indexNo, out var res1);
                        }

                        this.SendSMS(mySae, device);
                        info.set('1', "COST1", "", 0, Environment.TickCount - ticker, 1);
                        CPhotoStat.GetInstance().add_info(info);

                        #endregion
                    }
                    else if (commandType.Equals("PHB"))
                    {
                        #region 通信录设置指令

                        mySae.DictSend.TryRemove(indexNo, out var res2);
                        if (contentArr[2] == "1")
                        {
                            /*
                            int contactVersionNo = Utility.Convert.HexToInt(contentArr[1]);
                            if (device.ContactVersionNO > contactVersionNo)
                            {
                                //下发联系人
                                this.SendContact(mySae, device);
                            }
                            else
                            */
                            {
                                Model.Entity.Notification nf = new Model.Entity.Notification
                                {
                                    DeviceID = mySae.DeviceState.DeviceID,
                                    Type = 7
                                };
                                List<User> userList = UserDevice.GetInstance()
                                    .GetUserByDeviceId(device.DeviceID);
                                Notification.GetInstance().Send(nf, userList);
                            }
                        }

                        info.set('1', "PHB", "", 0, Environment.TickCount - ticker, 1);
                        CPhotoStat.GetInstance().add_info(info);

                        #endregion
                    }
                    else if (commandType.Equals("SET"))
                    {
                        #region 设置参数

                        mySae.DictSend.TryRemove(indexNo, out var res3);

                        if (contentArr != null && contentArr.Length > 1 && "1".Equals(contentArr[2]))
                        {
                            int setVersionNo = Utility.Convert.HexToInt(contentArr[1]);
                            /*
                            if (device.SetVersionNO > setVersionNo)
                            {
                                //下发配置信息[YW*YYYYYYYYYY*NNNN*LEN*SET,设置次数流水号,设置项,上课禁用时间段,定时开机时间,定时关机时间,亮屏时间]

                                Data.Logger.Info(String.Format("set SendSet:{0}:{1}:{2}",
                                    device.SetVersionNO,
                                    setVersionNo, contentArr[1]));

                                this.SendSet(mySae, device);
                            }
                            else
                            */
                            {
                                Model.Entity.Notification nf = new Model.Entity.Notification
                                {
                                    DeviceID = mySae.DeviceState.DeviceID,
                                    Type = 6
                                };
                                List<User> userList = UserDevice.GetInstance().GetUserByDeviceId(device.DeviceID);
                                Notification.GetInstance().Send(nf, userList);
                            }
                        }

                        info.set('1', "SET", "", 0, Environment.TickCount - ticker, 1);
                        CPhotoStat.GetInstance().add_info(info);

                        #endregion
                    }
                    else if (commandType.Equals("CR"))
                    {
                        #region

                        mySae.DictSend.TryRemove(indexNo, out var res4);

                        info.set('1', "TK", "", 0, Environment.TickCount - ticker, 1);
                        CPhotoStat.GetInstance().add_info(info);

                        #endregion
                    }
                    else if (commandType.Equals("AVATARQ"))
                    {
                        #region

                        string[] itemArr = dataContent.Split(',');
                        string photoIndex = itemArr[1];
                        string phoneNumber = itemArr[2];
                        int photoVersion = Utility.Convert.HexToInt(itemArr[3]);
                        int currentPackage = Convert.ToInt32(itemArr[4]);
                        DeviceContact _dc = Logic.DeviceContact.GetInstance()
                            .GetForPhoto(device.DeviceID, phoneNumber);
                        if (_dc != null && _dc.HeadImgVersion == photoVersion)
                        {
                            int byteLength = PackageSize;
                            string filePath = Config.GetInstance().Path + "\\Upload\\" +
                                              _dc.HeadImg.Replace('/', '\\');
                            var fs = new FileStream(filePath, FileMode.Open,
                                FileAccess.Read);
                            if (fs.Length > 0)
                            {
                                var startByte = (currentPackage - 1) * PackageSize;
                                //var send = new Send { Index = mySae.Index, SendType = Model.SendType.Avatar, Object = _dc };
                                //lock (mySae.DictSend)
                                //{
                                //    if (!mySae.DictSend.ContainsKey(send.Index))
                                //    {
                                //        mySae.DictSend.TryAdd(send.Index, send);
                                //    }
                                //    else
                                //    {
                                //        mySae.DictSend[send.Index] = send;
                                //    }
                                //}
                                var fileLength = (int) fs.Length;
                                int totalPackage = fileLength % PackageSize == 0
                                    ? fileLength / PackageSize
                                    : (fileLength / PackageSize + 1);
                                if (totalPackage == currentPackage)
                                {
                                    byteLength = fileLength - startByte;
                                }

                                string sendMessage = "[YW*" + device.SerialNumber + "*" + mySae.Index + "*";
                                string otherContent = "AVATAR," + photoIndex + "," + phoneNumber + "," +
                                                      photoVersion.ToString("X").PadLeft(4, '0') + "," + currentPackage +
                                                      "," + totalPackage + ",";
                                int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
                                sendMessage += (contLen + byteLength).ToString("X").PadLeft(4, '0') + "*" + otherContent;

                                var bytes1 = new byte[sendMessage.Length + byteLength + 1];
                                Array.Copy(Encoding.UTF8.GetBytes(sendMessage), bytes1, sendMessage.Length);
                                fs.Seek(startByte, SeekOrigin.Begin);
                                fs.Read(bytes1, sendMessage.Length, byteLength);
                                fs.Close();
                                fs.Dispose();
                                bytes1[bytes1.Length - 1] = 0x5D;
                                this._sendTcpHandler(mySae, Guid.NewGuid(), bytes1, 0, bytes1.Length);
                            }
                        }

                        info.set('1', "AVATAR", "", 0, Environment.TickCount - ticker, 1);
                        CPhotoStat.GetInstance().add_info(info);

                        #endregion
                    }

                    mySae.DictSend.TryRemove(indexNo, out var res5);
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(device == null ? "device==null" : device.ToString(), ex);
                try
                {
                    info.set('1', "Initialize", "", 0, Environment.TickCount - ticker, -2);
                    CPhotoStat.GetInstance().add_info(info);

                    Logger.Error(Encoding.UTF8.GetString(bytes, startOffset, length));
                }
                catch (Exception ex2)
                {
                    info.set('1', "Initialize", "", 0, Environment.TickCount - ticker, -3);
                    CPhotoStat.GetInstance().add_info(info);
                    Logger.Error(ex2);
                }

                return false;
            }
        }

        private Location GetLocation(MySAE mySae, string[] contentArr, int startOffset)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            DateTime? deviceTime = null;
            if (contentArr[1 + startOffset].Length == 6 && contentArr[2 + startOffset].Length == 6)
            {
                string hour = contentArr[2 + startOffset].Substring(0, 2);
                string min = contentArr[2 + startOffset].Substring(2, 2);
                string sec = contentArr[2 + startOffset].Substring(4, 2);

                string year = contentArr[1 + startOffset].Substring(4, 2);
                string month = contentArr[1 + startOffset].Substring(2, 2);
                string day = contentArr[1 + startOffset].Substring(0, 2);
                try
                {
                    deviceTime =
                        DateTime.Parse("20" + year + "/" + month + "/" + day + " " + hour + ":" + min + ":" + sec)
                            .AddMinutes(_timeZone);
                }
                catch
                {
                    deviceTime = DateTime.Now;
                }

                if (contentArr[3 + startOffset] == "V" &&
                    (deviceTime < DateTime.Now.AddMinutes(-60) || deviceTime > DateTime.Now.AddMinutes(60)))
                {
                    deviceTime = DateTime.Now;
                }
            }
            else
            {
                deviceTime = DateTime.Now;
            }

            var location = new Location(mySae.DeviceState.DeviceID, deviceTime, DateTime.Now);
            if (contentArr[16 + startOffset].Length > 0)
            {
                location.Status = Utility.Convert.HexToInt(contentArr[16 + startOffset]);
            }

            #region GPS

            if (contentArr[3 + startOffset] == "A")
            {
                location.LocationType = 1;
                location.Lat = double.Parse(contentArr[4 + startOffset]);
                location.Lng = double.Parse(contentArr[6 + startOffset]);
                if ((location.Lat < -180 || location.Lat > 180 || location.Lng < -180 || location.Lng > 180))
                {
                    location.LocationType = 0;
                    location.Lat = 0;
                    location.Lng = 0;
                }

                if (contentArr[5 + startOffset].Equals("S"))
                    location.Lat = -location.Lat;
                if (contentArr[7 + startOffset].Equals("W"))
                    location.Lng = -location.Lng;
                location.Speed = double.Parse(contentArr[8 + startOffset]);
                location.Course = double.Parse(contentArr[9 + startOffset]);
                location.Altitude = double.Parse(contentArr[10 + startOffset]);
            }

            #endregion

            #region LBS

            if (contentArr.Length > 17 + startOffset)
            {
                int lbsCount = int.Parse(contentArr[17 + startOffset]);
                if (lbsCount > 0)
                {
                    string lbsStr = "";
                    for (int x = 0; x < lbsCount * 3; x = x + 3)
                    {
                        if (x > 0)
                        {
                            lbsStr += "|";
                        }

                        lbsStr += contentArr[19 + startOffset].Trim() + "," + contentArr[20 + startOffset].Trim() + "," +
                                  contentArr[x + (21) + startOffset].Trim() + "," +
                                  contentArr[x + (22) + startOffset].Trim() + "," +
                                  contentArr[x + (23) + startOffset].Trim();
                    }

                    location.LBS = lbsStr;
                }
            }

            #endregion

            #region WIFI

            int wifiIndex = int.Parse(contentArr[17 + startOffset]) * 3 + 21 + startOffset;
            if (contentArr.Length > wifiIndex)
            {
                int wifiCount = int.Parse(contentArr[wifiIndex]);
                if (wifiCount > 0)
                {
                    string wifiStr = "";
                    for (int x = 0; x < wifiCount * 3; x = x + 3)
                    {
                        if (x > 0)
                        {
                            wifiStr += "|";
                        }

                        wifiStr += contentArr[wifiIndex + 2 + x] + "," + contentArr[wifiIndex + 3 + x] + "," +
                                   contentArr[wifiIndex + 1 + x];
                    }

                    location.WIFI = wifiStr;
                }
            }

            #endregion

            info.set('1', "GetLocation", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);

            return location;
        }


        /// <summary>
        /// 触发发送新的语音及未发完的语音
        /// </summary>
        /// <param name="mySae"></param>
        /// <param name="device"></param>
        public void SendVoice(MySAE mySae, Model.Entity.Device device)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            Logger.Info("SendVoice **** DeviceID:" + device.DeviceID + " StopSendVoice:" + mySae.DeviceState.StopSendVoice);
            if (mySae.DeviceState.StopSendVoice == false)
            {
                DeviceVoice deviceVoice = Logic.DeviceVoice.GetInstance().GetNewOne(device.DeviceID);
                //if (deviceVoice != null
                //&& (mySae.DeviceVoiceId == 0
                //||(mySae.DeviceVoiceId == deviceVoice.DeviceVoiceId )))
                //&& deviceVoice.UpdateTime.AddSeconds(30) < DateTime.Now))
                //   )
                if (mySae.DeviceVoiceId != 0)
                {
                    DeviceVoice mySaedeviceVoice = Logic.DeviceVoice.GetInstance().Get(mySae.DeviceVoiceId);
                    Logger.Info("**** mySaedeviceVoice is  " + mySae.DeviceVoiceId);
                    SendVoice(mySae, device, mySaedeviceVoice);
                }
                else if (deviceVoice != null)
                {
                    Logger.Info("start send voice true");
                    SendVoice(mySae, device, deviceVoice);
                    Logger.Info("DeviceID:" + device.DeviceID + "deviceVoice:" + deviceVoice + "DeviceVoiceId:" + mySae.DeviceVoiceId + "DeviceVoiceId:" + deviceVoice.DeviceVoiceId +
                                "UpdateTime:" + deviceVoice.UpdateTime + "Now:" + DateTime.Now);
                }
//                else
//                {
                    //Logger.Info("deviceVoice null err **** ");
//                }
            }

            info.set('1', "SendVoice", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        /// <summary>
        /// 直接发送制定语音
        /// </summary>
        /// <param name="mySae"></param>
        /// <param name="device"></param>
        /// <param name="deviceVoice"></param>
        private void SendVoice(MySAE mySae, Model.Entity.Device device, DeviceVoice deviceVoice)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            if (mySae.DeviceState.StopSendVoice == false)
            {
                int totalPackage = deviceVoice.TotalPackage;
                int currentPackage = deviceVoice.CurrentPackage;
                FileStream fs = null;
                try
                {
                    string filePath = Config.GetInstance().Path + "\\AMR\\" + deviceVoice.Path;
                    if (!File.Exists(filePath))
                    {
                        return;
                    }

                    fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    if (fs.Length == 0)
                    {
                        //直接设置发送失败
                        deviceVoice.State = 3;
                        deviceVoice.UpdateTime = DateTime.Now;
                        Logic.DeviceVoice.GetInstance().Save(deviceVoice);
                        //Logic.DeviceVoice.GetInstance().UpdateStateById(mySae.DeviceVoiceId, 4);
                        mySae.DeviceVoiceId = 0;
                        return;
                    }

                    int byteLength = PackageSize;
                    if (deviceVoice.State == 1)
                    {
                        int fileLength = (int) fs.Length;
                        totalPackage = fileLength % PackageSize == 0 ? fileLength / PackageSize : (fileLength / PackageSize + 1);
                        deviceVoice.State = 2;
                        deviceVoice.CurrentPackage = 1;
                        deviceVoice.TotalPackage = totalPackage;
                        currentPackage = 1;
                    }

                    int startByte = (currentPackage - 1) * PackageSize;
                    if (totalPackage == currentPackage)
                    {
                        byteLength = (int) fs.Length - (currentPackage - 1) * PackageSize;
                    }

                    Send send = new Send {Index = mySae.Index, SendType = SendType.Voice, Object = deviceVoice};
                    mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

                    string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*";
                    string otherContent = "";
                    if (deviceVoice.Type == 3)
                    {
                        User user = Logic.User.GetInstance().Get(deviceVoice.ObjectId);
                        if (user == null)
                        {
                            throw new Exception("用户数据异常，用户id:" + deviceVoice.ObjectId + " DeviceVoice:" + deviceVoice);
                        }

                        otherContent = "TK," + deviceVoice.MsgType + ":0:" + user.PhoneNumber + "," + deviceVoice.Mark + "," + deviceVoice.CurrentPackage + "," + deviceVoice.TotalPackage + ",";
                    }
                    else if (deviceVoice.Type == 4)
                    {
                        Model.Entity.Device fromDevice = Logic.Device.GetInstance().Get(deviceVoice.ObjectId);
                        otherContent = "TK," + deviceVoice.MsgType + ":1:" + fromDevice.SerialNumber + "," + deviceVoice.Mark + "," + deviceVoice.CurrentPackage + "," + deviceVoice.TotalPackage + ",";
                    }

                    int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
                    message += (contLen + byteLength).ToString("X").PadLeft(4, '0') + "*" + otherContent;

                    byte[] bytes = new byte[message.Length + byteLength + 1];
                    Array.Copy(Encoding.UTF8.GetBytes(message), bytes, message.Length);
                    fs.Seek(startByte, SeekOrigin.Begin);
                    fs.Read(bytes, message.Length, byteLength);
                    bytes[bytes.Length - 1] = 0x5D;
                    Logger.Info("active send DeviceID:" + device.DeviceID + "byteLength:" + byteLength + "Mid:" + mySae.DeviceVoiceId + "Did:" + deviceVoice.DeviceVoiceId);
                    this._sendTcpHandler(mySae, Guid.NewGuid(), bytes, 0, bytes.Length);
                    mySae.DeviceVoiceId = deviceVoice.DeviceVoiceId;
                    deviceVoice.UpdateTime = DateTime.Now;
                    Logic.DeviceVoice.GetInstance().Save(deviceVoice);
                }
                catch (Exception ex)
                {
                    deviceVoice.State = 4;
                    deviceVoice.UpdateTime = DateTime.Now;
                    Logic.DeviceVoice.GetInstance().Save(deviceVoice);
                    mySae.DeviceVoiceId = 0;
                    Logger.Error(ex);
                }
                finally
                {
                    if (fs != null)
                    {
                        try
                        {
                            fs.Close();
                            fs.Dispose();
                        }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
                        catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
                        {
                            //
                        }
                    }
                }

                info.set('1', "SendVoice2", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);
            }
        }

        public void SendContact(MySAE mySae, Model.Entity.Device device)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            List<DeviceContact> list = Logic.DeviceContact.GetInstance().GetByDeviceId(device.DeviceID);

            {
                string otherContent = null;
                if (list.Count > 0)
                {
                    otherContent = "PHB," + Convert.ToString(device.ContactVersionNO, 16).PadLeft(4, '0') + "," + list.Count + ",";
                    foreach (DeviceContact deviceContact in list)
                    {
                        int photoValue = deviceContact.Photo == 7 ? 0 : deviceContact.Photo;
                        string photoType = Convert.ToString(!string.IsNullOrEmpty(deviceContact.HeadImg) ? (photoValue + 8) : photoValue, 16);

                        otherContent += deviceContact.Type + "-" + deviceContact.Name + "-" + deviceContact.PhoneNumber + "-";
                        if ((device.CloudPlatform == 1 || device.CloudPlatform > 2) && !string.IsNullOrEmpty(deviceContact.AgentNumber))
                        {
                            string prefix = AppConfig.GetValue(Constants.CLOUDPLATFORM_G_DIAL_PREFIX);
                            otherContent += deviceContact.AgentNumber.Replace(prefix, "");
                        }
                        else
                        {
                            otherContent += deviceContact.PhoneShort;
                        }

                        otherContent += "-" + photoType + "-" + Convert.ToString(deviceContact.HeadImgVersion, 16).PadLeft(4, '0');

                        if (device.CloudPlatform > 0 && !string.IsNullOrEmpty(deviceContact.AgentNumber))
                        {
                            string prefix = AppConfig.GetValue(Constants.CLOUDPLATFORM_G_DIAL_PREFIX);
                            otherContent += '-' + (deviceContact.AgentNumber.StartsWith(prefix) ? deviceContact.AgentNumber : prefix + deviceContact.AgentNumber);
                        }

                        otherContent += "|";
                    }

                    otherContent = otherContent.Remove(otherContent.Length - 1, 1);
                }
                else
                {
                    otherContent = "PHB," + Convert.ToString(device.ContactVersionNO, 16).PadLeft(4, '0') + "," + 0 + ",";
                }

                Send send = new Send {Index = mySae.Index, SendType = SendType.Contact, Object = list};
                mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

                int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
                string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
                byte[] bts = Encoding.UTF8.GetBytes(message);
                _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);
            }

            info.set('1', "SendContact", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendSMS(MySAE mySae, Model.Entity.Device device)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();
            //获取有没新的需要发的，如果有就发，没有就跳过
            DeviceSMS deviceSms = Logic.DeviceSMS.GetInstance().GetOne(device.DeviceID);
            if (deviceSms != null)
            {
                string otherContent = "COST1," + deviceSms.Phone + "," + deviceSms.SMS;
                Send send = new Send {Index = mySae.Index, SendType = SendType.SMS, Object = deviceSms};
                mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

                int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
                string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
                byte[] bts = Encoding.UTF8.GetBytes(message);
                _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);
                deviceSms.State = 1;
                Logic.DeviceSMS.GetInstance().Save(deviceSms);

                info.set('1', "SendSMS", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);
            }
        }

        public void SendLocation(MySAE mySae, Model.Entity.Device device)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            //直接发送指令
            //[YW*8800000015*0001*0002*CR]
            Send send = new Send {Index = mySae.Index, SendType = SendType.Location, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*0002*CR]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendLocation", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendSet(MySAE mySae, Model.Entity.Device device)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            string otherContent;
            //直接发送指令
            DeviceSet deviceSet = Logic.DeviceSet.GetInstance().Get(device.DeviceID);

            if (deviceSet != null)
            {
                string setByte = deviceSet.SetInfo.Replace("-", "");
                string setByte1 = Convert.ToByte(setByte.Substring(0, 4), 2).ToString("X");
                string setByte2 = Convert.ToByte(setByte.Substring(4, 4), 2).ToString("X");
                string setByte3 = Convert.ToByte(setByte.Substring(8, 4), 2).ToString("X");

                otherContent = "SET,"
                               + device.PhoneNumber + ","
                               + Convert.ToString(device.SetVersionNO, 16).PadLeft(4, '0') + ","
                               + setByte1 + setByte2 + setByte3 + ","
                               + deviceSet.ClassDisabled1 + "|" + deviceSet.ClassDisabled2 + "|" +
                               deviceSet.WeekDisabled + ","
                               + deviceSet.TimerOpen + ","
                               + deviceSet.TimerClose + ","
                               + deviceSet.BrightScreen + ","
                               + ",," + ","
                               + deviceSet.WeekAlarm1 + ","
                               + deviceSet.WeekAlarm2 + ","
                               + deviceSet.WeekAlarm3 + ","
                               + deviceSet.Alarm1 + ","
                               + deviceSet.Alarm2 + ","
                               + deviceSet.Alarm3 + ","
                               + deviceSet.LocationMode + ","
                               + deviceSet.LocationTime + ","
                               + deviceSet.FlowerNumber + ","
                               + deviceSet.SleepCalculate + ","
                               + deviceSet.StepCalculate + ","
                               + deviceSet.HrCalculate + ","
                               + deviceSet.SosMsgswitch + ","
                               + device.BabyName;
            }
            else
            {
                otherContent = "SET,,0001,F0A,08:00-12:00|14:00-17:00|,07:00,00:00,10,,,,,,,,,,,,,,,";
            }

            var send = new Send {Index = mySae.Index, SendType = SendType.Set, Object = deviceSet};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendSet", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendFirmware(MySAE mySae, Model.Entity.Device device)
        {
            //直接发送指令
            if (device.CurrentFirmware != device.Firmware)
            {
            }
        }

        public void SendMonitor(MySAE mySae, Model.Entity.Device device, string paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            //远程监护-监听
            var send = new Send {Index = mySae.Index, SendType = SendType.Monitor, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string otherContent = "MONITOR," + paramter;
            int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendMonitor", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendSleepCalculate(MySAE mySae, Model.Entity.Device device, string paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            var send = new Send {Index = mySae.Index, SendType = SendType.SleepCalculate, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string otherContent = "SLEEPCALCULATE," + paramter;
            int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendSleepCal", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendStepCalculate(MySAE mySae, Model.Entity.Device device, string paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            var send = new Send {Index = mySae.Index, SendType = SendType.StepCalculate, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string otherContent = "STEPCALCULATE," + paramter;
            int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendStepCal", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendHrCalculate(MySAE mySae, Model.Entity.Device device, string paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            var send = new Send {Index = mySae.Index, SendType = SendType.HrCalculate, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string otherContent = "HRCALCULATE," + paramter;
            int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendHrCal", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendDeviceRecovery(MySAE mySae, Model.Entity.Device device, string paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            var send = new Send {Index = mySae.Index, SendType = SendType.DeviceRecovery, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string otherContent = "DEVICERECOVERY," + paramter;
            int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendDeviceR", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendDeviceReset(MySAE mySae, Model.Entity.Device device, string paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            var send = new Send {Index = mySae.Index, SendType = SendType.DeviceReset, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string otherContent = "DEVICERESET," + paramter;
            int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendDeviceRes", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendTqInfo(MySAE mySae, Model.Entity.Device device, string paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            var send = new Send {Index = mySae.Index, SendType = SendType.TqInfo, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(String.Format("AdapterOfYW.cs==IMEI:{0} ,Socket ID:{1} ,IP:{2} ,UserToken:{3}", device.SerialNumber, mySae.SocketId, mySae.Ip, mySae.UserToken));
            }

            string otherContent = "TQ," + paramter;
            int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendTqInfo", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendDwInfo(MySAE mySae, Model.Entity.Device device, string paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            var send = new Send {Index = mySae.Index, SendType = SendType.TqInfo, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string otherContent = "DWRES," + paramter;
            int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendDwInfo", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendGuard(MySAE mySae, Model.Entity.Device device, string paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            var send = new Send {Index = mySae.Index, SendType = SendType.GUARD, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string otherContent = "SendGuard," + paramter;
            int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendGuard", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }


        public void SendFriendListNotity(MySAE mySae, Model.Entity.Device device, string paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            var send = new Send {Index = mySae.Index, SendType = SendType.TqInfo, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            List<DeviceFriend> friends = Logic.DeviceFriend.GetInstance().GetByDeviceId(device.DeviceID);

            if (friends.Count == 0 && !paramter.Equals("1"))
                return;
            string otherContent = Convert.ToString(friends.Count);

            foreach (var item in friends)
            {
                Model.Entity.Device friend = Logic.Device.GetInstance().Get(item.ObjectId);
                otherContent += "," + friend.SerialNumber + "," + (item.Name != "" ? item.Name : friend.BabyName) + "," + friend.PhoneNumber + ",,,,";
            }

            otherContent = "FDLN," + otherContent;
            int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendFriendL", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }


        public void SendTakePhoto(MySAE mySae, Model.Entity.Device device, string paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            //拍照
            //return;
            var send = new Send {Index = mySae.Index, SendType = SendType.TakePhoto, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string otherContent = "CAPT," + paramter;
            int contLen = device.LengthCountType == Constants.LENGTH_COUNT_BYTE ? Encoding.UTF8.GetBytes(otherContent).Length : otherContent.Length;
            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*" + Convert.ToString(contLen, 16).PadLeft(4, '0') + "*" + otherContent + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendTakePhoto", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendFind(MySAE mySae, Model.Entity.Device device)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();
            //找手表
            var send = new Send {Index = mySae.Index, SendType = SendType.Find, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*0004*FIND]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendFind", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendPowerOff(MySAE mySae, Model.Entity.Device device)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            //远程关机
            var send = new Send {Index = mySae.Index, SendType = SendType.PowerOff, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*0008*POWEROFF]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendPowerOff", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public void SendInit(MySAE mySae, Model.Entity.Device device, string Paramter)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();
            //初始化
            var send = new Send {Index = mySae.Index, SendType = SendType.Init, Object = null};
            mySae.DictSend.AddOrUpdate(send.Index, send, (key, val) => send);

            string message = "[YW*" + device.SerialNumber + "*" + send.Index + "*0006*INIT," + Paramter + "]";
            byte[] bts = Encoding.UTF8.GetBytes(message);
            _sendTcpHandler(mySae, Guid.NewGuid(), bts, 0, bts.Length);

            info.set('1', "SendInit", "", 0, Environment.TickCount - ticker, 1);
            CPhotoStat.GetInstance().add_info(info);
        }

        public static byte[] StrToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }


        /// <summary>
        /// Http (GET/POST)
        /// </summary>
        /// <param name="url">请求URL</param>
        /// <param name="parameters">请求参数</param>
        /// <param name="method">请求方法</param>
        /// <returns>响应内容</returns>
        public static string sendPost(string url, IDictionary<string, string> parameters, string refer, string method)
        {
            int ticker = Environment.TickCount;
            RECORD_INFO info = new RECORD_INFO();

            if (method.ToLower() == "post")
            {
                HttpWebRequest req = null;
                HttpWebResponse rsp = null;
                Stream reqStream = null;
                try
                {
                    req = (HttpWebRequest) WebRequest.Create(url);
                    req.Method = method;
                    req.KeepAlive = false;
                    req.ProtocolVersion = HttpVersion.Version10;
                    req.Timeout = 5000;
                    req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                    req.Referer = refer;
                    if (parameters != null)
                    {
                        byte[] postData = Encoding.UTF8.GetBytes(BuildQuery(parameters, "utf8"));
                        reqStream = req.GetRequestStream();
                        reqStream.Write(postData, 0, postData.Length);
                    }

                    rsp = (HttpWebResponse) req.GetResponse();
                    Encoding encoding = Encoding.GetEncoding(rsp.CharacterSet);


                    info.set('1', "sendPost", "", 0, Environment.TickCount - ticker, 1);
                    CPhotoStat.GetInstance().add_info(info);

                    return GetResponseAsString(rsp, encoding);
                }
                catch (Exception ex)
                {
                    info.set('1', "sendPost", "", 0, Environment.TickCount - ticker, -1);
                    CPhotoStat.GetInstance().add_info(info);

                    return ex.Message;
                }
                finally
                {
                    if (reqStream != null)
                    {
                        reqStream.Close();
                    }

                    if (rsp != null)
                    {
                        rsp.Close();
                    }
                }
            }
            else
            {
                //创建请求
                string paramStr = BuildQuery(parameters, "utf8");
                if (!"".Equals(paramStr))
                {
                    url += "?" + paramStr;
                }

                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);

                //GET请求
                request.Method = "GET";
                request.ReadWriteTimeout = 5000;
                request.ContentType = "*/*;charset=UTF-8";
                request.Referer = refer;
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

                //返回内容
                string retString = myStreamReader.ReadToEnd();

                info.set('1', "sendPost", "", 0, Environment.TickCount - ticker, 1);
                CPhotoStat.GetInstance().add_info(info);

                return retString;
            }
        }

        /// <summary>
        /// 组装普通文本请求参数。
        /// </summary>
        /// <param name="parameters">Key-Value形式请求参数字典</param>
        /// <returns>URL编码后的请求数据</returns>
        public static string BuildQuery(IDictionary<string, string> parameters, string encode)
        {
            if (parameters == null)
            {
                return "";
            }

            StringBuilder postData = new StringBuilder();
            bool hasParam = false;
            IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value;
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name)) //&& !string.IsNullOrEmpty(value)
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }

                    postData.Append(name);
                    postData.Append("=");
                    if (encode == "gb2312")
                    {
                        postData.Append(HttpUtility.UrlEncode(value, Encoding.GetEncoding("gb2312")));
                    }
                    else if (encode == "utf8")
                    {
                        postData.Append(HttpUtility.UrlEncode(value, Encoding.UTF8));
                    }
                    else
                    {
                        postData.Append(value);
                    }

                    hasParam = true;
                }
            }

            return postData.ToString();
        }

        /// <summary>
        /// 把响应流转换为文本。
        /// </summary>
        /// <param name="rsp">响应流对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>响应文本</returns>
        public static string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            Stream stream = null;
            StreamReader reader = null;
            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
            finally
            {
                // 释放资源
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }
        }

        public void Dispose()
        {
        }
    }
}