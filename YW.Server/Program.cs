using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace YW.Server
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            System.Uri uri = new Uri(typeof(string).Assembly.CodeBase);
            string runtimePath = System.IO.Path.GetDirectoryName(uri.LocalPath);
            string strInstallUtilPath = System.IO.Path.Combine(runtimePath, "InstallUtil.exe");
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg == "/install")
                {
                    System.Diagnostics.Process.Start(strInstallUtilPath,
                        "\"" + Application.ExecutablePath + "\"");
                    return;
                }
                else if (arg == "/uninstall")
                {
                    System.Diagnostics.Process.Start(strInstallUtilPath,
                        "/u \"" + Application.ExecutablePath + "\"");
                    return;
                }
                else if (arg == "/client")
                {
                    // 启动客户端
                    Server server = new Server();
                    server.Start();
                    System.Console.ReadLine();
                    server.Stop();
                    return;
                }
            }
            // 运行服务对象
            ServiceBase.Run(new Service());
        }
    }
}
