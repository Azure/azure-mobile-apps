// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Web.Http.Tracing;
using Microsoft.Azure.Mobile.Internal;
using Xunit;

namespace Microsoft.Azure.Mobile.Diagnostics
{
    public class TraceLevelHelperTests : EnumHelperTestBase<TraceLevel>
    {
        public TraceLevelHelperTests()
            : base(new TraceLevelHelper(), (TraceLevel)999)
        {
        }

        [Theory]
        [InlineData("verbose", TraceLevel.Debug)]
        [InlineData("info", TraceLevel.Info)]
        [InlineData("information", TraceLevel.Info)]
        [InlineData("warning", TraceLevel.Warn)]
        [InlineData("warn", TraceLevel.Warn)]
        [InlineData("error", TraceLevel.Error)]
        [InlineData("invalid", TraceLevel.Error)]
        [InlineData("", TraceLevel.Error)]
        [InlineData(null, TraceLevel.Error)]
        public void ParseLogLevelSetting_ReturnsExpectedTraceLevel(string input, TraceLevel expectedOutput)
        {
            Assert.Equal(expectedOutput, TraceLevelHelper.ParseLogLevelSetting(input));
        }
    }
}
