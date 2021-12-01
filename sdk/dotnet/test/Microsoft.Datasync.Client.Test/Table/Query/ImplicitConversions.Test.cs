// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table.Query.Tables;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table.Query.Tables
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class ImplicitConversions_Tests
    {
        private enum TestEnum { One, Two, Three };

        [Fact]
        [Trait("Method", "Unwrap")]
        public void Unwrap_UnwrapsNullableString()
        {
            // Arrange
            Type tsut = typeof(string);

            // Act
            Type actual = ImplicitConversions.Unwrap(tsut);

            // Assert
            Assert.Equal(typeof(string), actual);
        }

        [Fact]
        [Trait("Method", "Unwrap")]
        [SuppressMessage("Simplification", "RCS1020:Simplify Nullable<T> to T?.", Justification = "Part of test")]
        public void Unwrap_UnwrapsNullableDate()
        {
            // Arrange
            Type sut = typeof(Nullable<Guid>);

            // Act
            Type actual = ImplicitConversions.Unwrap(sut);

            // Assert
            Assert.Equal(typeof(Guid), actual);
        }

        [Fact]
        [Trait("Method", "Unwrap")]
        public void Unwrap_ReturnsOriginal_WhenGeneric()
        {
            // Arrange
            Type sut = typeof(List<string>);

            // Act
            Type actual = ImplicitConversions.Unwrap(sut);

            // Assert
            Assert.Same(sut, actual);
        }

        [Fact]
        [Trait("Method", "IsImplicitConversion")]
        public void IsImplicitConversion_True_IfSameType()
        {
            Type sut = typeof(string);
            Assert.True(ImplicitConversions.IsImplicitConversion(sut, sut));
        }

        [Fact]
        [Trait("Method", "IsImplicitConversion")]
        public void IsImplicitConversion_True_IfFromEnum()
        {
            Type from = typeof(TestEnum);
            Type to = typeof(int);
            Assert.True(ImplicitConversions.IsImplicitConversion(from, to));
        }

        [Fact]
        [Trait("Method", "IsImplicitConversion")]
        public void IsImplicitConversion_True_IfInTable()
        {
            Type from = typeof(int);
            Type to = typeof(long);
            Assert.True(ImplicitConversions.IsImplicitConversion(from, to));
        }

        [Fact]
        [Trait("Method", "IsImplicitConversion")]
        public void IsImplicitConversion_False_IfInTableAndNoMatch()
        {
            Type from = typeof(long);
            Type to = typeof(int);
            Assert.False(ImplicitConversions.IsImplicitConversion(from, to));
        }

        [Fact]
        [Trait("Method", "IsImplicitConversion")]
        public void IsImplicitConversion_False_IfNotInTable()
        {
            Type from = typeof(char);
            Type to = typeof(byte);
            Assert.False(ImplicitConversions.IsImplicitConversion(from, to));
        }
    }
}
