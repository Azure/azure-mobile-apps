// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using Xunit;

namespace Microsoft.Azure.Mobile.Internal
{
    public abstract class EnumHelperTestBase<TEnum>
        where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        private EnumHelper<TEnum> helper;
        private Array values;
        private TEnum invalidValue;

        protected EnumHelperTestBase(object helper, TEnum invalidValue)
        {
            this.helper = (EnumHelper<TEnum>)helper;
            this.values = Enum.GetValues(typeof(TEnum));
            this.invalidValue = invalidValue;
        }

        [Fact]
        public void IsDefined_ReturnsTrueOnValidValue()
        {
            foreach (var v in this.values)
            {
                Assert.True(this.helper.IsDefined((TEnum)v));
            }
        }

        [Fact]
        public void IsDefined_ReturnsFalseOnInvalidValue()
        {
            Assert.False(this.helper.IsDefined(this.invalidValue));
        }

        [Fact]
        public void Validate_DoesNotThrowOnValidValue()
        {
            foreach (TEnum value in this.values)
            {
                this.helper.Validate(value, "value");
            }
        }

        [Fact]
        public void Validate_Throws_IfValueIsNotInEnum()
        {
            ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() => this.helper.Validate(this.invalidValue, "value"));
            Assert.Contains("The value must be from the '{0}' enumeration.".FormatForUser(typeof(TEnum).Name), ex.Message);
        }
    }
}
