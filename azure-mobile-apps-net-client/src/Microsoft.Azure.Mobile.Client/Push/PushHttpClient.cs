//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal sealed class PushHttpClient
    {
        private readonly MobileServiceClient client;

        internal PushHttpClient(MobileServiceClient client)
        {
            this.client = client;
        }

        public Task DeleteInstallationAsync()
        {
            return client.HttpClient.RequestAsync(HttpMethod.Delete, GetUriPath(), client.CurrentUser, ensureResponseContent: false);
        }

        public Task CreateOrUpdateInstallationAsync(JObject installation, CancellationToken cancellationToken = default)
        {
            return client.HttpClient.RequestAsync(HttpMethod.Put, GetUriPath(), client.CurrentUser, content: installation.ToString(), ensureResponseContent: false, cancellationToken: cancellationToken);
        }

        private string GetUriPath() => string.Format("push/installations/{0}", Uri.EscapeUriString(client.InstallationId));
    }
}