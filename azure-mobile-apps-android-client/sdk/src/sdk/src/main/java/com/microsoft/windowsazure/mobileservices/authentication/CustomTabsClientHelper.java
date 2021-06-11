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

import android.content.ComponentName;
import android.content.Context;
import android.support.customtabs.CustomTabsClient;
import android.support.customtabs.CustomTabsIntent;
import android.support.customtabs.CustomTabsServiceConnection;
import android.support.customtabs.CustomTabsSession;

/**
 * Helper class to establish connection with {@link CustomTabsClient}
 */
class CustomTabsClientHelper {

    /**
     * Package name of Chrome custom tabs
     */
    static final String CUSTOM_TABS_PACKAGE_NAME = "com.android.chrome";

    private final Context mContext;

    private CustomTabsClient mCustomTabsClient;

    private CustomTabsServiceConnection mConnection;

    CustomTabsClientHelper(Context context) {
        mContext = context;
    }

    /**
     * Bind Custom Tabs Service
     * @param browserPackage package name of Custom Tabs Service
     * @return true if binding succeeded, false if binding failed OR Custom Tabs Service not available
     */
    boolean bindCustomTabsService(String browserPackage) {
        mConnection = new CustomTabsServiceConnection() {

            @Override
            public void onCustomTabsServiceConnected(ComponentName componentName, CustomTabsClient customTabsClient) {
                customTabsClient.warmup(0);
                mCustomTabsClient = customTabsClient;
            }

            @Override
            public void onServiceDisconnected(ComponentName componentName) {
                mCustomTabsClient = null;
            }

        };

        return CustomTabsClient.bindCustomTabsService(mContext, browserPackage, mConnection);
    }

    CustomTabsIntent.Builder createCustomTabsIntentBuilder() {
        CustomTabsSession session = null;
        if (mCustomTabsClient != null) {
            session = mCustomTabsClient.newSession(null);
        }

        return new CustomTabsIntent.Builder(session);
    }

    /**
     * Unbind Custom Tabs Service for garbage collection
     */
    void unbindCustomTabsService() {
        if (mConnection == null) {
            return;
        }

        mContext.unbindService(mConnection);
        mCustomTabsClient = null;
    }
}
