// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class TemplatePushMessageTests
    {
        [Fact]
        public void Dictionary_IsCaseInsensitive()
        {
            // Arrange
            TemplatePushMessage pushMessage = new TemplatePushMessage();
            pushMessage.Add("key", "value");

            // Act
            string actual = pushMessage["KEY"];

            Assert.Equal("value", actual);
        }
    }
}
