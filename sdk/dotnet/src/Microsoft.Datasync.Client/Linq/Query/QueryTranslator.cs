// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Internal;
using Microsoft.Datasync.Client.Linq.Query.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Linq.Query
{
    /// <summary>
    /// Compiles a LINQ expression tree into a <see cref="QueryDescription"/>
    /// that can be executed on the server.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class QueryTranslator<T> where T : notnull
    {
        /// <summary>
        /// The compiled <see cref="QueryDescription"/> generated from the expression tree.
        /// </summary>
        protected QueryDescription QueryDescription { get; }

        /// <summary>
        /// The <see cref="DatasyncTableQuery{T}"/> being translated.
        /// </summary>
        protected DatasyncTableQuery<T> TableQuery { get; }

        /// <summary>
        /// The <see cref="DatasyncClientOptions"/> for the table being referenced.
        /// </summary>
        protected DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTranslator{T}"/>.
        /// </summary>
        /// <param name="query">The <see cref="DatasyncTableQuery{T}"/> to parse.</param>
        internal QueryTranslator(DatasyncTableQuery<T> query, DatasyncClientOptions clientOptions)
        {
            Validate.IsNotNull(query, nameof(query));
            Validate.IsNotNull(clientOptions, nameof(clientOptions));

            TableQuery = query;
            ClientOptions = clientOptions;
            QueryDescription = new QueryDescription()
            {
                Parameters = query.QueryParameters,
                Skip = query.SkipCount,
                Top = query.TakeCount
            };
        }

        /// <summary>
        /// Translate an expression tree into a compiled <see cref="QueryDescription"/>
        /// that can be executed on the server.
        /// </summary>
        /// <returns>A compiled <see cref="QueryDescription"/>.</returns>
        internal QueryDescription Translate()
        {
            // Evaluate any independent subexpressions so we end up with a tree full of
            // constants or things that depend directly on our values.
            var expression = PartiallyEvaluate(TableQuery.Query.Expression);

            // Initiate the visit to the expression.  The root of the expression will always
            // be a MethodCallExpression because we generate via the DatasyncTableQuery.
            if (expression is MethodCallExpression mce)
            {
                VisitMethodCall(mce);
            }

            // Set the projection type if there was no projection in the query
            QueryDescription.ProjectionArgumentType ??= typeof(T);
            return QueryDescription;
        }

        /// <summary>
        /// Process the core LINQ operators that are supported by the datasync service.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The visited expression</returns>
        protected Expression? VisitMethodCall(MethodCallExpression expression)
        {
            // Recurse down the target of the method call.
            if (expression.Arguments.Count >= 1)
            {
                Expression firstArgument = expression.Arguments[0];
                if (firstArgument is MethodCallExpression mce && firstArgument.NodeType == ExpressionType.Call)
                {
                    VisitMethodCall(mce);
                }
            }

            // Handle the method call itself
            //string? name = expression.Method.DeclaringType == typeof(Queryable) ? expression.Method.Name : null;
            switch (expression.Method.Name)
            {
                case "OrderBy":
                    AddOrdering(expression, ascending: true, prepend: true);
                    break;
                case "OrderByDescending":
                    AddOrdering(expression, ascending: false, prepend: true);
                    break;
                case "Select":
                    AddProjection(expression);
                    break;
                case "ThenBy":
                    AddOrdering(expression, ascending: true, prepend: false);
                    break;
                case "ThenByDescending":
                    AddOrdering(expression, ascending: false, prepend: false);
                    break;
                case "Where":
                    AddFilter(expression);
                    break;
                default:
                    throw new NotSupportedException($"'{expression.Method.Name} clause in query expression is not supported");
            }
            return expression;
        }

        /// <summary>
        /// Add a filtering expression to the query.
        /// </summary>
        /// <param name="expression">A Where method call expression.</param>
        protected void AddFilter(MethodCallExpression? expression)
        {
            if (IsValidLambdaExpression(expression, out LambdaExpression? lambda))
            {
                QueryNode filter = FilterBuildingExpressionVisitor.Compile(lambda!.Body, ClientOptions);
                if (QueryDescription.Filter != null)
                {
                    QueryDescription.Filter = new BinaryOperatorNode(BinaryOperatorKind.And, QueryDescription.Filter, filter);
                }
                else
                {
                    QueryDescription.Filter = filter;
                }
                return;
            }
            throw new NotSupportedException("'Where' clause in query expression contains an invalid predicate");
        }

        /// <summary>
        /// Add an ordering expression to the query
        /// </summary>
        /// <param name="expression">An ordering method call expression</param>
        /// <param name="ascending">True if the ordering is ascending, false otherwise</param>
        /// <param name="prepend">True to prepend the ordering to the list</param>
        protected void AddOrdering(MethodCallExpression? expression, bool ascending, bool prepend)
        {
            // We only allow keySelectors that are x => x.member expressions (i.e. MemberAccessNode).
            // Anything else will result in a NotSupportedException
            if (IsValidLambdaExpression(expression, out LambdaExpression? lambda) && lambda!.Body is MemberExpression memberExpression)
            {
                string? memberName = FilterBuildingExpressionVisitor.GetTableMemberName(memberExpression, ClientOptions);
                AddOrderByNode(memberName, ascending, prepend);
            }
            else
            {
                throw new NotSupportedException($"'{expression?.Method.Name}' query expressions must consist of members only.");
            }
        }

        /// <summary>
        /// Add an OrderByNode to the Orderings.
        /// </summary>
        /// <param name="memberName">The memberName to add</param>
        /// <param name="ascending">True if an ascending sort</param>
        /// <param name="prepend">True if the sort should be prepended to the list.</param>
        protected void AddOrderByNode(string? memberName, bool ascending, bool prepend)
        {
            if (memberName == null)
                return;
            var node = new OrderByNode(new MemberAccessNode(null, memberName), ascending);
            if (prepend)
            {
                QueryDescription.Ordering.Insert(0, node);
            }
            else
            {
                QueryDescription.Ordering.Add(node);
            }
        }

        /// <summary>
        /// Add a projection to the query
        /// </summary>
        /// <param name="expression">A Select Method Call expression</param>
        protected void AddProjection(MethodCallExpression? expression)
        {
            // We only allow projections consisting of Select(x => ...).  Anything else throws a NotSupportedException
            if (IsValidLambdaExpression(expression, out LambdaExpression? lambda) && lambda!.Parameters.Count == 1)
            {
                QueryDescription.Projections.Add(lambda.Compile());
                if (QueryDescription.ProjectionArgumentType == null)
                {
                    QueryDescription.ProjectionArgumentType = lambda.Parameters[0].Type;
                    foreach (var memberExpression in GetMemberExpressions(lambda.Body))
                    {
                        string? memberName = FilterBuildingExpressionVisitor.GetTableMemberName(memberExpression, ClientOptions);
                        if (memberName != null)
                        {
                            QueryDescription.Selection.Add(memberName);
                        }
                    }

                    // TODO: Add all members that would be required for deserialization, i.e. marked as Required in ProjectionArgumentType
                    // See https://github.com/Azure/azure-mobile-apps-net-client/blob/master/src/Microsoft.Azure.Mobile.Client/Table/Query/Linq/MobileServiceTableQueryTranslator.cs#L249
                }

                return;
            }

            throw new NotSupportedException("Invalid projection in 'Select' query expression");
        }

        /// <summary>
        /// Remove the quote from quoted expressions.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>An unquoted expression</returns>
        protected static Expression StripQuote(Expression expression)
            => expression.NodeType == ExpressionType.Quote ? ((UnaryExpression)expression).Operand : expression;

        /// <summary>
        /// Determines if the provided expression is a valid LambdaExpression.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <param name="lambdaExpression">The lambda expression equivalent</param>
        /// <returns>True if a lambda expression</returns>
        protected static bool IsValidLambdaExpression(MethodCallExpression? expression, out LambdaExpression? lambdaExpression)
        {
            lambdaExpression = null;
            if (expression?.Arguments.Count >= 2 && StripQuote(expression.Arguments[1]) is LambdaExpression lambda)
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
        protected IEnumerable<MemberExpression> GetMemberExpressions(Expression expression)
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
        /// Evaluate all subtrees of an expression that aren't dependent on parameters to
        /// that expression and replace the subtree with a constant expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate</param>
        /// <returns></returns>
        protected static Expression PartiallyEvaluate(Expression expression)
        {
            List<Expression> subtrees = FindIndependentSubtrees(expression);
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

        /// <summary>
        /// Walk the expression and compute all the subtrees that are not dependent on any
        /// of the expressions parameters.
        /// </summary>
        /// <param name="expression">The expression to analyze.</param>
        /// <returns>A collection of all the expression subtrees that are independent from the expression parameters.</returns>
        protected static List<Expression> FindIndependentSubtrees(Expression expression)
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
                        ConstantExpression? constant = expr as ConstantExpression;
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
    }
}
