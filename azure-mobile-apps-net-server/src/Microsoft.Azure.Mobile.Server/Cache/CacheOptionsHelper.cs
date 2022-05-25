// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace Microsoft.Azure.Mobile.Server.Cache
{
    /// <summary>
    /// Utility class used to validate <see cref="CacheOptions" /> enumeration values./>
    /// </summary>
    internal class CacheOptionsHelper : EnumHelper<CacheOptions>
    {
        private static readonly CacheOptionsHelper HelperInstance = new CacheOptionsHelper();

        public static CacheOptionsHelper Instance
        {
            get
            {
                return HelperInstance;
            }
        }

        public override bool IsDefined(CacheOptions value)
        {
            return CacheOptions.None <= value && value <= (CacheOptions.NoCache | CacheOptions.NoStore | CacheOptions.MustRevalidate | CacheOptions.ProxyRevalidate | CacheOptions.NoTransform | CacheOptions.Private | CacheOptions.Public);
        }
    }
}
