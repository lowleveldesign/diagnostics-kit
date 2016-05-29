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

using CommandLine;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.MusketeerUpdateShim
{
    public class CommandLineOptions
    {
        [Option("pid", HelpText = "Parent process ID", Required = true)]
        public int ParentPid { get; set; }

        [Option("update", HelpText = "A path to a .zip file containg the application update.", Required = true)]
        public string UpdateFilePath { get; set; }

        [Option("startsvc", HelpText = "Windows Service name which should be started when the update is done.", Required = false)]
        public string ServiceToStartAfterUpdate { get; set; }

        [Option("startapp", HelpText = "Application to start when the update is done - must be the last argument, " +
            "arguments after it are treated as new application arguments", Required = false)]
        public string AppToStartAfterUpdate { get; set; }


        [Value(0)]
        public IList<string> Arguments { get; set; }
    }
}
