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
        /// settings on which web servers are listening 
        /// on a mathcing urls.
        /// </summary>
        public string[] CustomServerIpAddresses { get; set; }

        public bool ShouldTamperRequest
        {
            get { return ServerTcpAddressWithPort != null || HostHeader != null ||
                    PathAndQuery != null; }
        }
    }
}
