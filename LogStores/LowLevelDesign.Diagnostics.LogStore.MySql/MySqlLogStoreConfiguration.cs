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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    internal static class MySqlLogStoreConfiguration
    {
        private static readonly MySqlLogStoreConfigSection mySqlConfigSection;
        private static readonly String connectionString;
        private static readonly String connectionStringName;

        static MySqlLogStoreConfiguration()
        {
            mySqlConfigSection = ConfigurationManager.GetSection("mySqlLogStore") as MySqlLogStoreConfigSection;
            if (mySqlConfigSection == null) {
                throw new ConfigurationErrorsException("mySqlLogStore section is required in the application configuration file.");
            }
            try {
                connectionStringName = mySqlConfigSection.ConnectionStringName;
                connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            } catch (Exception ex) {
                throw new ConfigurationErrorsException(String.Format(
                    "There is something wrong with the connection string to the MySql log store database, error: {0}", ex));
            }
        }

        public static String ConnectionString { get { return connectionString; } }

        public static String ConnectionStringName { get { return connectionStringName;  } }
    }

    public sealed class MySqlLogStoreConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("connectionStringName", IsRequired = true, IsKey = true)]
        public String ConnectionStringName { get { return (String)this["connectionStringName"]; } }
    }
}
