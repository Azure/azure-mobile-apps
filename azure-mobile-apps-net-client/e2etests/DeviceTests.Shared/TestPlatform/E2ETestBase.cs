// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceTests.Shared.TestPlatform
{
    public class E2ETestBase
    {
        public readonly string MobileServiceRuntimeUrl = "https://zumo-e2etest-net4.azurewebsites.net";

        /// <summary>
        /// If you have registered an entire push infrastructure, feel free to enable the push tests.
        /// Push Tests are not run by default.
        /// </summary>
        public readonly bool EnablePushTests = false;

        static MobileServiceClient staticClient;

        /// <summary>
        /// Get a client pointed at the test server without request logging.
        /// </summary>
        /// <returns>The test client.</returns>
        public MobileServiceClient GetClient()
        {
            if (staticClient == null)
            {
                staticClient = new MobileServiceClient(MobileServiceRuntimeUrl, new LoggingHttpHandler());
            }
            return staticClient;
        }
    }

    class LoggingHttpHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("    >>> {0} {1} {2}", request.Method, request.RequestUri, request.Content?.ReadAsStringAsync().Result);
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            Console.WriteLine("    <<< {0} {1} {2}", (int)response.StatusCode, response.ReasonPhrase, response.Content?.ReadAsStringAsync().Result);
            return response;
        }
    }
}
