using Fiddler;

namespace LowLevelDesign.Diagnostics.Bishop.Common
{
    public sealed class Request : IRequest
    {
        private readonly bool isLocal;
        private readonly bool isHttpsConnect;
        private readonly Session fiddlerSession;

        public Request(Session oSession)
        {
            isLocal = oSession.HostnameIs("localhost") || oSession.HostnameIs("127.0.0.1") || 
                oSession.HostnameIs("[::1]");
            isHttpsConnect = oSession.HTTPMethodIs("CONNECT");
            fiddlerSession = oSession;
        }

        public bool IsLocal { get { return isLocal; } }

        public bool IsHttps { get { return fiddlerSession.isHTTPS; } }

        public bool IsHttpsConnect { get { return isHttpsConnect; } }

        public string FullUrl { get { return fiddlerSession.fullUrl; } }

        public string PathAndQuery { get { return fiddlerSession.PathAndQuery; } }

        public string Host { get { return fiddlerSession.host; } }

        public int Port { get { return fiddlerSession.port; } }

        public Session FiddlerSession { get { return fiddlerSession; } }

        public string Protocol { get { return fiddlerSession.isHTTPS ? "https" : "http";  } }
    }
}
