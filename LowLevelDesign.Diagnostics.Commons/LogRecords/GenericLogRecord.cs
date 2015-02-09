using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Commons.LogRecords
{
    public class GenericLogRecord
    {
        public String LoggerName;

        public String LogLevel;

        public DateTime TimeUtc;

        public int ProcessId;

        public String ProcessName;

        public int ThreadId;

        public String Server;

        public String ApplicationPath;

        public String SystemUser;

        public String CorrelationId;

        public String Message;

        public String Exception;

        public String FullExceptionInfo;
    }
}
