// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage]
    public class DatasyncClient_Tests : BaseTest
    {
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
        }
    }
}
