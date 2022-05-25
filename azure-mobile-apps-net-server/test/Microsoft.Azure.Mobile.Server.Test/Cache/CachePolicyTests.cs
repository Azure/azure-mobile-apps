// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Cache
{
    public class CachePolicyTests
    {
        private CachePolicy policy;

        public CachePolicyTests()
        {
            this.policy = new CachePolicy();
        }

        [Fact]
        public void Options_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.policy, p => p.Options, defaultValue: CacheOptions.NoCache, minLegalValue: CacheOptions.NoCache, illegalLowerValue: (CacheOptions)(-1), illegalUpperValue: (CacheOptions)int.MaxValue, roundtripValue: CacheOptions.NoTransform);
        }
    }
}
