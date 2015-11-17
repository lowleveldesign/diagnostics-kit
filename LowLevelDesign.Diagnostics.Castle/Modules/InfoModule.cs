using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Models;
using Nancy;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class InfoModule : NancyModule
    {
        private readonly IRootPathProvider pathProvider;

        public InfoModule(GlobalConfig globalSettings, IRootPathProvider pathProvider)
        {
            this.pathProvider = pathProvider;

            Get["/about"] = _ => {
                var result = new DiagnosticsKitInformation {
                    Version = SelfInformation.ApplicationVersion,
                    UsedLogStore = AppSettings.GetLogStore().GetType().AssemblyQualifiedName,
                    UsedAppConfigurationManager = AppSettings.GetAppConfigurationManager().GetType().AssemblyQualifiedName,
                    UsedAppUserManager = AppSettings.GetAppUserManager().GetType().AssemblyQualifiedName,
                    IsAuthenticationEnabled = globalSettings.IsAuthenticationEnabled()
                };
                return Negotiate.WithView("About.cshtml").WithModel(result);
            };

            Get["/about-musketeer"] = _ => {
                return Response.AsJson(new ApplicationUpdate {
                    UpdateForApplication = FindUpdate("musketeer_"),
                    UpdateForApplicationShim = FindUpdate("musketeershim_")
                });
            };

            Get["/about-bishop"] = _ => {
                return Response.AsJson(new ApplicationUpdate {
                    UpdateForApplication = FindUpdate("bishop_"),
                    UpdateForApplicationShim = FindUpdate("bishopshim_")
                });
            };
        }

        private UpdateAvailability FindUpdate(string updatePrefix)
        {
            var update = new UpdateAvailability();

            var updatesBaseUrl = string.Format("{0}{1}/content/updates/", Request.Url.SiteBase, Request.Url.BasePath);
            var updatesFolder = Path.Combine(pathProvider.GetRootPath(), "Content", "updates");
            if (Directory.Exists(updatesFolder)) {
                var updateFilePath = Directory.GetFiles(updatesFolder, updatePrefix + "*.zip").OrderByDescending(
                    f => f).FirstOrDefault();
                if (updateFilePath != null) {
                    update.Version = Path.GetFileNameWithoutExtension(updateFilePath).Remove(0, updatePrefix.Length);
                    update.FileHash = CalculateFileHash(updateFilePath);
                    update.FullUrlToUpdate = updatesBaseUrl + Path.GetFileName(updateFilePath);
                }
            }

            return update;
        }

        private static string CalculateFileHash(string filePath)
        {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(filePath)) {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }
    }
}