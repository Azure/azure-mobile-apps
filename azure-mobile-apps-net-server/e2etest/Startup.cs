using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ZumoE2EServerApp.Startup))]

namespace ZumoE2EServerApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}
