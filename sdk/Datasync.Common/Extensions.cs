// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using FluentAssertions;
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
