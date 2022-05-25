// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Azure.Mobile
{
    public class TypeUtilsTests
    {
        public interface ITestType
        {
        }

        public interface INotImplementedTestType
        {
        }

        [Fact]
        public void GetTypes_ReturnsExpectedTypes()
        {
            // Arrange
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

            // Act
            ICollection<Type> actual = TypeUtils.GetTypes(asms, t => IsType<ITestType>(t));

            // Assert
            Assert.Equal(1, actual.Count);
            Assert.Equal(typeof(TestType), actual.Single());
        }

        [Fact]
        public void GetTypes_ReturnsEmptyIfNoneFound()
        {
            // Arrange
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

            // Act
            ICollection<Type> actual = TypeUtils.GetTypes(asms, t => IsType<INotImplementedTestType>(t));

            // Assert
            Assert.Empty(actual);
        }

        private static bool IsType<T>(Type t)
        {
            return
                t != null &&
                t.IsClass &&
                t.IsVisible &&
                !t.IsAbstract &&
                typeof(T).IsAssignableFrom(t);
        }

        [Fact]
        public void CreateInvoker_CreatesValidFunc()
        {
            // Arrange
            const string Expected = "Hello";
            TestType test = new TestType(Expected);
            MethodInfo method = typeof(TestType).GetMethod("Invoker");

            // Act
            Func<object, string> invoker = TypeUtils.CreateInvoker<string>(typeof(TestType), method);
            string actual = invoker(test);

            // Assert
            Assert.Equal(Expected, actual);
        }

        [Fact]
        public void CreateInvoker_Throws_OnInvalidInstanceType()
        {
            // Arrange
            MethodInfo method = typeof(TestType).GetMethod("Invoker");

            // Act
            Func<object, string> invoker = TypeUtils.CreateInvoker<string>(typeof(TestType), method);

            Assert.Throws<InvalidCastException>(() => invoker("invalid"));
        }

        public class TestType : ITestType
        {
            private string result;

            public TestType(string result)
            {
                this.result = result;
            }

            public string Invoker()
            {
                return this.result;
            }
        }
    }
}
