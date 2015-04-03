using System;
using System.Collections;
using System.Collections.Generic;
using LowLevelDesign.Diagnostics.Commons.Models;

namespace LowLevelDesign.Diagnostics.Commons.Storage
{
    public interface ILogStore
    {
        void AddLogRecord(LogRecord logrec);

        void AddLogRecords(IEnumerable<LogRecord> logrecs);

        // void DeleteLogRecords(DateTime olderThanDateUtc);

        // IEnumerable<LogRecord> SearchPerfLogs

        // IEnumerable<LogRecord> SearchLogs
    }
}
