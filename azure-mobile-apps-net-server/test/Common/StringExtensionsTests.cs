// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using TestUtilities;
using Xunit;

namespace System
{
    public class StringExtensionsTests
    {
        public static TheoryDataCollection<string, string[]> SplitAndTrimData
        {
            get
            {
                return new TheoryDataCollection<string, string[]>
                {
                    { null, new string[0] },
                    { string.Empty, new string[0] },
                    { "   ", new string[0] },
                    { "a,b,c", new string[] { "a", "b", "c" } },
                    { " a , b , c ", new string[] { "a", "b", "c" } },
                    { "你,,好,, 世, ,\t,\r\n , 界", new string[] { "你", "好", "世", "界" } },
                };
            }
        }

        [Theory]
        [MemberData("SplitAndTrimData")]
        public void SplitAndTrim_ReturnsExpectedResult(string input, string[] expected)
        {
            // Act
            IEnumerable<string> actual = input.SplitAndTrim(',');

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
