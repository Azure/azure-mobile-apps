// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using TestUtilities;
using Xunit;

namespace System.Net.Http
{
    public class TableTypeExtensionsTests
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
                    { typeof(Generic<,,>), "Generic" },
                    { typeof(Generic<int, string, double>), "Generic" },
                    { typeof(GenericBase), "GenericBase" },
                    { typeof(List<>), "List" },
                    { typeof(List<int>), "List" },
                };
            }
        }

        public static TheoryDataCollection<Type, Type> ElementTypes
        {
            get
            {
                return new TheoryDataCollection<Type, Type>
                {
                    { typeof(int), null },
                    { typeof(Assembly), null },
                    { typeof(Guid), null },
                    { typeof(Generic<,,>), null },
                    { typeof(Generic<int, string, double>), null },
                    { typeof(List<>), null },
                    { typeof(Dictionary<,>), null },
                    { typeof(IEnumerable<int>), typeof(int) },
                    { typeof(IQueryable<int>), typeof(int) },
                    { typeof(List<int>), typeof(int) },
                    { typeof(List<Guid>), typeof(Guid) },
                    { typeof(DerivedA), typeof(int) },
                    { typeof(DerivedB), typeof(int) },
                    { typeof(DerivedC), typeof(int) },
                    { typeof(DerivedD), typeof(int) },
                    { typeof(List<TypeExtensionsTests>), typeof(TypeExtensionsTests) },
                    { typeof(Dictionary<int, string>), typeof(KeyValuePair<int, string>) },
                    { typeof(SingleResult<int>), typeof(int) },
                    { typeof(SingleResult<Guid>), typeof(Guid) },
                    { typeof(SingleResult<List<Guid>>), typeof(List<Guid>) },
                    { typeof(SingleResult<Dictionary<int, string>>), typeof(Dictionary<int, string>) },
                    { typeof(SingleResult<DerivedC>), typeof(DerivedC) },
                    { typeof(SingleResult<DerivedD>), typeof(DerivedD) },
                };
            }
        }

        [Theory]
        [MemberData("ElementTypes")]
        public void GetEnumerableElementType_ReturnsExpectedResult(Type type, Type expected)
        {
            Assert.Equal(expected, type.GetEnumerableElementType());

            Type taskType = typeof(Task<>).MakeGenericType(type);
            Assert.Equal(expected, taskType.GetEnumerableElementType());
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
