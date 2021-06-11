/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */

package com.microsoft.windowsazure.mobileservices.authentication;

import java.util.HashMap;

/**
 * Class for passing login state between {@link CustomTabsLoginActivity} and {@link CustomTabsIntermediateActivity}
 */
public class CustomTabsLoginState {

    /**
     * Url scheme used in the redirect url
     */
    private String mUriScheme;

    /**
     * Code verifier used in Proof Key of Code Exchange (PKCE) authentication flow
     */
    private String mCodeVerifier;

    /**
     * Mobile Service authentication provider
     */
    private String mAuthenticationProvider;

    /**
     * Mobile Service app Url
     */
    private String mAppUrl;

    /**
     * Prefix for login endpoints. If not set, default to .auth/login
     */
    private String mLoginUriPrefix;

    /**
     * Alternate Host URI for login
     */
    private String mAlternateLoginHost;

    /**
     * Extra login parameters
     */
    private HashMap<String, String> mLoginParameters;

    public CustomTabsLoginState(String urlScheme, String codeVerifier, String authenticationProvider, String appUrl, String loginUriPrefix, String alternateLoginHost, HashMap<String, String> parameters) {
        this.mUriScheme = urlScheme;
        this.mCodeVerifier = codeVerifier;
        this.mAuthenticationProvider = authenticationProvider;
        this.mAppUrl = appUrl;
        this.mLoginUriPrefix = loginUriPrefix;
        this.mAlternateLoginHost = alternateLoginHost;
        this.mLoginParameters = parameters;
    }

    public String getUriScheme() {
        return mUriScheme;
    }

    public String getCodeVerifier() {
        return mCodeVerifier;
    }

    public String getAuthenticationProvider() {
        return mAuthenticationProvider;
    }

    public String getAppUrl() {
        return mAppUrl;
    }

    public String getLoginUriPrefix() {
        return mLoginUriPrefix;
    }

    public String getAlternateLoginHost() {
        return mAlternateLoginHost;
    }

    public HashMap<String, String> getLoginParameters() {
        return mLoginParameters;
    }
}
