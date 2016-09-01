using System;
using NLog;
using UI.Common.Tracers;

namespace Common.Logger
{
    class LoggingService : Actor<LogEventInfo>, ILoggingService
    {
        private static NLog.Logger _logger;
        private const string LoggerName = "UI.Common.Tracers";

        public LoggingService(string loggerName)
        {
            _logger = LogManager.GetLogger(loggerName);
        }

        public LoggingService()
        {
            _logger = LogManager.GetLogger(LoggerName);
        }

        protected override void Receive(LogEventInfo message)
        {
            if (message != null)
            {
                this.RaiseLogEvent(message);
            }
        }

        /// <summary>
        /// 对外接口，将log信息送入队列
        /// </summary>
        /// <param name="data"></param>
        public void LogData(LogEventInfo data)
        {
            this.Post(data);
        }

        public void ShutDown()
        {
        }

        private LogLevel GetNLogLevelFromSeverity(int severity)
        {
            LogLevel info = LogLevel.Info;
            try
            {
                info = LogLevel.FromOrdinal(severity);
            }
            catch (Exception e)
            {
                Ultities.EventLogException("LoggingService.GetNLogLevelFromSeverity", e);
            }
            return info;
        }

        private void RaiseLogEvent(LogEventInfo logEventInfo)
        {
            if (logEventInfo != null)
            {
                if (_logger == null)
                {
                    _logger = LogManager.GetLogger(LoggerName);
                }
                if (_logger != null)
                {
                    _logger.Log(logEventInfo);
                }
            }
        }
    }
}
