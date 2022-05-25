// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Microsoft.Azure.Mobile.Internal;

namespace Microsoft.Azure.Mobile.Server.Cache
{
    public class CacheOptionsHelperTests : EnumHelperTestBase<CacheOptions>
    {
        public CacheOptionsHelperTests()
            : base(new CacheOptionsHelper(), (CacheOptions)(-1))
        {
        }
    }
}
