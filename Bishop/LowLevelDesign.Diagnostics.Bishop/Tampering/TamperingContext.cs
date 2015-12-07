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
