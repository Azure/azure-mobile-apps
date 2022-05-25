// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Microsoft.Azure.Mobile.Server.TestModels;
using Moq;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    public class TableQueryFilterTests
    {
        private static readonly byte[] Version = Encoding.UTF8.GetBytes("ABCDEF");

        public static TheoryDataCollection<Type, Type, Type> ResponseTypes
        {
            get
            {
                return new TheoryDataCollection<Type, Type, Type>
                {
                    { typeof(HttpResponseMessage), typeof(int), null },
                    { typeof(HttpResponseMessage), typeof(Guid), null },
                    { typeof(HttpResponseMessage), typeof(List<int>), typeof(int) },
                    { typeof(HttpResponseMessage), typeof(List<Guid>), typeof(Guid) },
                    { typeof(HttpResponseMessage), typeof(Dictionary<int, string>), typeof(KeyValuePair<int, string>) },
                    { typeof(OkResult), typeof(int), null },
                    { typeof(OkResult), typeof(Guid), null },
                    { typeof(OkResult), typeof(List<int>), typeof(int) },
                    { typeof(OkResult), typeof(List<Guid>), typeof(Guid) },
                    { typeof(OkResult), typeof(Dictionary<int, string>), typeof(KeyValuePair<int, string>) },
                };
            }
        }

        public static TheoryDataCollection<HttpContent, string> VersionToResponseData
        {
            get
            {
                return new TheoryDataCollection<HttpContent, string>
                {
                    { null, null },
                    { new StringContent("Hello"), null },
                    { new ObjectContent<int>(1024, new JsonMediaTypeFormatter()), null },
                    { new ObjectContent<TestEntity>(new TestEntity(), new JsonMediaTypeFormatter()), null },
                    { new ObjectContent<TestEntity>(new TestEntity() { Version = Version }, new JsonMediaTypeFormatter()), "\"QUJDREVG\"" },
                };
            }
        }

        public static TheoryDataCollection<string> VersionToETagData
        {
            get
            {
                return new TheoryDataCollection<string>
                {
                    "hello",
                    "你好世界"
                };
            }
        }

        [Theory]
        [MemberData("ResponseTypes")]
        public void GetElementType_DetectsResponseTypeAttribute(Type returnType, Type responseType, Type expected)
        {
            // Arrange
            ResponseTypeAttribute responseTypeAttribute = new ResponseTypeAttribute(responseType);
            Mock<HttpActionDescriptor> descriptorMock = new Mock<HttpActionDescriptor> { CallBase = true };
            descriptorMock.Setup<Type>(d => d.ReturnType)
                .Returns(returnType)
                .Verifiable();
            descriptorMock.Setup<Collection<ResponseTypeAttribute>>(d => d.GetCustomAttributes<ResponseTypeAttribute>(true))
                .Returns(new Collection<ResponseTypeAttribute> { responseTypeAttribute })
                .Verifiable();

            // Act
            Type actual = TableQueryFilter.GetElementType(descriptorMock.Object);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("VersionToResponseData")]
        public void AddETagResponseHeader_FindsETag(HttpContent content, string expected)
        {
            // Arrange
            EntityTagHeaderValue expectedETag = expected != null ? EntityTagHeaderValue.Parse(expected) : null;
            HttpResponseMessage response = new HttpResponseMessage()
            {
                Content = content
            };

            // Act
            TableQueryFilter.AddETagResponseHeader(response);
            EntityTagHeaderValue etag = response.Headers.ETag;

            // Assert
            Assert.Equal(expectedETag, etag);
        }

        [Theory]
        [MemberData("VersionToETagData")]
        public void GetETagFromVersion_ConvertsETag(string version)
        {
            // Arrange
            byte[] versionData = Encoding.UTF8.GetBytes(version);
            string etag = "\"" + Convert.ToBase64String(versionData) + "\"";

            // Act
            EntityTagHeaderValue actualETag = TableQueryFilter.GetETagFromVersion(versionData);

            // Assert
            Assert.Equal(etag, actualETag.Tag);
        }
    }
}
