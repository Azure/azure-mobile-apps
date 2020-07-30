using Azure.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Azure.Mobile.Client.Test.Helpers
{
    internal static class HttpAssert
    {
        internal static void HeaderIsEqual(string headerName, string headerValue, Request request)
        {
            string actual;
            var isPresent = request.Headers.TryGetValue(headerName, out actual);
            Assert.IsTrue(isPresent);
            Assert.IsNotNull(actual);
            Assert.AreEqual(headerValue, actual);
        }
    }
}
