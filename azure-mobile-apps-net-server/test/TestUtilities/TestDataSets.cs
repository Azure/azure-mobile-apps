// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;

namespace TestUtilities
{
    public static class TestDataSets
    {
        public static TheoryDataCollection<object> MixedInstancesDataSet
        {
            get
            {
                return new TheoryDataCollection<object>
                {
                    "test",
                    new string[] { "A", "B", "C" },
                    1,
                    new List<int> { 1, 2, 3 },
                    1.0,
                    Guid.NewGuid(),
                    new Uri("http://localhost")
                };
            }
        }

        public static TheoryDataCollection<bool> BoolDataSet
        {
            get
            {
                return new TheoryDataCollection<bool> { true, false };
            }
        }

        public static TheoryDataCollection<string> EmptyOrWhiteSpaceStringDataSet
        {
            get
            {
                return new TheoryDataCollection<string> 
                { 
                    string.Empty,
                    "   ",
                    "\t",
                    "\u2000",
                    "\u1680",
                    "\u2028",
                    "\u2029",
                };
            }
        }
    }
}
