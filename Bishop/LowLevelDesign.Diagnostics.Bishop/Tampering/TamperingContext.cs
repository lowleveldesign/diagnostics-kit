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

using System.Linq;

namespace LowLevelDesign.Diagnostics.Bishop.Tampering
{
    public sealed class TamperingContext
    {
        public string Protocol { get; set; }

        public string ServerTcpAddressWithPort { get; set; }

        public string HostHeader { get; set;  }

        public string PathAndQuery { get; set; }

        /// <summary>
        /// It's a list of IP addresses set in the 
        /// settings on which web servers are listening.
        /// </summary>
        public string[] CustomServerIpAddresses { get; set; }

        /// <summary>
        /// It's a list of ports set in the settings
        /// on which web servers are listening.
        /// </summary>
        public ushort[] CustomServerPorts { get; set; }

        public bool IsIpAddressValidForRedirection(string ipaddr)
        {
            return CustomServerIpAddresses != null && CustomServerIpAddresses.Contains(ipaddr);
        }

        public bool IsPortValidForRedirection(ushort port)
        {
            return CustomServerPorts != null && CustomServerPorts.Contains(port);
        }

        public bool ShouldTamperRequest
        {
            get { return ServerTcpAddressWithPort != null || HostHeader != null ||
                    PathAndQuery != null; }
        }
    }
}
