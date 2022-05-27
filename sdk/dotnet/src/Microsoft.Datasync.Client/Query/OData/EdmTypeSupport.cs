// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq.Nodes;
using System;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.Query.OData
{
    /// <summary>
    /// A set of methods for converting to/from supported types.
    /// </summary>
    internal static class EdmTypeSupport
    {
        /// <summary>
        /// The format of the date/time transition (in universal time).
        /// </summary>
        private const string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffZ";

        /// <summary>
        /// A list of the known Edm types we have a mechanism to convert.
        /// </summary>
        private enum EdmType
        {
            DateTime,
            DateTimeOffset,
            Guid
        };

        /// <summary>
        /// The type lookup table to get the type over to the
        /// </summary>
        private static readonly Dictionary<long, EdmType> TypeLookupTable = new()
        {
            { (long)typeof(DateTime).TypeHandle.Value, EdmType.DateTime },
            { (long)typeof(DateTimeOffset).TypeHandle.Value, EdmType.DateTimeOffset },
            { (long)typeof(Guid).TypeHandle.Value, EdmType.Guid }
        };

        /// <summary>
        /// A lookup table to convert from the string EdmType to the enum EdmType.
        /// </summary>
        private static readonly Dictionary<string, EdmType> EdmLookupTable = new()
        {
            { "Edm.DateTime", EdmType.DateTime },
            { "Edm.DateTimeOffset", EdmType.DateTimeOffset },
            { "Edm.Guid", EdmType.Guid }
        };

        /// <summary>
        /// Converts the given value to the Edm OData form, or returns null
        /// if the value is not of a known type.
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <returns>The oData representation of the value</returns>
        public static string ToODataString(object value)
        {
            long handle = (long)value.GetType().TypeHandle.Value;
            if (!TypeLookupTable.TryGetValue(handle, out EdmType type))
            {
                return null;
            }

            string result = null;
            switch (type)
            {
                case EdmType.DateTime:
                    string dt = new DateTimeOffset(((DateTime)value).ToUniversalTime()).ToString(DateTimeFormat);
                    result = $"cast({dt},Edm.DateTimeOffset)";
                    break;
                case EdmType.DateTimeOffset:
                    string dto = ((DateTimeOffset)value).ToUniversalTime().ToString(DateTimeFormat);
                    result = $"cast({dto},Edm.DateTimeOffset)";
                    break;
                case EdmType.Guid:
                    Guid guid = (Guid)value;
                    result = $"cast({guid:D},Edm.Guid)";
                    break;
            }
            return result;
        }

        /// <summary>
        /// Converts an OData string back into the QueryNode.  This is used during OData filter parsing.
        /// </summary>
        /// <param name="literal">The value of the literal</param>
        /// <param name="typestr">The type string.</param>
        /// <returns>The <see cref="QueryNode"/> for the cast.</returns>
        public static QueryNode ToQueryNode(string literal, string typestr)
        {
            if (!EdmLookupTable.TryGetValue(typestr, out EdmType type))
            {
                throw new InvalidOperationException($"Edm Type '{typestr}' is not valid.");
            }
            var result = new ConstantNode(null);
            switch (type)
            {
                case EdmType.DateTime:
                    result.Value = DateTime.Parse(literal);
                    break;
                case EdmType.DateTimeOffset:
                    result.Value = DateTimeOffset.Parse(literal);
                    break;
                case EdmType.Guid:
                    result.Value = Guid.Parse(literal);
                    break;
            }
            return result;
        }
    }
}
