// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Extensions;

internal static class EnsureThatExtensions
{
    /// <summary>
    /// Validates that the parameter being checked is a valid endpoint for a Datasync service.
    /// </summary>
    /// <param name="param">The parameter to be checked.</param>
    /// <returns>The parameter (for chaining).</returns>
    /// <exception cref="UriFormatException">If the parameter being checked is not valid.</exception>
    internal static Param<Uri> IsValidEndpoint(this Param<Uri> param)
    {
        Uri endpoint = param.Value;
        if (!endpoint.IsAbsoluteUri)
        {
            throw new UriFormatException("The endpoint URI must be an absolute URI.");
        }
        if (endpoint.Scheme.Equals("null", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new UriFormatException("The endpoint URI must be set.");
        }
        if (endpoint.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase))
        {
            if (!endpoint.IsLoopback && !endpoint.Host.EndsWith(".local"))
            {
                throw new UriFormatException("The endpoint URI must use secure (https) when not using localhost.");
            }
        }
        else if (!endpoint.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new UriFormatException("The endpoint URI must use HTTP protocol.");
        }
        return param;
    }
}
