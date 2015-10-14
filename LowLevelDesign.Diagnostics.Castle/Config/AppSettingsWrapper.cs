using System;
using System.Configuration;

namespace LowLevelDesign.Diagnostics.Castle.Config
{
    public static class AppSettingsWrapper
    {
        public static readonly byte DefaultNoOfDaysToKeepLogs = Byte.Parse(ConfigurationManager.AppSettings["diag:defaultNoOfDaysToKeepLogs"] ?? "2");

        public static readonly int DefaultGridCacheTimeInSeconds = Int32.Parse(ConfigurationManager.AppSettings["diag:gridcacheInSeconds"] ?? "30");

        public static readonly bool AuthenticationEnabled;
        
        static AppSettingsWrapper()
        {
            bool flag;
            Boolean.TryParse(ConfigurationManager.AppSettings["diag:authentication-enabled"], out flag);
            AuthenticationEnabled = flag;
        } 
    }
}