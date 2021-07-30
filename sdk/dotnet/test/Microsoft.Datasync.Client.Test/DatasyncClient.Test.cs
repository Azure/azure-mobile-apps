// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Authentication;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage]
    public class DatasyncClient_Tests : BaseTest
    {
        private class IntDatasyncClient : DatasyncClient
        {
            public IntDatasyncClient(Uri endpoint, DatasyncClientOptions options = null) : base(endpoint, options)
            {
            }

            public void IntDispose(bool disposing) => Dispose(disposing);
            public void IntDispose() => Dispose();
        }

        private static readonly AuthenticationToken basicToken = new()
        {
            DisplayName = "John Smith",
            ExpiresOn = DateTimeOffset.Now.AddMinutes(5),
            Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJkYXRhc3luYy1mcmFtZXdvcmstdGVzdHMiLCJpYXQiOjE2Mjc2NTk4MTMsImV4cCI6MTY1OTE5NTgxMywiYXVkIjoiZGF0YXN5bmMtZnJhbWV3b3JrLXRlc3RzLmNvbnRvc28uY29tIiwic3ViIjoidGhlX2RvY3RvckBjb250b3NvLmNvbSIsIkdpdmVuTmFtZSI6IkpvaG4iLCJTdXJuYW1lIjoiU21pdGgiLCJFbWFpbCI6InRoZV9kb2N0b3JAY29udG9zby5jb20ifQ.6Sm-ghJBKLB1vC4NuCqYKwL1mbRnJ9ziSHQT5VlNVEY",
            UserId = "the_doctor"
        };

        private static readonly Func<Task<AuthenticationToken>> requestor = () => Task.FromResult(basicToken);
        private readonly AuthenticationProvider authProvider = new GenericAuthenticationProvider(requestor, "X-ZUMO-AUTH");

        #region Ctor
        [Fact]
        [Trait("Method", "Ctor(string)")]
        public void CtorString_Null_Throws()
        {
            const string endpoint = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncClient(endpoint));
        }

        [Theory, ClassData(typeof(TestCases.Invalid_Endpoints))]
        [Trait("Method", "Ctor(string)")]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Test case does not check for normalization")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Test case does not check for normalization")]
        public void CtorString_Invalid_Throws(string endpoint, bool isRelative)
        {
            Assert.Throws<UriFormatException>(() => new DatasyncClient(endpoint));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(string)")]
        public void CtorString_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var client = new DatasyncClient(endpoint);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(string,AuthenticationProvider)")]
        public void CtorStringAuth_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var client = new DatasyncClient(endpoint, authProvider);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Ctor(Uri)")]
        public void CtorUri_Null_Throws()
        {
            Uri endpoint = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncClient(endpoint));
        }

        [Theory, ClassData(typeof(TestCases.Invalid_Endpoints))]
        [Trait("Method", "Ctor(Uri)")]
        public void CtorUri_Invalid_Throws(string endpoint, bool isRelative)
        {
            Assert.Throws<UriFormatException>(() => new DatasyncClient(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint)));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(Uri)")]
        public void CtorUri_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var client = new DatasyncClient(new Uri(endpoint));
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(Uri,AuthenticationProvider)")]
        public void CtorUriAuth_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var client = new DatasyncClient(new Uri(endpoint), authProvider);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.NotNull(client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Ctor(string,DatasyncClientOptions)")]
        public void CtorStringOptions_Null_Throws()
        {
            const string endpoint = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncClient(endpoint));
        }

        [Theory, ClassData(typeof(TestCases.Invalid_Endpoints))]
        [Trait("Method", "Ctor(string,DatasyncClientOptions)")]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Test case does not check for normalization")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Test case does not check for normalization")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Test case does not check for normalization")]
        public void CtorStringOptions_Invalid_Throws(string endpoint, bool isRelative)
        {
            Assert.Throws<UriFormatException>(() => new DatasyncClient(endpoint));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(string,DatasyncClientOptions)")]
        public void CtorStringOptions_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncClient(endpoint, options);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(string,AuthenticationProvider,DatasyncClientOptions)")]
        public void CtorStringAuthOptions_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncClient(endpoint, authProvider, options);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Ctor(Uri,DatasyncClientOptions)")]
        public void CtorUriOptions_Null_Throws()
        {
            Uri endpoint = null;
            Assert.Throws<ArgumentNullException>(() => new DatasyncClient(endpoint));
        }

        [Theory, ClassData(typeof(TestCases.Invalid_Endpoints))]
        [Trait("Method", "Ctor(Uri,DatasyncClientOptions)")]
        public void CtorUriOptions_Invalid_Throws(string endpoint, bool isRelative)
        {
            Assert.Throws<UriFormatException>(() => new DatasyncClient(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint)));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(Uri,DatasyncClientOptions)")]
        public void CtorUriOptions_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncClient(new Uri(endpoint), options);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor(Uri,AuthenticationProvider,DatasyncClientOptions)")]
        public void CtorUriAuthOptions_Valid_SetsEndpoint(string endpoint, string expected)
        {
            var options = new DatasyncClientOptions();
            var client = new DatasyncClient(new Uri(endpoint), authProvider, options);
            Assert.Equal(expected, client.Endpoint.ToString());
            Assert.Same(options, client.ClientOptions);
            Assert.NotNull(client.HttpClient);
        }
        #endregion

        #region GetTable<T>()
        [Fact]
        [Trait("Method", "GetTable")]
        public void GetTable_ProducesTable_WithNormalOptions()
        {
            var client = new DatasyncClient(Endpoint);
            var table = client.GetTable<ClientMovie>();
            Assert.IsAssignableFrom<IDatasyncTable<ClientMovie>>(table);
            var expectedUri = new Uri(Endpoint, "tables/clientmovie/");
            Assert.Equal(expectedUri, table.Endpoint);
            Assert.Same(client.ClientOptions, table.ClientOptions);
        }

        [Fact]
        [Trait("Method", "GetTable")]
        public void GetTable_ProducesTable_WithTablesPrefix()
        {
            var options = new DatasyncClientOptions { TablesPrefix = "/api" };
            var client = new DatasyncClient(Endpoint, options);
            var table = client.GetTable<ClientMovie>();
            Assert.IsAssignableFrom<IDatasyncTable<ClientMovie>>(table);
            var expectedUri = new Uri(Endpoint, "api/clientmovie/");
            Assert.Equal(expectedUri, table.Endpoint);
            Assert.Same(options, table.ClientOptions);
        }
        #endregion

        #region GetTable<T>(string)
        [Fact]
        [Trait("Method", "GetTable")]
        public void GetTable_stringTable_ProducesTable()
        {
            var client = new DatasyncClient(Endpoint);
            var table = client.GetTable<ClientMovie>("movies");
            Assert.IsAssignableFrom<IDatasyncTable<ClientMovie>>(table);
            var expectedUri = new Uri(Endpoint, "tables/movies/");
            Assert.Equal(expectedUri, table.Endpoint);
            Assert.Same(client.ClientOptions, table.ClientOptions);
        }

        [Fact]
        [Trait("Method", "GetTable")]
        public void GetTable_stringTable_ProducesTable_WithTablesPrefix()
        {
            var options = new DatasyncClientOptions { TablesPrefix = "/api" };
            var client = new DatasyncClient(Endpoint, options);
            var table = client.GetTable<ClientMovie>("movies");
            Assert.IsAssignableFrom<IDatasyncTable<ClientMovie>>(table);
            var expectedUri = new Uri(Endpoint, "api/movies/");
            Assert.Equal(expectedUri, table.Endpoint);
            Assert.Same(options, table.ClientOptions);
        }

        [Fact]
        [Trait("Method", "GetTable")]
        public void GetTable_stringRelativeUri_ProducesTable()
        {
            var client = new DatasyncClient(Endpoint);
            var table = client.GetTable<ClientMovie>("/api/movies");
            Assert.IsAssignableFrom<IDatasyncTable<ClientMovie>>(table);
            var expectedUri = new Uri(Endpoint, "api/movies/");
            Assert.Equal(expectedUri, table.Endpoint);
            Assert.Same(client.ClientOptions, table.ClientOptions);
        }

        [Fact]
        [Trait("Method", "GetTable")]
        public void GetTable_stringRelativeUri_ProducesTable_WithTablesPrefix()
        {
            var options = new DatasyncClientOptions { TablesPrefix = "/api" };
            var client = new DatasyncClient(Endpoint, options);
            var table = client.GetTable<ClientMovie>("/foo/movies");
            Assert.IsAssignableFrom<IDatasyncTable<ClientMovie>>(table);
            var expectedUri = new Uri(Endpoint, "foo/movies/");
            Assert.Equal(expectedUri, table.Endpoint);
            Assert.Same(options, table.ClientOptions);
        }
        #endregion

        #region IDisposable
        [Fact]
        [Trait("Method", "Dispose(bool)")]
        public void Dispose_True_Disposes()
        {
            var client = new IntDatasyncClient(Endpoint, ClientOptions);
            Assert.NotNull(client.HttpClient);
            client.IntDispose(true);
            Assert.Null(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Dispose(bool)")]
        public void Dispose_False_Disposes()
        {
            var client = new IntDatasyncClient(Endpoint, ClientOptions);
            Assert.NotNull(client.HttpClient);
            client.IntDispose(false);
            Assert.NotNull(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Dispose")]
        public void Dispose_Disposes()
        {
            var client = new IntDatasyncClient(Endpoint, ClientOptions);
            Assert.NotNull(client.HttpClient);
            client.IntDispose();
            Assert.Null(client.HttpClient);
        }
        #endregion
    }
}
