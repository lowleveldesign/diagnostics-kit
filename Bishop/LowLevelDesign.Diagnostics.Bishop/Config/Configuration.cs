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

namespace LowLevelDesign.Diagnostics.Bishop.Config
{
    public sealed class HttpsLocalRedirect
    {
        public ushort RemoteHttpsPort { get; set; }

        public ushort LocalHttpPort { get; set; }
    }

    public sealed class RequestTransformation
    {
        private string[] destinationIpAddresses = new string[0];
        private ushort[] destinationPorts = new ushort[0];

        public string Name { get; set; }

        public string Protocol { get; set; }

        public string RegexToMatchAgainstHost { get; set; }

        public string RegexToMatchAgainstPathAndQuery { get; set; }

        public string DestinationPathAndQuery { get; set; }

        public string DestinationHostHeader { get; set; }

        public string[] DestinationIpAddresses
        {
            get { return destinationIpAddresses; }
            set { destinationIpAddresses = value; }
        }

        public ushort[] DestinationPorts
        {
            get { return destinationPorts; }
            set { destinationPorts = value; }
        }
    }
}
