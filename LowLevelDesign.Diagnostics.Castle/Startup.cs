using Owin;
using Microsoft.Owin.Extensions;

namespace LowLevelDesign.Diagnostics.Castle
{
    public class Startup
    {
        public void Configuration(IAppBuilder app) {
            app.UseNancy();
            app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}