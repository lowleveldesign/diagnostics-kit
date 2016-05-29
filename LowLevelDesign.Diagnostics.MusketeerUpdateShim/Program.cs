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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace LowLevelDesign.Diagnostics.MusketeerUpdateShim
{
    class Updater
    {
        static void Main(string[] args)
        {
            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener(
                File.Create(Path.Combine(Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location), "update.log"))) { 
                TraceOutputOptions = TraceOptions.DateTime
            });

            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(options => {
                    try {
                        Trace.TraceInformation("Waiting for a process {0} to exit.", options.ParentPid);
                        try {
                            Process.GetProcessById(options.ParentPid).WaitForExit();
                        } catch (ArgumentException) {
                            // we can safely swallow this exception - it just indicates that
                            // the parent process already stopped
                        }

                        Trace.TraceInformation("Starting update process.");
                        var updater = new ApplicationUpdater(options.UpdateFilePath);

                        updater.BackupApplicationFiles();
                        try {
                            updater.UpdateApplicationFiles();
                            File.Delete(options.UpdateFilePath);
                            Trace.TraceInformation("Update process finished successfully.");
                        } catch (Exception ex) {
                            Trace.TraceError("Error occurred while updating application files - old files will be restored, ex: {0}", ex);
                            updater.RestoreApplicationFiles();
                        }
                        updater.RemoveApplicationBackup();

                        if (!string.IsNullOrEmpty(options.AppToStartAfterUpdate)) {
                            Trace.TraceInformation("Starting application '{0}'", options.AppToStartAfterUpdate);
                            StartNewApplication(options.AppToStartAfterUpdate, options.Arguments);
                        } else if (!string.IsNullOrEmpty(options.ServiceToStartAfterUpdate)) {
                            Trace.TraceInformation("Starting service '{0}'", options.ServiceToStartAfterUpdate);
                            StartWindowsService(options.ServiceToStartAfterUpdate);
                        }
                    } catch (Exception ex) {
                        Console.WriteLine("ERROR: {0}", ex.Message);
                        Trace.TraceError("ERROR: {0}", ex);
                    }
                });
        }

        static void StartNewApplication(string appPath, IEnumerable<string> args)
        {
            Process.Start(appPath, string.Join(" ", args));
        }

        static void StartWindowsService(string serviceName)
        {
            var serviceController = new ServiceController(serviceName);
            serviceController.Start();
        }
    }
}
