﻿using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof($safeprojectname$.Startup))]

namespace $safeprojectname$
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}