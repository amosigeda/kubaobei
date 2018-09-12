using System;
using log4net;

namespace YW.Data
{
    public static class Logger
    {
        /// <summary>
        /// 获取在配置文件中的log4net配置对象
        /// </summary>
        static ILog Log;
        /// <summary>
        /// 初始化log4net
        /// </summary>
        static Logger()
        {
            InitLog4Net();
            Log = LogManager.GetLogger("log4netMainLogger");
        }
        /// <summary>
        /// 初始化log4net
        /// </summary>
        static void InitLog4Net()
        {
            log4net.Config.XmlConfigurator.Configure();
            //log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(Server.MapPath("~") + @"\log4net.xml"));
        }

        /// <summary>
        /// Gets a value indicating whether this instance is debug enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is debug enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsDebugEnabled
        {
            get { return Log.IsDebugEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is error enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is error enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsErrorEnabled
        {
            get { return Log.IsErrorEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is fatal enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is fatal enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsFatalEnabled
        {
            get { return Log.IsFatalEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is info enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is info enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsInfoEnabled
        {
            get { return Log.IsInfoEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is warn enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is warn enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsWarnEnabled
        {
            get { return Log.IsWarnEnabled; }
        }

        /// <summary>
        /// Logs the debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Debug(object message)
        {
            Log.Debug(message);
        }

        /// <summary>
        /// Logs the debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void Debug(object message, Exception exception)
        {
            Log.Debug(message, exception);
        }

        /// <summary>
        /// Logs the debug message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        public static void DebugFormat(string format, object arg0)
        {
            Log.DebugFormat(format, arg0);
        }

        /// <summary>
        /// Logs the debug message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void DebugFormat(string format, params object[] args)
        {
            Log.DebugFormat(format, args);
        }

        /// <summary>
        /// Logs the debug message.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            Log.DebugFormat(provider, format, args);
        }

        /// <summary>
        /// Logs the debug message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        public static void DebugFormat(string format, object arg0, object arg1)
        {
            Log.DebugFormat(format, arg0, arg1);
        }

        /// <summary>
        /// Logs the debug message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        public static void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            Log.DebugFormat(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// Logs the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Error(object message)
        {
            Log.Error(message);
        }

        /// <summary>
        /// Logs the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void Error(object message, Exception exception)
        {
            Log.Error(message, exception);
        }

        /// <summary>
        /// Logs the error message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        public static void ErrorFormat(string format, object arg0)
        {
            Log.ErrorFormat(format, arg0);
        }

        /// <summary>
        /// Logs the error message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void ErrorFormat(string format, params object[] args)
        {
            Log.ErrorFormat(format, args);
        }

        /// <summary>
        /// Logs the error message.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            Log.ErrorFormat(provider, format, args);
        }

        /// <summary>
        /// Logs the error message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        public static void ErrorFormat(string format, object arg0, object arg1)
        {
            Log.ErrorFormat(format, arg0, arg1);
        }

        /// <summary>
        /// Logs the error message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        public static void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            Log.ErrorFormat(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// Logs the fatal error message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Fatal(object message)
        {
            Log.Fatal(message);
        }

        /// <summary>
        /// Logs the fatal error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void Fatal(object message, Exception exception)
        {
            Log.Fatal(message, exception);
        }

        /// <summary>
        /// Logs the fatal error message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        public static void FatalFormat(string format, object arg0)
        {
            Log.FatalFormat(format, arg0);
        }

        /// <summary>
        /// Logs the fatal error message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void FatalFormat(string format, params object[] args)
        {
            Log.FatalFormat(format, args);
        }

        /// <summary>
        /// Logs the fatal error message.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            Log.FatalFormat(provider, format, args);
        }

        /// <summary>
        /// Logs the fatal error message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        public static void FatalFormat(string format, object arg0, object arg1)
        {
            Log.FatalFormat(format, arg0, arg1);
        }

        /// <summary>
        /// Logs the fatal error message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        public static void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            Log.FatalFormat(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// Logs the info message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Info(object message)
        {
            Log.Info(message);
        }

        /// <summary>
        /// Logs the info message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void Info(object message, Exception exception)
        {
            Log.Info(message, exception);
        }

        /// <summary>
        /// Logs the info message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        public static void InfoFormat(string format, object arg0)
        {
            Log.InfoFormat(format, arg0);
        }

        /// <summary>
        /// Logs the info message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void InfoFormat(string format, params object[] args)
        {
            Log.InfoFormat(format, args);
        }

        /// <summary>
        /// Logs the info message.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            Log.InfoFormat(provider, format, args);
        }

        /// <summary>
        /// Logs the info message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        public static void InfoFormat(string format, object arg0, object arg1)
        {
            Log.InfoFormat(format, arg0, arg1);
        }

        /// <summary>
        /// Logs the info message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        public static void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            Log.InfoFormat(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Warn(object message)
        {
            Log.Warn(message);
        }

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void Warn(object message, Exception exception)
        {
            Log.Warn(message, exception);
        }

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        public static void WarnFormat(string format, object arg0)
        {
            Log.WarnFormat(format, arg0);
        }

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void WarnFormat(string format, params object[] args)
        {
            Log.WarnFormat(format, args);
        }

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            Log.WarnFormat(provider, format, args);
        }

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        public static void WarnFormat(string format, object arg0, object arg1)
        {
            Log.WarnFormat(format, arg0, arg1);
        }

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        public static void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            Log.WarnFormat(format, arg0, arg1, arg2);
        }
    }
}
