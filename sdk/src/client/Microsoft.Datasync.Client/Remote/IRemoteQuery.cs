// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Remote;

/// <summary>
/// Definition of a remote query implementation.  This is rooted in the <see cref="IQueryable{T}"/>
/// interface, but adds the ability to execute the query remotely.
/// </summary>
/// <typeparam name="TEntity">The type of entity being queried.</typeparam>
public interface IRemoteQuery<TEntity> : IQueryable<TEntity>
{
}
