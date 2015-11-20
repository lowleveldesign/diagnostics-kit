namespace LowLevelDesign.Diagnostics.Bishop.Config
{
    public sealed class HttpsLocalRedirect
    {
        public int RemoteHttpsPort { get; set; }

        public int LocalHttpPort { get; set; }
    }

    public sealed class RequestTransformation
    {
        public string RegexToMatchAgainstPathAndQuery { get; set; }

        public string DestinationPathAndQuery { get; set; }

        public string DestinationHostHeader { get; set; }
    }
}
