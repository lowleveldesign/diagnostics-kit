namespace LowLevelDesign.Diagnostics.Bishop.Models
{
    public sealed class TamperParameters
    {
        public string ServerTcpAddressWithPort { get; set; }

        public string HostHeader { get; set;  }

        public bool ShouldTamperRequest
        {
            get { return ServerTcpAddressWithPort != null || HostHeader != null; }
        }
    }
}
