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
        private static readonly MySqlConfigSection mySqlConfigSection;
        private static readonly String connectionString;

        static MySqlLogStoreConfiguration()
        {
            mySqlConfigSection = ConfigurationManager.GetSection("mySqlLogStore") as MySqlConfigSection;
            if (mySqlConfigSection == null) {
                throw new ConfigurationErrorsException("mySqlLogStore section is required in the application configuration file.");
            }
            try {
                connectionString = ConfigurationManager.ConnectionStrings[mySqlConfigSection.ConnectionStringName].ConnectionString;
            } catch (Exception ex) {
                throw new ConfigurationErrorsException(String.Format(
                    "There is something wrong with the connection string to the MySql log store database, error: {0}", ex));
            }
        }

        public static String ConnectionString { get { return connectionString; } }
    }

    public sealed class MySqlConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("connectionStringName", IsRequired = true, IsKey = true)]
        public String ConnectionStringName { get { return (String)this["connectionStringName"]; } }
    }
}
