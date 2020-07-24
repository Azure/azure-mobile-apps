using Azure.Mobile.Server.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http.Headers;

namespace Azure.Mobile.Server.Test.Helpers
{
    public static class HttpAssert
    {
        public static void AreEqual(byte[] expected, EntityTagHeaderValue actual)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            Assert.AreEqual(ETag.FromByteArray(expected), actual.Tag);
        }

        public static void Match(DateTimeOffset expected, DateTimeOffset? actual)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.ToString("r"), (actual ?? DateTimeOffset.MinValue).ToString("r"));
        }

        public static void IsWithin(DateTimeOffset expected, DateTimeOffset actual, long timeDiff)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            var actualDiff = expected.Subtract(actual).TotalMilliseconds;
            Assert.IsTrue(Math.Abs(actualDiff) < timeDiff);
        }
    }
}
