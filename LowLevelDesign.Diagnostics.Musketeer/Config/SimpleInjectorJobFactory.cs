using Quartz;
using Quartz.Simpl;
using Quartz.Spi;
using SimpleInjector;

namespace LowLevelDesign.Diagnostics.Musketeer.Config
{
    public class SimpleInjectorJobFactory : SimpleJobFactory
    {
        private readonly Container container;

        public SimpleInjectorJobFactory(Container container)
        {
            this.container = container;
        }

        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return (IJob)container.GetInstance(bundle.JobDetail.JobType);
        }
    }
}
