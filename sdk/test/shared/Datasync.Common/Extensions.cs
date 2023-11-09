// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using FluentAssertions.Specialized;
using Microsoft.AspNetCore.Datasync;
using System.Diagnostics.CodeAnalysis;

namespace Datasync.Common;

/// <summary>
/// A set of extension methods to support the Azure Mobile Apps SDK tests.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Extensions
{
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
    /// <typeparam name="TEntity">The entity to clone.</typeparam>
    /// <param name="source">The source entity.</param>
    /// <returns>A copy of the source entity.</returns>
    public static TEntity Clone<TEntity>(this TEntity entity)
        => AnyClone.CloneExtensions.Clone(entity);

    public static AndConstraint<ObjectAssertions> HaveEquivalentMetadataTo(this ObjectAssertions current, ITableData source, string because = "", params object[] becauseArgs)
    {
        const string dateFormat = "yyyy-MM-ddTHH:mm:ss.fffK";

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(current.Subject is ITableData)
            .FailWith("Expected {context:object} to be an ITableData", current.Subject);

        ITableData metadata = current.Subject as ITableData ?? throw new InvalidOperationException("Object is not an ITableData");
        bool updatedAtEquals = source.UpdatedAt == metadata.UpdatedAt;
        bool updatedAtClose = source.UpdatedAt != null && metadata.UpdatedAt != null && (source.UpdatedAt - metadata.UpdatedAt) < TimeSpan.FromMilliseconds(1);
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(metadata.Id == source.Id)
            .FailWith("Expected {context:object} to have Id {0}, but found {1}", source.Id, metadata.Id)
        .Then
            .ForCondition(metadata.Version.SequenceEqual(source.Version))
            .FailWith("Expected {context:object} to have Version {0}, but found {1}", Convert.ToBase64String(source.Version), Convert.ToBase64String(metadata.Version))
        .Then
            .ForCondition(metadata.Deleted == source.Deleted)
            .FailWith("Expected {context:object} to have Deleted {0}, but found {1}", source.Deleted, metadata.Deleted)
        .Then
            .ForCondition(updatedAtEquals || updatedAtClose)
            .FailWith("Expected {context:object} to have UpdatedAt {0}, but found {1}", source.UpdatedAt?.ToString(dateFormat), metadata.UpdatedAt?.ToString(dateFormat));

        return new AndConstraint<ObjectAssertions>(current);
    }

    /// <summary>
    /// An extension to FluentAssertions to validate the payload of a <see cref="HttpException"/>.
    /// </summary>
    public static AndConstraint<ExceptionAssertions<HttpException>> WithPayload(this ExceptionAssertions<HttpException> current, object payload, string because = "", params object[] becauseArgs)
    {
        current.Subject.First().Payload.Should().NotBeNull().And.BeEquivalentTo(payload, because, becauseArgs);
        return new AndConstraint<ExceptionAssertions<HttpException>>(current);
    }

    /// <summary>
    /// An extension to FluentAssertions to validate the StatusCode of a <see cref="HttpException"/>
    /// </summary>
    public static AndConstraint<ExceptionAssertions<HttpException>> WithStatusCode(this ExceptionAssertions<HttpException> current, int statusCode, string because = "", params object[] becauseArgs)
    {
        current.Subject.First().StatusCode.Should().Be(statusCode, because, becauseArgs);
        return new AndConstraint<ExceptionAssertions<HttpException>>(current);
    }
}
