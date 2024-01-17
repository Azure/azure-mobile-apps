// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Remote;

/// <summary>
/// Provides functionality to evaluate queries against a remote datasync service.
/// </summary>
/// <typeparam name="TEntity">The type of the data to be returned by the remote datasync service.</typeparam>
internal class RemoteQuery<TEntity> : IRemoteQuery<TEntity>
{
    /// <summary>
    /// Creates a new <see cref="RemoteQuery{TEntity}"/> object without any query components.
    /// </summary>
    public RemoteQuery()
    {
        Provider = new RemoteQueryProvider();
        Expression = Expression.Constant(this);
    }

    /// <summary>
    /// Creates a new <see cref="RemoteQuery{TEntity}"/> using the provided query provider and expression.
    /// </summary>
    /// <param name="provider">The query provider to use.</param>
    /// <param name="expression">The current expression.</param>
    public RemoteQuery(IQueryProvider provider, Expression expression)
    {
        Provider = provider;
        Expression = expression;
    }

    /// <summary>
    /// Returns the type of the entities that are returned when the expression tree associated with this
    /// instance is executed.
    /// </summary>
    public Type ElementType => typeof(TEntity);

    /// <summary>
    /// Returns the expression tree that is associated with this instance.
    /// </summary>
    public Expression Expression { get; }

    /// <summary>
    /// Returns the query provided that is associated with this data source.
    /// </summary>
    public IQueryProvider Provider { get; }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the service table.</returns>
    public IEnumerator<TEntity> GetEnumerator()
        => Provider.Execute<IEnumerable<TEntity>>(Expression).GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the service table.</returns>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
