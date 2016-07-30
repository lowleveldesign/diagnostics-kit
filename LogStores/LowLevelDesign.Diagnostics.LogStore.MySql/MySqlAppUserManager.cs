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
using MySql.Data.MySqlClient;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    public sealed class MySqlAppUserManager : DefaultAppUserManager
    {
        public MySqlAppUserManager()
            : base(MySqlLogStoreConfiguration.ConnectionStringName)
        {
            using (var conn = new MySqlConnection(dbConnString)) {
                conn.Open();

                conn.Execute("create table if not exists Users (Id varchar(32) not null primary key," +
                        "UserName varchar(100) not null unique, PasswordHash varchar(1000), " +
                        "RegistrationDateUtc datetime not null)");
                conn.Execute("create unique index NCIX_Users_UserName on Users(UserName)");

                conn.Execute("create table if not exists UserClaims (UserId varchar(32) not null references Users(Id), ClaimType varchar(250) not null, " +
                    "ClaimValue varchar(1000) not null, primary key(UserId, ClaimType))");
            }
        }

        protected override System.Data.Common.DbConnection CreateConnection()
        {
            return new MySqlConnection(dbConnString);
        }
    }
}
