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
