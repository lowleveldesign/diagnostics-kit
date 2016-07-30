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

using LowLevelDesign.Diagnostics.LogStore.Commons.Auth;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LowLevelDesign.Diagnostics.Castle.Config
{
    internal static class AppSettings
    {
        private static readonly TraceSource logger = new TraceSource("LowLevelDesign.Diagnostics.Castle");

        // Diagnostics defaults - we should use them only if other options are not available
        private const string UserMgrKey = "diag:usermgr";
        private const string LogStoreKey = "diag:logstore";
        private const string ConfMgrKey = "diag:confmgr";

        public static readonly byte DefaultNoOfDaysToKeepLogs = Byte.Parse(ConfigurationManager.AppSettings["diag:defaultNoOfDaysToKeepLogs"] ?? "2");
        public static readonly int DefaultGridCacheTimeInSeconds = Int32.Parse(ConfigurationManager.AppSettings["diag:gridcacheInSeconds"] ?? "30");

        private static readonly IAppUserManager appUserManager;
        private static readonly ILogStore logStore;
        private static readonly IAppConfigurationManager appConfigurationManager;

        static AppSettings()
        {
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
            var executingAssembly = Assembly.GetExecutingAssembly();
            var implementers = new List<Type>();
            var bindir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            foreach (var asmpath in Directory.GetFiles(bindir, "LowLevelDesign.*.dll")) {
                try {
                    var asm = Assembly.LoadFrom(asmpath);
                    if (!executingAssembly.Equals(asm)) {
                        implementers.AddRange(asm.GetExportedTypes().Where(t => t.GetInterfaces().Contains(typeToImplement)));
                    }
                } catch (Exception ex) {
                    logger.TraceEvent(TraceEventType.Error, 0, "Failure while loading assembly from '{0}', ex: {1}", asmpath, ex);
                }
                if (implementers.Count > 1) {
                    // we may skip the default one if present
                    var ind = implementers.FindIndex(t => t.IsAbstract);
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
