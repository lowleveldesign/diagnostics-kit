namespace LowLevelDesign.Diagnostics.Castle.Models
{
    public sealed class DiagnosticsKitInformation
    {
        public string Version { get; set; }

        public string UsedLogStore { get; set; }

        public string UsedAppConfigurationManager { get; set; }

        public string UsedAppUserManager { get; set; }

        public bool IsAuthenticationEnabled { get; set; }
    }

    public sealed class UpdateAvailability
    {
        public string Version { get; set; }

        public string FileHash { get; set; }

        public string FullUrlToUpdate { get; set; }
    }

    public sealed class ApplicationUpdate
    {
        public UpdateAvailability UpdateForApplication { get; set; }

        public UpdateAvailability UpdateForApplicationShim { get; set; }
    }
}