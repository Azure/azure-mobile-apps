// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Internal
{
    public class ConcurrentHashsetTests
    {
        public static TheoryDataCollection<object> HashsetValues
        {
            get
            {
                return new TheoryDataCollection<object>
                {
                    "string",
                    new Uri("http://localhost"),
                    10,
                    DayOfWeek.Saturday,
                    typeof(ConcurrentHashsetTests)
                };
            }
        }

        [Theory]
        [MemberData("HashsetValues")]
        public void Add_ReturnsTrue_WhenItemAddedFirstTime<T>(T value)
        {
            // Arrange
            ConcurrentHashset<T> set = new ConcurrentHashset<T>();

            // Act
            bool actual = set.Add(value);

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [MemberData("HashsetValues")]
        public void Add_ReturnsFalse_WhenItemAlreadyPresent<T>(T value)
        {
            // Arrange
            ConcurrentHashset<T> set = new ConcurrentHashset<T>();

            // Act
            set.Add(value);
            bool actual = set.Add(value);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void Add_ReturnsFalse_WhenItemAlreadyPresentUsingCustomComparer()
        {
            // Arrange
            ConcurrentHashset<string> set = new ConcurrentHashset<string>(StringComparer.OrdinalIgnoreCase);

            // Act
            bool first = set.Add("value");
            bool second = set.Add("VaLue");

            // Assert
            Assert.True(first);
            Assert.False(second);
        }
    }
}
