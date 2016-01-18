namespace LowLevelDesign.Diagnostics.Bishop.Config
{
    public sealed class HttpsLocalRedirect
    {
        public short RemoteHttpsPort { get; set; }

        public short LocalHttpPort { get; set; }
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
