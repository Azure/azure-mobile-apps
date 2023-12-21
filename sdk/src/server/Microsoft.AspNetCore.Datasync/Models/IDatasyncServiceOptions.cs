// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.AspNetCore.Datasync.Models;

/// <summary>
/// The service options - used in dependency injection to configure the
/// various attributes with standardized options.
/// </summary>
public interface IDatasyncServiceOptions
{
    JsonSerializerOptions JsonSerializerOptions { get; }
}
