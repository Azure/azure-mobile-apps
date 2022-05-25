// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using System;
using Xunit;

namespace MobileClient.Tests.Table.Serialization
{
    public class MobileServiceconverter_Test
    {
        [Fact]
        public void CanConvertReturnsTrueForDateTimeDateTimeOffset()
        {
            var converter = new MobileServiceIsoDateTimeConverter();
            bool canConvert = converter.CanConvert(typeof(DateTime));
            Assert.True(canConvert);

            canConvert = converter.CanConvert(typeof(DateTimeOffset));
            Assert.True(canConvert);
        }

        [Fact]
        public void CanConvertReturnsFalseForNotDateTimeDateTimeOffset()
        {
            var converter = new MobileServiceIsoDateTimeConverter();
            //false
            bool canConvert = converter.CanConvert(typeof(byte));
            Assert.False(canConvert);

            canConvert = converter.CanConvert(typeof(ulong));
            Assert.False(canConvert);

            canConvert = converter.CanConvert(typeof(int));
            Assert.False(canConvert);

            canConvert = converter.CanConvert(typeof(short));
            Assert.False(canConvert);

            canConvert = converter.CanConvert(typeof(byte[]));
            Assert.False(canConvert);

            canConvert = converter.CanConvert(typeof(object));
            Assert.False(canConvert);

            canConvert = converter.CanConvert(typeof(string));
            Assert.False(canConvert);

            canConvert = converter.CanConvert(typeof(bool));
            Assert.False(canConvert);

            canConvert = converter.CanConvert(typeof(decimal));
            Assert.False(canConvert);

            canConvert = converter.CanConvert(typeof(double));
            Assert.False(canConvert);

            canConvert = converter.CanConvert(typeof(long));
            Assert.False(canConvert);
        }

        [Fact]
        public void CanReadShouldReturnTrue()
        {
            var converter = new MobileServiceIsoDateTimeConverter();
            bool canRead = converter.CanRead;

            Assert.True(canRead);
        }

        [Fact]
        public void CanWriteShouldReturnTrue()
        {
            var converter = new MobileServiceIsoDateTimeConverter();

            bool canWrite = converter.CanWrite;

            Assert.True(canWrite);
        }
    }
}
