namespace LowLevelDesign.Diagnostics.Bishop.Config
{
    public sealed class HttpsLocalRedirect
    {
        public int RemoteHttpsPort { get; set; }

        public int LocalHttpPort { get; set; }
    }

    public sealed class RequestTransformation
    {
        private string[] destinationIpAddresses = new string[0];

        public string RegexToMatchAgainstHost { get; set; }

        public string RegexToMatchAgainstPathAndQuery { get; set; }

        public string DestinationPathAndQuery { get; set; }

        public string DestinationHostHeader { get; set; }

        public string[] DestinationIpAddresses {
            get { return destinationIpAddresses; }
            set { destinationIpAddresses = value; }
        }
    }
}
