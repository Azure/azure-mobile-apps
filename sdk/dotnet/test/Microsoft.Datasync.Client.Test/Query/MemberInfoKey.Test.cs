﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Query
{
    [ExcludeFromCodeCoverage]
    public class MemberInfoKey_Tests : BaseQueryTest
    {
        [Fact]
        public void CorrectlyMatchesInstanceMemberInfos()
        {
            Dictionary<MethodInfo, MemberInfoKey> instanceMethods = new() 
            {
                { FindInstanceMethod(typeof(string), "ToLower"), new MemberInfoKey(typeof(string), "ToLower", true, true) },
                { FindInstanceMethod(typeof(string), "ToLowerInvariant"), new MemberInfoKey(typeof(string), "ToLowerInvariant", true, true) },
                { FindInstanceMethod(typeof(string), "ToUpper"),  new MemberInfoKey(typeof(string), "ToUpper", true, true) },
                { FindInstanceMethod(typeof(string), "ToUpperInvariant"), new MemberInfoKey(typeof(string), "ToUpperInvariant", true, true) },
                { FindInstanceMethod(typeof(string), "Trim"), new MemberInfoKey(typeof(string), "Trim", true, true) },
                { FindInstanceMethod(typeof(string), "StartsWith", typeof(string)), new MemberInfoKey(typeof(string), "StartsWith", true, true, typeof(string)) },
                { FindInstanceMethod(typeof(string), "EndsWith", typeof(string)), new MemberInfoKey(typeof(string), "EndsWith", true, true, typeof(string)) },
                { FindInstanceMethod(typeof(string), "IndexOf", typeof(string)), new MemberInfoKey(typeof(string), "IndexOf", true, true, typeof(string)) },
                { FindInstanceMethod(typeof(string), "IndexOf", typeof(char)), new MemberInfoKey(typeof(string), "IndexOf", true, true, typeof(char)) },
                { FindInstanceMethod(typeof(string), "Contains", typeof(string)), new MemberInfoKey(typeof(string), "Contains", true, true, typeof(string)) },
                { FindInstanceMethod(typeof(string), "Replace", typeof(string), typeof(string)), new MemberInfoKey(typeof(string), "Replace", true, true, typeof(string), typeof(string)) },
                { FindInstanceMethod(typeof(string), "Replace", typeof(char), typeof(char)), new MemberInfoKey(typeof(string), "Replace", true, true, typeof(char), typeof(char)) },
                { FindInstanceMethod(typeof(string), "Substring", typeof(int)), new MemberInfoKey(typeof(string), "Substring", true, true, typeof(int)) },
                { FindInstanceMethod(typeof(string), "Substring", typeof(int), typeof(int)), new MemberInfoKey(typeof(string), "Substring", true, true, typeof(int), typeof(int)) },
            };

            // Compare each key against all the other keys - they should only match for the same key.
            foreach (MethodInfo key in instanceMethods.Keys)
            {
                foreach (var pair in instanceMethods)
                {
                    MemberInfoKey other = new(key);
                    Assert.Equal(key == pair.Key, pair.Value.Equals(other));
                }
            }
        }

        [Fact]
        public void CorrectlyMatchesStaticMemberInfos()
        {
            Dictionary<MethodInfo, MemberInfoKey> staticMethods = new()
            {
                { FindStaticMethod(typeof(Math), "Floor", typeof(double)), new MemberInfoKey(typeof(Math), "Floor", true, false, typeof(double)) },
                { FindStaticMethod(typeof(Math), "Ceiling", typeof(double)), new MemberInfoKey(typeof(Math), "Ceiling", true, false, typeof(double)) },
                { FindStaticMethod(typeof(Math), "Round", typeof(double)), new MemberInfoKey(typeof(Math), "Round", true, false, typeof(double)) },
                { FindStaticMethod(typeof(string), "Concat", typeof(string), typeof(string)), new MemberInfoKey(typeof(string), "Concat", true, false, typeof(string), typeof(string)) },
                { FindStaticMethod(typeof(decimal), "Floor", typeof(decimal)), new MemberInfoKey(typeof(Decimal), "Floor", true, false, typeof(decimal)) },
                { FindStaticMethod(typeof(Decimal), "Ceiling", typeof(decimal)), new MemberInfoKey(typeof(Decimal), "Ceiling", true, false, typeof(decimal)) },
                { FindStaticMethod(typeof(Decimal), "Round", typeof(decimal)), new MemberInfoKey(typeof(Decimal), "Round", true, false, typeof(decimal)) },
                { FindStaticMethod(typeof(Math), "Ceiling", typeof(decimal)), new MemberInfoKey(typeof(Math), "Ceiling", true, false, typeof(decimal)) },
                { FindStaticMethod(typeof(Math), "Floor", typeof(decimal)), new MemberInfoKey(typeof(Math), "Floor", true, false, typeof(decimal)) },
                { FindStaticMethod(typeof(Math), "Round", typeof(decimal)), new MemberInfoKey(typeof(Math), "Round", true, false, typeof(decimal)) }
            };

            // Compare each key against all the other keys - they should only match for the same key.
            foreach (MethodInfo key in staticMethods.Keys)
            {
                foreach (var pair in staticMethods)
                {
                    MemberInfoKey other = new(key);
                    Assert.Equal(key == pair.Key, pair.Value.Equals(other));
                }
            }
        }

        [Fact]
        public void CorrectlyMatchesPropertyInfos()
        {
            Dictionary<MemberInfo, MemberInfoKey> instanceProperties = new() {
                 { FindInstanceProperty(typeof(string), "Length"), new MemberInfoKey(typeof(string), "Length", false, true) },
                 { FindInstanceProperty(typeof(DateTime), "Day"), new MemberInfoKey(typeof(DateTime), "Day", false, true) },
                 { FindInstanceProperty(typeof(DateTime), "Month"), new MemberInfoKey(typeof(DateTime), "Month", false, true) },
                 { FindInstanceProperty(typeof(DateTime), "Year"), new MemberInfoKey(typeof(DateTime), "Year", false, true) },
                 { FindInstanceProperty(typeof(DateTime), "Hour"), new MemberInfoKey(typeof(DateTime), "Hour", false, true) },
                 { FindInstanceProperty(typeof(DateTime), "Minute"), new MemberInfoKey(typeof(DateTime), "Minute", false, true) },
                 { FindInstanceProperty(typeof(DateTime), "Second"), new MemberInfoKey(typeof(DateTime), "Second", false, true) }
            };

            // Compare each key against all the other keys - they should only match for the same key.
            foreach (MemberInfo key in instanceProperties.Keys)
            {
                foreach (var pair in instanceProperties)
                {
                    MemberInfoKey other = new(key);
                    Assert.Equal(key == pair.Key, pair.Value.Equals(other));
                }
            }
        }
    }
}
