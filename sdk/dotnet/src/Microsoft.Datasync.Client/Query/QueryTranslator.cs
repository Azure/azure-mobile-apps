// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Nodes;
using Microsoft.Datasync.Client.Query.Visitor;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Query
{
    /// <summary>
    /// Compiles a LINQ expression tree into a <see cref="QueryDescription"/> that can be executed on the server.
    /// </summary>
    /// <remarks>
    /// We use internal protected instead of private methods so that the corner cases can be more easily tested.
    /// </remarks>
    /// <typeparam name="T">The type of the items being searched.</typeparam>
    internal class QueryTranslator<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTranslator{T}"/>.
        /// </summary>
        /// <param name="query">The <see cref="DatasyncTableQuery{T}"/> to parse.</param>
        /// <param name="clientOptions">The current <see cref="DatasyncClientOptions"/> used to serialize the request.</param>
        internal QueryTranslator(TableQuery<T> query, DatasyncClientOptions clientOptions)
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
        /// The <see cref="DatasyncClientOptions"/> for the table being referenced.
        /// </summary>
        internal protected DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// The compiled <see cref="QueryDescription"/> generated from the expression tree.
        /// </summary>
        internal protected QueryDescription QueryDescription { get; }

        /// <summary>
        /// The <see cref="DatasyncTableQuery{T}"/> being translated.
        /// </summary>
        internal protected TableQuery<T> TableQuery { get; }

        /// <summary>
        /// Translate an expression tree into a compiled <see cref="QueryDescription"/>
        /// that can be executed on the server.
        /// </summary>
        /// <returns>A compiled <see cref="QueryDescription"/>.</returns>
        internal QueryDescription Translate()
        {
            // Evaluate any independent subexpressions so we end up with a tree full of
            // constants or things that depend directly on our values.
            var expression = TableQuery.Query.Expression.PartiallyEvaluate();

            // Initiate the visit to the expression.  The root of the expression tree will
            // always be a MethodCallExpression because we generate it via the TableQuery<T>.
            if (expression is MethodCallExpression methodCall)
            {
                VisitMethodCall(methodCall);
            }

            // Set the projection type if there was no projection in the query.
            QueryDescription.ProjectionArgumentType ??= typeof(T);

            // ... and return the result.
            return QueryDescription;
        }

        /// <summary>
        /// Process the core LINQ operators that are supported by a datasync service using
        /// a visitor process.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The visited expression.</returns>
        internal protected Expression VisitMethodCall(MethodCallExpression expression)
        {
            // Recurse down the target of the method call until we get to something we need to process.
            if (expression.Arguments.Count >= 1)
            {
                Expression firstArgument = expression.Arguments[0];
                if (firstArgument is MethodCallExpression methodCall && firstArgument.NodeType == ExpressionType.Call)
                {
                    VisitMethodCall(methodCall);
                }
            }

            // Handle the method call itself.  There is only a certain list of LINQ method calls that we handle.
            // Note that Skip(), Take(), and non-standard LINQ calls are handled elsewhere.
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
                    throw new NotSupportedException($"'{expression.Method.Name}' caluse in query expression is not supported.");
            }
            return expression;
        }

        /// <summary>
        /// Add a filtering expression to the query.
        /// </summary>
        /// <param name="expression">A Where method call expression.</param>
        internal protected void AddFilter(MethodCallExpression expression)
        {
            if (expression.IsValidLambdaExpression(out LambdaExpression lambda))
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
            throw new NotSupportedException("'Where' clause in query expression contains in invalid predicate");
        }

        /// <summary>
        /// Add an ordering expression to the query
        /// </summary>
        /// <param name="expression">An ordering method call expression</param>
        /// <param name="ascending">True if the ordering is ascending, false otherwise</param>
        /// <param name="prepend">True to prepend the ordering to the list</param>
        internal protected void AddOrdering(MethodCallExpression expression, bool ascending, bool prepend)
        {
            // We only allow keySelectors that are x => x.member expressions (i.e. MemberAccessNode).
            // Anything else will result in a NotSupportedException
            if (expression.IsValidLambdaExpression(out LambdaExpression lambda) && lambda!.Body is MemberExpression memberExpression)
            {
                string memberName = FilterBuildingExpressionVisitor.GetTableMemberName(memberExpression, ClientOptions);
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
        internal protected void AddOrderByNode(string memberName, bool ascending, bool prepend)
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
        internal protected void AddProjection(MethodCallExpression expression)
        {
            // We only allow projections consisting of Select(x => ...).  Anything else throws a NotSupportedException
            if (expression.IsValidLambdaExpression(out LambdaExpression lambda) && lambda!.Parameters.Count == 1)
            {
                QueryDescription.Projections.Add(lambda.Compile());
                if (QueryDescription.ProjectionArgumentType == null)
                {
                    QueryDescription.ProjectionArgumentType = lambda.Parameters[0].Type;
                    foreach (var memberExpression in lambda.Body.GetMemberExpressions())
                    {
                        string memberName = FilterBuildingExpressionVisitor.GetTableMemberName(memberExpression, ClientOptions);
                        if (memberName != null)
                        {
                            QueryDescription.Selection.Add(memberName);
                        }
                    }
                }

                return;
            }

            throw new NotSupportedException("Invalid projection in 'Select' query expression");
        }
    }
}
