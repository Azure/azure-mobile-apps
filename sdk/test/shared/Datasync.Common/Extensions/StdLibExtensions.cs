﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Datasync.Common;

/// <summary>
/// A set of extension methods to support the Azure Mobile Apps SDK tests.
/// </summary>
[ExcludeFromCodeCoverage]
public static class StdLibExtensions
{
    /// <summary>
    /// Enables the correct logging on a database context.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="current"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public static DbContextOptionsBuilder<TContext> EnableLogging<TContext>(this DbContextOptionsBuilder<TContext> current, ITestOutputHelper output) where TContext : DbContext
    {
        bool enableLogging = (Environment.GetEnvironmentVariable("ENABLE_SQL_LOGGING") ?? "false") == "true";
        if (output != null && enableLogging)
        {
            current
                .UseLoggerFactory(new TestLoggerFactory(output, new string[] { "Microsoft.EntityFrameworkCore.Database.Command" }))
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();
        }
        return current;
    }

    /// <summary>
    /// Fills in the common metadata properties for an entity.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The source metadata</param>
    /// <returns>A new entity with the source metadata filled in.</returns>
    public static T ToTableEntity<T>(this ITableData entity) where T : ITableData, new()
       => new() { Id = entity.Id, Version = entity.Version, UpdatedAt = entity.UpdatedAt, Deleted = entity.Deleted };

    /// <summary>
    /// Creates a deep clone of an entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to clone.</typeparam>
    /// <param name="entity">The source entity.</param>
    /// <returns>A copy of the source entity.</returns>
    public static TEntity Clone<TEntity>(this TEntity entity)
        => AnyClone.CloneExtensions.Clone(entity);
}
