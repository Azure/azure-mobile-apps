// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var _ = require('../Utilities/Extensions'),
    easyAuthRedirectUriKey = 'post_login_redirect_url';

exports.supportsCurrentRuntime = function () {
    /// <summary>
    /// Determines whether or not this login UI is usable in the current runtime.
    /// </summary>

    return isWebAuthBrokerAvailable();
};

exports.login = function (startUri, endUri, callback) {
    /// <summary>
    /// Displays the login UI and calls back on completion
    /// </summary>

    // Define shortcuts for namespaces
    var windowsWebAuthBroker = Windows.Security.Authentication.Web.WebAuthenticationBroker;
    var noneWebAuthOptions = Windows.Security.Authentication.Web.WebAuthenticationOptions.none;
    var successWebAuthStatus = Windows.Security.Authentication.Web.WebAuthenticationStatus.success;
    var activationKindWebAuthContinuation = Windows.ApplicationModel.Activation.ActivationKind.webAuthenticationBrokerContinuation;

    var webAuthBrokerSuccessCallback = null;
    var webAuthBrokerErrorCallback = null;
    var webAuthBrokerContinuationCallback = null;


    // define callbacks for WebAuthenticationBroker
    webAuthBrokerSuccessCallback = function (result) {
        var error = null;
        var token = null;

        if (result.responseStatus !== successWebAuthStatus) {
            error = result;
        }
        else {
            var callbackEndUri = result.responseData;
            var tokenAsJson = null;
            var i = callbackEndUri.indexOf('#token=');
            if (i > 0) {
                tokenAsJson = decodeURIComponent(callbackEndUri.substring(i + 7));
            }
            else {
                i = callbackEndUri.indexOf('#error=');
                if (i > 0) {
                    error = new Error(decodeURIComponent(callbackEndUri.substring(i + 7)));
                }
            }

            if (tokenAsJson !== null) {
                try {
                    token = JSON.parse(tokenAsJson);
                }
                catch (e) {
                    error = e;
                }
            }
        }

        callback(error, token);
    };
    webAuthBrokerErrorCallback = function (error) {
        callback(error, null);
    };
    // Continuation callback is used when we're running on WindowsPhone which uses 
    // AuthenticateAndContinue method instead of AuthenticateAsync, which uses different async model
    // Continuation callback need to be assigned to Application's 'activated' event.
    webAuthBrokerContinuationCallback = function (activationArgs) {
        if (activationArgs.detail.kind === activationKindWebAuthContinuation) {
            var result = activationArgs.detail.webAuthenticationResult;
            if (result.responseStatus == successWebAuthStatus) {
                webAuthBrokerSuccessCallback(result);
            } else {
                webAuthBrokerErrorCallback(result);
            }
            WinJS.Application.removeEventListener('activated', webAuthBrokerContinuationCallback);
        }
    };

    // If no endURI was given, we construct the startUri with a redirect parameter 
    // pointing to the app SID for single sign on.
    // Single sign-on requires that the application's Package SID 
    // be registered with the Microsoft Azure Mobile Service, but it provides a better 
    // experience as HTTP cookies are supported so that users do not have to
    // login in everytime the application is launched.
    if (endUri) {
        endUri = new Windows.Foundation.Uri(endUri);
    } else {
        var ssoQueryParameter = {},
            redirectUri = windowsWebAuthBroker.getCurrentApplicationCallbackUri().absoluteUri;

        ssoQueryParameter[easyAuthRedirectUriKey] = redirectUri;
        startUri = _.url.combinePathAndQuery(startUri, _.url.getQueryString(ssoQueryParameter));
    }
    
    startUri = new Windows.Foundation.Uri(startUri);
    
    // If authenticateAndContinue method is available, we should use it instead of authenticateAsync.
    // In the event that it exists, but fails (which is the case with Win 10), we fallback to authenticateAsync.
    var isLoginWindowLaunched;
    try {
        WinJS.Application.addEventListener('activated', webAuthBrokerContinuationCallback, true);
        windowsWebAuthBroker.authenticateAndContinue(startUri, endUri);

        isLoginWindowLaunched = true;
    } catch (ex) {
        WinJS.Application.removeEventListener('activated', webAuthBrokerContinuationCallback);
    }

    if (!isLoginWindowLaunched) {
        windowsWebAuthBroker.authenticateAsync(noneWebAuthOptions, startUri, endUri)
        .done(webAuthBrokerSuccessCallback, webAuthBrokerErrorCallback);
    }
};

function isWebAuthBrokerAvailable() {
    // If running on windows8/8.1 or Windows Phone returns true, otherwise false
    return !!(window.Windows &&
        window.Windows.Security &&
        window.Windows.Security.Authentication &&
        window.Windows.Security.Authentication.Web &&
        window.Windows.Security.Authentication.Web.WebAuthenticationBroker);
}
