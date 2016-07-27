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
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    internal class LogTable : ILogTable
    {
        private static readonly object lck = new object();
        private static readonly ISet<string> availableTables = new HashSet<string>();

        private readonly Func<DateTime> currentUtcDateRetriever;

        static LogTable()
        {
            // query the mysql database and retrieve all the log tables
            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                conn.Open();
                availableTables = new HashSet<string>(conn.Query<string>("select table_name from information_schema.tables where table_schema = @Database", conn),
                                                        StringComparer.OrdinalIgnoreCase);

                if (!availableTables.Contains("ApplicationStatus")) {
                    // we need to create a table which will store application statuses for the grid
                    conn.Execute("create table if not exists ApplicationStatus (ApplicationHash char(32), Server varchar(200) not null, ApplicationPath varchar(2000) not null, " +
                        "Cpu float, Memory float null, LastPerformanceDataUpdateTimeUtc datetime, LastErrorType varchar(100), LastErrorTimeUtc datetime," +
                        "LastUpdateTimeUtc datetime not null, primary key (ApplicationHash, Server))");
                }
            }
        }

        public bool IsLogTableAvailable(string tableName)
        {
            return availableTables.Contains(tableName);
        }

        public LogTable(Func<DateTime> currentUtcDateRetriever)
        {
            this.currentUtcDateRetriever = currentUtcDateRetriever;
        }

        public void CreateLogTableIfNotExists(IDbConnection conn, IDbTransaction tran, string tableName)
        {
            if (!availableTables.Contains(tableName)) {
                lock (lck) {
                    if (!availableTables.Contains(tableName)) {
                        var currentUtcDate = currentUtcDateRetriever().Date;
                        conn.Execute("create table if not exists " + tableName + " (Id int unsigned auto_increment not null,LoggerName varchar(200) not null" +
                            ",LogLevel smallint not null ,TimeUtc datetime not null ,Message varchar(7000) null ,ExceptionType varchar(100) null" +
                            ",ExceptionMessage varchar(2000) null ,ExceptionAdditionalInfo text null ,CorrelationId varchar(100) null" +
                            ",Server varchar(200) not null ,ApplicationPath varchar(2000) null ,ProcessId int null ,ThreadId int null" +
                            ",Identity varchar(200) null ,Host varchar(100) null ,LoggedUser varchar(200) null ,HttpStatusCode varchar(15) character set ascii null" +
                            ",Url varchar(2000) null ,Referer varchar(2000) null ,ClientIP varchar(50) character set ascii null ,RequestData text null" +
                            ",ResponseData text null,ServiceName varchar(100) null ,ServiceDisplayName varchar(200) null, PerfData varchar(3000) null" +
                            ",PRIMARY KEY (TimeUtc, Server, Id), KEY(Id)) COLLATE='utf8_general_ci' PARTITION BY RANGE COLUMNS(TimeUtc)" +
                            string.Format("({0},{1})",
                            GetPartitionDefinition(currentUtcDate), GetPartitionDefinition(currentUtcDate.AddDays(1))),
                            transaction: tran);
                        availableTables.Add(tableName);
                    }
                }
            }
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
            return (uint)(await conn.QueryAsync<ulong>("insert into " + tableName + "(LoggerName ,LogLevel ,TimeUtc ,Message ,ExceptionType " +
                    ",ExceptionMessage ,ExceptionAdditionalInfo ,CorrelationId ,Server ,ApplicationPath ,ProcessId ,ThreadId ,Identity ,Host " +
                    ",LoggedUser ,HttpStatusCode ,Url ,Referer ,ClientIP ,RequestData ,ResponseData ,ServiceName ,ServiceDisplayName, PerfData) values (" +
                    "@LoggerName ,@LogLevel ,@TimeUtc ,@Message ,@ExceptionType ,@ExceptionMessage ,@ExceptionAdditionalInfo ,@CorrelationId " +
                    ",@Server ,@ApplicationPath ,@ProcessId ,@ThreadId ,@Identity ,@Host ,@LoggedUser ,@HttpStatusCode ,@Url ,@Referer ,@ClientIP " +
                    ",@RequestData ,@ResponseData ,@ServiceName ,@ServiceDisplayName, @PerfData); select LAST_INSERT_ID()", new DbAppLogRecord {
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
            var columnsToUpdate = new List<string>() { "ApplicationHash", "ApplicationPath", "Server", "LastUpdateTimeUtc" };
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
            await conn.ExecuteAsync(string.Format("replace into ApplicationStatus ({0}) values (@{1})", string.Join(",", columnsToUpdate),
                string.Join(",@", columnsToUpdate)), model, tran);
        }

        private static string GetPartitionDefinition(DateTime dt)
        {
            return string.Format("PARTITION {0} VALUES LESS THAN ('{1:yyyy-MM-dd HH:mm}')", Partition.ForDay(dt).Name, dt.Date.AddDays(1));
        }


        public async Task ManageTablePartitions(IDbConnection conn, string tableName, TimeSpan keepTime)
        {
            var partitions = await conn.QueryAsync<Partition>("select partition_name as Name from information_schema.partitions " +
                "where table_schema = @Database and table_name = @TableName", new { conn.Database, TableName = tableName });

            DateTime today = currentUtcDateRetriever(), tomorrow = today.AddDays(1);
            // if zero timespan is passed it means that no partition should be removed
            var oldestPartition = Partition.ForDay(keepTime == TimeSpan.Zero ? DateTime.MinValue : today.Subtract(keepTime));
            var currentPartition = Partition.ForDay(today);
            var futurePartition = Partition.ForDay(tomorrow);

            bool isCurrentPartitionCreated = false, isFuturePartitionCreated = false;
            var partitionsToDrop = new List<string>();
            foreach (var partition in partitions) {
                if (oldestPartition.CompareTo(partition) > 0) {
                    partitionsToDrop.Add(partition.Name);
                } else if (currentPartition.Equals(partition)) {
                    isCurrentPartitionCreated = true;
                } else if (futurePartition.Equals(partition)) {
                    isFuturePartitionCreated = true;
                }
            }

            if (!isCurrentPartitionCreated) {
                await conn.ExecuteAsync(string.Format("alter table {0} add partition (partition {1} values less than ('{2:yyyy-MM-dd HH:mm}'))",
                    tableName, currentPartition.Name, today.AddDays(1)));
            }
            if (!isFuturePartitionCreated) {
                await conn.ExecuteAsync(string.Format("alter table {0} add partition (partition {1} values less than ('{2:yyyy-MM-dd HH:mm}'))",
                    tableName, futurePartition.Name, today.AddDays(2)));
            }
            // remove older partitions
            foreach (var p in partitionsToDrop) {
                await conn.ExecuteAsync(string.Format("alter table {0} drop partition {1}", tableName, p));
            }
        }
    }
}
