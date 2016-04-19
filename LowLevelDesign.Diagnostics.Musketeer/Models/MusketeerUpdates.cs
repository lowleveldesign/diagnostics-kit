namespace LowLevelDesign.Diagnostics.Musketeer.Models
{
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
