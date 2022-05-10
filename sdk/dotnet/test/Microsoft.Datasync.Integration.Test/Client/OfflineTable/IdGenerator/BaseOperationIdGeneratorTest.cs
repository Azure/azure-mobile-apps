// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using FluentAssertions.Common;
using FluentAssertions.Specialized;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.SQLiteStore;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTable
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseOperationIdGeneratorTest : BaseOperationTest
    {
        protected new readonly DatasyncClient client;

        protected BaseOperationIdGeneratorTest(ITestOutputHelper logger) : base(logger)
        {
            client = GetMovieClientWithIdGenerator(store: store);
        }
    }
}
