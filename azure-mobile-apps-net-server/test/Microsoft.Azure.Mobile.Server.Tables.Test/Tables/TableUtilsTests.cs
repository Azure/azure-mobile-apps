// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Net.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using Microsoft.Azure.Mobile.Server.Mocks;
using Microsoft.Azure.Mobile.Server.TestModels;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    public class TableUtilsTests
    {
        public TableUtilsTests()
        {
            TableUtils.PageSize = 50;
        }

        public static TheoryDataCollection<string, int?, int> EffectivePageSize
        {
            get
            {
                return new TheoryDataCollection<string, int?, int>
                {
                    { null, null, 50 },
                    { "$top=10", null, 10 },
                    { "$top=100", null, 50 },
                    { null, 10, 10 },
                    { "$top=10", 15, 10 },
                    { "$top=15", 10, 10 },
                };
            }
        }

        [Fact]
        public void PageSize_Roundtrips()
        {
            TableUtilsWrapper wrapper = new TableUtilsWrapper();
            PropertyAssert.Roundtrips(wrapper, w => w.PageSize, defaultValue: 50, roundtripValue: 100, minLegalValue: 1, illegalLowerValue: 0);

            // reset this static value for future tests
            wrapper.PageSize = 50;
        }

        [Theory]
        [MemberData("EffectivePageSize")]
        public void GetResultSize_ReturnsEffectivePageSize(string top, int? pageSize, int expected)
        {
            // Arrange
            ODataQueryContext context = new ODataQueryContext(EdmModelMock.Create<Movie>("Movies"), typeof(Movie));
            Uri address = new Uri("http://localhost?{0}".FormatInvariant(top));
            ODataQueryOptions query = new ODataQueryOptions(context, new HttpRequestMessage { RequestUri = address });
            ODataQuerySettings settings = new ODataQuerySettings() { PageSize = pageSize };

            // Act
            int actual = TableUtils.GetResultSize(query, settings);

            // Assert
            Assert.Equal(expected, actual);
        }

        private class TableUtilsWrapper
        {
            public int PageSize
            {
                get
                {
                    return TableUtils.PageSize;
                }

                set
                {
                    TableUtils.PageSize = value;
                }
            }
        }
    }
}
