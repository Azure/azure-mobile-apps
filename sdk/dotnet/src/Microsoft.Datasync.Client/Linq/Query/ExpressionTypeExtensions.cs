// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Linq.Query.Nodes;
using System;
using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Linq.Query
{
    internal static class ExpressionTypeExtensions
    {
        /// <summary>
        /// Convert an <see cref="ExpressionType"/> into a <see cref="BinaryOperatorKind"/>
        /// </summary>
        /// <param name="type">The <see cref="ExpressionType"/></param>
        /// <returns>The equivalent <see cref="BinaryOperatorKind"/></returns>
        internal static BinaryOperatorKind ToBinaryOperatorKind(this ExpressionType type)
            => type switch
            {
                ExpressionType.Add => BinaryOperatorKind.Add,
                ExpressionType.AddChecked => BinaryOperatorKind.Add,
                ExpressionType.AndAlso => BinaryOperatorKind.And,
                ExpressionType.Divide => BinaryOperatorKind.Divide,
                ExpressionType.Equal => BinaryOperatorKind.Equal,
                ExpressionType.GreaterThan => BinaryOperatorKind.GreaterThan,
                ExpressionType.GreaterThanOrEqual => BinaryOperatorKind.GreaterThanOrEqual,
                ExpressionType.LessThan => BinaryOperatorKind.LessThan,
                ExpressionType.LessThanOrEqual => BinaryOperatorKind.LessThanOrEqual,
                ExpressionType.Modulo => BinaryOperatorKind.Modulo,
                ExpressionType.Multiply => BinaryOperatorKind.Multiply,
                ExpressionType.MultiplyChecked => BinaryOperatorKind.Multiply,
                ExpressionType.NotEqual => BinaryOperatorKind.NotEqual,
                ExpressionType.OrElse => BinaryOperatorKind.Or,
                ExpressionType.Subtract => BinaryOperatorKind.Subtract,
                ExpressionType.SubtractChecked => BinaryOperatorKind.Subtract,
                _ => throw new NotSupportedException($"The operator '{type}' is not supported in the 'Where' query expression")
            };
    }
}
