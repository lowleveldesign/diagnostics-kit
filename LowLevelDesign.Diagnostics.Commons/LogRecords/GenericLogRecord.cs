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

        public String Message;

        public String Exception;

        public String FullExceptionInfo;

        public String Server;

        public String ServerIP;

        public short ServerPort;

        public String ApplicationPath;

        public int ProcessId;

        public int ThreadId;

        public String SystemUser;

        public Guid CorrelationId;
    }
}
