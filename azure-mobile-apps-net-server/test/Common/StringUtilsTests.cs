// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile
{
    public class StringUtilsTests
    {
        public static TheoryDataCollection<string, string> CamelData
        {
            get
            {
                return new TheoryDataCollection<string, string>
                {
                    { null, null },
                    { string.Empty, string.Empty },
                    { "string", "string" },
                    { "sTRING", "sTRING" },
                    { "STRING", "string" },
                    { "String", "string" },
                    { "你好世界", "你好世界" },
                    { "你好世界a", "你好世界a" },
                    { "A你好世界", "a你好世界" },
                    { "a你好世界", "a你好世界" }
                };
            }
        }

        public static TheoryDataCollection<string, string> PascalData
        {
            get
            {
                return new TheoryDataCollection<string, string>
                {
                    { null, null },
                    { string.Empty, string.Empty },
                    { "string", "String" },
                    { "sTRING", "STRING" },
                    { "STRING", "STRING" },
                    { "String", "String" },
                    { "你好世界", "你好世界" },
                    { "你好世界a", "你好世界a" },
                    { "A你好世界", "A你好世界" },
                    { "a你好世界", "A你好世界" }
                };
            }
        }

        [Theory]
        [MemberData("CamelData")]
        public void ToCamelCase(string input, string expected)
        {
            Assert.Equal(expected, StringUtils.ToCamelCase(input));
        }

        [Theory]
        [MemberData("PascalData")]
        public void ToPascalCase(string input, string expected)
        {
            Assert.Equal(expected, StringUtils.ToPascalCase(input));
        }
    }
}
