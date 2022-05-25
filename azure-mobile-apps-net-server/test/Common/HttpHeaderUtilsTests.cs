// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile
{
    public class HttpHeaderUtilsTests
    {
        public static TheoryDataCollection<string> InvalidTokenData
        {
            get
            {
                return new TheoryDataCollection<string>
                {
                    { "()" },
                    { "\"" },
                    { "[]" },
                    { "===" },
                };
            }
        }

        public static TheoryDataCollection<string, string> QuotedStringData
        {
            get
            {
                return new TheoryDataCollection<string, string>
                {
                    { null, null },
                    { string.Empty, string.Empty },
                    { "a", "\"a\"" },
                    { "\"", "\"\"\"" },
                    { "a\"", "\"a\"\"" },
                    { "some\"quote", "\"some\"quote\"" },
                    { "你好世界", "\"你好世界\"" },
                    { "hello", "\"hello\"" },
                };
            }
        }

        public static TheoryDataCollection<string, string> UnquotedStringData
        {
            get
            {
                return new TheoryDataCollection<string, string>
                {
                    { null, null },
                    { string.Empty, string.Empty },
                    { "\"", "\"" },
                    { "\"\"", string.Empty },
                    { "some\"quote", "some\"quote" },
                    { "\"你好世界\"", "你好世界" },
                    { "\"hel\"lo\"", "hel\"lo" },
                };
            }
        }

        [Theory]
        [MemberData("InvalidTokenData")]
        public void ValidateToken_ThrowsOnInvalidInput(string input)
        {
            // Act
            FormatException ex = Assert.Throws<FormatException>(() => HttpHeaderUtils.ValidateToken(input));

            // Assert
            Assert.Contains("The format of value '{0}' is invalid".FormatForUser(input), ex.Message);
        }

        [Theory]
        [MemberData("QuotedStringData")]
        public void GetQuotedString(string input, string expected)
        {
            // Act
            string actual = HttpHeaderUtils.GetQuotedString(input);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("UnquotedStringData")]
        public void GetUnquotedString(string input, string expected)
        {
            // Act
            string actual = HttpHeaderUtils.GetUnquotedString(input);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
