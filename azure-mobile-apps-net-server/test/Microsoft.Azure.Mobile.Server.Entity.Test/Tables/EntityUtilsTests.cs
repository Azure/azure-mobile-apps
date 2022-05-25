// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    public class EntityUtilsTests
    {
        public static TheoryDataCollection<DbEntityValidationException, string> ValidationExceptions
        {
            get
            {
                return new TheoryDataCollection<DbEntityValidationException, string>
                {
                    { new DbEntityValidationException("exception message"), "exception message" },
                };
            }
        }

        public static TheoryDataCollection<HttpRequestMessage, HttpStatusCode> ConflictData
        {
            get
            {
                HttpRequestMessage noIfMatch = new HttpRequestMessage();

                HttpRequestMessage ifMatchAny = new HttpRequestMessage();
                ifMatchAny.Headers.IfMatch.Add(EntityTagHeaderValue.Any);

                HttpRequestMessage ifMatch = new HttpRequestMessage();
                ifMatch.Headers.IfMatch.Add(EntityTagHeaderValue.Parse("\"ABCDEF\""));

                return new TheoryDataCollection<HttpRequestMessage, HttpStatusCode>
                {
                    { noIfMatch, HttpStatusCode.Conflict },
                    { ifMatchAny, HttpStatusCode.PreconditionFailed },
                    { ifMatch, HttpStatusCode.PreconditionFailed },
                };
            }
        }

        [Theory]
        [MemberData("ValidationExceptions")]
        public void GetValidationDescription_HandlesValidationErrors(DbEntityValidationException ex, string expected)
        {
            // Act
            string actual = EntityUtils.GetValidationDescription(ex);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("ConflictData")]
        public void GetConflictStatusCode_ReturnsExpectedStatus(HttpRequestMessage request, HttpStatusCode expected)
        {
            // Act
            HttpStatusCode actual = EntityUtils.GetConflictStatusCode(request);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetUnderlyingSqlException_ReturnsInnerSqlException()
        {
            SqlException sqlException = this.CreateSqlException();

            DbUpdateConcurrencyException updateException = new DbUpdateConcurrencyException("Error!", new DbUpdateException("Error!", sqlException));

            Assert.Same(sqlException, EntityUtils.GetUnderlyingSqlException(updateException));
        }

        // This is necessary, since you can't new up a SqlException for testing
        private SqlException CreateSqlException()
        {
            SqlException exception = null;
            try
            {
                SqlConnection conn = new SqlConnection(@"Data Source=.;Database=INVALID;Connection Timeout=1");
                conn.Open();
            }
            catch (SqlException ex)
            {
                exception = ex;
            }

            return exception;
        }
    }
}
