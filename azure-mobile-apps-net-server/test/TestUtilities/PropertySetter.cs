// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace TestUtilities
{
    /// <summary>
    /// Determines how the property under test is expected to behave.
    /// </summary>
    public enum PropertySetter
    {
        // It is not allowed to set the property to null
        NullThrows = 0,

        // Setting the property to null causes it to get reinitialized to some non-null value
        NullSetsDefault,

        // Null roundtrips like any other value
        NullRoundtrips
    }
}
