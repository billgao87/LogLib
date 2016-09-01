using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Common.Tracers
{
    /// <summary>
    /// Please use Tracer to log. EventEntryLogger：向系统日志记录日志信息。
    /// 可通过控制面板-管理工具-事件查看器来查看这些事件。
    /// EventEntryLogger 应主要用于无法使用Tracer的场合。在绝大多数场合，应该直接使用Tracer 
    /// </summary>
    public static class EventLogWrapper
    {
        private const string EventSourceName = "UITracingSvc";

        private const string LogName = "UITracingSvc Event";

        static EventLogWrapper()
        {
            try
            {
                if (!EventLog.SourceExists(EventSourceName))
                {
                    EventLog.CreateEventSource(EventSourceName, LogName);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 记录Error信息到系统日志 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteEntryError(string message)
        {
            try
            {
                EventLog.WriteEntry(EventSourceName, message, EventLogEntryType.Error);
            }
            catch (Exception)
            {
            }
        }

        public static void WriteEntryInfo(string message)
        {
            try
            {
                EventLog.WriteEntry(EventSourceName, message, EventLogEntryType.Information);
            }
            catch (Exception)
            {
            }
        }
    }
}
