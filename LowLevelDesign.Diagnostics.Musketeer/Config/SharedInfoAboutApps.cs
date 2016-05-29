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

using LowLevelDesign.Diagnostics.Musketeer.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LowLevelDesign.Diagnostics.Musketeer.Config
{
    public interface ISharedInfoAboutApps
    {
        IEnumerable<AppInfo> GetApps();

        IEnumerable<int> GetProcessIds();

        IEnumerable<AppInfo> FindAppsByLogsPath(string logPath);

        IEnumerable<AppInfo> FindAppsByProcessId(int pid);

        void UpdateAppWorkerProcessesMap(Dictionary<string, AppInfo> map);
    }

    public sealed class SharedInfoAboutApps : ISharedInfoAboutApps
    {
        public static readonly string MachineName = Environment.MachineName;
        private static readonly AppInfo[] NoApps = new AppInfo[0];

        private readonly ReaderWriterLockSlim lck = new ReaderWriterLockSlim();
        // maps paths to apps
        private Dictionary<string, AppInfo> appPathToAppInfoMap = new Dictionary<string, AppInfo>();
        // maps PID to path
        private Dictionary<int, IEnumerable<AppInfo>> workerProcessIdToAppInfoMap = new Dictionary<int, IEnumerable<AppInfo>>();
        // maps log paths to apps
        private Dictionary<string, IEnumerable<AppInfo>> logsPathToAppInfoMap = new Dictionary<string, IEnumerable<AppInfo>>();

        public IEnumerable<AppInfo> GetApps()
        {
            lck.EnterReadLock();
            try {
                return appPathToAppInfoMap.Values;
            } finally {
                lck.ExitReadLock();
            }
        }

        public IEnumerable<int> GetProcessIds()
        {
            lck.EnterReadLock();
            try {
                return workerProcessIdToAppInfoMap.Keys;
            } finally {
                lck.ExitReadLock();
            }
        }

        public IEnumerable<AppInfo> FindAppsByLogsPath(string logPath)
        {
            lck.EnterReadLock();
            try {
                IEnumerable<AppInfo> apps;
                if (!logsPathToAppInfoMap.TryGetValue(logPath, out apps)) {
                    return NoApps;
                }
                return apps;
            } finally {
                lck.ExitReadLock();
            }
        }

        public IEnumerable<AppInfo> FindAppsByProcessId(int pid)
        {
            lck.EnterReadLock();
            try {
                IEnumerable<AppInfo> apps;
                if (!workerProcessIdToAppInfoMap.TryGetValue(pid, out apps)) {
                    return NoApps;
                }
                return apps;
            } finally {
                lck.ExitReadLock();
            }
        }

        /// <summary>
        /// Updates the shared configuration mappings - we use those structures
        /// to monitor only interesting for us applications.
        /// </summary>
        /// <param name="map"></param>
        public void UpdateAppWorkerProcessesMap(Dictionary<string, AppInfo> map)
        {
            var wpmap = new Dictionary<int, IEnumerable<AppInfo>>();
            var logmap = new Dictionary<string, IEnumerable<AppInfo>>(StringComparer.Ordinal);
            foreach (var m in map) {
                foreach (var pid in m.Value.ProcessIds) {
                    IEnumerable<AppInfo> en;
                    if (!wpmap.TryGetValue(pid, out en)) {
                        en = new List<AppInfo>();
                        wpmap.Add(pid, en);
                    }
                    ((List<AppInfo>)en).Add(m.Value);
                }
                if (m.Value.LogEnabled) {
                    IEnumerable<AppInfo> en;
                    if (!logmap.TryGetValue(m.Value.LogsPath, out en)) {
                        en = new List<AppInfo>();
                        logmap.Add(m.Value.LogsPath, en);
                    }
                    ((List<AppInfo>)en).Add(m.Value);
                }
            }

            lck.EnterWriteLock();
            try {
                appPathToAppInfoMap = map;
                workerProcessIdToAppInfoMap = wpmap;
                logsPathToAppInfoMap = logmap;
            } finally {
                lck.ExitWriteLock();
            }
        }
    }
}
