using System;
using System.Collections.Generic;
using System.Configuration;
using System.Management;
using System.Text;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Net;
using System.Collections;

namespace YW.Utility
{
    public class SystemInfo
    {
        public static string GetCpuId()
        { 
            ManagementClass mClass = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = mClass.GetInstances();
            string cpuId=null;
            foreach (ManagementObject mo in moc)
            {            
                cpuId = mo.Properties["ProcessorId"].Value.ToString();
                break;
            }
            return cpuId;
        }
        public static int GetCpuCount()
        {
            ManagementClass mClass = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = mClass.GetInstances();
            int cpuCount = 0;
            foreach (ManagementObject mo in moc)
            {
                PropertyDataCollection properties = mo.Properties;
                cpuCount += int.Parse(properties["NumberOfLogicalProcessors"].Value.ToString());
                break;
            }
            return cpuCount;
        }
        public static IList getTCPUsePort()
        {
            //获取本地计算机的网络连接和通信统计数据的信息 
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();
            //返回本地计算机上的所有UDP监听程序 
            //IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();
            //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。 
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            IList allPorts = new ArrayList();
            foreach (IPEndPoint ep in ipsTCP) allPorts.Add(ep.Port);
            foreach (TcpConnectionInformation conn in tcpConnInfoArray) allPorts.Add(conn.LocalEndPoint.Port);
            ipsTCP = null;
            tcpConnInfoArray = null;
            return allPorts;
        }
        public static IList getUDPUsePort()
        {
            //获取本地计算机的网络连接和通信统计数据的信息 
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            //返回本地计算机上的所有UDP监听程序 
            IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();
            IList allPorts = new ArrayList();
            foreach (IPEndPoint ep in ipsUDP) allPorts.Add(ep.Port);
            ipsUDP = null;
            return allPorts;
        }
    }
}
