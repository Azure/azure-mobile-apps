// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Linq.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Linq.Query
{
    /// <summary>
    /// Only cover the exceptions  Everything else is covered
    /// by the <see cref="LinqFunctionality"/> tests.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class MemberInfoKey_Tests
    {
        private class TestClass
        {
            public string field = "test";

            public string Field { get; set; }
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_WithFieldInfo_Throws()
        {
            // Ctor only works with properties and methods - fields not allowed.
            FieldInfo fieldInfo = typeof(TestClass).GetField("field");
            Assert.Throws<ArgumentException>(() => new MemberInfoKey(fieldInfo));
        }

        [Fact]
        [Trait("Method", "Equals(object)")]
        public void Equals_NotMemberInfo_ReturnsFalse()
        {
            var fieldInfo = typeof(TestClass).GetField("field");
            var propInfo = typeof(TestClass).GetProperty("Field");
            MemberInfoKey sut = new(propInfo);

            Assert.False(sut.Equals(fieldInfo));
        }

        [Fact]
        [Trait("Method", "Equals(object)")]
        public void Equals_MatchingMemberInfo_ReturnsTrue()
        {
            var propInfo = typeof(TestClass).GetProperty("Field");
            MemberInfoKey sut = new(propInfo);
            MemberInfoKey tester = new(propInfo);

            Assert.True(sut.Equals((object)tester));
        }

        [Fact]
        [Trait("Method", "Equals(object)")]
        public void Equals_NonMatchingMemberInfo_ReturnsFalse()
        {
            var propInfo = typeof(TestClass).GetProperty("Field");
            MemberInfoKey sut = new(propInfo);
            MemberInfoKey tester = new(typeof(string), "NotField", false, true);

            Assert.False(sut.Equals((object)tester));
        }
    }
}
