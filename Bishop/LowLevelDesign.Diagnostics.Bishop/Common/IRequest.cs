namespace LowLevelDesign.Diagnostics.Bishop.Common
{
    public interface IRequest
    {
        bool IsLocal { get; } 

        bool IsHttps { get; }

        bool IsHttpsConnect { get; }

        string FullUrl { get; }

        string PathAndQuery { get; }

        string Host { get; }

        int Port { get; }

        string Protocol { get; }
    }
}
