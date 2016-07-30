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
using LowLevelDesign.Diagnostics.LogStore.Defaults;
using System.Data.Common;
using System.Data.SqlClient;

namespace LowLevelDesign.Diagnostics.LogStore.SqlServer.Standard
{
    public sealed class SqlServerAppConfigurationManager : DefaultAppConfigurationManager
    { 
        public SqlServerAppConfigurationManager()
            : base(SqlServerLogStoreConfiguration.ConnectionStringName)
        {
            using (var conn = new SqlConnection(dbConnString)) {
                conn.Open();

                conn.Execute("if object_id('Applications') is null create table Applications (PathHash binary(16) primary key," +
                        "Path nvarchar(2000) not null, Name nvarchar(500) not null, IsExcluded bit not null, IsHidden bit not null, DaysToKeepLogs tinyint)");

                conn.Execute("if object_id('ApplicationConfigs') is null create table ApplicationConfigs (PathHash binary(16) not null, Path nvarchar(2000) not null, " +
                                "Server nvarchar(200) not null, ServerFqdnOrIp nvarchar(255) not null, Binding nvarchar(3000) not null, AppPoolName nvarchar(500), " +
                                "AppType char(3), ServiceName nvarchar(300), DisplayName nvarchar(500), primary key (PathHash, Server))");

                conn.Execute("if object_id('Globals') is null create table Globals (ConfKey varchar(100) not null primary key, ConfValue nvarchar(1000))");
            }
        }

        protected override DbConnection CreateConnection()
        {
            return new SqlConnection(dbConnString);
        }
    }
}
