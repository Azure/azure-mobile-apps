/**
 * Copyright (c) Microsoft Open Technologies, Inc.
 * All Rights Reserved
 * Apache 2.0 License
 * <p/>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p/>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p/>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * <p/>
 * See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 * <p/>
 * UriHelper.java
 * <p/>
 * UriHelper.java
 * <p/>
 * UriHelper.java
 * <p/>
 * UriHelper.java
 * <p/>
 * UriHelper.java
 */

/**
 * UriHelper.java
 */
package com.microsoft.windowsazure.mobileservices.authentication;

import java.io.UnsupportedEncodingException;
import java.net.MalformedURLException;
import java.net.URL;
import java.net.URLEncoder;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;

class UriHelper {
    /**
     * Normalizes the parameters to add it to the Url
     *
     * @param parameters list of the parameters.
     * @return the parameters to add to the url.
     */
    public static String normalizeParameters(HashMap<String, String> parameters) {

        String result = "";

        if (parameters != null && parameters.size() > 0) {
            for (Map.Entry<String, String> parameter : parameters.entrySet()) {

                if (result.isEmpty()) {
                    result = "?";
                } else {
                    result += "&";
                }

                result += parameter.getKey() + "=" + parameter.getValue();
            }
        }

        return result;
    }

    /* Extracts the host portion of the URL and creates a URL object using the host
            @param appUrl The URL to extract hostname from
            @return a new URL object using only the Host section of the URL.
    */
    public static URL createHostOnlyUrl(URL appUrl) {
        try {
            appUrl = new URL(appUrl.getProtocol(), appUrl.getHost(), appUrl.getPort(), "/");
        } catch (MalformedURLException mex) {
        }
        return appUrl;
    }

    private static final char Slash = '/';

    /**
     * Concatenates two URI path segments into a single path and ensures that there is not an extra forward-slash.
     @param path1 The first path.
     @param path2 The second path.
     @return Contantenated path.
     The concatenated URI path and query string.
     */
    public static String CombinePath(String path1, String path2) {
        if (path1.length() == 0) {
            return path1;
        }

        if (path2.length() == 0) {
            return path1;
        }

        //trim all the trailing slash
        path1 = path1.replaceAll("[" + Slash + "]+$", "");

        //trim all the starting slash
        path2 = path2.replaceAll("^[" + Slash + "]+", "");
        return String.format("%s%c%s", path1, Slash, path2);
    }

    /**
     * Normalizes the provider name to match the value required in the mobile
     * service REST API. For example, WindowsAzureActiveDirectory needs to be
     * sent as /login/aad.
     *
     * @param provider the name of the authentication provider.
     * @return the normalized provider name.
     */
    public static String normalizeProvider(String provider) {
        if (provider == null || provider.length() == 0) {
            throw new IllegalArgumentException("provider cannot be null or empty");
        }

        provider = provider.toLowerCase(Locale.getDefault());
        if (provider.equalsIgnoreCase(MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory.toString())) {
            provider = "aad";
        }

        return provider;
    }

    public static String normalizeAndUrlEncodeParameters(HashMap<String, String> parameters, String encoding) {
        String result = "";

        if (parameters != null && parameters.size() > 0) {
            for (Map.Entry<String, String> parameter : parameters.entrySet()) {

                if (result.isEmpty()) {
                    result = "?";
                } else {
                    result += "&";
                }

                try {
                    result += URLEncoder.encode(parameter.getKey(), encoding) + "=" + URLEncoder.encode(parameter.getValue(), encoding);
                } catch (UnsupportedEncodingException e) {
                    e.printStackTrace();
                }
            }
        }

        return result;
    }
}
