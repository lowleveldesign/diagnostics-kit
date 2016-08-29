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

namespace LowLevelDesign.Diagnostics.Musketeer.Models
{
    public enum ELogType
    {
        W3SVC,
        TextFile
    }
    public struct AppDomainInfo
    {
        public long Id;

        public string Name;
    }


    public class AppInfo
    {
        public string Path { get; set; }

        public int[] ProcessIds { get; set; }

        public bool LogEnabled { get; set; }

        public ELogType LogType { get; set; }

        public string LogsPath { get; set; }

        public string LogFilter { get; set; }

        public AppDomainInfo[] AppDomains { get; set; }
    }
}
