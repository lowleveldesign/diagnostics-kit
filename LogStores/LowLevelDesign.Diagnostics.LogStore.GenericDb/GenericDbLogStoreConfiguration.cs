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

namespace LowLevelDesign.Diagnostics.LogStore.GenericDb
{
    internal static class GenericDbLogStoreConfiguration
    {
        private static readonly GenericDbLogStoreConfigSection configSection;
        private static readonly string connectionString;
        private static readonly string connectionStringName;

        static GenericDbLogStoreConfiguration()
        {
            configSection = ConfigurationManager.GetSection("genericDbLogStore") as GenericDbLogStoreConfigSection;
            if (configSection == null) {
                throw new ConfigurationErrorsException("genericDbLogStore section is required in the application configuration file.");
            }
            try {
                connectionStringName = configSection.ConnectionStringName;
                connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            } catch (Exception ex) {
                throw new ConfigurationErrorsException(string.Format(
                    "There is something wrong with the connection string to the MySql log store database, error: {0}", ex));
            }
        }

        public static string ConnectionString { get { return connectionString; } }

        public static string ConnectionStringName { get { return connectionStringName;  } }
    }

    public sealed class GenericDbLogStoreConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("connectionStringName", IsRequired = true, IsKey = true)]
        public string ConnectionStringName { get { return (string)this["connectionStringName"]; } }
    }
}
