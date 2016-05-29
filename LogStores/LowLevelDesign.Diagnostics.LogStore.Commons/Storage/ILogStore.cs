/**
 *  Part of the Diagnostics Kit
 *
 *  Copyright (C) 2016  Sebastian Solnica
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

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
        Task AddLogRecordAsync(LogRecord logrec);

        /// <summary>
        /// Adds a batch of records to the store.
        /// </summary>
        Task AddLogRecordsAsync(IEnumerable<LogRecord> logrecs);

        /// <summary>
        /// Updates application status - based on what was received 
        /// in the log record.
        /// 
        /// REMARK: Only group of fields might be updated.
        /// </summary>
        Task UpdateApplicationStatusAsync(LastApplicationStatus status);

        /// <summary>
        /// Updates application statuses - based on what was received
        /// in the log records. Statuses must be unique per application.
        /// 
        /// REMARK: Only group of fields might be updated.
        /// </summary>
        Task UpdateApplicationStatusesAsync(IEnumerable<LastApplicationStatus> statuses);

        /// <summary>
        /// Retrieves logs from the store based on the passed search criteria.
        /// </summary>
        Task<LogSearchResults> FilterLogsAsync(LogSearchCriteria searchCriteria);

        /// <summary>
        /// Gets application statuses - this method is used by the grid
        /// to quickly display the status of the application farm. There should be
        /// only one application status returned per application path.
        /// </summary>
        /// <param name="lastDateTimeUtcToQuery">Last date to filter the logs</param>
        /// <returns></returns>
        Task<IEnumerable<LastApplicationStatus>> GetApplicationStatusesAsync(DateTime lastDateTimeUtcToQuery);

        /// <summary>
        /// Performs storage maintenance - removes old logs, compacts the 
        /// storage etc. You need to specify a global time for which we need 
        /// to keep the logs and you may adjust it per application. Applications
        /// are identified by their paths. Additionaly where timespan for an application
        /// is equal to zero, its logs won't be deleted.
        /// </summary>
        Task MaintainAsync(TimeSpan logsKeepTime, IDictionary<String, TimeSpan> logsKeepTimePerApplication = null);
    }
}
