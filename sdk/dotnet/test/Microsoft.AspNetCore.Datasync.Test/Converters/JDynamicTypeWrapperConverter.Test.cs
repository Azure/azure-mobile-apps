// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Converters;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.AspNetCore.Datasync.Test.Converters
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class JDynamicTypeWrapperConverter_Tests
    {
        private readonly JsonSerializerSettings _settings;

        public JDynamicTypeWrapperConverter_Tests()
        {
            _settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            _settings.Converters.Add(new JDynamicTypeWrapperConverter());
        }

        [Fact]
        public void JsonConverter_Basics()
        {
            var converter = new JDynamicTypeWrapperConverter();

            Assert.False(converter.CanRead);
            Assert.True(converter.CanWrite);

            Assert.True(converter.CanConvert(typeof(DynamicTypeWrapper)));
            Assert.False(converter.CanConvert(typeof(string)));

            Assert.Throws<ArgumentNullException>(() => converter.CanConvert(null));
        }
    }
}
