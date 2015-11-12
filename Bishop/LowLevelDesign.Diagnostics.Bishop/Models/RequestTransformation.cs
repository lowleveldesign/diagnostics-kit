using System.Text.RegularExpressions;

namespace LowLevelDesign.Diagnostics.Bishop.Models
{
    public sealed class RequestTransformation
    {
        public string RegexToMatch { get; set; }

        public string DestinationUrl { get; set; }

        public string DestinationHostHeader { get; set; }
    }
}
