// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Serialization;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Serialization
{
    [ExcludeFromCodeCoverage]
    public class ServiceSerializer_Tests : BaseTest
    {
        private ServiceSerializer serializer;

        public ServiceSerializer_Tests()
        {
            serializer = new();
        }

        [Fact]
        public void Ctor_CreatesSerializerSettings()
        {
            Assert.NotNull(serializer.SerializerSettings);
        }

        [Fact]
        public void SerializerSettings_CanRoundtrip()
        {
            var settings = new DatasyncSerializerSettings();
            serializer.SerializerSettings = settings;
            Assert.Same(settings, serializer.SerializerSettings);
        }
    }
}
