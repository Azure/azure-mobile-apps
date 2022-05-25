// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using System;
using Xunit;

namespace MobileClient.Tests
{
    public class MobileServiceUser_Tests
    {
        [Fact]
        public void Constructor_NullThrowsException()
            => Assert.Throws<ArgumentNullException>(() => new MobileServiceUser(null));

        [Fact]
        public void Constructor_EmptyThrowsException()
            => Assert.Throws<ArgumentException>(() => new MobileServiceUser(string.Empty));

        [Fact]
        public void Constructor_WhitespaceThrowsException()
            => Assert.Throws<ArgumentException>(() => new MobileServiceUser(" "));

        [Fact]
        public void Constructor_BuildsObject()
            => Assert.Equal("userId", new MobileServiceUser("userId").UserId);
    }
}
