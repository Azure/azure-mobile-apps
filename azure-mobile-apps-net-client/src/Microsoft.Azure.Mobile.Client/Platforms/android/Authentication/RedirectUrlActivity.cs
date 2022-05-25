// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Android.App;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Activity class for the callback from the authentication process.
    /// </summary>
    [Activity(Name = "com.microsoft.windowsazure.mobileservices.authentication.RedirectUrlActivity")]
    public class RedirectUrlActivity : Xamarin.Essentials.WebAuthenticatorCallbackActivity
    {

    }
}
