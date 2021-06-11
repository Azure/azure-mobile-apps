using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ZUMOAPPNAMEService.Startup))]

namespace ZUMOAPPNAMEService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}