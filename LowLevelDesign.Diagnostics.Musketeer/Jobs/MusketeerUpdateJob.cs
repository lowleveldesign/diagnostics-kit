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
using LowLevelDesign.Diagnostics.Musketeer.Output;
using NLog;
using Quartz;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace LowLevelDesign.Diagnostics.Musketeer.Jobs
{
    [DisallowConcurrentExecution]
    public sealed class MusketeerUpdateJob : IJob
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string executingAssemblyPath = Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location);

        private readonly IMusketeerConnector castleConnector;

        public MusketeerUpdateJob(IMusketeerConnectorFactory castleConnectorFactory) {
            castleConnector= castleConnectorFactory.CreateConnector();
        }

        public void Execute(IJobExecutionContext context)
        {
            var updateInformation = castleConnector.GetInformationAboutMusketeerUpdates();

            UpdateShimIfNecessary(updateInformation.UpdateForApplicationShim);

            UpdateMusketeerIfNecessary(updateInformation.UpdateForApplication);
        }

        private void UpdateShimIfNecessary(UpdateAvailability shimUpdateAvailability)
        {
            if (string.IsNullOrEmpty(shimUpdateAvailability.Version)) {
                return;
            }
            var updateShimPath = Path.Combine(executingAssemblyPath, "MusketeerUpdateShim.exe");
            var shimVersion = File.Exists(updateShimPath) ? AssemblyName.GetAssemblyName(updateShimPath).Version 
                : new Version("0.0.0.0");
            var updateVersion = new Version(shimUpdateAvailability.Version);
            if (updateVersion.CompareTo(shimVersion) <= 0) {
                return;
            }

            logger.Info("New version of the MusketeerUpdateShim found: '{0}'. Starting update.", updateVersion);
            var downloadFilePath = Path.Combine(executingAssemblyPath, "updateshim.zip");
            castleConnector.DownloadFile(shimUpdateAvailability.FullUrlToUpdate, downloadFilePath);
            if (IsFileUnaltered(downloadFilePath, shimUpdateAvailability.FileHash)) {
                FileUtils.ExtractZipToDirectoryAndOverrideExistingFiles(downloadFilePath, executingAssemblyPath);
                logger.Info("Shim update to version '{0}' finished.", updateVersion);
            } else {
                logger.Warn("Shim update failed. The checksum of the file did not match the original.");
            }
            File.Delete(downloadFilePath);
        }

        private void UpdateMusketeerIfNecessary(UpdateAvailability updateAvailability)
        {
            if (string.IsNullOrEmpty(updateAvailability.Version)) {
                return;
            }

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var updateVersion = new Version(updateAvailability.Version);
            if (updateVersion.CompareTo(currentVersion) <= 0) {
                return;
            }

            var downloadFilePath = Path.Combine(executingAssemblyPath, "update.zip");
            if (File.Exists(downloadFilePath)) {
                logger.Error("The update file exists - this indicates that the previous update was unsuccessful and " +
                    "we won't try again. Please check the update.log and remove the update.zip file when the problem " +
                    "is fixed.");
                return;
            }

            logger.Info("New version of the Musketeer service found: '{0}'. Starting update procedure.", updateVersion);
            castleConnector.DownloadFile(updateAvailability.FullUrlToUpdate, downloadFilePath);
            if (!IsFileUnaltered(downloadFilePath, updateAvailability.FileHash))
            {
                logger.Warn("The downloaded file checksum did not match the original - update aborted. We will try again later.");
                File.Delete(downloadFilePath);
                return;
            }

            var currentProcess = Process.GetCurrentProcess();
            var shimPath = Path.Combine(executingAssemblyPath, "MusketeerUpdateShim.exe");
            Process shimProcess;
            if (currentProcess.SessionId == 0) {
                shimProcess = Process.Start(shimPath, string.Format("--pid {0} --update \"{1}\" --startsvc \"{2}\"",
                    currentProcess.Id, downloadFilePath, MusketeerService.ServiceName));
            logger.Info("Update process was started with PID: {0}. The Musketeer service is stopping now.",
                shimProcess.Id);
                new ServiceController(MusketeerService.ServiceName).Stop();
            } else {
                shimProcess = Process.Start(shimPath, string.Format("--pid {0} --update \"{1}\" --startapp \"{2}\"",
                    currentProcess.Id, downloadFilePath, Assembly.GetEntryAssembly().Location));
                logger.Info("Update process was started with PID: {0}. The update will start after Musketeer is closed.",
                    shimProcess.Id);
            }

        }

        private static bool IsFileUnaltered(string fileName, string fileHash)
        {
            return fileHash == null || string.Equals(FileUtils.CalculateFileHash(fileName), 
                fileHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
