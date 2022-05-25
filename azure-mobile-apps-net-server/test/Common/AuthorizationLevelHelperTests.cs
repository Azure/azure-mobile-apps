// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Microsoft.Azure.Mobile.Internal;

namespace Microsoft.Azure.Mobile.Security
{
    public class AuthorizationLevelHelperTests : EnumHelperTestBase<AuthorizationLevel>
    {
        public AuthorizationLevelHelperTests()
            : base(new AuthorizationLevelHelper(), (AuthorizationLevel)999)
        {
        }
    }
}
