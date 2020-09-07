// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;

namespace Microsoft.Zumo.E2EServer.Utils
{
    public static class SqlFuncs
    {
        [DbFunction("SqlServer", "STR")]
        public static string StringConvert(long number)
        {
            return number.ToString();
        }

        [DbFunction("SqlServer", "LTRIM")]
        public static string LTrim(string s)
        {
            return s?.TrimStart();
        }

        public static long LongParse(string s)
        {
            long.TryParse(s, out long ret);
            return ret;
        }
    }
}
