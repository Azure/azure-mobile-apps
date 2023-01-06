// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Datasync.Client;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Helpers
{
    [ExcludeFromCodeCoverage]
    public class RemoteServiceFixture : BaseTest, IDisposable
    {
        public RemoteServiceFixture()
        {
            ServiceClient = GetMovieClient();
            MovieTable = ServiceClient.GetRemoteTable<ClientMovie>("movies");
        }

        public void Dispose()
        {
        }

        public DatasyncClient ServiceClient { get; }

        public IRemoteTable<ClientMovie> MovieTable { get; }

        public new TestServer MovieServer { get => base.MovieServer; }

        public new int MovieCount { get => BaseTest.MovieCount; }
    }

    [ExcludeFromCodeCoverage]
    [CollectionDefinition("RemoteServiceCollection")]
    public class RemoteServiceCollection : ICollectionFixture<RemoteServiceFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
