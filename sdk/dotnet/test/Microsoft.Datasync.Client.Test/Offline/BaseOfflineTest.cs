// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Offline;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Microsoft.Datasync.Client.Test.Offline
{
    [ExcludeFromCodeCoverage]
    public class BaseOfflineTest : BaseTest
    {
        protected MockDelegatingHandler hijack;

        protected DatasyncClient GetHijackedClient()
        {
            hijack = new MockDelegatingHandler();
            return new(Endpoint, new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { hijack } });
        }
    }
}
