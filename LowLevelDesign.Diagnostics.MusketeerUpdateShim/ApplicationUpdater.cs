using LowLevelDesign.Diagnostics.Musketeer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace LowLevelDesign.Diagnostics.MusketeerUpdateShim
{
    public class ApplicationUpdater
    {
        private class ConfigFileBackup
        {
            public string OriginalConfigFilePath { get; set; }

            public string BackupConfigFilePath { get; set; }
        }

        private readonly string updateFilePath;
        private readonly string destinationFolderPath;
        private readonly string backupFilePath;
        private IList<ConfigFileBackup> configBackups;

        public ApplicationUpdater(string updateFilePath)
        {
            this.updateFilePath = updateFilePath;
            destinationFolderPath = Path.GetDirectoryName(updateFilePath);
            backupFilePath = Path.Combine(Path.GetTempPath(),
                string.Format("musketeer-backup_{0:yyyyMMdd_HHmm}.zip", DateTime.Now));
        }

        public void BackupApplicationFiles()
        {
            Trace.TraceInformation("Backuping application files to '{0}'", backupFilePath);

            var updaterAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var exclusionList = new string[] { "update.zip", "update.log", updaterAssemblyName + ".dll",
                updaterAssemblyName + ".exe", updaterAssemblyName + ".pdb" };
            FileUtils.CreateZipFromDirectory(destinationFolderPath, backupFilePath, exclusionList);
        }

        public void RestoreApplicationFiles()
        {
            FileUtils.ExtractZipToDirectoryAndOverrideExistingFiles(backupFilePath, destinationFolderPath);
        }

        public void RemoveApplicationBackup()
        {
            if (File.Exists(backupFilePath)) {
                File.Delete(backupFilePath);
            }
        }

        public void UpdateApplicationFiles()
        {
            if (!File.Exists(updateFilePath)) {
                throw new ArgumentException("The update file does not exist.");
            }
            var destinationFolder = Path.GetDirectoryName(updateFilePath);

            BackupConfigurationFiles();

            FileUtils.ExtractZipToDirectoryAndOverrideExistingFiles(updateFilePath, destinationFolder);

            ApplySettingsFromBackupConfigurationFiles();
        }

        private void BackupConfigurationFiles()
        {
            configBackups = new List<ConfigFileBackup>();
            foreach (var configFile in Directory.GetFiles(destinationFolderPath, "*.exe.config")) {
                var backupConfigFile = configFile + ".old";
                configBackups.Add(new ConfigFileBackup {
                    OriginalConfigFilePath = configFile,
                    BackupConfigFilePath = backupConfigFile
                });
                File.Copy(configFile, backupConfigFile, true);
            }
        }

        private void ApplySettingsFromBackupConfigurationFiles()
        {
            foreach (var configBackup in configBackups) {
                var backupConfiguration = ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap { ExeConfigFilename = configBackup.BackupConfigFilePath },
                    ConfigurationUserLevel.None);
                var destinationConfiguration = ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap { ExeConfigFilename = configBackup.OriginalConfigFilePath },
                    ConfigurationUserLevel.None);

                var sourceSettings = backupConfiguration.AppSettings.Settings;
                var destSettings = destinationConfiguration.AppSettings.Settings;
                foreach (var appSettingKey in sourceSettings.AllKeys) {
                    destSettings.Remove(appSettingKey);
                    destSettings.Add(appSettingKey, sourceSettings[appSettingKey].Value);
                }

                destinationConfiguration.Save();
            }
        }
    }
}
