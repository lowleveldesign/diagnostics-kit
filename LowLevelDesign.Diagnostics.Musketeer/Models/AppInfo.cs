namespace LowLevelDesign.Diagnostics.Musketeer.Models
{
    public enum ELogType
    {
        W3SVC,
        TextFile
    }

    public class AppInfo
    {
        public string Path { get; set; }

        public int[] ProcessIds { get; set; }

        public bool LogEnabled { get; set; }

        public ELogType LogType { get; set; }

        public string LogsPath { get; set; }

        public string LogFilter { get; set; }
    }
}
