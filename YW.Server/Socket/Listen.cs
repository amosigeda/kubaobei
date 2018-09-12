using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace YW.Server.Socket
{
    public class Listen : IDisposable
    {
        private System.Net.Sockets.Socket _listenSocketTcp;
        private System.Net.Sockets.Socket _listenSocketUdp;
        private IPEndPoint _localEndAny;
        private readonly Int32 _maxConnect;
        private readonly Int32 _receiveBufferSize;
        private BufferManager _tcpReceiveBufferManager;
        private BufferManager _udpReceiveBufferManager;
        private SAEPool _tcpReceiveSaePool;
        private SAEPool _udpReceiveSaePool;
        private SAEPool _sendSaePool;

        public delegate void AcceptConnectHandler(MySAE mySae);

        public event AcceptConnectHandler OnAcceptConnect;

        public delegate void DisConnectHandler(MySAE mySae);

        public event DisConnectHandler OnDisConnect;

        public delegate bool ReceiveMsgHandler(MySAE mySae, byte[] bytes, int startOffset, int length, ref int msgLength
        );

        public event ReceiveMsgHandler OnMsgReceived;
        private Semaphore _tcpReceiveSemaphoreSendedClients;
        private Semaphore _udpReceiveSemaphoreSendedClients;
        private Semaphore _sendSemaphoreSendedClients;
        private Dictionary<int, Dictionary<System.Net.IPEndPoint, MySAE>> _udpListConnect;
        private readonly int _totalUdpListConnect;
        private readonly int _port;
        private readonly bool _log;
        private bool isRun;

        public Listen()
        {
            this._port = int.Parse(Utility.AppConfig.GetValue("Port"));
            this._receiveBufferSize = int.Parse(Utility.AppConfig.GetValue("BufferSize")); //1024字节
            this._maxConnect = int.Parse(Utility.AppConfig.GetValue("MaxConnect"));
            this._log = bool.Parse(Utility.AppConfig.GetValue("Log"));
            this._sendSaePool = new SAEPool(_maxConnect);
            this._sendSemaphoreSendedClients = new Semaphore(_maxConnect, _maxConnect);
            if (this._port > 0)
            {
                this._tcpReceiveBufferManager = new BufferManager(_receiveBufferSize * _maxConnect * 2, _receiveBufferSize);
                this._tcpReceiveSaePool = new SAEPool(_maxConnect);
                this._tcpReceiveSemaphoreSendedClients = new Semaphore(_maxConnect, _maxConnect);
                this._udpReceiveBufferManager = new BufferManager(_receiveBufferSize * _maxConnect * 2, _receiveBufferSize);
                this._udpReceiveSemaphoreSendedClients = new Semaphore(_maxConnect, _maxConnect);
                this._udpReceiveSaePool = new SAEPool(_maxConnect);
            }

            this._totalUdpListConnect = 20;
        }

        /// <summary>
        /// 服务端初始化
        /// </summary>
        public void Init()
        {
            if (this._port > 0)
            {
                this._tcpReceiveBufferManager.InitBuffer();
                this._udpReceiveBufferManager.InitBuffer();
                _udpListConnect = new Dictionary<int, Dictionary<IPEndPoint, MySAE>>();
                for (int i = 0; i < this._totalUdpListConnect; i++)
                {
                    Dictionary<IPEndPoint, MySAE> ms = new Dictionary<IPEndPoint, MySAE>();
                    _udpListConnect.Add(i, ms);
                }
            }

            for (Int32 i = 0; i < this._maxConnect; i++)
            {
                MySAE mysae = new MySAE(false, false);
                mysae.Completed += OnSendCompleted;
                this._sendSaePool.Push(mysae);
                if (this._port > 0)
                {
                    mysae = new MySAE(true, false);
                    mysae.Completed += new EventHandler<SocketAsyncEventArgs>(OnTcpReceiveCompleted);
                    this._tcpReceiveBufferManager.SetBuffer(mysae);
                    mysae.RecoverHandler = RecoverTcpSocket;
                    mysae.DisconnectHandler = CloseTCPSocket;
                    this._tcpReceiveSaePool.Push(mysae);
                    mysae = new MySAE(true, true);
                    this._udpReceiveBufferManager.SetBuffer(mysae);
                    mysae.RecoverHandler = SyncCloseUdpSocket;
                    mysae.DisconnectHandler = SyncCloseUdpSocket;
                    this._udpReceiveSaePool.Push(mysae);
                }
            }
        }

        public void Start()
        {
            isRun = true;
            _localEndAny = new IPEndPoint(IPAddress.Any, 0);
            if (this._port > 0)
            {
                IPEndPoint tcpLocalEndPoint = new IPEndPoint(IPAddress.Any, this._port);
                this._listenSocketTcp = new System.Net.Sockets.Socket(tcpLocalEndPoint.AddressFamily, SocketType.Stream,
                    ProtocolType.Tcp);
                if (tcpLocalEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    this._listenSocketTcp.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName) 27, false);
                    this._listenSocketTcp.Bind(new IPEndPoint(IPAddress.IPv6Any, tcpLocalEndPoint.Port));
                }
                else
                {
                    this._listenSocketTcp.Bind(tcpLocalEndPoint);
                }

                this._listenSocketTcp.Listen(100);
                this.StartTcpAccept(null);
                IPEndPoint udpLocalEndPoint = new IPEndPoint(IPAddress.Any, this._port);
                this._listenSocketUdp = new System.Net.Sockets.Socket(udpLocalEndPoint.AddressFamily, SocketType.Dgram,
                    ProtocolType.Udp);
                this._listenSocketUdp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this._listenSocketUdp.DontFragment = true;
                this._listenSocketUdp.Ttl = 255;
                if (udpLocalEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    this._listenSocketUdp.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName) 27, false);
                    this._listenSocketUdp.Bind(new IPEndPoint(IPAddress.IPv6Any, udpLocalEndPoint.Port));
                }
                else
                {
                    this._listenSocketUdp.Bind(udpLocalEndPoint);
                }

                for (int i = 0; i < 80; i++)
                    this.StartUdpAccept(null);
            }
        }

        #region 消息接收

        private void StartTcpAccept(SocketAsyncEventArgs acceptEventArg)
        {
            try
            {
                if (acceptEventArg == null)
                {
                    acceptEventArg = new SocketAsyncEventArgs();
                    acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnTcpAcceptCompleted);
                }
                else
                    acceptEventArg.AcceptSocket = null;

                if (isRun)
                {
                    Boolean willRaiseEvent = this._listenSocketTcp.AcceptAsync(acceptEventArg);
                    if (!willRaiseEvent)
                    {
                        this.OnTcpAcceptCompleted(this, acceptEventArg);
                    }
                }
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
        }

        private void StartUdpAccept(MySAE mysae)
        {
            if (mysae == null)
            {
                byte[] udpBuffer = new byte[this._receiveBufferSize * 2];
                mysae = new MySAE(true, true);
                mysae.Completed += new EventHandler<SocketAsyncEventArgs>(OnUdpAcceptCompleted);
                mysae.SetBuffer(udpBuffer, 0, udpBuffer.Length);
                mysae.BufferOffset = 0;
                mysae.BufferLength = udpBuffer.Length;
                mysae.RemoteEndPoint = _localEndAny;
                mysae.SocketId = Guid.NewGuid();
                mysae.MsgId = mysae.SocketId;
            }

            if (isRun)
            {
                if (!_listenSocketUdp.ReceiveFromAsync(mysae))
                {
                    OnUdpAcceptCompleted(this, mysae);
                }
            }
        }

        private void OnTcpAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.LastOperation != SocketAsyncOperation.Accept) //检查上一次操作是否是Accept，不是就返回
                {
                    return;
                }

                this._tcpReceiveSemaphoreSendedClients.WaitOne();
                MySAE mysae = this._tcpReceiveSaePool.Pop(Guid.NewGuid());
                if (mysae == null)
                {
                    return;
                }
                try
                {
                    mysae.MsgId = mysae.SocketId;
                    mysae.UserToken = e.AcceptSocket;
                    mysae.IsUdp = false;
                    mysae.Ip = ((IPEndPoint) (e.AcceptSocket.RemoteEndPoint)).Address.ToString();
                    mysae.Port = ((IPEndPoint) (e.AcceptSocket.RemoteEndPoint)).Port;
                }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
                catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
                {
                    //Data.Logger.Error(ex);
                }

                if (OnAcceptConnect != null)
                {
                    try
                    {
                        this.OnAcceptConnect(mysae);
                    }
                    catch (Exception ex)
                    {
                        Data.Logger.Error(ex);
                    }
                }

                if (!e.AcceptSocket.ReceiveAsync(mysae))
                {
                    OnTcpReceiveCompleted(this, mysae);
                }

                this.StartTcpAccept(e);
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
        }

        private void OnTcpReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            MySAE mysae = (MySAE) e;
            lock (mysae)
            {
                if (mysae.LastOperation != SocketAsyncOperation.Receive)
                    return;
                if (mysae.SocketError == SocketError.Success && mysae.BytesTransferred > 0)
                {
                    mysae.ClearTime();
                    try
                    {
                        if (_log && Data.Logger.IsDebugEnabled)
                        {
                            Data.Logger.Debug(String.Format("{0}:{1}	TCP接收:[{2}]\n{3}", mysae.Ip, mysae.Port,
                                Utility.Convert.HexByteToStr(mysae.Buffer, mysae.Offset,
                                    mysae.BytesTransferred),
                                System.Text.Encoding.UTF8.GetString(mysae.Buffer,
                                    mysae.Offset, mysae.BytesTransferred)));
                        }

                        int bufferOffset = mysae.BufferOffset;
                        int bufferLastOffset = mysae.BufferOffset + mysae.BufferLength;
                        int receiveBufferLastOffset = mysae.Offset + mysae.BytesTransferred;
                        while (bufferOffset < receiveBufferLastOffset)
                        {
                            int msgLength = 1;
                            if (this.OnMsgReceived(mysae, mysae.Buffer, bufferOffset, receiveBufferLastOffset - bufferOffset, ref msgLength))
                            {
                                bufferOffset = bufferOffset + msgLength;
                                mysae.ErrorTime = 0;
                            }
                            else
                            {
                                if (bufferOffset < mysae.Offset)
                                {
                                    bufferOffset = mysae.Offset;
                                }
                                else
                                {
                                    mysae.ErrorTime = mysae.ErrorTime + 1;
                                    if (bufferOffset == mysae.Offset)
                                        bufferOffset = mysae.BufferOffset;
                                    if (bufferLastOffset - (receiveBufferLastOffset - bufferOffset) <
                                        this._receiveBufferSize || mysae.ErrorTime > 10)
                                    {
                                        bufferOffset = receiveBufferLastOffset;
                                        mysae.ErrorTime = 0;
                                    }

                                    break;
                                }
                            }
                        }

                        if (bufferOffset != mysae.BufferOffset)
                        {
                            if (bufferOffset < receiveBufferLastOffset) //移动缓存区
                            {
                                Array.Copy(mysae.Buffer, bufferOffset, mysae.Buffer, mysae.BufferOffset,
                                    receiveBufferLastOffset - bufferOffset);
                                receiveBufferLastOffset = mysae.BufferOffset + receiveBufferLastOffset - bufferOffset;
                            }
                            else
                            {
                                receiveBufferLastOffset = mysae.BufferOffset;
                            }
                        }

                        mysae.SetBuffer(receiveBufferLastOffset, bufferLastOffset - receiveBufferLastOffset);
                    }
                    catch (Exception ex)
                    {
                        Data.Logger.Error(ex);
                    }

                    try
                    {
                        Boolean willRaiseEvent = (mysae.UserToken as System.Net.Sockets.Socket).ReceiveAsync(mysae);
                        if (!willRaiseEvent)
                            OnTcpReceiveCompleted(this, mysae);
                    }
                    catch (ObjectDisposedException)
                    {
                        this.RecoverTcpSocket(mysae);
                    }
                }
                else
                {
                    this.RecoverTcpSocket(mysae);
                }
            }
        }

        private void OnUdpAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            MySAE mysae = (MySAE) e;
            try
            {
                if (mysae.LastOperation == SocketAsyncOperation.ReceiveFrom)
                {
                    if (mysae.BytesTransferred > 0)
                    {
                        if (mysae.SocketError == SocketError.Success)
                        {
                            Int32 byteTransferred = mysae.BytesTransferred;
                            try
                            {
                                IPEndPoint ipEndPoint = mysae.RemoteEndPoint as IPEndPoint;
                                if (_log && Data.Logger.IsDebugEnabled)
                                    Data.Logger.Debug(String.Format("{0}:{1}	UDP接收:[{2}]\n{3}",
                                        ipEndPoint.Address.ToString(),
                                        ipEndPoint.Port,
                                        Utility.Convert.HexByteToStr(mysae.Buffer, mysae.Offset, mysae.BytesTransferred),
                                        System.Text.Encoding.UTF8.GetString(mysae.Buffer, mysae.Offset,
                                            mysae.BytesTransferred)));
                                int hashCode = ipEndPoint.Port % this._totalUdpListConnect;
                                MySAE themysae = null;
                                lock (_udpListConnect[hashCode])
                                {
                                    if (!_udpListConnect[hashCode].ContainsKey(ipEndPoint))
                                    {
                                        this._udpReceiveSemaphoreSendedClients.WaitOne();
                                        themysae = this._udpReceiveSaePool.Pop(Guid.NewGuid());
                                        if (themysae == null)
                                        {
                                            this._udpReceiveSemaphoreSendedClients.Release();
                                            return;
                                        }

                                        themysae.RemoteEndPoint = mysae.RemoteEndPoint;
                                        themysae.Ip = ipEndPoint.Address.ToString();
                                        themysae.Port = ipEndPoint.Port;
                                        _udpListConnect[hashCode].Add(ipEndPoint, themysae);
                                        if (OnAcceptConnect != null)
                                        {
                                            try
                                            {
                                                this.OnAcceptConnect(themysae);
                                            }
                                            catch (Exception ex)
                                            {
                                                Data.Logger.Error(ex);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        themysae = this._udpListConnect[hashCode][ipEndPoint];
                                    }
                                }

                                lock (themysae)
                                {
                                    if (themysae.SocketId == Guid.Empty)
                                        return;
                                    themysae.ClearTime();
                                    if ((themysae.BufferLength - (themysae.Offset - themysae.BufferOffset)) >=
                                        mysae.BytesTransferred)
                                    {
                                        Array.Copy(mysae.Buffer, mysae.Offset, themysae.Buffer, themysae.Offset,
                                            mysae.BytesTransferred);
                                        try
                                        {
                                            int bufferOffset = themysae.BufferOffset;
                                            int bufferLastOffset = themysae.BufferOffset + themysae.BufferLength;
                                            int receiveBufferLastOffset = themysae.Offset + mysae.BytesTransferred;
                                            while (bufferOffset < receiveBufferLastOffset)
                                            {
                                                int msgLength = 1;
                                                if (this.OnMsgReceived(themysae, themysae.Buffer, bufferOffset,
                                                    receiveBufferLastOffset - bufferOffset, ref msgLength))
                                                {
                                                    bufferOffset = bufferOffset + msgLength;
                                                    themysae.ErrorTime = 0;
                                                }
                                                else
                                                {
                                                    if (bufferOffset < themysae.Offset)
                                                    {
                                                        bufferOffset = themysae.Offset;
                                                    }
                                                    else
                                                    {
                                                        themysae.ErrorTime++;
                                                        if (bufferOffset == themysae.Offset)
                                                            bufferOffset = themysae.BufferOffset;
                                                        if (bufferLastOffset - (receiveBufferLastOffset - bufferOffset) <
                                                            this._receiveBufferSize || themysae.ErrorTime > 10)
                                                        {
                                                            bufferOffset = receiveBufferLastOffset;
                                                            themysae.ErrorTime = 0;
                                                        }

                                                        break;
                                                    }
                                                }
                                            }

                                            if (bufferOffset != themysae.BufferOffset)
                                            {
                                                if (bufferOffset < receiveBufferLastOffset) //移动缓存区
                                                {
                                                    Array.Copy(themysae.Buffer, bufferOffset, themysae.Buffer,
                                                        themysae.BufferOffset, receiveBufferLastOffset - bufferOffset);
                                                    receiveBufferLastOffset = themysae.BufferOffset +
                                                                              receiveBufferLastOffset - bufferOffset;
                                                }
                                                else
                                                {
                                                    receiveBufferLastOffset = themysae.BufferOffset;
                                                }
                                            }

                                            themysae.SetBuffer(receiveBufferLastOffset,
                                                bufferLastOffset - receiveBufferLastOffset);
                                        }
                                        catch (Exception ex)
                                        {
                                            Data.Logger.Error(ex);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Data.Logger.Error(ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
            finally
            {
                StartUdpAccept(mysae);
            }
        }

        private void CloseTCPSocket(MySAE mysae)
        {
            try
            {
                if (_log && Data.Logger.IsDebugEnabled)
                    Data.Logger.Debug(String.Format("{0}:{1}	TCP断开", mysae.Ip, mysae.Port));
                System.Net.Sockets.Socket s = mysae.UserToken as System.Net.Sockets.Socket;
                if (s != null && s.Connected)
                {
                    try
                    {
                        s.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }

                    s.Close();
                }
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
        }

        private void RecoverTcpSocket(MySAE mysae)
        {
            try
            {
                if (mysae.SocketId == Guid.Empty)
                    return;
                if (OnDisConnect != null)
                {
                    try
                    {
                        OnDisConnect(mysae);
                    }
                    catch (Exception ex)
                    {
                        Data.Logger.Error(ex);
                    }
                }

                CloseTCPSocket(mysae);
                mysae.SetBuffer(mysae.BufferOffset, mysae.BufferLength);
                this._tcpReceiveSaePool.CheckPush(mysae);
                this._tcpReceiveSemaphoreSendedClients.Release();
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
        }

        private void CloseUdpSocket(Guid socketId)
        {
            try
            {
                if (socketId == Guid.Empty)
                    return;
                MySAE mySocketAsyncEventArg = this._udpReceiveSaePool.FindById(socketId);
                if (mySocketAsyncEventArg == null)
                    return;
                CloseUdpSocket(mySocketAsyncEventArg);
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
        }

        private delegate void DisconnectAsyncEventHandler(MySAE mysae);

        private void SyncCloseUdpSocket(MySAE mysae)
        {
            DisconnectAsyncEventHandler asy = new DisconnectAsyncEventHandler(CloseUdpSocket);
            IAsyncResult ia = asy.BeginInvoke(mysae, null, asy);
        }

        private void CloseUdpSocket(MySAE mysae)
        {
            if (mysae.SocketId == Guid.Empty)
                return;
            IPEndPoint ipEndPoint = mysae.RemoteEndPoint as IPEndPoint;
            if (ipEndPoint != null)
            {
                int hashCode = ipEndPoint.Port % this._totalUdpListConnect;
                lock (this._udpListConnect[hashCode])
                {
                    if (this._udpListConnect[hashCode].ContainsKey(ipEndPoint))
                    {
                        this._udpReceiveSemaphoreSendedClients.Release();
                        lock (mysae)
                        {
                            if (mysae.SocketId == Guid.Empty)
                                return;
                            if (OnDisConnect != null)
                            {
                                try
                                {
                                    OnDisConnect(mysae);
                                }
                                catch (Exception ex)
                                {
                                    Data.Logger.Error(ex);
                                }
                            }

                            this._udpListConnect[hashCode].Remove(ipEndPoint);
                            mysae.SetBuffer(mysae.BufferOffset, mysae.BufferLength);
                            this._udpReceiveSaePool.Push(mysae);
                            if (_log && Data.Logger.IsDebugEnabled)
                                Data.Logger.Debug(String.Format("{0}:{1}	UDP断开", ipEndPoint.Address.ToString(),
                                    ipEndPoint.Port));
                        }
                    }
                }
            }
        }

        #endregion


        #region 消息发送

        public bool SendBinaryByTcp(MySAE mysae, Guid msgId, byte[] bytes, int index, int length)
        {
            try
            {
                if (mysae == null)
                {
                    return false;
                }


                if (mysae.SocketId == Guid.Empty)
                    return false;
                this._sendSemaphoreSendedClients.WaitOne();
                MySAE sendSae = this._sendSaePool.Pop(msgId);
                if (sendSae == null)
                {
                    this._sendSemaphoreSendedClients.Release();
                    return false;
                }

                lock (sendSae)
                {
                    sendSae.UserToken = mysae.UserToken;
                    sendSae.SocketId = mysae.SocketId;
                    sendSae.Ip = mysae.Ip;
                    sendSae.Port = mysae.Port;

                    if (bytes != null && bytes.Length >= index + length)
                    {
                        sendSae.SetBuffer(bytes, index, length);
                        return this.SendByTcp(sendSae, length, 1);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
                return false;
            }
        }

        private bool SendByTcp(MySAE mySae, int length, int sendTimes)
        {
            try
            {
                if (_log && Data.Logger.IsDebugEnabled)
                    Data.Logger.Debug(String.Format("{0}:{1}	TCP发送:[{2}]\n{3}",
                        mySae.Ip, mySae.Port,
                        Utility.Convert.HexByteToStr(mySae.Buffer, mySae.Offset, length),
                        System.Text.Encoding.UTF8.GetString(mySae.Buffer, mySae.Offset,
                            length)));
                try
                {
                    Boolean willRaiseEvent = (mySae.UserToken as System.Net.Sockets.Socket).SendAsync(mySae);
                    if (!willRaiseEvent)
                    {
                        this.OnSendCompleted(this, mySae);
                    }

                    return true;
                }
                catch (ObjectDisposedException e)
                {
                    if (Data.Logger.IsDebugEnabled)
                    {
                        Data.Logger.Debug(String.Format("Listen.cs==连接断开：{0},异常：{1}", mySae, e.Message));
                    }

                    this.CloseSendSocket(mySae);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
                if (sendTimes <= 2)
                {
                    Thread.Sleep(10);
                    return SendByTcp(mySae, length, sendTimes + 1);
                }
                else
                {
                    this.CloseSendSocket(mySae);
                    return false;
                }
            }
        }

        public bool SendBinaryByUdp(MySAE mySae, Guid msgId, byte[] bytes, int index, int length)
        {
            try
            {
                if (mySae == null)
                {
                    return false;
                }
                else
                {
                    if (mySae.SocketId == Guid.Empty)
                        return false;
                    this._sendSemaphoreSendedClients.WaitOne();
                    MySAE sendSae = this._sendSaePool.Pop(msgId);
                    if (sendSae == null)
                    {
                        this._sendSemaphoreSendedClients.Release();
                        return false;
                    }

                    sendSae.UserToken = mySae.UserToken;
                    sendSae.RemoteEndPoint = mySae.RemoteEndPoint;
                    sendSae.IsUdp = true;
                    sendSae.Ip = mySae.Ip;
                    sendSae.Port = mySae.Port;
                    while (length > sendSae.BufferLength)
                    {
                        SendBinaryByUdp(mySae, Guid.NewGuid(), bytes, index, sendSae.BufferLength);
                        index = index + sendSae.BufferLength;
                        length = length - sendSae.BufferLength;
                    }

                    mySae.SetBuffer(bytes, index, length);
                    return SendByUdp(sendSae, length, 1);
                }
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
                return false;
            }
        }

        private bool SendByUdp(MySAE mySae, int length, int sendTimes)
        {
            try
            {
                if (_log && Data.Logger.IsDebugEnabled)
                    Data.Logger.Debug(String.Format("{0}:{1}	UDP发送:[{2}]\n{3}", mySae.Ip, mySae.Port,
                        Utility.Convert.HexByteToStr(mySae.Buffer, mySae.Offset, length),
                        System.Text.Encoding.UTF8.GetString(mySae.Buffer, mySae.Offset, length)));
                Boolean willRaiseEvent = this._listenSocketUdp.SendToAsync(mySae);
                if (!willRaiseEvent)
                {
                    this.OnSendCompleted(this, mySae);
                }

                return true;
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
                if (sendTimes <= 2)
                {
                    Thread.Sleep(10);
                    return SendByUdp(mySae, length, sendTimes + 1);
                }
                else
                {
                    this.CloseSendSocket(mySae);
                    return false;
                }
            }
        }

        /// <summary>
        /// 发送成功
        /// </summary>
        /// <param name="sender">来源</param>
        /// <param name="e">Socket连接套接字</param>
        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                MySAE mySae = (MySAE) e;
                if (mySae.IsUdp)
                {
                    if (mySae.LastOperation != SocketAsyncOperation.SendTo)
                        return;
                }
                else
                {
                    if (mySae.LastOperation != SocketAsyncOperation.Send)
                        return;
                }

                this.CloseSendSocket(mySae);
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
        }

        private void CloseSendSocket(MySAE mySae)
        {
            try
            {
                mySae.RemoteEndPoint = null;
                mySae.IsUdp = false;
                mySae.SetBuffer(mySae.BufferOffset, mySae.BufferLength);
                _sendSaePool.Push(mySae);
                _sendSemaphoreSendedClients.Release();
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
        }

        #endregion

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            isRun = false;
            try
            {
                if (_listenSocketTcp != null)
                {
                    _listenSocketTcp.Close();
                    //listenSocketTCP.Dispose();
                }

                if (_listenSocketUdp != null)
                {
                    _listenSocketUdp.Close();
                    //listenSocketUDP.Dispose();
                }
            }
            catch (Exception ex)
            {
                Data.Logger.Error(ex);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            this._sendSaePool.Dispose();
            this._sendSaePool = null;
            this._sendSemaphoreSendedClients.Close();
            //this.sendSemaphoreSendedClients.Dispose();
            this._sendSemaphoreSendedClients = null;
            if (this._port > 0)
            {
                this._tcpReceiveBufferManager.Dispose();
                this._tcpReceiveBufferManager = null;
                this._tcpReceiveSaePool.Dispose();
                this._tcpReceiveSaePool = null;
                this._tcpReceiveSemaphoreSendedClients.Close();
                //this.tcpReceiveSemaphoreSendedClients.Dispose();
                this._tcpReceiveSemaphoreSendedClients = null;

                this._udpReceiveBufferManager.Dispose();
                this._udpReceiveBufferManager = null;
                this._udpReceiveSaePool.Dispose();
                this._udpReceiveSaePool = null;
                this._udpReceiveSemaphoreSendedClients.Close();
                //this.udpReceiveSemaphoreSendedClients.Dispose();
                this._udpReceiveSemaphoreSendedClients = null;
            }

            _listenSocketTcp = null;
            _listenSocketUdp = null;
        }

        #endregion
    }
}