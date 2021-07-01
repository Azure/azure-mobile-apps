// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Utils
{
    [ExcludeFromCodeCoverage]
    public class Validate_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "IsNotNull(object,string)")]
        public void IsNotNull_Null_Throws()
        {
            object sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsNotNull(sut, nameof(sut)));
        }

        [Fact]
        [Trait("Method", "IsNotNull(object,string)")]
        public void IsNotNull_NotNull_Passes()
        {
            object sut = new();
            Validate.IsNotNull(sut, nameof(sut));
        }

        [Fact]
        [Trait("Method", "IsValidEndpoint(Uri,string)")]
        public void IsValidEndpoint_Null_Throws()
        {
            Uri sut = null;
            Assert.Throws<ArgumentNullException>(() => Validate.IsValidEndpoint(sut, nameof(sut)));
        }

        [Theory, ClassData(typeof(TestCases.Invalid_Endpoints))]
        [Trait("Method", "IsValidEndpoint(Uri,string)")]
        public void IsValidEndpoint_Invalid_Throws(string endpoint, bool isRelative = false)
        {
            Assert.Throws<UriFormatException>(() => Validate.IsValidEndpoint(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint), "sut"));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "IsValidEndpoint(Uri,string)")]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Test case does not check for normalization")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Test case does not check for normalization")]
        public void IsValidEndpoint_Valid_Passes(string endpoint, string normalizedEndpoint)
        {
            Uri sut = new(endpoint);
            Validate.IsValidEndpoint(sut, nameof(sut));
        }
    }
}
