// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Datasync.Extensions
{
    internal static class IHeaderDictionaryExtensions
    {
        /// <summary>
        /// Adds the appropriate headers to the provided header dictionary, based on the
        /// system properties within the entity.
        /// </summary>
        /// <param name="headers">The <see cref="IHeaderDictionary"/> to adjust.</param>
        /// <param name="entity">The entity to use as source for the headers.</param>
        internal static void AddFromEntity(this IHeaderDictionary headers, ITableData entity)
        {
            if (entity != null)
            {
                if (entity.HasValidVersion())
                {
                    headers.Remove(HeaderNames.ETag);
                    headers.Add(HeaderNames.ETag, entity.GetETag());
                }
                if (entity.UpdatedAt != default)
                {
                    headers.Remove(HeaderNames.LastModified);
                    headers.Add(HeaderNames.LastModified, entity.UpdatedAt.ToString(DateTimeFormatInfo.InvariantInfo.RFC1123Pattern, CultureInfo.InvariantCulture));
                }
            }
        }
    }
}
