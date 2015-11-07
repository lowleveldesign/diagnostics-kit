using LowLevelDesign.Diagnostics.Commons.Models;

namespace LowLevelDesign.Diagnostics.Castle
{
    public class SelfInformation
    {
        public static readonly string ApplicationVersion = typeof(LogRecord).Assembly.GetName().Version.ToString();
    }
}