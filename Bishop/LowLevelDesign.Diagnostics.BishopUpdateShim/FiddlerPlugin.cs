using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: RequiredVersion("4.4.4.0")]

namespace LowLevelDesign.Diagnostics.BishopUpdateShim
{
    public class FiddlerPlugin : IAutoTamper
    {
        private IAutoTamper bishop;

        public void OnLoad()
        {
            // load servant.dll
            var fiddlerUserPath = CONFIG.GetPath("AutoFiddlers_User");
            var asm = Assembly.UnsafeLoadFrom(Path.Combine(fiddlerUserPath, "Bishop.dll"));
            if (asm == null) {
                throw new InvalidOperationException("Bishop not found.");
            }
            var t = asm.GetType("LowLevelDesign.Diagnostics.Bishop.FiddlerPlugin");
            if (t == null) {
                throw new InvalidOperationException("Bishop.dll is not valid - does not contain the plugin class.");
            }
            var s = (IAutoTamper)Activator.CreateInstance(t);
            s.OnLoad();

            bishop = s;
        }
        public void OnBeforeUnload()
        {
            if (bishop != null)
            {
                bishop.OnBeforeUnload();
            }
        }

        public void AutoTamperRequestAfter(Session oSession)
        {
            if (bishop != null)
            {
                bishop.AutoTamperRequestAfter(oSession);
            }
        }

        public void AutoTamperRequestBefore(Session oSession)
        {
            if (bishop != null)
            {
                bishop.AutoTamperRequestBefore(oSession);
            }
        }

        public void AutoTamperResponseAfter(Session oSession)
        {
            if (bishop != null)
            {
                bishop.AutoTamperResponseAfter(oSession);
            }
        }

        public void AutoTamperResponseBefore(Session oSession)
        {
            if (bishop != null)
            {
                bishop.AutoTamperResponseAfter(oSession);
            }
        }

        public void OnBeforeReturningError(Session oSession)
        {
            if (bishop != null)
            {
                bishop.OnBeforeReturningError(oSession);
            }
        }
    }
}
