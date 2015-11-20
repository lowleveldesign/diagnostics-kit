namespace LowLevelDesign.Diagnostics.Bishop.Tampering
{
    public sealed class TamperParameters
    {
        public string ServerTcpAddressWithPort { get; set; }

        public string HostHeader { get; set;  }

        public string PathAndQuery { get; set; }

        public bool ShouldTamperRequest
        {
            get { return ServerTcpAddressWithPort != null || HostHeader != null; }
        }
    }
}
