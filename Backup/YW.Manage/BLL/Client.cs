using System;
using System.Collections.Generic;
using System.ServiceModel;
using YW.Contracts;

namespace YW.Manage.BLL
{
    class Client
    {
        public static Guid LoginId { get; set; }
        private readonly ChannelFactory<IClient> _channelFactory;
        private static readonly Dictionary<string, Client> Dictionary = new Dictionary<string, Client>();
        private static readonly object LockHelper=new object();

        public static void Clear()
        {
            lock (LockHelper)
                Dictionary.Clear();
        }

        public static IClient Get(string ipAndrPort)
        {
            Client client;
            if (!Dictionary.TryGetValue(ipAndrPort, out client))
            {
                lock (LockHelper)
                {
                    if (!Dictionary.ContainsKey(ipAndrPort))
                    {
                        client = new Client(ipAndrPort);
                        Dictionary.Add(ipAndrPort, client);
                    }
                    else
                    {
                        client = Dictionary[ipAndrPort];
                    }
                }
            }
            return client.CreateChannel();
        }
        public Client(string ipAndrPort)
        {
            var remoteAddress = new EndpointAddress("http://" + ipAndrPort + "/Client");
            _channelFactory = new ChannelFactory<IClient>("httpEndpoint",remoteAddress);
        }

        public IClient CreateChannel()
        {
            return _channelFactory.CreateChannel();
        }
    }
}
