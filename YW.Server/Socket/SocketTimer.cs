using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace YW.Server.Socket
{
    class SocketTimer
    {
        public static SocketTimer Timer = new SocketTimer();
        readonly List<MySAE> _list;

        public SocketTimer()
        {
            _list = new List<MySAE>();
        }
        public void Do()
        {
            List<MySAE> tList;
            lock (_list)
            {
                tList = _list.ToList();
            }
            foreach (MySAE t in tList)
            {
                t.Timer();
            }
        }
        public void Add(MySAE arg)
        {
            lock (_list)
                    _list.Add(arg);
        }

        public void Remove(MySAE arg)
        {
            lock (_list)
                _list.Remove(arg);
        }
    }
}
