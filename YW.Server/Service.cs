using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace YW.Server
{
    public partial class Service : ServiceBase
    {
        private Server server;      
        public Service()
        {
            InitializeComponent();
            server = new Server();
        }

        protected override void OnStart(string[] args)
        {
            server.Start();
            Data.Logger.Debug("服务器启动");
        }

        protected override void OnStop()
        {
            server.Stop();
            Data.Logger.Debug("服务器停止");
        }
    }
}
