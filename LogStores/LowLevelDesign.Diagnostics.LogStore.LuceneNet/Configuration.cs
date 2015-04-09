using System;
using System.Configuration;

namespace LowLevelDesign.Diagnostics.LogStore.LuceneNet
{
    internal static class LogStoreConfiguration
    {
        private static readonly LuceneNetConfigSection luceneNetConfigSection;

        static LogStoreConfiguration()
        {
            luceneNetConfigSection = ConfigurationManager.GetSection("luceneNetLogStore") as LuceneNetConfigSection;
            if (luceneNetConfigSection == null) {
                throw new ConfigurationErrorsException("luceneNetLogStore section is required in the application configuration file.");
            }
            if (luceneNetConfigSection.LogEnabled && String.IsNullOrEmpty(luceneNetConfigSection.LogPath)) {
                throw new ConfigurationErrorsException("Log store logging is enabed and no log file path is defined. Add logPath attribute to the luceneNetLogStore section.");
            }
        }

        public static LuceneNetConfigSection LuceneNetConfigSection { get { return luceneNetConfigSection; } }
    }

    public sealed class LuceneNetConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("indexPath", IsRequired = true, IsKey = true)]
        public String IndexPath { get { return (String)this["indexPath"]; } }

        [ConfigurationProperty("logEnabled", IsRequired = false, DefaultValue = false)]
        public bool LogEnabled { get { return (bool)this["logEnabled"]; } }

        [ConfigurationProperty("logPath", IsRequired = false, DefaultValue = null)]
        public String LogPath { get { return (String)this["logPath"]; } }
    }
}
