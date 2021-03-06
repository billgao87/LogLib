﻿using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using NLog;
using UI.Common.Tracers;

namespace Common.Logger
{
    public static class Log
    {
        #region Log Property
        private const string PropertyAssemblyName = "CallingAssemblyName";
        private const string PropertyErrorNumber = "ErrorNumber";
        private const string PropertyExceptionName = "ExceptionName";
        private const string PropertyExceptionString = "ExceptionString";
        private const string PropertyFileName = "FileName";
        private const string PropertyFunctionName = "FunctionName";
        private const string PropertyInnerException = "InnerException";
        private const string PropertyLineNumber = "LineNumber";
        private const string PropertyLogTime = "LogTime";
        private const string PropertyMachineName = "MachineName";
        private const string PropertyProcessId = "ProcessId";
        private const string PropertyProcessName = "ProcessName";
        private const string PropertyRaisedErrorNamespace = "RaisedErrorNamespace";
        private const string PropertyStackTrace = "StackTrace";
        private const string PropertyThreadId = "ThreadId";
        private const string PropertyThreadName = "ThreadName";
        #endregion

        private static readonly ILoggingService LoggingService;

        public static readonly string MachineName = Environment.MachineName;
        public static readonly string ProcessName = AppDomain.CurrentDomain.FriendlyName;

        private static int _minLevel = 0;

        static Log()
        {
            try
            {
                LoggingService = new LoggingService();
            }
            catch (Exception e)
            {
                Ultities.EventLogException("Tracer constructor", e);
            }
        }

        /// <summary>
        /// 0: Trace;
        /// 1: Debug;
        /// 2: Info;
        /// 3: Warn;
        /// 4: Error;
        /// 5: Fatal;
        /// 6: Off;
        /// </summary>
        /// <param name="traceMinLevel"></param>
        public static void SetMinTraceLevel(int traceMinLevel)
        {
            _minLevel = traceMinLevel;
        }

        internal static void LogData(LogEventInfo data)
        {
            if (LoggingService != null)
            {
                LoggingService.LogData(data);
            }
        }

        public static void LogTrace(LogLevel severity, string message)
        {
            try
            {
                string fullName = Assembly.GetCallingAssembly().FullName;
                LogMessage(severity, message, null, fullName, null, 0, false);
            }
            catch (Exception e)
            {
                Ultities.EventLogException("Tracer.Trace", e);
            }
        }

        public static void LogDebug(string message, params object[] parameters)
        {
            try
            {
                string fullName = Assembly.GetCallingAssembly().FullName;
                LogMessage(LogLevel.Debug, message + FormatParams(parameters), null, fullName, null, 0, false);
            }
            catch (Exception e)
            {
                Ultities.EventLogException("Tracer.TraceDebug", e);
            }
        }

        public static void LogWarning(string message, params object[] parameters)
        {
            try
            {
                LogMessage(LogLevel.Warn, message + FormatParams(parameters), null,
                    Assembly.GetCallingAssembly().FullName, null, 0, false);
            }
            catch (Exception e)
            {
                Ultities.EventLogException("Tracer.TraceWarning", e);
            }
        }


        public static void LogError(string errorMessage, int errorNumber = 0)
        {
            try
            {
                string fullName = Assembly.GetCallingAssembly().FullName;
                LogMessage(LogLevel.Error, errorMessage, null, fullName, null, errorNumber, false);
            }
            catch (Exception e)
            {
                Ultities.EventLogException("Tracer.TraceError", e);
            }
        }

        public static void LogException(Exception exception, string message = null)
        {
            try
            {
                string fullName = Assembly.GetCallingAssembly().FullName;
                if (!string.IsNullOrEmpty(message))
                {
                    LogMessage(LogLevel.Error, message, null, fullName, null, 0, false);
                }
                LogMessage(LogLevel.Error, null, null, fullName, exception, 0, false);
            }
            catch (Exception e)
            {
                Ultities.EventLogException("Tracer.TraceException", e);
            }
        }

        private static void LogException(LogLevel severity, Exception exception, string message = null)
        {
            try
            {
                string fullName = Assembly.GetCallingAssembly().FullName;
                if (!string.IsNullOrEmpty(message))
                {
                    LogMessage(severity, message, null, fullName, null, 0, false);
                }
                LogMessage(severity, null, null, fullName, exception, 0, false);
            }
            catch (Exception e)
            {
                Ultities.EventLogException("Tracer.TraceException", e);
            }
        }

        public static void LogInfo(string message, params object[] parameters)
        {
            try
            {
                LogMessage(LogLevel.Info, message + FormatParams(parameters), null,
                    Assembly.GetCallingAssembly().FullName, null, 0, false);
            }
            catch (Exception e)
            {
                Ultities.EventLogException("Tracer.TraceInfo", e);
            }
        }

        public static void LogEnterFunc(string functionName)
        {
            LogFunc(functionName, true, null);
        }

        public static void LogEnterFunc(string functionName, params object[] parameters)
        {
            LogFunc(functionName, true, parameters);
        }

        public static void LogExitFunc(string functionName)
        {
            LogFunc(functionName, false, null);
        }

        public static void LogExitFunc(string functionName, params object[] parameters)
        {
            LogFunc(functionName, false, parameters);
        }

        private static void LogFunc(string functionName, bool enterFunc = false, params object[] parameters)
        {
            string fullName = Assembly.GetCallingAssembly().FullName;

            string isEnterStr = enterFunc ? "LogEnterFunc" : "LogExitFunc";
            try
            {
                if (string.IsNullOrEmpty(functionName))
                {
                    LogMessage(LogLevel.Error, isEnterStr + " called with a null function name.", null, fullName, null,
                        0, false);
                }
                else
                {
                    LogMessage(LogLevel.Trace, isEnterStr + functionName + FormatParams(parameters), null, fullName,
                        null, 0, false);
                }
            }
            catch (Exception e)
            {
                Ultities.EventLogException("Logger.Logger." + isEnterStr, e);
            }
        }


        public static void LogDataSet(LogLevel severity, DataSet dataset)
        {
            string fullName = Assembly.GetCallingAssembly().FullName;
            try
            {
                if (dataset == null)
                {
                    LogMessage(LogLevel.Error, "TraceDataSet called with a null dataset", null, fullName, null, 0, false);
                }
                else
                {
                    string tempFileName = Path.GetTempFileName();
                    dataset.WriteXml(tempFileName);
                    LogMessage(severity, "The DataSet file was written to:" + tempFileName, null, fullName, null, 0, false);
                }
            }
            catch (Exception e)
            {
                Ultities.EventLogException("Logger.LogDataSet", e);
            }
        }

        private static void LogMessage(LogLevel severity, string message = null, string loggerName = null,
            string callingAssemblyName = null, Exception exception = null, int errorNumber = 0,
            bool logStackTrace = false)
        {
            if (LogLevel.FromOrdinal(_minLevel)> severity)
            {
                return;
            }

            if (string.IsNullOrEmpty(loggerName))
            {
                loggerName = AppDomain.CurrentDomain.FriendlyName;
            }
            LogEventInfo info = new LogEventInfo
            {
                TimeStamp = DateTime.Now,
                Level = severity,
                LoggerName = loggerName
            };
            if (!string.IsNullOrEmpty(message))
            {
                info.Message = message;
            }

            info.Properties[PropertyLogTime] = info.TimeStamp.ToString("yyyy-MM-dd,HH:mm:ss.fffffff");
            if (!string.IsNullOrEmpty(callingAssemblyName))
            {
                info.Properties[PropertyAssemblyName] = callingAssemblyName;
            }
            if (exception != null)
            {
                info.Message = exception.Message;
                info.Properties[PropertyExceptionString] = exception.ToString();
                info.Properties[PropertyExceptionName] = exception.GetType().FullName;
                if (exception.InnerException != null)
                {
                    info.Properties[PropertyInnerException] =
                        exception.InnerException.ToString();
                }
            }
            info.Properties[PropertyErrorNumber] = errorNumber;
            info.Properties[PropertyMachineName] = MachineName;
            info.Properties[PropertyThreadId] = Thread.CurrentThread.ManagedThreadId;
            info.Properties[PropertyProcessId] = Process.GetCurrentProcess().Id;
            info.Properties[PropertyThreadName] = Thread.CurrentThread.Name;
            info.Properties[PropertyProcessName] = ProcessName;
            if (logStackTrace || (severity == LogLevel.Error) || (severity == LogLevel.Warn))
            {
                StackFrame frame = new StackFrame(3, true);
                info.Properties[PropertyFileName] = frame.GetFileName();
                info.Properties[PropertyLineNumber] = frame.GetFileLineNumber();
                MethodBase method = frame.GetMethod();
                if (method != null)
                {
                    info.Properties[PropertyFunctionName] = method.ToString();
                    if (method.ReflectedType != null)
                    {
                        info.Properties[PropertyRaisedErrorNamespace] = method.ReflectedType.FullName;
                    }
                }
                info.Properties[PropertyStackTrace] = frame.ToString();
                //info.Properties[PropertyStackTrace] = frame.ToString();
            }
            LogData(info);
        }

        private static string FormatParams(params object[] parameters)
        {
            if (parameters == null)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(0x200);
            builder.Append(":");
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }
                object obj2 = parameters[i];
                builder.Append(obj2 == null ? "null" : obj2.ToString());
            }
            return builder.ToString();
        }
    }
}
