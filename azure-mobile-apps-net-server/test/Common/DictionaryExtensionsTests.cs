// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using TestUtilities;
using Xunit;

namespace System.Collections.Generic
{
    public class DictionaryExtensionsTests
    {
        private const string TestKey = "newkey";

        private readonly Dictionary<object, object> objObjDict = new Dictionary<object, object>
        {
            { 1.0D, "novalue" },
            { new object(), "novalue" },
            { Guid.NewGuid(), "novalue" },
            { 8, "novalue" }, 
            { "key", "novalue" }
        };

        private readonly Dictionary<string, object> strObjDict = new Dictionary<string, object>
        {
            { "key", "novalue" },
            { "KEY", "novalue" },
            { "other", "novalue" },
            { "OTHER", "你好世界" }, 
        };

        private readonly Dictionary<string, string> strStrDict = new Dictionary<string, string>
        {
            { "key", "novalue" },
            { "KEY", "novalue" },
            { "other", "novalue" },
            { "OTHER", "你好世界" }, 
        };

        private readonly Dictionary<Type, object> typeObjDict = new Dictionary<Type, object>
        {
            { typeof(int), "novalue" },
            { typeof(string), "novalue" },
            { typeof(Guid), "novalue" },
        };

        public static TheoryDataCollection<object> ObjectData
        {
            get
            {
                return new TheoryDataCollection<object>
                {
                    new object(),
                    Guid.NewGuid(),
                    9,
                    typeof(DictionaryExtensionsTests),
                    "string",
                    "你好世界"
                };
            }
        }

        public static TheoryDataCollection<string> StringData
        {
            get
            {
                return new TheoryDataCollection<string>
                {
                    string.Empty,
                    "STRING",
                    "string",
                    "你好世界"
                };
            }
        }

        public static TheoryDataCollection<Type> TypeData
        {
            get
            {
                return new TheoryDataCollection<Type>
                {
                    typeof(object),
                    typeof(DictionaryExtensionsTests),
                    typeof(double),
                };
            }
        }

        public static TheoryDataCollection<object> SetDataValues
        {
            get
            {
                return new TheoryDataCollection<object>
                {
                    "string",
                    Guid.NewGuid(),
                    new Uri("http://localhost"),
                    10,
                };
            }
        }

        public static TheoryDataCollection<string, IConvertible> ConvertibleValues
        {
            get
            {
                return new TheoryDataCollection<string, IConvertible>
                {
                    { "string", "string" },
                    { "True", true },
                    { "FALSE", false },
                    { "133", 133 }, 
                    { "12.34", 12.34 }
                };
            }
        }

        [Theory]
        [MemberData("ObjectData")]
        public void DictionaryObjectObject_TryGetValue_FindsValue(object key)
        {
            // Arrange
            this.objObjDict.Add(key, "value");

            // Act
            string actual;
            bool result = DictionaryExtensions.TryGetValue(this.objObjDict, key, out actual);

            // Assert
            Assert.True(result);
            Assert.Equal("value", actual);
        }

        [Fact]
        public void DictionaryObjectObject_TryGetValue_ReturnsDefaultValueIfNotFound()
        {
            // Arrange
            Guid actual;

            // Act
            bool result = this.objObjDict.TryGetValue("unknown", out actual);

            // Assert
            Assert.False(result);
            Assert.Equal(Guid.Empty, actual);
        }

        [Theory]
        [MemberData("ObjectData")]
        public void DictionaryObjectObject_GetValueOrDefault_FindsValue(object key)
        {
            // Arrange
            this.objObjDict.Add(key, "value");

            // Act
            string actual = DictionaryExtensions.GetValueOrDefault<string>(this.objObjDict, key);

            // Assert
            Assert.Equal("value", actual);
        }

        [Fact]
        public void DictionaryObjectObject_GetValueOrDefault_ReturnsDefaultValueIfNotFound()
        {
            // Act
            Guid actual = this.objObjDict.GetValueOrDefault<Guid>("unknown");

            // Assert
            Assert.Equal(Guid.Empty, actual);
        }

        [Theory]
        [MemberData("StringData")]
        public void DictionaryStringObject_TryGetValue_FindsValue(string key)
        {
            // Arrange
            this.strObjDict.Add(key, "value");

            // Act
            string actual;
            bool result = DictionaryExtensions.TryGetValue(this.strObjDict, key, out actual);

            // Assert
            Assert.True(result);
            Assert.Equal("value", actual);
        }

        [Fact]
        public void DictionaryStringObject_TryGetValue_ReturnsDefaultValueIfNotFound()
        {
            // Arrange
            Guid actual;

            // Act
            bool result = this.strObjDict.TryGetValue("unknown", out actual);

            // Assert
            Assert.False(result);
            Assert.Equal(Guid.Empty, actual);
        }

        [Theory]
        [MemberData("StringData")]
        public void DictionaryStringObject_GetValueOrDefault_FindsValue(string key)
        {
            // Arrange
            this.strObjDict.Add(key, "value");

            // Act
            string actual = DictionaryExtensions.GetValueOrDefault<string>(this.strObjDict, key);

            // Assert
            Assert.Equal("value", actual);
        }

        [Fact]
        public void DictionaryStringObject_GetValueOrDefault_ReturnsDefaultValueIfNotFound()
        {
            // Act
            Guid actual = this.strObjDict.GetValueOrDefault<Guid>("unknown");

            // Assert
            Assert.Equal(Guid.Empty, actual);
        }

        [Theory]
        [MemberData("SetDataValues")]
        public void DictionaryStringObject_SetOrClearValue_ClearsEntryIfDefaultValue<T>(T nonDefaultValue)
        {
            // Arrange
            this.strObjDict[TestKey] = nonDefaultValue;

            // Act
            this.strObjDict.SetOrClearValue(TestKey, default(T));

            // Assert
            Assert.False(this.strObjDict.ContainsKey(TestKey));
        }

        [Theory]
        [MemberData("StringData")]
        public void DictionaryStringObject_SetOrClearValue_SetsEntry(string value)
        {
            // Act
            this.strObjDict.SetOrClearValue("key", value);

            // Assert
            Assert.Equal(value, this.strObjDict["key"]);
        }

        [Theory]
        [MemberData("StringData")]
        public void DictionaryStringString_GetValueOrDefault_FindsValue(string key)
        {
            // Arrange
            this.strStrDict.Add(key, "value");

            // Act
            string actual = DictionaryExtensions.GetValueOrDefault(this.strStrDict, key);

            // Assert
            Assert.Equal("value", actual);
        }

        [Fact]
        public void DictionaryStringString_GetValueOrDefault_ReturnsDefaultValueIfNotFound()
        {
            // Act
            string actual = this.strStrDict.GetValueOrDefault("unknown");

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [MemberData("ConvertibleValues")]
        public void DictionaryStringString_GetValueOrDefaultGeneric_FindsValue<T>(string value, T expected) where T : IConvertible
        {
            // Arrange
            this.strStrDict.Add("test", value);

            // Act
            T actual = this.strStrDict.GetValueOrDefault<T>("test");

            // Assert
            Assert.Equal(actual, expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(false)]
        [InlineData(0.0)]
        public void DictionaryStringString_GetValueOrDefaultGeneric_ReturnsDefaultValueIfNotFound<T>(T expectedDefaultValue) where T : IConvertible
        {
            // Act
            T actual = this.strStrDict.GetValueOrDefault<T>("nonexistent");

            // Assert
            Assert.Equal(actual, expectedDefaultValue);
        }

        [Theory]
        [InlineData("2147483648")] // OverflowException
        [InlineData("one")] // FormatException
        [InlineData(null)] // InvalidCastException
        public void DictionaryStringString_GetValueOrDefaultGeneric_ReturnsDefaultValueIfConversionFails(string value)
        {
            // Arrange
            this.strStrDict.Add("test", value);

            // Act
            int actual = this.strStrDict.GetValueOrDefault<int>("test");

            // Assert
            Assert.Equal(0, actual);
        }

        [Theory]
        [MemberData("TypeData")]
        public void DictionaryTypeObject_TryGetValue_FindsValue(Type key)
        {
            // Arrange
            this.typeObjDict.Add(key, "value");

            // Act
            string actual;
            bool result = DictionaryExtensions.TryGetValue(this.typeObjDict, key, out actual);

            // Assert
            Assert.True(result);
            Assert.Equal("value", actual);
        }

        [Fact]
        public void DictionaryTypeObject_TryGetValue_ReturnsDefaultValueIfNotFound()
        {
            // Arrange
            DayOfWeek actual;

            // Act
            bool result = this.typeObjDict.TryGetValue(typeof(DayOfWeek), out actual);

            // Assert
            Assert.False(result);
            Assert.Equal(DayOfWeek.Sunday, actual);
        }

        [Theory]
        [MemberData("TypeData")]
        public void DictionaryTypeObject_GetValueOrDefault_FindsValue(Type key)
        {
            // Arrange
            this.typeObjDict.Add(key, "value");

            // Act
            string actual = DictionaryExtensions.GetValueOrDefault<string>(this.typeObjDict, key);

            // Assert
            Assert.Equal("value", actual);
        }

        [Fact]
        public void DictionaryTypeObject_GetValueOrDefault_ReturnsDefaultValueIfNotFound()
        {
            // Act
            double actual = this.typeObjDict.GetValueOrDefault<double>(typeof(double));

            // Assert
            Assert.Equal(0, actual);
        }
    }
}
