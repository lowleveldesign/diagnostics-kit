using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LowLevelDesign.Diagnostics.Commons.Models
{
    public sealed class Application
    {
        public String Name { get; set; }

        public String Path { get; set; }

        private bool isExcluded;
        public bool IsExcluded {
            get { return IsHidden || isExcluded; }
            set { isExcluded = value; }
        }

        public bool IsHidden { get; set; }

        public byte? DaysToKeepLogs { get; set; }

        private String enckey;
        public String GetBase64EncodedKey()
        {
            if (enckey == null) {
                if (Path == null) {
                    return null;
                }
                enckey = GetBase64EncodedKey(Path);
            }
            return enckey;
        }

        public static String GetBase64EncodedKey(String path)
        {
            var bytes = Encoding.UTF8.GetBytes(path);
            return Convert.ToBase64String(bytes).Replace("=", String.Empty)
                .Replace('+', '-').Replace('/', '_');
        }

        public static String GetPathFromBase64Key(String base64)
        {
            if (base64 == null) {
                return null;
            }
            base64 = base64.Replace('_', '/').Replace('-', '+')
                .PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
    }
}