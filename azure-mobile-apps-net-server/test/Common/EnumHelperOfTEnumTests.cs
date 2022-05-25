// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using Xunit;

namespace Microsoft.Azure.Mobile.Internal
{
    public class EnumHelperOfTEnumTests
    {
        private TestEnumHelper helper = new TestEnumHelper();

        [Fact]
        public void Validate_Throws_IfValueIsNotInEnum()
        {
            // Act
            ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() => this.helper.Validate((DayOfWeek)999, "day"));

            // Assert
            Assert.Contains("The value must be from the 'DayOfWeek' enumeration.", ex.Message);
        }

        [Fact]
        public void Validate_DoesNotThrowOnValidValue()
        {
            this.helper.Validate(DayOfWeek.Monday, "day");
        }

        private class TestEnumHelper : EnumHelper<DayOfWeek>
        {
            public override bool IsDefined(DayOfWeek value)
            {
                return value == DayOfWeek.Monday
                    || value == DayOfWeek.Tuesday
                    || value == DayOfWeek.Wednesday
                    || value == DayOfWeek.Thursday
                    || value == DayOfWeek.Friday
                    || value == DayOfWeek.Saturday
                    || value == DayOfWeek.Sunday;
            }
        }
    }
}
