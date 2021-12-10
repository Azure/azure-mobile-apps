// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Table.Query;
using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Test.Table.Query
{
    /// <summary>
    /// A wrapped version of <see cref="QueryTranslator{T}"/> that exposes all
    /// the protected values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TestQueryTranslator<T> : QueryTranslator<T>
    {
        internal TestQueryTranslator(DatasyncTableQuery<T> query, DatasyncClientOptions options) : base(query, options)
        {
        }

        internal int CountOrderings() => QueryDescription.Ordering.Count;

        internal new void AddFilter(MethodCallExpression expression)
            => base.AddFilter(expression);

        internal void AddOrdering(MethodCallExpression expression)
            => base.AddOrdering(expression, true, false);

        internal void AddOrderByNode(string memberName)
            => base.AddOrderByNode(memberName, true, false);

        internal new void AddProjection(MethodCallExpression expression)
            => base.AddProjection(expression);
    }
}
