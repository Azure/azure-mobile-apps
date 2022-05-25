// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TestUtilities;
using Xunit;

namespace System.Net.Http
{
    public class TypeExtensionsTests
    {
        public static TheoryDataCollection<Type, string> ShortTypeNames
        {
            get
            {
                return new TheoryDataCollection<Type, string>
                {
                    { typeof(Int32), "Int32" },
                    { typeof(int), "Int32" },
                    { typeof(Guid), "Guid" },
                    { typeof(Assembly), "Assembly" },
                    { typeof(Abstract), "Abstract" },
                    { typeof(Nested), "Nested" },
                    { typeof(Nested.Inner), "Inner" },
                    { typeof(Generic<,,>), "Generic" },
                    { typeof(Generic<int, string, double>), "Generic" },
                    { typeof(GenericBase), "GenericBase" },
                    { typeof(List<>), "List" },
                    { typeof(List<int>), "List" },
                };
            }
        }

        [Theory]
        [MemberData("ShortTypeNames")]
        public void GetShortName_ReturnsExpectedResult(Type type, string expected)
        {
            Assert.Equal(expected, type.GetShortName());
        }

        private abstract class Abstract
        {
        }

        private class Nested
        {
            public string Name { get; set; }

            public class Inner
            {
            }
        }

        private class Generic<T1, T2, T3>
        {
        }

        private class GenericBase : Generic<int, string, double>
        {
        }

        private class DerivedA : List<int>
        {
        }

        private class DerivedB : DerivedA
        {
        }

        private class DerivedC : IEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        private class DerivedD : IQueryable<int>
        {
            public Type ElementType
            {
                get { throw new NotImplementedException(); }
            }

            public Expression Expression
            {
                get { throw new NotImplementedException(); }
            }

            public IQueryProvider Provider
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerator<int> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}
