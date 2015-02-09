using Owin;

namespace LowLevelDesign.Diagnostics.Castle
{
    public class Startup
    {
        public void Configuration(IAppBuilder app) {
            app.UseNancy();
        }
    }
}