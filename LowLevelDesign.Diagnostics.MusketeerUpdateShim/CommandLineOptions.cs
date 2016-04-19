using CommandLine;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.MusketeerUpdateShim
{
    public class CommandLineOptions
    {
        [Option("pid", HelpText = "Parent process ID", Required = true)]
        public int ParentPid { get; set; }

        [Option("update", HelpText = "A path to a .zip file containg the application update.", Required = true)]
        public string UpdateFilePath { get; set; }

        [Option("startsvc", HelpText = "Windows Service name which should be started when the update is done.", Required = false)]
        public string ServiceToStartAfterUpdate { get; set; }

        [Option("startapp", HelpText = "Application to start when the update is done - must be the last argument, " +
            "arguments after it are treated as new application arguments", Required = false)]
        public string AppToStartAfterUpdate { get; set; }


        [Value(0)]
        public IList<string> Arguments { get; set; }
    }
}
