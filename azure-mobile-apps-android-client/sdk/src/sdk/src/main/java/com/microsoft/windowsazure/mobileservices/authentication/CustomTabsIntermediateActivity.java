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
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;

import com.google.gson.Gson;

import static com.microsoft.windowsazure.mobileservices.authentication.CustomTabsLoginManager.*;

/**
 * Activity working together with {@link CustomTabsLoginActivity} to handle the login flow using
 * Chrome Custom Tabs or a regular browser. When Chrome Custom Tabs are not available on the device,
 * fallback to browser. This activity is started by {@link CustomTabsLoginManager} and {@link CustomTabsLoginActivity}.
 */
public class CustomTabsIntermediateActivity extends Activity {

    private boolean mLoginInProgress = false;

    private String mUserId;

    private String mAuthenticationToken;

    private String mErrorMessage;

    private CustomTabsLoginState mLoginState;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        if (savedInstanceState == null) {
            extractState(getIntent().getExtras());
        } else {
            extractState(savedInstanceState);
        }
    }

    @Override
    protected void onSaveInstanceState(Bundle outState) {
        super.onSaveInstanceState(outState);
        outState.putBoolean(KEY_LOGIN_IN_PROGRESS, mLoginInProgress);
        outState.putString(KEY_LOGIN_USER_ID, mUserId);
        outState.putString(KEY_LOGIN_ERROR, mErrorMessage);
        outState.putString(KEY_LOGIN_AUTHENTICATION_TOKEN, mAuthenticationToken);
        outState.putString(KEY_LOGIN_STATE, new Gson().toJson(mLoginState));
    }

    @Override
    protected void onRestoreInstanceState(Bundle savedInstanceState) {
        super.onRestoreInstanceState(savedInstanceState);
        extractState(savedInstanceState);
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);

        if (intent.getExtras() != null) {
            mUserId = intent.getExtras().getString(KEY_LOGIN_USER_ID);
            mAuthenticationToken = intent.getExtras().getString(KEY_LOGIN_AUTHENTICATION_TOKEN);
            mErrorMessage = intent.getExtras().getString(KEY_LOGIN_ERROR);
        }
    }

    @Override
    protected void onResume() {
        super.onResume();

        // This method is called twice during the server login flow. The first time it is called from
        // {@link CustomTabsLoginManager.authenticate} to initiate the login flow.
        // The second time it's called from {@link CustomTabsLoginActivity} to pass login result back
        // to the caller. mLoginInProgress is used to differentiate the first time from the second time.

        if (!mLoginInProgress) {

            // Validate mLoginState is not null and start CustomTabsLoginActivity.
            // Otherwise, complete the login flow with error.

            if (mLoginState != null) {
                Intent i = new Intent(this, CustomTabsLoginActivity.class);

                String loginStateJson = new Gson().toJson(mLoginState);
                i.putExtra(KEY_LOGIN_STATE, loginStateJson);
                startActivity(i);

                mLoginInProgress = true;
            } else {
                // error handling
                Intent data = new Intent();
                data.putExtra(KEY_LOGIN_ERROR, TokenRequestAsyncTask.AUTHENTICATION_ERROR_MESSAGE);
                setResult(Activity.RESULT_OK, data);
                finish();
            }
        } else {
            Intent data = new Intent();

            // Validate mUserId and mAuthenticationToken is not null and complete the login flow
            // with userId and authenticationToken. Otherwise, complete the login flow with error.

            if (mUserId != null && mAuthenticationToken != null) {
                data.putExtra(KEY_LOGIN_USER_ID, mUserId);
                data.putExtra(KEY_LOGIN_AUTHENTICATION_TOKEN, mAuthenticationToken);
            } else {
                // error handling
                data.putExtra(KEY_LOGIN_ERROR, mErrorMessage != null ? mErrorMessage : TokenRequestAsyncTask.AUTHENTICATION_ERROR_MESSAGE);
            }
            setResult(Activity.RESULT_OK, data);
            finish();
        }
    }

    /**
     * Create an intent with the authenticated MobileService user and an error message
     * to complete the login flow.
     * @param context Context of the activity
     * @param user Authenticated Mobile Service user
     * @param errorMessage Error message
     * @return Intent
     */
    public static Intent createLoginCompletionIntent(Context context, MobileServiceUser user, String errorMessage) {
        Intent intent = new Intent(context, CustomTabsIntermediateActivity.class);

        if (user != null) {
            intent.putExtra(KEY_LOGIN_USER_ID, user.getUserId());
            intent.putExtra(KEY_LOGIN_AUTHENTICATION_TOKEN, user.getAuthenticationToken());
        }

        if (errorMessage != null) {
            intent.putExtra(KEY_LOGIN_ERROR, errorMessage);
        }

        // Launch CustomTabsIntermediateActivity using two special flags.
        // (1) FLAG_ACTIVITY_SINGLE_TOP ensures that if there already is an instance of this activity
        // at the top of stack in the caller task, there would not be any new instance of this activity created.
        // Instead, an intent will be sent to the current instance through onNewIntent().
        // (2) FLAG_ACTIVITY_CLEAR_TOP ensures all the other activities on top of this activity
        // will be closed and this activity will be delivered to the top.

        intent.addFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP | Intent.FLAG_ACTIVITY_CLEAR_TOP);
        return intent;
    }

    private void extractState(Bundle state) {
        if (state != null) {
            mLoginInProgress = state.getBoolean(KEY_LOGIN_IN_PROGRESS);
            mUserId = state.getString(KEY_LOGIN_USER_ID);
            mAuthenticationToken = state.getString(KEY_LOGIN_AUTHENTICATION_TOKEN);
            mErrorMessage = state.getString(KEY_LOGIN_ERROR);

            String loginStateJson = state.getString(KEY_LOGIN_STATE);
            mLoginState = new Gson().fromJson(loginStateJson, CustomTabsLoginState.class);
        }
    }
}
