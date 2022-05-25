//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Extension methods for UI-based login.
    /// </summary>
    public static partial class MobileServiceClientExtensions
    {
        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">The MobileServiceClient instance to login with</param>
        /// <param name="viewController">UIViewController used to display modal login UI on iPhone/iPods.</param>
        /// <param name="provider">Authentication provider to use.</param>
        /// <param name="uriScheme">The URL scheme of the application.</param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, UIViewController viewController, MobileServiceAuthenticationProvider provider, string uriScheme)
            => LoginAsync(client, viewController, provider, uriScheme, parameters: null);

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">The MobileServiceClient instance to login with</param>
        /// <param name="viewController">UIViewController used to display modal login UI on iPhone/iPods.</param>
        /// <param name="provider">Authentication provider to use.</param>
        /// <param name="uriScheme">The URL scheme of the application.</param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, UIViewController viewController, MobileServiceAuthenticationProvider provider, string uriScheme, IDictionary<string, string> parameters)
            => LoginAsync(client, default, viewController, provider.ToString(), uriScheme, parameters);

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">The MobileServiceClient instance to login with</param>
        /// <param name="viewController">UIViewController used to display modal login UI on iPhone/iPods.</param>
        /// <param name="provider">Authentication provider to use.</param>
        /// <param name="uriScheme">The URL scheme of the application.</param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, UIViewController viewController, string provider, string uriScheme)
            => LoginAsync(client, viewController, provider, uriScheme, parameters: null);

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">The MobileServiceClient instance to login with</param>
        /// <param name="viewController">UIViewController used to display modal login UI on iPhone/iPods.</param>
        /// <param name="provider">Authentication provider to use.</param>
        /// <param name="uriScheme">The URL scheme of the application.</param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, UIViewController viewController, string provider, string uriScheme, IDictionary<string, string> parameters)
            => LoginAsync(client, default, viewController, provider, uriScheme, parameters);

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">The MobileServiceClient instance to login with</param>
        /// <param name="rectangle">The area in <paramref name="view"/> to anchor to.</param>
        /// <param name="view">UIView used to display a popover from on iPad.</param>
        /// <param name="provider">Authentication provider to use.</param>
        /// <param name="uriScheme">The URL scheme of the application.</param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, RectangleF rectangle, UIView view, MobileServiceAuthenticationProvider provider, string uriScheme)
            => LoginAsync(client, rectangle, view, provider, uriScheme, parameters: null);

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">The MobileServiceClient instance to login with</param>
        /// <param name="rectangle">The area in <paramref name="view"/> to anchor to.</param>
        /// <param name="view">UIView used to display a popover from on iPad.</param>
        /// <param name="provider">Authentication provider to use.</param>
        /// <param name="uriScheme">The URL scheme of the application.</param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, RectangleF rectangle, UIView view, MobileServiceAuthenticationProvider provider, string uriScheme, IDictionary<string, string> parameters)
            => LoginAsync(client, rectangle, (object)view, provider.ToString(), uriScheme, parameters);

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">The MobileServiceClient instance to login with</param>
        /// <param name="rectangle">The area in <paramref name="view"/> to anchor to.</param>
        /// <param name="view">UIView used to display a popover from on iPad.</param>
        /// <param name="provider">Authentication provider to use.</param>
        /// <param name="uriScheme">The URL scheme of the application.</param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, RectangleF rectangle, UIView view, string provider, string uriScheme)
            => LoginAsync(client, rectangle, view, provider, uriScheme, parameters: null);

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">The MobileServiceClient instance to login with</param>
        /// <param name="rectangle">The area in <paramref name="view"/> to anchor to.</param>
        /// <param name="view">UIView used to display a popover from on iPad.</param>
        /// <param name="provider">Authentication provider to use.</param>
        /// <param name="uriScheme">The URL scheme of the application.</param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, RectangleF rectangle, UIView view, string provider, string uriScheme, IDictionary<string, string> parameters)
            => LoginAsync(client, rectangle, (object)view, provider, uriScheme, parameters);

#pragma warning disable IDE0060 // Remove unused parameter
        internal static Task<MobileServiceUser> LoginAsync(MobileServiceClient client, RectangleF rect, object view, string provider, string uriScheme, IDictionary<string, string> parameters)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var auth = new MobileServiceUIAuthentication(client, provider, uriScheme, parameters);
            return auth.LoginAsync();
        }

        /// <summary>
        /// Resume login process with the specified URL.
        /// </summary>
#pragma warning disable IDE0060 // Remove unused parameter
        public static bool ResumeWithURL(this MobileServiceClient client,
#pragma warning restore IDE0060 // Remove unused parameter
            UIApplication app, NSUrl url, NSDictionary options)
        {
            return Xamarin.Essentials.Platform.OpenUrl(app, url, options);
        }

        /// <summary>
        /// Extension method to get a <see cref="Push"/> object made from an existing <see cref="MobileServiceClient"/>.
        /// </summary>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> to create with.
        /// </param>
        /// <returns>
        /// The <see cref="Push"/> object used for registering for notifications.
        /// </returns>
        public static Push GetPush(this IMobileServiceClient client) => new Push(client);
    }
}
