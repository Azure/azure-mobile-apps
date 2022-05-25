// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using MobileClient.Tests.Helpers;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MobileClient.Tests
{
    public class Push_Test
    {
        const string DefaultServiceUri = MobileAppUriValidator.DummyMobileApp;
        const string InstallationsPath = "push/installations";

        [Fact]
        public async Task DeleteInstallationAsync()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.InstallationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Delete, null, HttpStatusCode.NoContent);

            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            await pushHttpClient.DeleteInstallationAsync();
        }

        [Fact]
        public async Task DeleteInstallationAsync_Error()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.InstallationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Delete, null, HttpStatusCode.BadRequest);
            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);
            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(() => pushHttpClient.DeleteInstallationAsync());
        }
    }
}
