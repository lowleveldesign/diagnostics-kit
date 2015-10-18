using LowLevelDesign.Diagnostics.LogStore.Commons.Auth;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LowLevelDesign.Diagnostics.Castle.Config
{
    internal static class AppSettings
    {
        // Diagnostics defaults - we should use them only if other options are not available
        private const String defaultTypesNamespace = "LowLevelDesign.Diagnostics.LogStore.Defaults";
        private const String UserMgrKey = "diag:usermgr";
        private const String LogStoreKey = "diag:logstore";
        private const String ConfMgrKey = "diag:confmgr";

        public static readonly byte DefaultNoOfDaysToKeepLogs = Byte.Parse(ConfigurationManager.AppSettings["diag:defaultNoOfDaysToKeepLogs"] ?? "2");
        public static readonly int DefaultGridCacheTimeInSeconds = Int32.Parse(ConfigurationManager.AppSettings["diag:gridcacheInSeconds"] ?? "30");

        private static readonly IAppUserManager appUserManager;
        private static readonly ILogStore logStore;
        private static readonly IAppConfigurationManager appConfigurationManager;

        static AppSettings()
        {
            // Logging configuration
            Log.Logger = new LoggerConfiguration().WriteTo.Trace().CreateLogger();

            /* CONFIGURATION */
            var typeName = ConfigurationManager.AppSettings[ConfMgrKey];
            Type type;
            if (typeName == null) {
                type = FindSingleTypeInLowLevelDesignAssemblies(typeof(IAppConfigurationManager), ConfMgrKey);
            } else {
                type = Type.GetType(typeName);
            }
            appConfigurationManager = (IAppConfigurationManager)Activator.CreateInstance(type);

            /* LOG STORAGE */
            typeName = ConfigurationManager.AppSettings[LogStoreKey];
            if (typeName == null) {
                type = FindSingleTypeInLowLevelDesignAssemblies(typeof(ILogStore), LogStoreKey);
            } else {
                type = Type.GetType(typeName);
            }
            logStore = (ILogStore)Activator.CreateInstance(type);

            /* SECURITY */
            typeName = ConfigurationManager.AppSettings[UserMgrKey];
            if (typeName == null) {
                type = FindSingleTypeInLowLevelDesignAssemblies(typeof(IAppUserManager), UserMgrKey);
            } else {
                type = Type.GetType(typeName);
            }
            appUserManager = (IAppUserManager)Activator.CreateInstance(type);
        }

        private static Type FindSingleTypeInLowLevelDesignAssemblies(Type typeToImplement, String confkey)
        {
            var implementers = new List<Type>();
            var bindir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            foreach (var asmpath in Directory.GetFiles(bindir, "LowLevelDesign.*.dll")) {
                try {
                    var asm = Assembly.LoadFrom(asmpath);
                    implementers.AddRange(asm.GetExportedTypes().Where(t => t.GetInterfaces().Contains(typeToImplement)));
                } catch (Exception ex) {
                    // just swallow and check the next one
                    Log.Debug(ex, "Failure while loading assembly from '{0}'", asmpath);
                }
                if (implementers.Count > 1) {
                    // we may skip the default one if present
                    var ind = implementers.FindIndex(t => t.Namespace.StartsWith(defaultTypesNamespace, StringComparison.Ordinal));
                    if (ind >= 0) {
                        implementers.RemoveAt(ind);
                    }
                    if (implementers.Count > 1) {
                        throw new ConfigurationErrorsException("More than one class implementing " + typeToImplement.FullName +
                            ". Please specify which one should be used by adding '" + confkey + " ' " +
                            "key in the appsettings. Please check documentation if in doubt.");
                    }
                }
            }
            if (implementers.Count == 0) {
                // no log store found
                throw new ConfigurationErrorsException("No class implementing " + typeToImplement.FullName + " found. Please add at least one " +
                    "assembly where we could find such a class. Please check documentation if in doubt.");
            }
            return implementers[0];
        }

        public static IAppUserManager GetAppUserManager()
        {
            return appUserManager;
        }

        public static ILogStore GetLogStore()
        {
            return logStore;
        }

        public static IAppConfigurationManager GetAppConfigurationManager()
        {
            return appConfigurationManager;
        }
    }
}