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

using LowLevelDesign.Diagnostics.LogStore.Defaults;
using System;
using System.Data;
using System.Data.SqlClient;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System.Threading.Tasks;
using LowLevelDesign.Diagnostics.Commons.Models;
using Dapper;
using System.Linq;

namespace LowLevelDesign.Diagnostics.LogStore.SqlServer.Standard
{
    public sealed class SqlServerLogStore : DefaultLogStore
    {
        public SqlServerLogStore() : this(() => DateTime.UtcNow.Date) { }

        internal SqlServerLogStore(Func<DateTime> currentUtcDateRetriever) : base(new LogTable(currentUtcDateRetriever)) { }

        protected override IDbConnection CreateConnection()
        {
            return new SqlConnection(SqlServerLogStoreConfiguration.ConnectionString);
        }

        public override async Task<LogSearchResults> FilterLogsAsync(LogSearchCriteria searchCriteria)
        {
            var hash = GetApplicationHash(searchCriteria.ApplicationPath);
            if (!logTable.IsLogTableAvailable(AppLogTablePrefix + hash)) {
                return new LogSearchResults {
                    FoundItems = new LogRecord[0]
                };
            }

            if (string.IsNullOrEmpty(searchCriteria.ApplicationPath)) {
                throw new ArgumentException("ApplicationPath is required to filter the logs");
            }
            var whereSql = PrepareWhereSectionOfTheQuery(searchCriteria);

            var query = string.Format(@"
select LoggerName ,LogLevel ,TimeUtc ,Message ,ExceptionType ,ExceptionMessage ,ExceptionAdditionalInfo ,CorrelationId 
      ,Server ,ApplicationPath ,ProcessId ,ThreadId ,[Identity] ,Host ,LoggedUser ,HttpStatusCode ,Url ,Referer ,ClientIP 
      ,RequestData ,ResponseData ,ServiceName ,ServiceDisplayName, PerfData
from (
    select LoggerName ,LogLevel ,TimeUtc ,Message ,ExceptionType ,ExceptionMessage ,ExceptionAdditionalInfo ,CorrelationId 
      ,Server ,ApplicationPath ,ProcessId ,ThreadId ,[Identity] ,Host ,LoggedUser ,HttpStatusCode ,Url ,Referer ,ClientIP 
      ,RequestData ,ResponseData ,ServiceName ,ServiceDisplayName, PerfData, row_number() over (order by TimeUtc desc) AS RowNum
    from {0}{1} {2}
) as MyDerivedTable
where MyDerivedTable.RowNum >= @StartRow and MyDerivedTable.RowNum < @EndRow", AppLogTablePrefix, hash, whereSql);

            using (var conn = CreateConnection()) {
                conn.Open();
                var dbapplogs = (await conn.QueryAsync<DbAppLogRecord>(query, new {
                    searchCriteria.FromUtc,
                    searchCriteria.ToUtc,
                    searchCriteria.Server,
                    Logger = searchCriteria.Logger + "%",
                    searchCriteria.Levels,
                    HttpStatusCode = searchCriteria.Keywords.HttpStatus + "%",
                    Url = searchCriteria.Keywords.Url + "%",
                    ClientIp = searchCriteria.Keywords.ClientIp + "%",
                    ServiceName = searchCriteria.Keywords.Service + "%",
                    StartRow = searchCriteria.Offset,
                    EndRow = searchCriteria.Offset + searchCriteria.Limit
                })).ToArray();

                return new LogSearchResults { FoundItems = ConvertDbLogRecordToPublicModel(dbapplogs) };
            }
        }
    }
}
