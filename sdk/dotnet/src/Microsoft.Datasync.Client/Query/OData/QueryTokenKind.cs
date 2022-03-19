// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq.Nodes;
using System;

namespace Microsoft.Datasync.Client.Query.OData
{
    /// <summary>
    /// A list of kinds for the <see cref="QueryToken"/> objects
    /// used in the OData expression parser.
    /// </summary>
    public enum QueryTokenKind
    {
        Unknown,
        End,
        Identifier,
        StringLiteral,
        IntegerLiteral,
        RealLiteral,
        Not,
        Modulo,
        OpenParen,
        CloseParen,
        Multiply,
        Add,
        Subtract,
        Comma,
        Minus,
        Dot,
        Divide,
        LessThan,
        Equal,
        GreaterThan,
        NotEqual,
        And,
        LessThanEqual,
        GreaterThanEqual,
        Or
    }

    internal static class QueryTokenKindExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if the token kind is a comparison operator.
        /// </summary>
        /// <param name="kind">The <see cref="QueryTokenKind"/> to check.</param>
        /// <returns><c>true</c> if the token kind is a comparison operator.</returns>
        internal static bool IsComparisonOperator(this QueryTokenKind kind)
            => kind == QueryTokenKind.Equal || kind == QueryTokenKind.NotEqual
            || kind == QueryTokenKind.LessThan || kind == QueryTokenKind.LessThanEqual
            || kind == QueryTokenKind.GreaterThan || kind == QueryTokenKind.GreaterThanEqual;

        /// <summary>
        /// Returns <c>true</c> if the token kind is a number literal.
        /// </summary>
        /// <param name="kind">The <see cref="QueryTokenKind"/> to check.</param>
        /// <returns><c>true</c> if the token kind is a comparison operator.</returns>
        internal static bool IsNumberLiteral(this QueryTokenKind kind)
            => kind == QueryTokenKind.IntegerLiteral | kind == QueryTokenKind.RealLiteral;

        /// <summary>
        /// Converts a <see cref="QueryTokenKind"/> to a <see cref="BinaryOperatorKind"/> where possible.
        /// </summary>
        /// <param name="kind">The <see cref="QueryTokenKind"/> to convert</param>
        /// <returns>The <see cref="BinaryOperatorKind"/> that is equivalent</returns>
        /// <exception cref="InvalidOperationException">if the token kind cannot be converted.</exception>
        internal static BinaryOperatorKind ToBinaryOperatorKind(this QueryTokenKind kind) => kind switch
        {
            QueryTokenKind.Add => BinaryOperatorKind.Add,
            QueryTokenKind.And => BinaryOperatorKind.And,
            QueryTokenKind.Or => BinaryOperatorKind.Or,
            QueryTokenKind.Equal => BinaryOperatorKind.Equal,
            QueryTokenKind.NotEqual => BinaryOperatorKind.NotEqual,
            QueryTokenKind.LessThan => BinaryOperatorKind.LessThan,
            QueryTokenKind.LessThanEqual => BinaryOperatorKind.LessThanOrEqual,
            QueryTokenKind.GreaterThan => BinaryOperatorKind.GreaterThan,
            QueryTokenKind.GreaterThanEqual => BinaryOperatorKind.GreaterThanOrEqual,
            QueryTokenKind.Subtract => BinaryOperatorKind.Subtract,
            QueryTokenKind.Multiply => BinaryOperatorKind.Multiply,
            QueryTokenKind.Divide => BinaryOperatorKind.Divide,
            QueryTokenKind.Modulo => BinaryOperatorKind.Modulo,
            _ => throw new InvalidOperationException("Unknown query token to binary operator conversion")
        };
    }
}
