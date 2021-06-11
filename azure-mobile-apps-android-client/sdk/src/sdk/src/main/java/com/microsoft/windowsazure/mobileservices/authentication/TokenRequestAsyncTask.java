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

import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonObject;
import com.google.gson.JsonParseException;
import com.google.gson.JsonParser;
import com.google.gson.JsonSyntaxException;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.http.MobileServiceConnection;
import com.microsoft.windowsazure.mobileservices.http.RequestAsyncTask;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;

class TokenRequestAsyncTask extends RequestAsyncTask {

    /**
     * Generic error message for authentication failure
     */
    static final String AUTHENTICATION_ERROR_MESSAGE = "Error while authenticating user.";

    /**
     * Authentication Token parameter in JSON objects
     */
    private static final String TOKEN_JSON_PARAMETER = "authenticationToken";

    /**
     * UserId property in JSON objects
     */
    private static final String USERID_JSON_PROPERTY = "userId";

    /**
     * User property in JSON objects
     */
    private static final String USER_JSON_PROPERTY = "user";

    private SettableFuture<MobileServiceUser> mFuture;

    TokenRequestAsyncTask(ServiceFilterRequest request, MobileServiceConnection connection) {
        super(request, connection);
        mFuture = SettableFuture.create();
    }

    SettableFuture<MobileServiceUser> getMobileServiceUserFuture() {
        return mFuture;
    }

    @Override
    protected void onPostExecute(ServiceFilterResponse response) {
        if (mTaskException == null && response != null) {
            Exception exception = null;
            JsonObject json = null;

            try {
                // Get the user from the response and create a
                // MobileServiceUser object from the JSON
                String content = response.getContent();
                json = (JsonObject) new JsonParser().parse((content.trim()));
            } catch (JsonSyntaxException ex) {
                exception = ex;
            }

            // If the JSON object is valid, create a MobileServiceUser object
            if (json != null) {

                if (json.has(USER_JSON_PROPERTY)) {
                    JsonObject jsonUser = json.getAsJsonObject(USER_JSON_PROPERTY);

                    if (jsonUser.has(USERID_JSON_PROPERTY)) {
                        String userId = jsonUser.get(USERID_JSON_PROPERTY).getAsString();
                        MobileServiceUser user = new MobileServiceUser(userId);

                        if (json.has(TOKEN_JSON_PARAMETER)) {
                            user.setAuthenticationToken(json.get(TOKEN_JSON_PARAMETER).getAsString());
                            mFuture.set(user);
                            return;
                        } else {
                            exception = new JsonParseException(TOKEN_JSON_PARAMETER + " property expected");
                        }
                    } else {
                        exception = new JsonParseException(USERID_JSON_PROPERTY + " property expected");
                    }

                } else {
                    // If the JSON contains an error property show it, otherwise raise
                    // an error with JSON content
                    if (json.has("error")) {
                        exception = new MobileServiceException(json.get("error").getAsString());
                    } else {
                        exception = new JsonParseException(json.toString());
                    }
                }
            }
            mFuture.setException(new MobileServiceException(AUTHENTICATION_ERROR_MESSAGE, exception, response));
        } else {
            mFuture.setException(new MobileServiceException(AUTHENTICATION_ERROR_MESSAGE, mTaskException));
        }
    }
}
