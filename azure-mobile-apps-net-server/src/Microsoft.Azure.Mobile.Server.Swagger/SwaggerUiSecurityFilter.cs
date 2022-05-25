// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using Swashbuckle.Application;

namespace Microsoft.Azure.Mobile.Server
{
    internal class SwaggerUiSecurityFilter : DelegatingHandler
    {
        private HttpConfiguration configuration;

        public SwaggerUiSecurityFilter(HttpConfiguration config)
        {
            this.configuration = config;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IHttpRouteData routeData = this.configuration.Routes.GetRouteData(request);
            bool isSwaggerUi = routeData?.Route?.Handler?.GetType() == typeof(SwaggerUiHandler);
            string userAgent = request.Headers.UserAgent.ToString();

            if (isSwaggerUi && (userAgent.Contains("MSIE") || userAgent.Contains("Trident")))
            {
                HttpResponseMessage notSupportedResponse = new HttpResponseMessage();
                notSupportedResponse.Content = new StringContent("Internet Explorer is not supported for viewing swagger-ui.");
                return notSupportedResponse;
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (isSwaggerUi)
            {
                response.Headers.Add("Content-Security-Policy", "connect-src 'self' online.swagger.io");
            }

            return response;
        }
    }
}
