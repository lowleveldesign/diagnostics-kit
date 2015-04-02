using System;
using System.Collections;
using LowLevelDesign.Diagnostics.Commons.Models;

namespace LowLevelDesign.Diagnostics.Commons.Storage
{
    public interface ILogStore
    {
        void AddLogRecord(LogRecord logrec);

        // void DeleteLogRecords(DateTime olderThanDateUtc);

        // FIXME: search logs
    }
}
