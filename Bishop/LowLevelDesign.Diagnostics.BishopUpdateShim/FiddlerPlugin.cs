using Fiddler;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

[assembly: RequiredVersion("4.4.4.0")]

namespace LowLevelDesign.Diagnostics.BishopUpdateShim
{
    public class FiddlerPlugin : IAutoTamper
    {
        private IAutoTamper bishop;

        public FiddlerPlugin()
        {
            var files = new [] { "_Bishop.dll", "_Bishop.pdb" };
            try
            {
                Version installedVer = new Version(0, 0, 0, 0);
                Version currentVer;
                // get server version of Goniec
                var req = WebRequest.Create("https://FIXME/about-bishop"); // FIXME valid https url
                using (var s = new StreamReader(req.GetResponse().GetResponseStream()))
                {
                    currentVer = new Version(s.ReadToEnd());
                }
                var fiddlerUserPath = CONFIG.GetPath("AutoFiddlers_User");
                var asmpath = Path.Combine(fiddlerUserPath, files[0]);
                if (File.Exists(asmpath))
                {
                    installedVer = AssemblyName.GetAssemblyName(asmpath).Version;
                }

                if (currentVer > installedVer)
                {
                    if (MessageBox.Show(String.Format("New Bishop version available: {0}. Update?", currentVer), "Bishop update",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        // we need to update fiddler
                        // get Goniec.zip from a server
                        var tempPath = Path.Combine(Path.GetTempPath(), "Bishop.zip");
                        using (var wc = new WebClient())
                        {
                            wc.DownloadFile("https://FIXME/bishop.zip", tempPath);
                        }
                        // unzip
                        var unzipDir = Path.Combine(Path.GetTempPath(), "Bishop");
                        ZipFile.ExtractToDirectory(tempPath, unzipDir);

                        // move files to machine wide directory
                        foreach (var f in files)
                        {
                            var p = Path.Combine(fiddlerUserPath, f);
                            File.Delete(p);
                            File.Move(Path.Combine(unzipDir, f), p);
                        }

                        // remove old files and directories 
                        Directory.Delete(unzipDir, true);
                        File.Delete(tempPath);
                    }
                }
            }
            catch (Exception ex)
            {
                // this is cruel but what can we do? :)
                Debug.Fail("Error when trying to check the update version", "Exception: " + ex);
            }
        }

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
