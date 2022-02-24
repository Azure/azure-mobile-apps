// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Serialization
{
    [ExcludeFromCodeCoverage]
    public class DatasyncIsoDateTimeConverter_Tests : BaseTest
    {
        private DatasyncIsoDateTimeConverter Converter { get; }

        public DatasyncIsoDateTimeConverter_Tests() : base()
        {
            Converter = new DatasyncIsoDateTimeConverter();
        }

        [Fact]
        [Trait("Method", "ReadJson")]
        public void ReadJson_CanReadDateTime()
        {
            throw new NotImplementedException();
        }

        [Fact]
        [Trait("Method", "ReadJson")]
        public void ReadJson_CanReadDateTimeOffset()
        {

        }

        [Fact]
        [Trait("Method", "WriteJson")]
        public void WriteJson_CanWriteDateTime()
        {

        }

        [Fact]
        [Trait("Method", "WriteJson")]
        public void WriteJson_CanWriteDateTimeOffset()
        {

        }
    }
}
