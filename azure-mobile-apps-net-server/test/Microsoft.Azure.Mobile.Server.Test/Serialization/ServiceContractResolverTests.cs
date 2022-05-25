// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Net.Http.Formatting;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Serialization
{
    public class ServiceContractResolverTests
    {
        private MediaTypeFormatter formatter;
        private ServiceContractResolverMock resolverMock;

        public ServiceContractResolverTests()
        {
            this.formatter = new JsonMediaTypeFormatter();
            this.resolverMock = new ServiceContractResolverMock(this.formatter);
        }

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

        [Theory]
        [MemberData("CamelData")]
        public void ResolverPropertyName_MakesCamelCase(string input, string expected)
        {
            // Act
            string actual = this.resolverMock.ResolvePropertyName(input);

            // Assert
            Assert.Equal(expected, actual);
        }

        private class ServiceContractResolverMock : ServiceContractResolver
        {
            public ServiceContractResolverMock(MediaTypeFormatter formatter)
                : base(formatter)
            {
            }

            public new string ResolvePropertyName(string propertyName)
            {
                return base.ResolvePropertyName(propertyName);
            }
        }
    }
}
