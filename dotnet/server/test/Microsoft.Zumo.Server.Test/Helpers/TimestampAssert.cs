// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Microsoft.Zumo.Server.Test.Helpers
{
    /// <summary>
    /// Assertions for handling DateTimeOffset timestamps.
    /// </summary>
    public static class TimestampAssert
    {
        /// <summary>
        /// Helper method to assert when the time stamp is not within a certain number of ms.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="ms"></param>
        public static void AreClose(DateTimeOffset expected, DateTimeOffset actual, long ms = 500)
        {
            var difference = Math.Abs(expected.Subtract(actual).TotalMilliseconds);
            Assert.IsTrue(difference < ms, $"Expected <{expected}> is not within {ms}ms of Actual <{actual}>");
        }
    }
}
