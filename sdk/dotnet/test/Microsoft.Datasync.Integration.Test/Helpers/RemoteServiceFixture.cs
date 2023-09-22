// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.TestHost;

namespace Microsoft.Datasync.Integration.Test.Helpers;

[ExcludeFromCodeCoverage]
public class RemoteServiceFixture : BaseTest, IDisposable
{
    public RemoteServiceFixture()
    {
        ServiceClient = GetMovieClient();
        MovieTable = ServiceClient.GetRemoteTable<ClientMovie>("movies");
        DateTimeTable = ServiceClient.GetRemoteTable<DateTimeClientModel>("datetime");
    }

    public void Dispose()
    {
    }

    public DatasyncClient ServiceClient { get; }

    public IRemoteTable<ClientMovie> MovieTable { get; }

    public IRemoteTable<DateTimeClientModel> DateTimeTable { get; }

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

[ExcludeFromCodeCoverage]
public class DateTimeClientModel : DatasyncClientData
{
    public DateOnly DateOnly { get; set; }
    public TimeOnly TimeOnly { get; set; }
}
