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

using Dapper;
using LowLevelDesign.Diagnostics.Commons;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Defaults;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.SqlServer.Standard
{
    internal class LogTable : ILogTable
    {
        private static readonly object lck = new object();
        private static readonly ISet<string> availableTables = new HashSet<string>();

        private readonly Func<DateTime> currentUtcDateRetriever;

        static LogTable()
        {
            // query the mysql database and retrieve all the log tables
            using (var conn = new SqlConnection(SqlServerLogStoreConfiguration.ConnectionString)) {
                conn.Open();
                availableTables = new HashSet<string>(conn.Query<string>("select table_name from information_schema.tables where table_schema = @Database", conn),
                                                        StringComparer.OrdinalIgnoreCase);

                if (!availableTables.Contains("ApplicationStatus")) {
                    // we need to create a table which will store application statuses for the grid
                    conn.Execute("if object_id('ApplicationStatus') is null create table ApplicationStatus (ApplicationHash char(32), Server varchar(200) not null, ApplicationPath varchar(2000) not null, " +
                        "Cpu float, Memory float null, LastPerformanceDataUpdateTimeUtc datetime, LastErrorType varchar(100), LastErrorTimeUtc datetime," +
                        "LastUpdateTimeUtc datetime not null, primary key (ApplicationHash, Server))");
                }
            }
        }

        public LogTable(Func<DateTime> currentUtcDateRetriever)
        {
            this.currentUtcDateRetriever = currentUtcDateRetriever;
        }

        public bool IsLogTableAvailable(string tableName)
        {
            return availableTables.Contains(tableName);
        }

        public void CreateLogTableIfNotExists(IDbConnection conn, IDbTransaction tran, string tableName)
        {
            if (!availableTables.Contains(tableName)) {
                lock (lck) {
                    if (!availableTables.Contains(tableName)) {
                        var currentUtcDate = currentUtcDateRetriever().Date;
                        conn.Execute("if object_id('" + tableName + "') is null create table " + tableName + " (Id int identity(1,1) not null,LoggerName nvarchar(200) not null" +
                            ",LogLevel smallint not null ,TimeUtc datetime not null ,Message nvarchar(max) null ,ExceptionType nvarchar(100) null" +
                            ",ExceptionMessage nvarchar(2000) null ,ExceptionAdditionalInfo nvarchar(max) null ,CorrelationId nvarchar(100) null" +
                            ",Server nvarchar(200) not null ,ApplicationPath nvarchar(2000) null ,ProcessId int null ,ThreadId int null" +
                            ",[Identity] nvarchar(200) null ,Host nvarchar(100) null ,LoggedUser nvarchar(200) null ,HttpStatusCode nvarchar(15) null" +
                            ",Url nvarchar(2000) null ,Referer nvarchar(2000) null ,ClientIP varchar(50) null ,RequestData nvarchar(max) null" +
                            ",ResponseData nvarchar(max) null,ServiceName nvarchar(100) null ,ServiceDisplayName nvarchar(200) null, PerfData nvarchar(3000) null" +
                            ",PRIMARY KEY (TimeUtc, Server, Id))", transaction: tran);
                        availableTables.Add(tableName);
                    }
                }
            }
        }

        public async Task ManageTablePartitions(IDbConnection conn, string tableName, TimeSpan keepTime)
        {
            var timeLimit = currentUtcDateRetriever().Subtract(keepTime);

            await conn.ExecuteAsync("delete from " + tableName + " where TimeUtc < @timeLimit", new { timeLimit });
        }

        public async Task<uint> SaveLogRecord(IDbConnection conn, IDbTransaction tran, string tableName, LogRecord logrec)
        {
            CreateLogTableIfNotExists(conn, tran, tableName);

            // we need to make sure that the additional fields collection is initialized
            if (logrec.AdditionalFields == null) {
                logrec.AdditionalFields = new Dictionary<string, Object>();
            }

            // save log in the table
            object v;
            return (await conn.QueryAsync<uint>("insert into " + tableName + "(LoggerName ,LogLevel ,TimeUtc ,Message ,ExceptionType " +
                    ",ExceptionMessage ,ExceptionAdditionalInfo ,CorrelationId ,Server ,ApplicationPath ,ProcessId ,ThreadId ,[Identity] ,Host " +
                    ",LoggedUser ,HttpStatusCode ,Url ,Referer ,ClientIP ,RequestData ,ResponseData ,ServiceName ,ServiceDisplayName, PerfData) " +
                    " output inserted.Id values (" +
                    "@LoggerName ,@LogLevel ,@TimeUtc ,@Message ,@ExceptionType ,@ExceptionMessage ,@ExceptionAdditionalInfo ,@CorrelationId " +
                    ",@Server ,@ApplicationPath ,@ProcessId ,@ThreadId ,@Identity ,@Host ,@LoggedUser ,@HttpStatusCode ,@Url ,@Referer ,@ClientIP " +
                    ",@RequestData ,@ResponseData ,@ServiceName ,@ServiceDisplayName, @PerfData)", new DbAppLogRecord {
                        LoggerName = logrec.LoggerName,
                        LogLevel = logrec.LogLevel,
                        TimeUtc = logrec.TimeUtc,
                        Message = logrec.Message,
                        ExceptionType = logrec.ExceptionType,
                        ExceptionMessage = logrec.ExceptionMessage,
                        ExceptionAdditionalInfo = logrec.ExceptionAdditionalInfo,
                        CorrelationId = logrec.CorrelationId,
                        Server = logrec.Server,
                        ApplicationPath = logrec.ApplicationPath,
                        ProcessId = logrec.ProcessId,
                        ThreadId = logrec.ThreadId,
                        Identity = logrec.Identity,
                        Host = logrec.AdditionalFields.TryGetValue("Host", out v) ? ((string)v).ShortenIfNecessary(100) : null,
                        LoggedUser = logrec.AdditionalFields.TryGetValue("LoggedUser", out v) ? ((string)v).ShortenIfNecessary(200) : null,
                        HttpStatusCode = logrec.AdditionalFields.TryGetValue("HttpStatusCode", out v) ? ((string)v).ShortenIfNecessary(15) : null,
                        Url = logrec.AdditionalFields.TryGetValue("Url", out v) ? ((string)v).ShortenIfNecessary(2000) : null,
                        Referer = logrec.AdditionalFields.TryGetValue("Referer", out v) ? ((string)v).ShortenIfNecessary(2000) : null,
                        ClientIP = logrec.AdditionalFields.TryGetValue("ClientIP", out v) ? ((string)v).ShortenIfNecessary(50) : null,
                        RequestData = logrec.AdditionalFields.TryGetValue("RequestData", out v) ? ((string)v).ShortenIfNecessary(2000) : null,
                        ResponseData = logrec.AdditionalFields.TryGetValue("ResponseData", out v) ? ((string)v).ShortenIfNecessary(2000) : null,
                        ServiceName = logrec.AdditionalFields.TryGetValue("ServiceName", out v) ? ((string)v).ShortenIfNecessary(100) : null,
                        ServiceDisplayName = logrec.AdditionalFields.TryGetValue("ServiceDisplayName", out v) ? ((string)v).ShortenIfNecessary(200) : null,
                        PerfData = logrec.PerformanceData != null && logrec.PerformanceData.Count > 0 ? JsonConvert.SerializeObject(
                            logrec.PerformanceData).ShortenIfNecessary(3000) : null
                    }, tran)).Single();
        }

        public async Task UpdateApplicationStatus(IDbConnection conn, IDbTransaction tran, string apphash, LastApplicationStatus status)
        {
            var columnsToUpdate = new List<string>() { "LastUpdateTimeUtc", "ApplicationPath" };
            if (status.ContainsPerformanceData()) {
                columnsToUpdate.AddRange(new[] { "Cpu", "Memory", "LastPerformanceDataUpdateTimeUtc" });
            }
            if (status.ContainsErrorInformation()) {
                columnsToUpdate.AddRange(new[] { "LastErrorTimeUtc", "LastErrorType" });
            }
            var model = new {
                ApplicationHash = apphash,
                status.ApplicationPath,
                status.Server,
                status.Cpu,
                status.Memory,
                status.LastUpdateTimeUtc,
                status.LastPerformanceDataUpdateTimeUtc,
                status.LastErrorType,
                status.LastErrorTimeUtc
            };

            var buffer = new StringBuilder();
            foreach (var col in columnsToUpdate) {
                buffer.AppendFormat("{0}=@{0},", col);
            }
            buffer.Remove(buffer.Length - 1, 1); // last comma

            if (await conn.ExecuteAsync(string.Format("update ApplicationStatus set {0}", buffer.ToString()), model, transaction: tran) == 0) {

                columnsToUpdate.Add("ApplicationHash");
                columnsToUpdate.Add("Server");

                await conn.ExecuteAsync(string.Format("insert into ApplicationStatus ({0}) values (@{1})", string.Join(",", columnsToUpdate),
                    string.Join(",@", columnsToUpdate)), model, tran);
            }
        }
    }
}
