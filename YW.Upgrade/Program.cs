using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;

namespace YW.Upgrade
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    Log.GetInstance().WriteLog("开始升级");
                    string url = args[0];
                    const string fileName = "Upgrade.zip"; //客户端保存的文件名
                    String stmp = Assembly.GetExecutingAssembly().Location;
                    string path = stmp.Substring(0, stmp.LastIndexOf('\\')) + "\\" + fileName; //路径
                    if (Download(url, path))
                    {
                        StopService();
                        KillProgram();
                        bool zip = false;
                        while (!zip)
                        {
                            try
                            {
                                Log.GetInstance().WriteLog("解压缩文件");
                                UnZip(path, stmp.Substring(0, stmp.LastIndexOf('\\')));
                                zip = true;
                            }
                            catch (Exception ex)
                            {
                                Log.GetInstance().WriteLog(ex);
                                Thread.Sleep(10000);
                            }
                        }
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        StartService();
                        if (!CheckProgram())
                        {
                            Log.GetInstance().WriteLog("服务启动失败，手动启动程序");
                            var proc = new System.Diagnostics.Process
                            {
                                StartInfo = { FileName = stmp.Substring(0, stmp.LastIndexOf('\\')) + "\\YWServer.exe", Arguments = "/client" },
                                EnableRaisingEvents = true
                            };
                            proc.Start();
                            
                        }
                        Log.GetInstance().WriteLog("升级完成");
                    }
                    else
                    {
                        Log.GetInstance().WriteLog("获取升级文件失败");
                    }
                }
                catch (Exception ex)
                {
                    Log.GetInstance().WriteLog(ex);
                }

            }


        }

        private static bool Download(string url, string path)
        {

            bool flag = false;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            FileStream fStream = new FileStream(path, FileMode.Create);
            try
            {
                //打开网络连接
                HttpWebRequest myRequest = (HttpWebRequest) HttpWebRequest.Create(url);
                //向服务器请求,获得服务器的回应数据流
                using (Stream myStream = myRequest.GetResponse().GetResponseStream())
                {
                    byte[] btContent = new byte[512];
                    int intSize = 0;
                    intSize = myStream.Read(btContent, 0, 512);
                    while (intSize > 0)
                    {
                        fStream.Write(btContent, 0, intSize);
                        intSize = myStream.Read(btContent, 0, 512);
                    }
                    //关闭流
                    myStream.Close();
                }
                flag = true; //返回true下载成功
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                flag = false; //返回false下载失败
            }
            finally
            {
                fStream.Close();
            }
            return flag;
        }

        private static void UnZip(string fileFromUnZip, string fileToUnZip)
        {
            ZipInputStream inputStream = new
                ZipInputStream(File.OpenRead(fileFromUnZip));
            try
            {
                ZipEntry theEntry;
                while ((theEntry = inputStream.GetNextEntry()) != null)
                {
                    fileToUnZip += "/";
                    string fileName = Path.GetFileName(theEntry.Name);
                    string path = Path.GetDirectoryName(fileToUnZip) + "/";
                    //if (File.Exists(path + fileName))
                    //{
                    //    File.Delete(path + fileName);
                    //}
                    // Directory.CreateDirectory(path);//生成解压目录                 if (fileName != String.Empty)                 { 
                    FileStream streamWriter = File.Create(path + fileName); //解压文件到指定的目录  
                    try
                    {
                        int size = 2048;
                        byte[] data = new byte[size];
                        while (true)
                        {
                            size = inputStream.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    finally
                    {
                        streamWriter.Close();
                        streamWriter.Dispose();
                    }
                }
            }
            finally
            {
                inputStream.Close();
                inputStream.Dispose();
            }
        }


        private static void StartService()
        {
            //开启服务   
            ProcessStartInfo a = new ProcessStartInfo(@"c:/windows/system32/cmd.exe", "/c  net start YWServer")
            {
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process process = Process.Start(a);
            while (!process.HasExited)
            {
                Thread.Sleep(500);
            }
            Log.GetInstance().WriteLog("服务启动");
        }

        private static void StopService()
        {
            //开启服务   
            ProcessStartInfo a = new ProcessStartInfo(@"c:/windows/system32/cmd.exe", "/c  net stop YWServer")
            {
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process process = Process.Start(a);
            while (!process.HasExited)
            {
                Thread.Sleep(500);
            }

            Log.GetInstance().WriteLog("服务关闭");
        }

        private static void KillProgram()
        {
            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process myProcess in myProcesses)
            {
                if ("YWServer" == myProcess.ProcessName)
                    myProcess.Kill(); //强制关闭该程序
            }
        }

        private static bool CheckProgram()
        {
            System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
            bool isRun=false;
            foreach (System.Diagnostics.Process myProcess in myProcesses)
            {
                if ("YWServer" == myProcess.ProcessName)
                    isRun = true;
            }
            return isRun;
        }
    }
}
