using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Storage
{
    public interface ILogStore
    {
        /// <summary>
        /// Adds one log record to the store.
        /// </summary>
        /// <param name="logrec"></param>
        Task AddLogRecord(LogRecord logrec);

        /// <summary>
        /// Adds a batch of records to the store.
        /// </summary>
        /// <param name="logrecs"></param>
        Task AddLogRecords(IEnumerable<LogRecord> logrecs);

        /// <summary>
        /// Retrieves logs from the store based on the passed search criteria.
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        Task<LogSearchResults> FilterLogs(LogSearchCriteria searchCriteria);

        /// <summary>
        /// Gets application statuses - this method is used by the grid
        /// to quickly display the status of the application farm. There should be
        /// only one application status returned per application path.
        /// </summary>
        /// <param name="lastDateTimeUtcToQuery">Last date to filter the logs</param>
        /// <returns></returns>
        Task<IEnumerable<LastApplicationStatus>> GetApplicationStatuses(DateTime lastDateTimeUtcToQuery);

        /// <summary>
        /// Performs storage maintenance - removes old logs, compacts the 
        /// storage etc. You need to specify a global time for which we need 
        /// to keep the logs and you may adjust it per application. Applications
        /// are identified by their paths. Additionaly where timespan for an application
        /// is equal to zero, its logs won't be deleted.
        /// </summary>
        Task Maintain(TimeSpan logsKeepTime, IDictionary<String, TimeSpan> logsKeepTimePerApplication = null);
    }
}
