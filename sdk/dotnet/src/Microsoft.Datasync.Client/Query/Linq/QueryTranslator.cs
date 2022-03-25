// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Query.Linq
{
    /// <summary>
    /// Compiles a LINQ expression tree into a <see cref="QueryDescription"/> that can
    /// be executed on the server or turned into other forms.
    /// </summary>
    /// <remarks>
    /// This code is based on the concepts discussed in <see href="http://blogs.msdn.com/b/mattwar/archive/2008/11/18/linq-links.aspx"/>.
    /// </remarks>
    /// <typeparam name="T">The type of the model used in the query.</typeparam>
    internal class QueryTranslator<T>
    {
        internal QueryTranslator(ITableQuery<T> query)
        {
            Arguments.IsNotNull(query, nameof(query));

            Query = query;
            QueryDescription = new(Query.RemoteTable.TableName) { IncludeTotalCount = query.RequestTotalCount };
        }

        /// <summary>
        /// The query which is being translated.
        /// </summary>
        internal ITableQuery<T> Query { get; }

        /// <summary>
        /// The compiled query description generated from the expression tree.
        /// </summary>
        internal QueryDescription QueryDescription { get; }

        /// <summary>
        /// The contract resolvers to use to determine property names from members used with expressions.
        /// </summary>
        private DatasyncContractResolver ContractResolver => Query.RemoteTable.ServiceClient.Serializer.SerializerSettings.ContractResolver;

        /// <summary>
        /// Translate an expression tree into a compiled query description that can be
        /// turned into whatever form is required.
        /// </summary>
        /// <returns>A compiled query description</returns>
        public QueryDescription Translate()
        {
            // Evaluate any independent subexpressions so we end up with a tree full of
            // constants or things that depend directly on our values.
            var expression = Query.Query.Expression.PartiallyEvaluate();

            // Build a new query from the expression tree.
            if (expression is MethodCallExpression methodCall)
            {
                VisitMethodCall(methodCall);
            }

            // Set the projection type if there was no projection in the query.
            QueryDescription.ProjectionArgumentType ??= typeof(T);

            // And return the query description
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
                case "Skip":
                    int skipCount = QueryTranslator<T>.GetCountArgument(expression);
                    QueryDescription.Skip = QueryDescription.Skip.HasValue ? QueryDescription.Skip.Value + skipCount : skipCount;
                    break;
                case "Take":
                    int takeCount = QueryTranslator<T>.GetCountArgument(expression);
                    QueryDescription.Top = QueryDescription.Top.HasValue ? Math.Min(QueryDescription.Top.Value, takeCount) : takeCount;
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
                QueryNode filter = FilterBuildingExpressionVisitor.Compile(lambda!.Body, ContractResolver);
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
                string memberName = FilterBuildingExpressionVisitor.GetTableMemberName(memberExpression, ContractResolver);
                if (memberName != null)
                {
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
            }
            else
            {
                throw new NotSupportedException($"'{expression?.Method.Name}' query expressions must consist of members only.");
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
                        string memberName = FilterBuildingExpressionVisitor.GetTableMemberName(memberExpression, ContractResolver);
                        if (memberName != null)
                        {
                            QueryDescription.Selection.Add(memberName);
                        }
                    }

                    // Make sure we also include all the members that would be required for deserialization
                    JsonContract contract = ContractResolver.ResolveContract(QueryDescription.ProjectionArgumentType);
                    if (contract is JsonObjectContract objectContract)
                    {
                        foreach (string propName in objectContract.Properties.Where(p => p.Required == Required.Always || p.Required == Required.AllowNull).Select(p => p.PropertyName))
                        {
                            if (!QueryDescription.Selection.Contains(propName))
                            {
                                QueryDescription.Selection.Add(propName);
                            }
                        }
                    }
                }

                return;
            }

            throw new NotSupportedException("Invalid projection in 'Select' query expression");
        }

        /// <summary>
        /// Gets the count argument value for a <c>Skip</c> or <c>Take</c> method call.
        /// </summary>
        /// <param name="expression">The method call expression.</param>
        /// <returns>The count argument</returns>
        internal protected static int GetCountArgument(MethodCallExpression expression)
        {
            Expression deepest = expression;

            // We only allow Skip(x) expressions.  Anything else will result in an exception.
            if (expression?.Arguments.Count >= 2)
            {
                if (expression.Arguments[1] is ConstantExpression constant)
                {
                    deepest = constant;
                    if (constant.Value is int @int)
                    {
                        return @int;
                    }
                }
            }

            throw new NotSupportedException($"'{expression?.Method?.Name}' query expressions must consist of a single integer, not '{deepest?.ToString()}'.");
        }
    }
}
