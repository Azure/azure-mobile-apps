// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Query.Linq
{
    /// <summary>
    /// A set of useful extension methods for the <see cref="Expression"/> class and other LINQ classes.
    /// </summary>
    internal static class ExpressionExtensions
    {
        /// <summary>
        /// Convert an <see cref="ExpressionType"/> into a <see cref="BinaryOperatorKind"/>
        /// </summary>
        /// <param name="type">The <see cref="ExpressionType"/></param>
        /// <returns>The equivalent <see cref="BinaryOperatorKind"/></returns>
        internal static BinaryOperatorKind ToBinaryOperatorKind(this ExpressionType type) => type switch
        {
            ExpressionType.Add => BinaryOperatorKind.Add,
            ExpressionType.AndAlso => BinaryOperatorKind.And,
            ExpressionType.Divide => BinaryOperatorKind.Divide,
            ExpressionType.Equal => BinaryOperatorKind.Equal,
            ExpressionType.GreaterThan => BinaryOperatorKind.GreaterThan,
            ExpressionType.GreaterThanOrEqual => BinaryOperatorKind.GreaterThanOrEqual,
            ExpressionType.LessThan => BinaryOperatorKind.LessThan,
            ExpressionType.LessThanOrEqual => BinaryOperatorKind.LessThanOrEqual,
            ExpressionType.Modulo => BinaryOperatorKind.Modulo,
            ExpressionType.Multiply => BinaryOperatorKind.Multiply,
            ExpressionType.NotEqual => BinaryOperatorKind.NotEqual,
            ExpressionType.OrElse => BinaryOperatorKind.Or,
            ExpressionType.Subtract => BinaryOperatorKind.Subtract,
            _ => throw new NotSupportedException($"The operator '{type}' is not supported in the 'Where' query expression")
        };

        /// <summary>
        /// Remove the quote from quoted expressions.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>An unquoted expression</returns>
        internal static Expression StripQuote(this Expression expression)
            => expression.NodeType == ExpressionType.Quote ? ((UnaryExpression)expression).Operand : expression;

        /// <summary>
        /// Determines if the provided expression is a valid LambdaExpression.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <param name="lambdaExpression">The lambda expression equivalent</param>
        /// <returns>True if a lambda expression</returns>
        internal static bool IsValidLambdaExpression(this MethodCallExpression expression, out LambdaExpression lambdaExpression)
        {
            lambdaExpression = null;
            if (expression?.Arguments.Count >= 2 && expression.Arguments[1].StripQuote() is LambdaExpression lambda)
            {
                lambdaExpression = lambda;
            }
            return lambdaExpression != null;
        }

        /// <summary>
        /// Returns the member expressions in the expression hierarchy of the <paramref name="expression"/>
        /// </summary>
        /// <param name="expression">The expression to search</param>
        /// <returns>A collection of <see cref="MemberExpression"/> objects</returns>
        internal static IEnumerable<MemberExpression> GetMemberExpressions(this Expression expression)
        {
            List<MemberExpression> members = new();
            VisitorHelper.VisitMembers(expression, (expr, recurse) =>
            {
                members.Add(expr);
                return recurse(expr);
            });
            return members;
        }

        /// <summary>
        /// Walk the expression and compute all the subtrees that are not dependent on any
        /// of the expressions parameters.
        /// </summary>
        /// <param name="expression">The expression to analyze.</param>
        /// <returns>A collection of all the expression subtrees that are independent from the expression parameters.</returns>
        internal static List<Expression> FindIndependentSubtrees(this Expression expression)
        {
            List<Expression> subtrees = new();

            // The dependenty and isMemberInit flags are used to communicate between different layers
            // of the recursive visitor.
            bool dependent = false;
            bool isMemberInit = false;

            // Walk the tree, finding the independent subtrees
            VisitorHelper.VisitAll(expression, (expr, recurse) =>
            {
                if (expr != null)
                {
                    bool parentIsDependent = dependent;
                    bool parentIsMemberInit = isMemberInit;

                    // Set flags
                    dependent = false;
                    isMemberInit = expr is MemberInitExpression;

                    // Recurse
                    recurse(expr);

                    // If nothing in my subtree is dependent
                    if (!dependent)
                    {
                        // A NewExpression itself will appear to be indepentt, but if the parent is a MemberInitExpression,
                        // then the NewExpression can't be evaluated by itself.  The MemberInitExpression will determine
                        // if the full expression is dependent or not, so don't check it here.
                        if (expr is NewExpression newExpression && parentIsMemberInit)
                        {
                            return expr;
                        }

                        // The current node is independent if it's not related to the parameter and it's not the constant query root.
                        ConstantExpression constant = expr as ConstantExpression;
                        if (expr.NodeType == ExpressionType.Parameter || (constant?.Value is IQueryable))
                        {
                            dependent = true;
                        }
                        else
                        {
                            subtrees.Add(expr);
                        }
                    }
                    dependent |= parentIsDependent;
                }
                return expr;
            });

            return subtrees;
        }

        /// <summary>
        /// Evaluate all subtrees of an expression that aren't dependent on parameters to
        /// that expression and replace the subtree with a constant expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate</param>
        /// <returns>The partially evaluated expression</returns>
        internal static Expression PartiallyEvaluate(this Expression expression)
        {
            List<Expression> subtrees = expression.FindIndependentSubtrees();
            return VisitorHelper.VisitAll(expression, (expr, recurse) =>
            {
                if (expr != null && subtrees.Contains(expr) && expr.NodeType != ExpressionType.Constant)
                {
                    Delegate compiled = Expression.Lambda(expr).Compile();
                    object value = compiled.DynamicInvoke();
                    return Expression.Constant(value, expr.Type);
                }
                else
                {
                    return recurse(expr);
                }
            });
        }
    }
}
