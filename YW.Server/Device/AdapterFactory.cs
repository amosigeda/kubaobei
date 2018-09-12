using System;
using YW.Server.Socket;

namespace YW.Server.Device
{
    public class AdapterFactory
    {
        public delegate bool SendTcpHandler(MySAE mysae, Guid msgId, byte[] bytes, int index, int length);
        public delegate void LocationHandler(Model.Location location);

        public LocationHandler OnLocation;
        public LocationHandler OnLocationLbsWifi;
        public LocationHandler OnLocationGaode;

        private static AdapterOfYW _adapterOfYW;

        public AdapterFactory(SendTcpHandler sendTcpHandler, YW.WCF.Client.SendHandler send)
        {
            _adapterOfYW = new AdapterOfYW(sendTcpHandler, send);
            _adapterOfYW.OnLocation += new AdapterFactory.LocationHandler(_adapter_OnLocation);
            _adapterOfYW.OnLocationLbsWifi += new AdapterFactory.LocationHandler(_adapter_OnLocationLbsWifi);
            _adapterOfYW.OnLocationGaode += new AdapterFactory.LocationHandler(_adapter_OnLocationGaode);
        }

        private void _adapter_OnLocation(Model.Location loctaion)
        {
            this.OnLocation(loctaion);
        }

        private void _adapter_OnLocationLbsWifi(Model.Location loctaion)
        {
            this.OnLocationLbsWifi(loctaion);
        }

        private void _adapter_OnLocationGaode(Model.Location loctaion)
        {
            this.OnLocationGaode(loctaion);
        }

        public void SendCommand(Model.Entity.Device device, Socket.MySAE mySae, Model.SendType commandType, string Paramter)
        {
            switch (mySae.DeviceType)
            {
                case Model.DeviceType.YW:
                    switch (commandType)
                    {
                        case Model.SendType.Voice:
                            _adapterOfYW.SendVoice(mySae, device);
                            break;
                        case Model.SendType.Contact:
                            _adapterOfYW.SendContact(mySae, device);
                            break;
                        case Model.SendType.Set:
                            _adapterOfYW.SendSet(mySae, device);
                            break;
                        case Model.SendType.SMS:
                            _adapterOfYW.SendSMS(mySae, device);
                            break;
                        case Model.SendType.Location:
                            _adapterOfYW.SendLocation(mySae, device);
                            break;
                        case Model.SendType.Firmware:
                            _adapterOfYW.SendFirmware(mySae, device);
                            break;
                        case Model.SendType.Monitor:
                            _adapterOfYW.SendMonitor(mySae, device, Paramter);
                            break;
                        case Model.SendType.Find:
                            _adapterOfYW.SendFind(mySae, device);
                            break;
                        case Model.SendType.PowerOff:
                            _adapterOfYW.SendPowerOff(mySae, device);
                            break;
                        case Model.SendType.Init:
                            _adapterOfYW.SendInit(mySae, device, Paramter);
                            break;
                        case Model.SendType.TakePhoto:
                            _adapterOfYW.SendTakePhoto(mySae, device, Paramter);
                            break;
                        case Model.SendType.SleepCalculate:
                            _adapterOfYW.SendSleepCalculate(mySae, device, Paramter);
                            break;
                        case Model.SendType.StepCalculate:
                            _adapterOfYW.SendStepCalculate(mySae, device, Paramter);
                            break;
                        case Model.SendType.HrCalculate:
                            _adapterOfYW.SendHrCalculate(mySae, device, Paramter);
                            break;
                        case Model.SendType.DeviceRecovery:
                            _adapterOfYW.SendDeviceRecovery(mySae, device, Paramter);
                            break;
                        case Model.SendType.DeviceReset:
                            _adapterOfYW.SendDeviceReset(mySae, device, Paramter);
                            break;
                        case Model.SendType.TqInfo:
                            _adapterOfYW.SendTqInfo(mySae, device, Paramter);
                            break;
                        case Model.SendType.FriendsListNotify:
                            _adapterOfYW.SendFriendListNotity(mySae, device, Paramter);
                            break;
                        case Model.SendType.DwInfo:
                            _adapterOfYW.SendDwInfo(mySae, device, Paramter);
                            break;
                        case Model.SendType.GUARD:
                            _adapterOfYW.SendGuard(mySae, device, Paramter);
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        public bool Initialize(MySAE mySae, byte[] bytes, int startOffset, int length, ref int msgLength)
        {
            if (mySae.DeviceType == 0)
            {
                if (_adapterOfYW.Initialize(mySae, bytes, startOffset, length, ref msgLength))
                {
                    return true;
                }
                
                else
                {
                    return false;
                }
            }
            else
            {
                switch (mySae.DeviceType)
                {
                    case Model.DeviceType.YW:
                        return _adapterOfYW.Initialize(mySae, bytes, startOffset, length, ref msgLength);
                    default:
                        return false;
                }
            }

        }
    }
}
