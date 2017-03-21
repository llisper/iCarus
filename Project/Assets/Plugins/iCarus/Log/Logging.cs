using UnityEngine;

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;

using log4net;
using log4net.Layout;
using log4net.Config;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace iCarus.Log
{
    public class Logging
    {
        public class Define<T>
        {
            protected static ILog log;

            #region logging API
            public static log4net.Core.ILogger Logger { get { return log.Logger; } }

            public static bool IsDebugEnabled { get { return log.IsDebugEnabled; } }
            public static bool IsErrorEnabled { get { return log.IsErrorEnabled; } }
            public static bool IsFatalEnabled { get { return log.IsFatalEnabled; } }
            public static bool IsInfoEnabled { get { return log.IsInfoEnabled; } }
            public static bool IsWarnEnabled { get { return log.IsWarnEnabled; } }

            public static void Debug(object message) { log.Debug(message); }
            public static void Debug(object message, Exception exception) { log.Debug(message, exception); }
            public static void DebugFormat(string format, params object[] args) { log.DebugFormat(format, args); }
            public static void DebugFormat(string format, object arg0) { log.DebugFormat(format, arg0); }
            public static void DebugFormat(IFormatProvider provider, string format, params object[] args) { log.DebugFormat(provider, format, args); }
            public static void DebugFormat(string format, object arg0, object arg1) { log.DebugFormat(format, arg0, arg1); }
            public static void DebugFormat(string format, object arg0, object arg1, object arg2) { log.DebugFormat(format, arg0, arg1, arg2); }
            public static void Error(object message) { log.Error(message); }
            public static void Error(object message, Exception exception) { log.Error(message, exception); }
            public static void ErrorFormat(string format, params object[] args) { log.ErrorFormat(format, args); }
            public static void ErrorFormat(string format, object arg0) { log.ErrorFormat(format, arg0); }
            public static void ErrorFormat(IFormatProvider provider, string format, params object[] args) { log.ErrorFormat(provider, format, args); }
            public static void ErrorFormat(string format, object arg0, object arg1) { log.ErrorFormat(format, arg0, arg1); }
            public static void ErrorFormat(string format, object arg0, object arg1, object arg2) { log.ErrorFormat(format, arg0, arg1, arg2); }
            public static void Fatal(object message) { log.Fatal(message); }
            public static void Fatal(object message, Exception exception) { log.Fatal(message, exception); }
            public static void FatalFormat(string format, object arg0) { log.FatalFormat(format, arg0); }
            public static void FatalFormat(string format, params object[] args) { log.FatalFormat(format, args); }
            public static void FatalFormat(IFormatProvider provider, string format, params object[] args) { log.FatalFormat(provider, format, args); }
            public static void FatalFormat(string format, object arg0, object arg1) { log.FatalFormat(format, arg0, arg1); }
            public static void FatalFormat(string format, object arg0, object arg1, object arg2) { log.FatalFormat(format, arg0, arg1, arg2); }
            public static void Info(object message) { log.Info(message); }
            public static void Info(object message, Exception exception) { log.Info(message, exception); }
            public static void InfoFormat(string format, object arg0) { log.InfoFormat(format, arg0); }
            public static void InfoFormat(string format, params object[] args) { log.InfoFormat(format, args); }
            public static void InfoFormat(string format, object arg0, object arg1) { log.InfoFormat(format, arg0, arg1); }
            public static void InfoFormat(IFormatProvider provider, string format, params object[] args) { log.InfoFormat(provider, format, args); }
            public static void InfoFormat(string format, object arg0, object arg1, object arg2) { log.InfoFormat(format, arg0, arg1, arg2); }
            public static void Warn(object message) { log.Warn(message); }
            public static void Warn(object message, Exception exception) { log.Warn(message, exception); }
            public static void WarnFormat(string format, object arg0) { log.WarnFormat(format, arg0); }
            public static void WarnFormat(string format, params object[] args) { log.WarnFormat(format, args); }
            public static void WarnFormat(string format, object arg0, object arg1) { log.WarnFormat(format, arg0, arg1); }
            public static void WarnFormat(IFormatProvider provider, string format, params object[] args) { log.WarnFormat(provider, format, args); }
            public static void WarnFormat(string format, object arg0, object arg1, object arg2) { log.WarnFormat(format, arg0, arg1, arg2); }

            public static void Exception(System.Exception exception) { log.Error(string.Empty, exception); }
            public static void Exception(string message, System.Exception exception) { log.Error(message, exception); }
            #endregion logging API
        }

        public static IEnumerator Initialize(string config, bool force = false)
        {
            if (force || !LogManager.GetRepository().Configured)
            {
                string path = Path.Combine(Application.streamingAssetsPath, config);
                if (path.Contains(":///"))
                {
                    WWW www = new WWW(path);
                    yield return www;
                    using (MemoryStream stream = new MemoryStream(www.bytes))
                        XmlConfigurator.Configure(stream);
                }
                else
                {
                    using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(path)))
                        XmlConfigurator.Configure(stream);
                }
                LogRegistry.Initialize();
            }
        }

        public static void AddUdpAppender(int port)
        {
            var repositroy = (Hierarchy)log4net.LogManager.GetRepository();
            if (!repositroy.Configured)
                return;

            var root = repositroy.Root;
            if (null == root.GetAppender("UdpAppender"))
            {
                PatternLayout layout = new PatternLayout()
                {
                    ConversionPattern = @"[%logger]: %message %exception"
                };
                layout.ActivateOptions();

                UdpAppender udpAppender = new UdpAppender()
                {
                    Name = "UdpAppender",
                    Layout = layout,
                    RemoteAddress = IPAddress.Parse("224.0.0.1"),
                    RemotePort = port,
                    Encoding = Encoding.UTF8,
                };
                udpAppender.ActivateOptions();

                root.AddAppender(udpAppender);
                repositroy.Configured = true;
                repositroy.RaiseConfigurationChanged(EventArgs.Empty);
            }
        }

        public static void RemoveUdpAppender()
        {
            var repositroy = (Hierarchy)log4net.LogManager.GetRepository();
            if (!repositroy.Configured)
                return;

            var root = repositroy.Root;
            IAppender udpAppender = root.GetAppender("UdpAppender");
            if (null != udpAppender)
            {
                root.RemoveAppender(udpAppender);
                repositroy.Configured = true;
                repositroy.RaiseConfigurationChanged(EventArgs.Empty);
            }
        }
    }
}