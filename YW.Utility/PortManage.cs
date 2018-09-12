using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace YW.Utility
{
    class PortManage
    {
        private static PortManage _PortManage;
        private static object LockHelper = new object();
        public const int MaxPort = 65535;
        public const int MinPort = 20000;
        internal Stack<int> tcpPool;
        internal Stack<int> udpPool;
        private System.Timers.Timer timer;
        public PortManage()
        {
            tcpPool = new Stack<int>();
            udpPool = new Stack<int>();
            timer = new System.Timers.Timer(60000); //10分钟更新一次
            timer.AutoReset = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;
            InitPool();
        }
        public static PortManage GetInstance()
        {
            lock (LockHelper)
            {
                if (_PortManage==null)
                {
                    _PortManage=new PortManage();
                }
            }
            return _PortManage;
        }
        private void InitPool()
        {
            tcpPool.Clear();
            IList portUsed = SystemInfo.getTCPUsePort();
            for (int i = MaxPort; i > MinPort; i--)
            {
                if (!portUsed.Contains(i))
                    this.tcpPool.Push(i);
            }
            portUsed.Clear();
            udpPool.Clear();
            portUsed = SystemInfo.getUDPUsePort();
            for (int i = MaxPort; i > MinPort; i--)
            {
                if (!portUsed.Contains(i))
                    this.udpPool.Push(i);
            }
            portUsed.Clear();
            timer.Interval = 60000;
        }
        public int GetTCP()
        {
            lock (tcpPool)
            {
                if (tcpPool.Count < 100)
                    InitPool();
            }
            lock (tcpPool)
            {
                return tcpPool.Pop();
            }
        }
        public int GetUDP()
        {
            lock (udpPool)
            {
                if (udpPool.Count < 100)
                    InitPool();
            }
            lock (udpPool)
            {
                return udpPool.Pop();
            }
        }
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (tcpPool)
                lock(udpPool)
                    InitPool();
        }
    }
}
