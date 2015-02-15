using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Commons.Models
{
    [Obsolete]
    public sealed class WebAppLogRecord : LogRecord
    {
        public String Host;

        public String LoggedUser;

        public String HttpStatusCode;

        public String Url;

        public String Referer;

        public String Cookies;

        public String ClientIP;

        public String ServerIP;

        public short ServerPort;

    }
}
