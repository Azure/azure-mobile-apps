﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using FluentAssertions.Specialized;
using Microsoft.AspNetCore.Datasync;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Datasync.Common;

/// <summary>
/// A set of extension methods to support the Azure Mobile Apps SDK tests.
/// </summary>
[ExcludeFromCodeCoverage]
public static class FluentAssertionExtensions
{
    /// <summary>
    /// Checks that the current object is a <see cref="JsonElement"/> that is a boolean with the specified value.
    /// </summary>
    public static AndConstraint<ObjectAssertions> BeJsonElement(this ObjectAssertions current, bool value, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(current.Subject is JsonElement)
            .FailWith("Expected {context:object} to be a JsonElement", current.Subject);
        JsonElement jsonElement = (JsonElement)current.Subject;
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(jsonElement.ValueKind == JsonValueKind.False || jsonElement.ValueKind == JsonValueKind.True)
            .FailWith("Expected {context:object} to be a boolean, but found {0}", jsonElement.ValueKind);
        bool elementValue = jsonElement.GetBoolean();
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(elementValue == value)
            .FailWith("Expected {context:object} to be a boolean with value {0}, but found {1}", value, elementValue);

        return new AndConstraint<ObjectAssertions>(current);
    }

    /// <summary>
    /// Checks that the current object is a <see cref="JsonElement"/> that is a double with the specified value.
    /// </summary>
    public static AndConstraint<ObjectAssertions> BeJsonElement(this ObjectAssertions current, double value, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(current.Subject is JsonElement)
            .FailWith("Expected {context:object} to be a JsonElement", current.Subject);
        JsonElement jsonElement = (JsonElement)current.Subject;
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(jsonElement.ValueKind == JsonValueKind.Number)
            .FailWith("Expected {context:object} to be a number, but found {0}", jsonElement.ValueKind);
        double elementValue = jsonElement.GetDouble();
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(elementValue == value)
            .FailWith("Expected {context:object} to be a double with value {0}, but found {1}", value, elementValue);

        return new AndConstraint<ObjectAssertions>(current);
    }

    /// <summary>
    /// Checks that the current object is a <see cref="JsonElement"/> that is an int with the specified value.
    /// </summary>
    public static AndConstraint<ObjectAssertions> BeJsonElement(this ObjectAssertions current, int value, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(current.Subject is JsonElement)
            .FailWith("Expected {context:object} to be a JsonElement", current.Subject);
        JsonElement jsonElement = (JsonElement)current.Subject;
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(jsonElement.ValueKind == JsonValueKind.Number)
            .FailWith("Expected {context:object} to be a number, but found {0}", jsonElement.ValueKind);
        int elementValue = jsonElement.GetInt32();
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(elementValue == value)
            .FailWith("Expected {context:object} to be an int with value {0}, but found {1}", value, elementValue);

        return new AndConstraint<ObjectAssertions>(current);
    }

    /// <summary>
    /// Checks that the current object is a <see cref="JsonElement"/> that is a string with the specified value.
    /// </summary>
    public static AndConstraint<ObjectAssertions> BeJsonElement(this ObjectAssertions current, string value, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(current.Subject is JsonElement)
            .FailWith("Expected {context:object} to be a JsonElement", current.Subject);

        // Check that jsonElement is a string
        JsonElement jsonElement = (JsonElement)current.Subject;
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(jsonElement.ValueKind == JsonValueKind.String)
            .FailWith("Expected {context:object} to be a string, but found {0}", jsonElement.ValueKind);
        string elementValue = jsonElement.GetString();
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(elementValue == value)
            .FailWith("Expected {context:object} to be a string with value {0}, but found {1}", value, elementValue);

        return new AndConstraint<ObjectAssertions>(current);
    }

    /// <summary>
    /// Checks that the current object is a <see cref="JsonElement"/> that is a string with the specified value.
    /// </summary>
    public static AndConstraint<ObjectAssertions> BeNullJsonElement(this ObjectAssertions current, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(current.Subject == null || current.Subject is JsonElement)
            .FailWith("Expected {context:object} to be a JsonElement", current.Subject);
        if (current.Subject is JsonElement jsonElement)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(jsonElement.ValueKind == JsonValueKind.Null)
                .FailWith("Expected {context:object} to be a NULL, but found {0}", jsonElement.ValueKind);
        }
        return new AndConstraint<ObjectAssertions>(current);
    }

    /// <summary>
    /// Checks to see if the client metadata is set correctly.
    /// </summary>
    public static AndConstraint<ObjectAssertions> HaveChangedMetadata(this ObjectAssertions current, string id, DateTimeOffset startTime, string because = "", params string[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(current.Subject is ClientTableData)
            .FailWith("Expected {context:object} to be derived from ClientTableData");
        ClientTableData metadata = (ClientTableData)current.Subject;
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(id == null ? !string.IsNullOrEmpty(metadata.Id) : metadata.Id == id)
            .FailWith(id == null ? "Expected {context.object}.Id to be set" : "Exepcted {context.object}.Id to be {0}, but found {1}", id, metadata.Id)
        .Then
            .ForCondition(metadata.UpdatedAt >= startTime && metadata.UpdatedAt <= DateTimeOffset.UtcNow)
            .FailWith("Expected {context.object}.UpdatedAt to be recent, but found {0}", metadata.UpdatedAt)
        .Then
            .ForCondition(!string.IsNullOrEmpty(metadata.Version))
            .FailWith("Exepcted {context.object}.Version to be set");
        return new AndConstraint<ObjectAssertions>(current);
    }

    /// <summary>
    /// Checks to see if the client metadata is set correctly.
    /// </summary>
    public static AndConstraint<ObjectAssertions> HaveChangedMetadata(this ObjectAssertions current, object source, DateTimeOffset startTime, string because = "", params string[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(current.Subject is ClientTableData)
            .FailWith("Expected {context:object} to be derived from ClientTableData")
        .Then
            .ForCondition(source is ITableData || source is ClientTableData)
            .FailWith("Expected source to be derived from ClientTableData or ITableData");
        ClientTableData metadata = (ClientTableData)current.Subject;
        ClientTableData sourceMetadata = source is ClientTableData data ? data : new ClientTableData(source);
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(metadata.Id == sourceMetadata.Id)
            .FailWith("Exepcted {context.object}.Id to be {0}, but found {1}", sourceMetadata.Id, metadata.Id)
        .Then
            .ForCondition(metadata.UpdatedAt >= startTime && metadata.UpdatedAt <= DateTimeOffset.UtcNow)
            .FailWith("Expected {context.object}.UpdatedAt to be recent, but found {0}", metadata.UpdatedAt)
        .Then
            .ForCondition(metadata.Version != sourceMetadata.Version)
            .FailWith("Exepcted {context.object}.Version to be different to {0}, but found {1}", sourceMetadata.Version, metadata.Version);
        return new AndConstraint<ObjectAssertions>(current);
    }

    /// <summary>
    /// Checks that the current object is an <see cref="ITableData"/> and that it has the same metadata as the source.
    /// </summary>
    public static AndConstraint<ObjectAssertions> HaveEquivalentMetadataTo(this ObjectAssertions current, ITableData source, string because = "", params object[] becauseArgs)
    {
        const string dateFormat = "yyyy-MM-ddTHH:mm:ss.fffK";

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(current.Subject is ITableData || current.Subject is ClientTableData)
            .FailWith("Expected {context:object} to be derived from ITableData or ClientTableData", current.Subject);

        ClientTableData metadata = current.Subject is ClientTableData data ? data : new ClientTableData(current.Subject);
        bool updatedAtEquals = source.UpdatedAt == metadata.UpdatedAt;
        bool updatedAtClose = source.UpdatedAt != null && metadata.UpdatedAt != null && (source.UpdatedAt - metadata.UpdatedAt) < TimeSpan.FromMilliseconds(1);
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(metadata.Id == source.Id)
            .FailWith("Expected {context:object} to have Id {0}, but found {1}", source.Id, metadata.Id)
        .Then
            .ForCondition(metadata.Version.Equals(Convert.ToBase64String(source.Version)))
            .FailWith("Expected {context:object} to have Version {0}, but found {1}", Convert.ToBase64String(source.Version), metadata.Version)
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

    /// <summary>
    /// An extension to FluentAssertions to validate that a string is a GUID.
    /// </summary>
    public static AndConstraint<StringAssertions> BeAGuid(this StringAssertions current, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Guid.TryParse(current.Subject, out _))
            .FailWith("Expected {context:object} to be a Guid, but found {0}", current.Subject);
        return new AndConstraint<StringAssertions>(current);
    }

    /// <summary>
    /// Checks that the provided object is an <see cref="EntityTagHeaderValue"/> with a specific value.
    /// </summary>
    public static AndConstraint<ObjectAssertions> BeETag(this ObjectAssertions current, string value, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(current.Subject is EntityTagHeaderValue)
            .FailWith("Expected {context:object} to be an EntityTagHeaderValue", current.Subject)
        .Then
            .ForCondition(((EntityTagHeaderValue)current.Subject).Tag == value)
            .FailWith("Expected {context:object} to have value {0}, but found {1}", value, ((EntityTagHeaderValue)current.Subject).Tag);
        return new AndConstraint<ObjectAssertions>(current);
    }
}
