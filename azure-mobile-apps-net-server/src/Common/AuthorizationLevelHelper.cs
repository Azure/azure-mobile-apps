// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace Microsoft.Azure.Mobile.Security
{
    /// <summary>
    /// Utility class used to validate <see cref="AuthorizationLevel" /> enumeration values./>
    /// </summary>
    internal class AuthorizationLevelHelper : EnumHelper<AuthorizationLevel>
    {
        private static readonly AuthorizationLevelHelper HelperInstance = new AuthorizationLevelHelper();

        public static AuthorizationLevelHelper Instance
        {
            get
            {
                return HelperInstance;
            }
        }

        public override bool IsDefined(AuthorizationLevel value)
        {
            return value == AuthorizationLevel.Admin
                || value == AuthorizationLevel.User
                || value == AuthorizationLevel.Application
                || value == AuthorizationLevel.Anonymous;
        }
    }
}
