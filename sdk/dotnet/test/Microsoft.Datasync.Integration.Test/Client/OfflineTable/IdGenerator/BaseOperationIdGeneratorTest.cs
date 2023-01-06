// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client;
using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTable
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseOperationIdGeneratorTest : BaseOperationTest
    {
        protected new readonly DatasyncClient client;

        protected BaseOperationIdGeneratorTest(ITestOutputHelper logger, bool useFile = true) : base(logger, useFile)
        {
            client = GetMovieClientWithIdGenerator(store: store);
        }
    }
}
