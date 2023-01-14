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

#if HAS_DATEONLY
        /// <summary>
        /// The format for the DateOnly type
        /// </summary>
        private const string DateOnlyFormat = "yyyy-MM-dd";
#endif

#if HAS_TIMEONLY
        /// <summary>
        /// The format for the TimeOnly type
        /// </summary>
        private const string TimeOnlyFormat = "hh:mm:ss";
#endif

        /// <summary>
        /// A list of the known Edm types we have a mechanism to convert.
        /// </summary>
        private enum EdmType
        {
#if HAS_DATEONLY
            Date,
#endif
#if HAS_TIMEONLY
            TimeOfDay,
#endif
            DateTime,
            DateTimeOffset,
            Guid
        };

        /// <summary>
        /// The type lookup table to get the type over to the
        /// </summary>
        private static readonly Dictionary<long, EdmType> TypeLookupTable = new()
        {
#if HAS_DATEONLY
            { (long)typeof(DateOnly).TypeHandle.Value, EdmType.Date },
#endif
#if HAS_TIMEONLY
            { (long)typeof(TimeOnly).TypeHandle.Value, EdmType.TimeOfDay },
#endif
            { (long)typeof(DateTime).TypeHandle.Value, EdmType.DateTime },
            { (long)typeof(DateTimeOffset).TypeHandle.Value, EdmType.DateTimeOffset },
            { (long)typeof(Guid).TypeHandle.Value, EdmType.Guid }
        };

        /// <summary>
        /// A lookup table to convert from the string EdmType to the enum EdmType.
        /// </summary>
        private static readonly Dictionary<string, EdmType> EdmLookupTable = new()
        {
#if HAS_DATEONLY
            { "Edm.Date", EdmType.Date },
#endif
#if HAS_TIMEONLY
            { "Edm.TimeOfDay", EdmType.TimeOfDay },
#endif
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

            string formattedString = string.Empty;
            string result = null;
            switch (type)
            {
#if HAS_DATEONLY
                case EdmType.Date:
                    formattedString = ((DateOnly)value).ToString(DateOnlyFormat);
                    result = $"cast({formattedString},Edm.Date)";
                    break;
#endif
#if HAS_TIMEONLY
                case EdmType.TimeOfDay:
                    formattedString = ((TimeOnly)value).ToString(TimeOnlyFormat);
                    result = $"cast({formattedString},Edm.TimeOfDay)";
                    break;
#endif
                case EdmType.DateTime:
                    formattedString = new DateTimeOffset(((DateTime)value).ToUniversalTime()).ToString(DateTimeFormat);
                    result = $"cast({formattedString},Edm.DateTimeOffset)";
                    break;
                case EdmType.DateTimeOffset:
                    formattedString = ((DateTimeOffset)value).ToUniversalTime().ToString(DateTimeFormat);
                    result = $"cast({formattedString},Edm.DateTimeOffset)";
                    break;
                case EdmType.Guid:
                    formattedString = string.Format("{0:D}", (Guid)value);
                    result = $"cast({formattedString},Edm.Guid)";
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
#if HAS_DATEONLY
                case EdmType.Date:
                    result.Value = DateOnly.ParseExact(literal, DateOnlyFormat);
                    break;
#endif
#if HAS_TIMEONLY
                case EdmType.TimeOfDay:
                    result.Value = TimeOnly.ParseExact(literal, TimeOnlyFormat);
                    break;
#endif
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
