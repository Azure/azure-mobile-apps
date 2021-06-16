// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    /// <summary>
    /// Common test artifacts
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public abstract class BaseTest
    {
        protected JsonSerializerOptions SerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}
