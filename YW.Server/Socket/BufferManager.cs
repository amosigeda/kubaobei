using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace YW.Server.Socket
{
    public sealed class BufferManager : IDisposable
    {
        private Byte[] _buffer;
        private readonly Int32 _bufferSize;
        private readonly Int32 _numSize;
        private Int32 _currentIndex;

        public BufferManager(Int32 numSize, Int32 bufferSize)
        {
            this._bufferSize = bufferSize;
            this._numSize = numSize;
            this._currentIndex = 0;
        }

        public void InitBuffer()
        {
            this._buffer = new Byte[this._numSize];
        }

        public Boolean SetBuffer(MySAE args)
        {
            args.BufferLength = this._bufferSize*2; //2倍于发送缓冲区大小
            if ((this._numSize - args.BufferLength) < this._currentIndex)
            {
                return false;
            }
            args.BufferOffset = this._currentIndex;
            args.SetBuffer(this._buffer, args.BufferOffset, args.BufferLength);
            this._currentIndex += args.BufferLength;
            return true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _buffer = null;
        }

        #endregion
    }
}
