using System;
using System.Configuration;

namespace LowLevelDesign.Diagnostics.Castle.Config
{
    internal static class AppSettingsWrapper
    {
        public static readonly byte DefaultNoOfDaysToKeepLogs = Byte.Parse(ConfigurationManager.AppSettings["diag:defaultNoOfDaysToKeepLogs"] ?? "2");
    }
}