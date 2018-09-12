using System;
using System.ServiceModel;
using YW.Contracts;

namespace YW.TestApi
{
    class Client
    {
        public static Guid LoginId { get; set; }
        private readonly ChannelFactory<IClient> _channelFactory;
        private static Client _object;
        private static readonly object LockHelper=new object();

        public static void Clear()
        {
            _object=null;
        }

        public static IClient Get()
        {
            if (_object == null)
            {
                lock (LockHelper)
                {
                    if (_object == null)
                    {
                        _object = new Client();
                    }
                }
            }
            return _object._channelFactory.CreateChannel();
        }
        public Client()
        {
            _channelFactory = new ChannelFactory<IClient>("httpEndpoint");
        }
    }
}
