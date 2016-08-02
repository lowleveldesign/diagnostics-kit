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


using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Musketeer.Models;
using System;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Musketeer.Connectors
{
    public sealed class LogStashConnector : IMusketeerConnector
    {


        public LogStashConnector()
        {

        }

        public bool IsEnabled {
            get { throw new NotImplementedException(); }
        }

        public bool SupportsApplicationConfigs { get { return false; } }

        public string[] SendApplicationConfigs(IEnumerable<ApplicationServerConfig> configs)
        {
            throw new InvalidOperationException();
        }

        public void SendLogRecord(LogRecord logrec)
        {
            throw new NotImplementedException();
        }

        public void SendLogRecords(IEnumerable<LogRecord> logrecs)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
