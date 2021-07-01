// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

#pragma warning disable RCS1196 // Call extension method as instance method.

namespace Microsoft.Datasync.Client.Test.Utils
{
    [ExcludeFromCodeCoverage]
    public class StdLibExtensions_Test : BaseTest
    {
        [Fact]
        [Trait("Method", "NormalizeEndpoint(Uri)")]
        public void IsValidEndpoint_Null_Throws()
        {
            Uri sut = null;
            Assert.Throws<ArgumentNullException>(() => StdLibExtensions.NormalizeEndpoint(sut));
        }

        [Theory, ClassData(typeof(TestCases.Invalid_Endpoints))]
        [Trait("Method", "NormalizeEndpoint(Uri)")]
        public void IsValidEndpoint_Invalid_Throws(string endpoint, bool isRelative = false)
        {
            Assert.Throws<UriFormatException>(() => StdLibExtensions.NormalizeEndpoint(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint)));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "NormalizeEndpoint(Uri)")]
        public void IsValidEndpoint_Valid_Passes(string endpoint, string normalizedEndpoint)
        {
            Uri sut = new(endpoint);
            Assert.Equal(normalizedEndpoint, sut.NormalizeEndpoint().ToString());
        }
    }
}
