using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace UI.Common.Tracers
{
    /// <summary>
    /// log接口
    /// </summary>
    interface ILoggingService
    {
        void LogData(LogEventInfo data);
        void ShutDown();
    }
}
