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
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.GenericDb
{
    public class GenericDbLogStore : ILogStore
    {
        public Task AddLogRecordAsync(LogRecord logrec)
        {
            throw new NotImplementedException();
        }

        public Task AddLogRecordsAsync(IEnumerable<LogRecord> logrecs)
        {
            throw new NotImplementedException();
        }

        public Task<LogSearchResults> FilterLogsAsync(LogSearchCriteria searchCriteria)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LastApplicationStatus>> GetApplicationStatusesAsync(DateTime lastDateTimeUtcToQuery)
        {
            throw new NotImplementedException();
        }

        public Task MaintainAsync(TimeSpan logsKeepTime, IDictionary<string, TimeSpan> logsKeepTimePerApplication = null)
        {
            throw new NotImplementedException();
        }

        public Task UpdateApplicationStatusAsync(LastApplicationStatus status)
        {
            throw new NotImplementedException();
        }

        public Task UpdateApplicationStatusesAsync(IEnumerable<LastApplicationStatus> statuses)
        {
            throw new NotImplementedException();
        }
    }
}
