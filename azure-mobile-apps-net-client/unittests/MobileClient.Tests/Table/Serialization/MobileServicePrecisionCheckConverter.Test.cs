// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using System;
using Xunit;

namespace MobileClient.Tests.Table.Serialization
{
    public class MobileServicePrecisionCheckConverter_Test
    {
        private MobileServicePrecisionCheckConverter PrecisionConverter { get; } = new MobileServicePrecisionCheckConverter();

        [Fact]
        public void CanConvertReturnsTrueForDecimalDoubleLong()
        {
            bool canConvert = PrecisionConverter.CanConvert(typeof(decimal));
            Assert.True(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(ulong));
            Assert.True(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(long));
            Assert.True(canConvert);
        }

        [Fact]
        public void CanConvertReturnsFalseForNotDecimalDoubleLong()
        {
            //false
            bool canConvert = PrecisionConverter.CanConvert(typeof(byte));
            Assert.False(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(int));
            Assert.False(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(short));
            Assert.False(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(byte[]));
            Assert.False(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(object));
            Assert.False(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(string));
            Assert.False(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(bool));
            Assert.False(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(DateTime));
            Assert.False(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(DateTimeOffset));
            Assert.False(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(double));
            Assert.False(canConvert);
        }

        [Fact]
        public void CanReadShouldReturnFalse()
        {
            bool canRead = PrecisionConverter.CanRead;

            Assert.False(canRead);
        }

        [Fact]
        public void CanWriteShouldReturnTrue()
        {
            bool canWrite = PrecisionConverter.CanWrite;

            Assert.True(canWrite);
        }
    }
}
