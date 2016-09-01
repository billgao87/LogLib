using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Common.Tracers
{
    internal static class Ultities
    {
        public static void EventLogException(string exceptionLocation, Exception e)
        {
            StringBuilder logMsg = new StringBuilder(200);
            logMsg.Append("Exception occured in ");
            logMsg.Append(exceptionLocation);
            logMsg.Append(", and exception: ");
            logMsg.Append(e.Message.ToString());
            logMsg.Append(";");
            logMsg.Append(e.StackTrace);
            EventLogWrapper.WriteEntryError(logMsg.ToString());
        }
    }
}
