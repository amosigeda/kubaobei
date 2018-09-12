using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace YW.Logic
{
    public class LBSWIFIClient
    {
        private static LBSWIFIClient _object;
        private static readonly object LockHelper=new object();
        private static Api.Contracts.IClient _channel;
        public LBSWIFIClient()
        {
            var channelFactory = new ChannelFactory<Api.Contracts.IClient>("Api");
            _channel = channelFactory.CreateChannel();
        }
        public static Api.Contracts.IClient Get()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new LBSWIFIClient();
                    }
                }
            }
            return _channel;
        }
    }
}
