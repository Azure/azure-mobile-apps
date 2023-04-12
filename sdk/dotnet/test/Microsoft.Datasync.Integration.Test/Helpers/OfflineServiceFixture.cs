// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.TestHost;
using Microsoft.Datasync.Integration.Test.Client.OfflineTableOfT;

namespace Microsoft.Datasync.Integration.Test.Helpers;

[ExcludeFromCodeCoverage]
public class OfflineServiceFixture : BaseOperationTest, IDisposable
{
    private bool isInitialized = false;

    public OfflineServiceFixture() : base()
    {
    }

    public new async Task InitializeAsync(bool pullItems = true)
    {
        if (!isInitialized)
        {
            await base.InitializeAsync(pullItems);
            isInitialized = true;
        }
    }

    public new TestServer MovieServer { get => base.MovieServer; }

    public new int MovieCount { get => BaseTest.MovieCount; }
}

[ExcludeFromCodeCoverage]
[CollectionDefinition("OfflineServiceCollection")]
public class OfflineServiceCollection : ICollectionFixture<OfflineServiceFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
