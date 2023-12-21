// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.AspNetCore.Datasync.Extensions;

internal static class StdLibExtensions
{
    private static readonly Lazy<JsonSerializerOptions> _options = new(() => new JsonSerializerOptions(JsonSerializerDefaults.General));

    /// <summary>
    /// Serializes an object to a JSON string.  This is used in logging, so we capture any
    /// exceptions and return a default string.
    /// </summary>
    /// <param name="object">The object to be serialized.</param>
    /// <returns>The serialized object.</returns>
    internal static string ToJsonString(this object @object)
    {
        try
        {
            if (@object == null)
            {
                return "null";
            }
            return JsonSerializer.Serialize(@object, _options.Value);
        }
        catch (Exception)
        {
            return "unserializable object";
        }
    }

    /// <summary>
    /// Converts a byte array to an entity tag value.
    /// </summary>
    /// <param name="version">The version to convert.</param>
    /// <returns>The version string.</returns>
    internal static string ToEntityTagValue(this byte[] @version)
        => Convert.ToBase64String(@version);
}
