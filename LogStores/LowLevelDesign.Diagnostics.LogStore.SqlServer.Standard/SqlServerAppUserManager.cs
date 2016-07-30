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
    public class SqlServerAppUserManager : DefaultAppUserManager
    {
        public SqlServerAppUserManager() 
            : base(SqlServerLogStoreConfiguration.ConnectionStringName)
        {
            using (var conn = new SqlConnection(dbConnString)) {
                conn.Open();

                conn.Execute("if object_id('Users') is null create table Users (Id nvarchar(32) not null primary key," +
                        "UserName nvarchar(100) not null unique, PasswordHash nvarchar(1000), " +
                        "RegistrationDateUtc datetime not null)");
                conn.Execute("create unique nonclustered index NCIX_Users_UserName on Users(UserName)");

                conn.Execute("if object_id('UserClaims') is null create table UserClaims (UserId nvarchar(32) not null references Users(Id)," +
                    "ClaimType nvarchar(250) not null, ClaimValue nvarchar(1000) not null, primary key(UserId, ClaimType))");
            }
        }

        protected override DbConnection CreateConnection()
        {
            return new SqlConnection(dbConnString);
        }
    }
}
