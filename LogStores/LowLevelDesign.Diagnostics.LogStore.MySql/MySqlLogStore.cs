﻿/**
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
using MySql.Data.MySqlClient;
using System;
using System.Data;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System.Threading.Tasks;
using LowLevelDesign.Diagnostics.Commons.Models;
using Dapper;
using System.Linq;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    public class MySqlLogStore : DefaultLogStore
    {
        public MySqlLogStore() : this(() => DateTime.UtcNow.Date) { }

        internal MySqlLogStore(Func<DateTime> currentUtcDateRetriever) : base(new LogTable(currentUtcDateRetriever)) { }

        protected override IDbConnection CreateConnection()
        {
            return new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString);
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
            var orderBySql = string.Format("order by TimeUtc desc limit {0},{1}", searchCriteria.Offset, searchCriteria.Limit);

            using (var conn = CreateConnection()) {
                conn.Open();
                var dbapplogs = (await conn.QueryAsync<DbAppLogRecord>(string.Format("select * from {0}{1} {2} {3}", AppLogTablePrefix, hash, whereSql, orderBySql), new {
                    searchCriteria.FromUtc,
                    searchCriteria.ToUtc,
                    searchCriteria.Server,
                    Logger = searchCriteria.Logger + "%",
                    searchCriteria.Levels,
                    HttpStatusCode = searchCriteria.Keywords.HttpStatus + "%",
                    Url = searchCriteria.Keywords.Url + "%",
                    ClientIp = searchCriteria.Keywords.ClientIp + "%",
                    ServiceName = searchCriteria.Keywords.Service + "%"
                })).ToArray();

                return new LogSearchResults { FoundItems = ConvertDbLogRecordToPublicModel(dbapplogs) };
            }
        }
    }
}
