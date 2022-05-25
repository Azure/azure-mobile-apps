// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using Microsoft.Azure.Mobile.Server.Properties;

namespace Microsoft.Azure.Mobile
{
    /// <summary>
    /// Defines a helper class for evaluating whether an enumeration value is valid or invalid. 
    /// </summary>
    /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
    internal abstract class EnumHelper<TEnum>
        where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        protected EnumHelper()
        {
        }

        /// <summary>
        /// Indicates whether the given value is defined in the enumeration.
        /// </summary>
        /// <param name="value">The parameter to validate.</param>
        /// <returns><c>true</c> if value is valid; false otherwise.</returns>
        public abstract bool IsDefined(TEnum value);

        /// <summary>
        /// Verifies that a enumeration value is valid and if not throws an <see cref="ArgumentOutOfRangeException"/>
        /// exception.
        /// </summary>
        /// <param name="value">Value to validate.</param>
        /// <param name="parameterName">The name of the parameter holding the value.</param>
        public void Validate(TEnum value, string parameterName)
        {
            if (!this.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, value, CommonResources.ArgumentOutOfRange_InvalidEnum.FormatForUser(typeof(TEnum).Name));
            }
        }
    }
}
