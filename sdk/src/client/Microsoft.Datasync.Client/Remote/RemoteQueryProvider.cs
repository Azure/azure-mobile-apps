// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Remote;

/// <summary>
/// Defines methods to create and execute queries that are described by an <see cref="Expression"/>
/// available within a <see cref="RemoteQuery{TEntity}"/> instance.
/// </summary>
internal class RemoteQueryProvider : IQueryProvider
{
    /// <summary>
    /// Constructs a queryable object that can evaluate the query represented by the specified expression tree.
    /// </summary>
    /// <param name="expression">The expression tree to be evaluated.</param>
    /// <returns>An <see cref="IQueryable"/> that represents the expression tree.</returns>
    public IQueryable CreateQuery(Expression expression)
        => throw new NotImplementedException();

    /// <summary>
    /// Constructs a queryable object that can evaluate the query represented by the specified expression tree.
    /// </summary>
    /// <param name="expression">The expression tree to be evaluated.</param>
    /// <returns>An <see cref="IQueryable{TEntity}"/> that represents the expression tree.</returns>
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new RemoteQuery<TElement>(this, expression);

    /// <summary>
    /// Executes the query represented by the specified expression tree.
    /// </summary>
    /// <param name="expression">The expression tree to execute.</param>
    /// <returns>The results of the expression tree.</returns>
    public object? Execute(Expression expression)
        => throw new NotImplementedException();

    /// <summary>
    /// Executes the query represented by the specified expression tree.
    /// </summary>
    /// <param name="expression">The expression tree to execute.</param>
    /// <returns>The results of the expression tree.</returns>
    public TResult Execute<TResult>(Expression expression)
        => throw new NotImplementedException();
}
