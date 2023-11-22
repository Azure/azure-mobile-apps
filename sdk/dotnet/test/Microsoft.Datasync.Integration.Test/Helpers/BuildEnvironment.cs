// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test;

internal static class BuildEnvironment
{
    // Returns true if we're running on an agent (i.e. in an ADO pipeline).
    internal static bool IsPipeline()
        => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AGENT_ID"));
}
