// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.Query.Linq
{
    /// <summary>
    /// The list of instance properties and the OData method name that supports them.
    /// </summary>
    internal static class InstanceProperties
    {
        internal static readonly Lazy<Dictionary<MemberInfoKey, string>> _table = new(() => new()
        {
#if HAS_DATEONLY
            { new MemberInfoKey(typeof(DateOnly), "Day", false, true), "day" },
            { new MemberInfoKey(typeof(DateOnly), "Month", false, true), "month" },
            { new MemberInfoKey(typeof(DateOnly), "Year", false, true), "year" },
#endif
#if HAS_TIMEONLY
            { new MemberInfoKey(typeof(TimeOnly), "Hour", false, true), "hour" },
            { new MemberInfoKey(typeof(TimeOnly), "Minute", false, true), "minute" },
            { new MemberInfoKey(typeof(TimeOnly), "Second", false, true), "second" },
#endif
            { new MemberInfoKey(typeof(string), "Length", false, true), "length" },
            { new MemberInfoKey(typeof(DateTime), "Day", false, true), "day" },
            { new MemberInfoKey(typeof(DateTime), "Month", false, true), "month" },
            { new MemberInfoKey(typeof(DateTime), "Year", false, true), "year" },
            { new MemberInfoKey(typeof(DateTime), "Hour", false, true), "hour" },
            { new MemberInfoKey(typeof(DateTime), "Minute", false, true), "minute" },
            { new MemberInfoKey(typeof(DateTime), "Second", false, true), "second" },
            { new MemberInfoKey(typeof(DateTimeOffset), "Day", false, true), "day" },
            { new MemberInfoKey(typeof(DateTimeOffset), "Month", false, true), "month" },
            { new MemberInfoKey(typeof(DateTimeOffset), "Year", false, true), "year" },
            { new MemberInfoKey(typeof(DateTimeOffset), "Hour", false, true), "hour" },
            { new MemberInfoKey(typeof(DateTimeOffset), "Minute", false, true), "minute" },
            { new MemberInfoKey(typeof(DateTimeOffset), "Second", false, true), "second" }
        });

        /// <summary>
        /// Gets the method name from the key, or null if it doesn't exist.
        /// </summary>
        /// <param name="key">The <see cref="MemberInfoKey"/></param>
        /// <returns>The method name</returns>
        internal static string GetMethodName(MemberInfoKey key)
            => _table.Value.TryGetValue(key, out string methodName) ? methodName : null;
    }
}
