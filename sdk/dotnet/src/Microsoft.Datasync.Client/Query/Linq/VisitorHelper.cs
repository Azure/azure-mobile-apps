// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Query.Linq
{
    /// <summary>
    /// Visit all the members of specific types of expression tree nodes.  This
    /// helps reduce the number of specific visitor classes we need to declare.
    /// </summary>
    /// <remarks>
    /// We don't test for code coverage here, since not all paths are relevant to the product.  What
    /// paths are relevant are tested by using the FilterBuildingExpressionVisitor.
    /// </remarks>
    internal sealed class VisitorHelper : ExpressionVisitor
    {
        /// <summary>
        /// A visitor callback for the generic Visit method that takes the
        /// Expression along with a function that will recurse and returns the
        /// visited expression or a modified value.
        /// </summary>
        private Func<Expression, Func<Expression, Expression>, Expression> visitor;

        /// <summary>
        /// A visitor callback for the VisitMember method that takes the
        /// MemberExpression along with a function that will recurse and
        /// returns the visisted expression or a modified value.
        /// </summary>
        private Func<MemberExpression, Func<MemberExpression, Expression>, Expression> memberVisitor;

        /// <summary>
        /// Private constructor to prevent instantiation.
        /// </summary>
        private VisitorHelper()
        {
        }

        /// <summary>
        /// Visit all the nodes in the expression.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <param name="visitor">
        /// A function that takes the current node and another function to
        /// recurse and returns an expression.
        /// </param>
        /// <returns>
        /// The expression obtained by visiting the input expression.
        /// </returns>
        public static Expression VisitAll(Expression expression, Func<Expression, Func<Expression, Expression>, Expression> visitor)
        {
            Arguments.IsNotNull(expression, nameof(expression));
            Arguments.IsNotNull(visitor, nameof(visitor));

            return new VisitorHelper() { visitor = visitor }.Visit(expression);
        }

        /// <summary>
        /// Visit all the member accesses in the expression.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <param name="visitor">
        /// A function that takes the current member access node and another
        /// function to recurse and returns and expression.
        /// </param>
        /// <returns>
        /// The expression obtained by visiting the input expression.
        /// </returns>
        public static Expression VisitMembers(Expression expression, Func<MemberExpression, Func<MemberExpression, Expression>, Expression> visitor)
        {
            Arguments.IsNotNull(expression, nameof(expression));
            Arguments.IsNotNull(visitor, nameof(visitor));

            return new VisitorHelper() { memberVisitor = visitor }.Visit(expression);
        }

        /// <summary>
        /// Visit every node.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The visisted expression.</returns>
        public override Expression Visit(Expression expression)
            => visitor != null ? visitor(expression, e => base.Visit(e)) : base.Visit(expression);

        /// <summary>
        /// Visit member access.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The visited expression.</returns>
        protected override Expression VisitMember(MemberExpression expression)
            => memberVisitor != null ? memberVisitor(expression, e => base.VisitMember(e)) : base.VisitMember(expression);
    }
}
