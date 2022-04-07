// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.SQLiteStore.Utils
{
    /// <summary>
    /// A set of extension methods to make the code more readable.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Converts a list of values into a parameter dictionary.
        /// </summary>
        /// <param name="values">The list of values.</param>
        /// <param name="prefix">The prefix for each parameter (do not include the <c>@</c> symbol)</param>
        /// <returns>The parameter dictionary</returns>
        internal static Dictionary<string, object> ToParameterList(this IEnumerable<object> values, string prefix = "p")
        {
            Dictionary<string, object> result = new();
            int paramNo = 0;
            foreach (object value in values)
            {
                result.Add($"@{prefix}{paramNo++}", value);
            }
            return result;
        }

        /// <summary>
        /// Splits the given sequence into sequences of the given length.
        /// </summary>
        internal static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int maxLength)
        {
            Arguments.IsNotNull(source, nameof(source));
            Arguments.IsPositiveInteger(maxLength, nameof(maxLength));

            var enumerator = source.GetEnumerator();
            var batch = new List<T>(maxLength);

            while (enumerator.MoveNext())
            {
                batch.Add(enumerator.Current);
                if (batch.Count == maxLength)
                {
                    yield return batch;
                    batch = new List<T>(maxLength);
                }
            }

            if (batch.Count > 0)
            {
                yield return batch;
            }
        }

        /// <summary>
        /// Converts the given <see cref="BinaryOperatorKind"/> into the SQL equivalent.
        /// </summary>
        internal static string ToSqlOperator(this BinaryOperatorKind kind) => kind switch
        {
            BinaryOperatorKind.Or => "OR",
            BinaryOperatorKind.And => "AND",
            BinaryOperatorKind.Equal => "=",
            BinaryOperatorKind.NotEqual => "!=",
            BinaryOperatorKind.GreaterThan => ">",
            BinaryOperatorKind.GreaterThanOrEqual => ">=",
            BinaryOperatorKind.LessThan => "<",
            BinaryOperatorKind.LessThanOrEqual => "<=",
            BinaryOperatorKind.Add => "+",
            BinaryOperatorKind.Subtract => "-",
            BinaryOperatorKind.Multiply => "*",
            BinaryOperatorKind.Divide => "/",
            BinaryOperatorKind.Modulo => "%",
            _ => throw new InvalidOperationException($"Invalid Binary Operator '{kind}'")
        };
    }
}
