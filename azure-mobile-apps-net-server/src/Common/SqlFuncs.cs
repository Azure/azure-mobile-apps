// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Azure.Mobile.Server
{
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Funcs", Justification = "Temporary")]
    public static class SqlFuncs
    {
        private const int DefaultStrLength = 10;

        /// <summary>Returns character data converted from numeric data.</summary>
        /// <returns>The input expression converted to a string.</returns>
        /// <param name="number">A numeric expression.</param>
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Double? number)
        {
            return StringConvert(number, DefaultStrLength);
        }

        /// <summary>Returns character data converted from numeric data.</summary>
        /// <returns>The numeric input expression converted to a string.</returns>
        /// <param name="number">A numeric expression.</param>
        /// <param name="length">The total length of the string. This includes decimal point, sign, digits, and spaces. The default is 10.</param>
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Double? number, Int32? length)
        {
            string format = String.Format(CultureInfo.InvariantCulture, "{{0,{0}}}", length);
            return String.Format(CultureInfo.InvariantCulture, format, number.Value);
        }

#if false
        /// <summary>Returns character data converted from numeric data.</summary>
        /// <returns>The input expression converted to a string.</returns>
        /// <param name="number">A numeric expression.</param>
        /// <param name="length">The total length of the string. This includes decimal point, sign, digits, and spaces. The default is 10.</param>
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Decimal? number, Int32? length)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns character data converted from numeric data.</summary>
        /// <returns>The numeric input expression converted to a string.</returns>
        /// <param name="number">A numeric expression.</param>
        /// <param name="length">The total length of the string. This includes decimal point, sign, digits, and spaces. The default is 10.</param>
        /// <param name="decimalArg">The number of places to the right of the decimal point.  decimal  must be less than or equal to 16. If  decimal  is more than 16 then the result is truncated to sixteen places to the right of the decimal point.</param>
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Double? number, Int32? length, Int32? decimalArg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns character data converted from numeric data.</summary>
        /// <returns>The input expression converted to a string.</returns>
        /// <param name="number">A numeric expression.</param>
        /// <param name="length">The total length of the string. This includes decimal point, sign, digits, and spaces. The default is 10.</param>
        /// <param name="decimalArg">The number of places to the right of the decimal point.  decimal  must be less than or equal to 16. If  decimal  is more than 16 then the result is truncated to sixteen places to the right of the decimal point.</param>
        [DbFunction("SqlServer", "STR")]
        public static String StringConvert(Decimal? number, Int32? length, Int32? decimalArg)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        private static int GetEffectiveLength(Int32? length)
        {
            return (length != null && length.HasValue) ? length.Value : 10;
        }
#endif
    }
}