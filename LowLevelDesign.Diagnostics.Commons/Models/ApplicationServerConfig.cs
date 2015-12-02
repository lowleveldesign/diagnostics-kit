namespace LowLevelDesign.Diagnostics.Commons.Models
{
    /// <summary>
    /// Application configuration on the server
    /// </summary>
    public sealed class ApplicationServerConfig
    {
        public const string WebAppType = "WEB";
        public const string WinSvcType = "SVC";

        public string AppPath { get; set; }

        public string Server { get; set; }

        public string ServerFqdnOrIp { get; set; }

        public string AppType { get; set; }

        /// <summary>
        /// Web apps only: name of the application pool which serves
        /// the application.
        /// </summary>
        public string AppPoolName { get; set; }

        /// <summary>
        /// Web apps only: server bindings of the application in IIS
        /// </summary>
        public string[] Bindings { get; set; }

        /// <summary>
        /// Win services only: service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Win services only: service display name
        /// </summary>
        public string DisplayName { get; set; }
    }

}
