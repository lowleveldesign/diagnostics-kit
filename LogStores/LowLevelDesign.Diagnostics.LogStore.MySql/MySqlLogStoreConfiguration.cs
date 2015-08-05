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
