using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;
using System.ComponentModel;
using System.Web;
using System.Reflection;

namespace YW.Upgrade
{
    public class Log : IDisposable
    {
        private string _LogPath;
        private static Log _Object;
        private static object _LockHelper = new object();
        private IntPtr handle;
        private Component component = new Component();
        private bool disposed = false;
        public static Log GetInstance()
        {
            if (_Object == null)
            {
                lock (_LockHelper)
                {
                    if (_Object == null)
                    {
                        _Object = new Log();
                    }
                }
            }
            return _Object;
        }
        public Log()
        {
            if (_LogPath == null || _LogPath.Length == 0)
            {

                    string stmp = Assembly.GetExecutingAssembly().Location;
                    _LogPath = stmp.Substring(0, stmp.LastIndexOf('\\')) + "\\Log\\";//删除文件名
            }
        }

        public void WriteLog(Exception ex)
        {
            StringBuilder strInfo = new StringBuilder();
            strInfo.Append("******************************************* Exception Information ************************************************");
            strInfo.AppendFormat("{0}Time:{1}{0}{2}{0}{0}", Environment.NewLine, DateTime.Now, ex.ToString());
            WriteLog(strInfo.ToString());
        }

        public void WriteLog(string Message)
        {
            lock (_LockHelper)
            {
                Message = Message + Environment.NewLine;
                string m_LogName = _LogPath + "\\Upgrade_"+System.DateTime.Now.ToString("yyyyMMdd") + ".txt";
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(m_LogName));
                System.IO.TextWriter output = null;
                try
                {
                    output = TextWriter.Synchronized(System.IO.File.AppendText(m_LogName));
                    output.Write(Message.ToString());
                }
                catch { }
                finally
                {
                    if (output != null)
                    {
                        output.Close();
                    }

                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    component.Dispose();
                }
                CloseHandle(handle);
                handle = IntPtr.Zero;
                disposed = true;
            }
        }
        [System.Runtime.InteropServices.DllImport("Kernel32")]
        private extern static Boolean CloseHandle(IntPtr handle);
        ~Log()
        {
            Dispose(false);
        }
    }
}
