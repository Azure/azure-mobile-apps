using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;
using Owin;
using TryMobileAppsService.DataObjects;
using TryMobileAppsService.Models;

namespace TryMobileAppsService
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration(); 
 
            new MobileAppConfiguration() 
                .UseDefaultConfiguration() 
                .ApplyTo(config); 

            Database.SetInitializer(new TryMobileAppsInitializer());

            app.UseWebApi(config);
        }
    }

    public class TryMobileAppsInitializer : DropCreateDatabaseAlways<TryMobileAppsContext>
    {
        protected override void Seed(TryMobileAppsContext context)
        {
            List<TodoItem> todoItems = new List<TodoItem>
            {
                new TodoItem { Id = Guid.NewGuid().ToString(), Text = "First item", Complete = false },
                new TodoItem { Id = Guid.NewGuid().ToString(), Text = "Second item", Complete = false },
            };

            foreach (TodoItem todoItem in todoItems)
            {
                context.Set<TodoItem>().Add(todoItem);
            }

            base.Seed(context);
        }
    }
}

