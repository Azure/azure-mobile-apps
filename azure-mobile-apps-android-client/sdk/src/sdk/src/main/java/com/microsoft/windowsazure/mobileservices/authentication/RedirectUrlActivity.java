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

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;

/**
 * Activity that receives the redirect url sent by App Service backend.
 * It launches {@link CustomTabsLoginActivity} with the redirect url.
 * Developers using this library must override the {@code redirectUriScheme} property
 * in their {@code build.gradle} to specify the custom scheme to be used for the redirect.
 *
 * For example, to handle redirect url {@code yourUrlScheme://easyauth.callback}:
 *
 * <pre>
 * {@code
 * <intent-filter>
 *   <action android:name="android.intent.action.VIEW"/>
 *   <category android:name="android.intent.category.DEFAULT"/>
 *   <category android:name="android.intent.category.BROWSABLE"/>
 *   <data android:scheme="yourUrlScheme" android:host="easyauth.callback" />
 * </intent-filter>
 * }
 * </pre>
 */
public class RedirectUrlActivity extends Activity {

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        Uri responseUri = getIntent().getData();
        Intent intent = new Intent(this, CustomTabsLoginActivity.class);
        intent.setData(responseUri);
        startActivity(intent);

        finish();
    }
}
