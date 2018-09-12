using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Timers;

namespace YW.Server.Socket
{
    public sealed class MySAE : SocketAsyncEventArgs
    {
        private Guid _socketId;
        private Guid _msgId;
        public Model.DeviceType DeviceType { get; set; }
        private int _time;
        public bool IsReceive= false;
        public bool IsUdp;
        public string Ip;
        public int Port;
        public int ErrorTime;
        public Model.Entity.DeviceState DeviceState { get; set; }
        public delegate void CloseSocketHandler(MySAE mySocketAsyncEventArg);
        public CloseSocketHandler RecoverHandler;
        public CloseSocketHandler DisconnectHandler;
        public int DeviceVoiceId { get; set; }
        private  int _index = 0;
        public readonly ConcurrentDictionary<string, Send> DictSend;
        public string Index
        {
            get
            {

                if (_index > 9999)
                {
                    _index = 0;
                }
                string str = Convert.ToString(_index, 16).PadLeft(4, '0');
                _index++;
                return str;

            }
        }
        public MySAE(bool isReceive, bool isUdp)
        {
            State = false;
            this.IsUdp = isUdp;
            this.IsReceive = isReceive;
            if (isReceive)
            {
                DictSend = new ConcurrentDictionary<string, Send>();
            }
        }
        public void StartTimer()
        {
            if (IsReceive)
            {
                _time = 0;
                SocketTimer.Timer.Add(this);
            }
        }
        public void EndTimer()
        {
            if (IsReceive)
            {
                SocketTimer.Timer.Remove(this);
            }
        }
        public void ClearTime()
        {
            if (IsReceive)
            {
                this._time = 0;
            }
        }
        public void Timer()
        {
            _time = _time + 1;
            try
            {
                if (_time >= 8)
                {
                    if (Data.Logger.IsDebugEnabled) {
                        Data.Logger.Debug("===计数器超时主动断开连接！===");
                    }
                    if (_time == 8)
                    {
                        DisconnectHandler?.Invoke(this);
                    }
                    else
                    {
                        RecoverHandler?.Invoke(this);
                    }
                }
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
        }
        public Guid SocketId
        {
            get { return _socketId; }
            set
            {
                _socketId = value;
            }
        }
        public Guid MsgId
        {
            get { return _msgId; }
            set
            {
                _msgId = value;
            }
        }

        public bool State { get; set; }

        public int BufferOffset { get; set; }

        public int BufferLength { get; set; }

        public void Recover()
        {

            this.SocketId = Guid.Empty;
            this.MsgId = Guid.Empty;
            this.State = false;
            this.EndTimer();
            this.ClearTime();
            this.DeviceState = null;
            this.Ip = null;
            this.Port = 0;
            this._index = 0;
            this.DeviceVoiceId = 0;
            this.DeviceType = 0;
            if (this.IsReceive)
            {
                DictSend.Clear();
            }
        }
    }
}
