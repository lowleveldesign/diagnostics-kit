namespace LowLevelDesign.Diagnostics.Bishop.Models
{
    public sealed class HttpsLocalRedirect
    {
        public int RemoteHttpsPort { get; set; }

        public int LocalHttpPort { get; set; }
    }

    public sealed class RequestTransformation
    {
        public string RegexToMatch { get; set; }

        public string DestinationUrl { get; set; }

        public string DestinationHostHeader { get; set; }
    }
}
