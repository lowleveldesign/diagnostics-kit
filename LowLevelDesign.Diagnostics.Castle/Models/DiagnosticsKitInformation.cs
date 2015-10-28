namespace LowLevelDesign.Diagnostics.Castle.Models
{
    public class DiagnosticsKitInformation
    {
        public string Version { get; set; }

        public string UsedLogStore { get; set; }

        public string UsedAppConfigurationManager { get; set; }

        public string UsedAppUserManager { get; set; }

        public bool IsAuthenticationEnabled { get; set; }
    }
}