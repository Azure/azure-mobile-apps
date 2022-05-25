using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Swagger;
using Swashbuckle.Application;

namespace Local
{
    public partial class Startup
    {
        public static void ConfigureSwagger(HttpConfiguration config)
        {
            // Use the custom ApiExplorer that applies constraints. This prevents
            // duplicate routes on /api and /tables from showing in the Swagger doc.
            config.Services.Replace(typeof(IApiExplorer), new MobileAppApiExplorer(config));
            config
               .EnableSwagger(c =>
               {
                   c.SingleApiVersion("v1", "myService");

                   // Tells the Swagger doc that any MobileAppController needs a
                   // ZUMO-API-VERSION header with default 2.0.0
                   c.OperationFilter<MobileAppHeaderFilter>();

                   // Looks at attributes on properties to decide whether they are readOnly.
                   // Right now, this only applies to the DatabaseGeneratedAttribute.
                   c.SchemaFilter<MobileAppSchemaFilter>();

                   // 1. Adds an OAuth implicit flow description that points to App Service Auth with the specified provdier
                   // 2. Adds a Swashbuckle filter that applies this Oauth description to any Action with [Authorize]
                   c.AppServiceAuthentication("https://{mysite}.azurewebsites.net/", "{provider}");
               })
               .EnableSwaggerUi(c =>
               {
                   c.EnableOAuth2Support("na", "na", "na");

                   // Replaces some javascript files with specific logic to:
                   // 1. Do the OAuth flow using the App Service Auth parameters
                   // 2. Parse the returned token
                   // 3. Apply the token to the X-ZUMO-AUTH header
                   c.MobileAppUi(config);
               });
        }
    }
}