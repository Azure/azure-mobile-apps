// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.Datasync.Swashbuckle.Tests.Helpers;

[ExcludeFromCodeCoverage]
public static class FluentAssertionExtensions
{
    public static OpenApiPathItemAssertions Should(this OpenApiPathItem instance)
        => new(instance);
}

[ExcludeFromCodeCoverage]
public class OpenApiPathItemAssertions : ReferenceTypeAssertions<OpenApiPathItem, OpenApiPathItemAssertions>
{
    public OpenApiPathItemAssertions(OpenApiPathItem instance) : base(instance)
    {
    }

    protected override string Identifier => "OpenApiPathItem";

    public AndConstraint<OpenApiPathItemAssertions> HaveOperations(string[] operations, string because = "", params object[] becauseArgs)
    {
        string[] subjectOperations = Subject.Operations.Keys.Select(x => x.ToString().ToLowerInvariant()).OrderBy(x => x).ToArray();
        string[] expectedOperations = operations.Select(x => x.ToLowerInvariant()).OrderBy(x => x).ToArray();

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(subjectOperations.SequenceEqual(expectedOperations))
            .FailWith("Exepcted {context:OpenApiPathItem} to have operations {0}{reason}, but found {1}", expectedOperations, subjectOperations);

        return new AndConstraint<OpenApiPathItemAssertions>(this);
    }
}