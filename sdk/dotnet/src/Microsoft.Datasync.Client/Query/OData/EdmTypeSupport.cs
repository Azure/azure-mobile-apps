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
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToODataString(object value)
        {
            long handle = (long)value.GetType().TypeHandle.Value;
            if (!TypeLookupTable.TryGetValue(handle, out EdmType type))
            {
                return null;
            }

            switch (type)
            {
                case EdmType.DateTime:
                    string dt = new DateTimeOffset(((DateTime)value).ToUniversalTime()).ToString(DateTimeFormat);
                    return $"cast({dt},Edm.DateTimeOffset)";
                case EdmType.DateTimeOffset:
                    string dto = ((DateTimeOffset)value).ToUniversalTime().ToString(DateTimeFormat);
                    return $"cast({dto},Edm.DateTimeOffset)";
                case EdmType.Guid:
                    Guid guid = (Guid)value;
                    return $"cast({guid:D},Edm.Guid)";
                default:
                    throw new NotImplementedException("EdmType not found.  This should never happen.");
            }
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
            return type switch
            {
                EdmType.DateTime => new ConstantNode(DateTime.Parse(literal)),
                EdmType.DateTimeOffset => new ConstantNode(DateTimeOffset.Parse(literal)),
                EdmType.Guid => new ConstantNode(Guid.Parse(literal)),
                _ => throw new NotImplementedException("EdmType not found.  This should never happen."),
            };
        }
    }
}
