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

using System;
using System.Configuration;

namespace LowLevelDesign.Diagnostics.LogStore.SqlServer.Standard
{
    internal static class SqlServerLogStoreConfiguration
    {
        private static readonly SqlServerLogStoreConfigSection configSection;
        private static readonly string connectionString;
        private static readonly string connectionStringName;

        static SqlServerLogStoreConfiguration()
        {
            configSection = ConfigurationManager.GetSection("sqlServerLogStore") as SqlServerLogStoreConfigSection;
            if (configSection == null) {
                throw new ConfigurationErrorsException("sqlServerLogStore section is required in the application configuration file.");
            }
            try {
                connectionStringName = configSection.ConnectionStringName;
                connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            } catch (Exception ex) {
                throw new ConfigurationErrorsException(string.Format(
                    "There is something wrong with the connection string to the Sql Server log store database, error: {0}", ex));
            }
        }

        public static string ConnectionString { get { return connectionString; } }

        public static string ConnectionStringName { get { return connectionStringName;  } }
    }

    public sealed class SqlServerLogStoreConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("connectionStringName", IsRequired = true, IsKey = true)]
        public String ConnectionStringName { get { return (String)this["connectionStringName"]; } }
    }
}
