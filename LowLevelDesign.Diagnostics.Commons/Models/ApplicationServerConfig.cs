using System;

namespace LowLevelDesign.Diagnostics.Commons.Models
{
    /// <summary>
    /// Application configuration on the server
    /// </summary>
    public sealed class ApplicationServerConfig
    {
        public const String WebAppType = "WEB";
        public const String WinSvcType = "SVC";

        public String AppPath { get; set; }

        public String Server { get; set; }

        public String AppType { get; set; }

        /// <summary>
        /// Web apps only: name of the application pool which serves
        /// the application.
        /// </summary>
        public String AppPoolName { get; set; }

        /// <summary>
        /// Web apps only: server bindings of the application in IIS
        /// </summary>
        public String[] Bindings { get; set; }

        /// <summary>
        /// Win services only: service name
        /// </summary>
        public String ServiceName { get; set; }

        /// <summary>
        /// Win services only: service display name
        /// </summary>
        public String DisplayName { get; set; }
    }

}
