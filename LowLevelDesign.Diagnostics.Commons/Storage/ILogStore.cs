using System;
using System.Collections;
using System.Collections.Generic;
using LowLevelDesign.Diagnostics.Commons.Models;

namespace LowLevelDesign.Diagnostics.Commons.Storage
{
    public interface ILogStore
    {
        /// <summary>
        /// Initializes the store - creates the necessary structures 
        /// required to store the logs etc. If exception is thrown here
        /// the Diagnostics Board will report it and won't start
        /// </summary>
        void Initialize();

        /// <summary>
        /// Adds one log record to the store.
        /// </summary>
        /// <param name="logrec"></param>
        void AddLogRecord(LogRecord logrec);

        /// <summary>
        /// Adds a batch of records to the store.
        /// </summary>
        /// <param name="logrecs"></param>
        void AddLogRecords(IEnumerable<LogRecord> logrecs);

        /// <summary>
        /// Retrieves logs from the store based on the passed search criteria.
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        IEnumerable<LogRecord> SearchLogs(LogSearchCriteria searchCriteria);

        // void DeleteLogRecords(DateTime olderThanDateUtc);
    }
}
