using System;
using System.Collections.Generic;

namespace YW.Server.Socket
{
    public sealed class SAEPool : IDisposable
    {
        internal Queue<MySAE> Pool;
        internal IDictionary<Guid, MySAE> BusyPool;
        private readonly Guid[] _keys;
        public Int32 Count
        {
            get
            {
                lock (this.Pool)
                {
                    return this.Pool.Count;
                }
            }
        }

        public SAEPool(Int32 capacity)
        {
            _keys = new Guid[capacity + 1];
            this.Pool = new Queue<MySAE>(capacity);
            this.BusyPool = new Dictionary<Guid, MySAE>(capacity);
        }

        public MySAE Pop(Guid msgId)
        {
            if (msgId == Guid.Empty)
                return null;
            MySAE si = null;
            lock (BusyPool)
            {
                if (!this.BusyPool.ContainsKey(msgId))
                {

                    lock (this.Pool)
                    {
                        if (this.Pool.Count > 0)
                            si = this.Pool.Dequeue();
                        else
                            return null;
                    }
                    si.MsgId = msgId;
                    si.SocketId = msgId;
                    si.State = true;
                    si.StartTimer();
                    BusyPool.Add(msgId, si);
                }
            }
            return si;
        }
        public void Push(MySAE item)
        {
            if (item.State == true)
            {
                lock (BusyPool)
                    if (BusyPool.Keys.Count != 0)
                    {
                        if (BusyPool.Keys.Contains(item.MsgId))
                            BusyPool.Remove(item.MsgId);
                    }
            }

            item.Recover();
            lock (this.Pool)
            {
                this.Pool.Enqueue(item);
            }
        }
        public bool CheckPush(MySAE item)
        {
            if (item == null)
                return false;
            if (item.State == true)
            {
                lock (BusyPool)
                {
                    if (BusyPool.Keys.Contains(item.MsgId))
                    {
                        BusyPool.Remove(item.MsgId);
                        item.Recover();
                        lock (this.Pool)
                        {
                            this.Pool.Enqueue(item);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public MySAE FindById(Guid msgId)
        {
            if (msgId == Guid.Empty)
                return null;
            MySAE si = null;
            lock (BusyPool)
            {
                if (BusyPool.Keys.Contains(msgId))
                    si = BusyPool[msgId];
                else
                    return null;
            }
            return si;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Pool.Clear();
            lock (BusyPool)
                BusyPool.Clear();
            Pool = null;
            BusyPool = null;
        }
        #endregion
    }
}