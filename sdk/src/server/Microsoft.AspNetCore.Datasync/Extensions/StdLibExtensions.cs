// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.AspNetCore.Datasync.Extensions;

internal static class StdLibExtensions
{
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
            return JsonSerializer.Serialize(@object, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (Exception)
        {
            return "unserializable object";
        }
    }

    internal static string ToEntityTagValue(this byte[] @version)
        => Convert.ToBase64String(@version);
}
