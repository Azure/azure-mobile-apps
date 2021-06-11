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

package com.microsoft.windowsazure.mobileservices.notifications;

import android.content.Context;
import android.net.Uri;
import android.util.Pair;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.MoreExecutors;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonPrimitive;
import com.microsoft.windowsazure.mobileservices.MobileServiceApplication;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceFeatures;
import com.microsoft.windowsazure.mobileservices.http.HttpConstants;
import com.microsoft.windowsazure.mobileservices.http.MobileServiceConnection;
import com.microsoft.windowsazure.mobileservices.http.MobileServiceHttpClient;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;

import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;
import java.util.Map;

/**
 * MobileServicePush instance facilitates creation/deletion of notification registrations
 */
public class MobileServicePush {

    /**
     * Push registration path
     */
    private static final String PNS_API_URL = "push";
    private static final String PNS_PLATFORM = "gcm";

    /**
     * The class used to make HTTP clients associated with this instance
     */
    private MobileServiceHttpClient mHttpClient;

    /**
     * Creates a new NotificationHub client
     *
     * @param client  The MobileServiceClient used to invoke the push operations
     * @param context Android context used to access SharedPreferences
     */
    public MobileServicePush(MobileServiceClient client, Context context) {

        mHttpClient = new MobileServiceHttpClient(client);

        if (context == null) {
            throw new IllegalArgumentException("context");
        }
    }

    private static boolean isNullOrWhiteSpace(String str) {
        return str == null || str.trim().equals("");
    }

    /**
     * Registers the client for native notifications
     *
     * @param pnsHandle PNS specific identifier
     * @return Future with Registration Information
     */
    public ListenableFuture<Void> register(String pnsHandle) {
        return register(pnsHandle, (JsonObject) null);
    }

    /**
     * Registers the client for template notifications
     *
     * @param pnsHandle PNS specific identifier
     * @param templates Template to be used for registration
     * @return Future with Registration Information
     */
    public ListenableFuture<Void> register(String pnsHandle, JsonObject templates) {
        final SettableFuture<Void> resultFuture = SettableFuture.create();

        if (isNullOrWhiteSpace(pnsHandle)) {
            resultFuture.setException(new IllegalArgumentException("pnsHandle"));
            return resultFuture;
        }

        if (templates != null) {
            for (Map.Entry<String, JsonElement> entry : templates.entrySet()) {
                if (entry.getValue() != null && entry.getValue().isJsonObject()) {
                    JsonObject template = entry.getValue().getAsJsonObject();
                    if (template.get("body") != null && template.get("body").isJsonObject()) {
                        JsonObject templateBody = template.get("body").getAsJsonObject();
                        template.addProperty("body", templateBody.toString());
                    }
                }
            }
        }

        ListenableFuture<Void> registerInternalFuture = createOrUpdateInstallation(pnsHandle, templates);

        Futures.addCallback(registerInternalFuture, new FutureCallback<Void>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(Void v) {
                resultFuture.set(v);
            }
        }, MoreExecutors.directExecutor());

        return resultFuture;
    }

    /**
     * Registers the client for native notifications
     *
     * @param pnsHandle PNS specific identifier
     * @param callback  The callback to invoke after the Push
     * @deprecated use {@link #register(String pnsHandle)} instead
     */
    public void register(String pnsHandle, final RegistrationCallback callback) {
        ListenableFuture<Void> registerFuture = register(pnsHandle);

        Futures.addCallback(registerFuture, new FutureCallback<Void>() {
            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onRegister((Exception) exception);
                }
            }

            @Override
            public void onSuccess(Void v) {
                callback.onRegister(null);
            }
        }, MoreExecutors.directExecutor());
    }

    /**
     * Registers the client for push notification using device {@link Installation}
     *
     * @param installation device installation in Azure Notification Hub (https://msdn.microsoft.com/en-us/library/azure/mt621153.aspx)
     * @return Future with registration information
     */
    public ListenableFuture<Void> register(Installation installation) {
        final SettableFuture<Void> resultFuture = SettableFuture.create();

        ListenableFuture<Void> registerInternalFuture = createOrUpdateInstallation(installation);

        Futures.addCallback(registerInternalFuture, new FutureCallback<Void>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(Void v) {
                resultFuture.set(v);
            }
        }, MoreExecutors.directExecutor());

        return resultFuture;
    }

    /**
     * Registers the client for template notifications with tags
     *
     * @param pnsHandle    PNS specific identifier
     * @param templateName The template name
     * @param templateBody The template body
     * @return Future with TemplateRegistration Information
     */
    public ListenableFuture<Void> registerTemplate(String pnsHandle, String templateName, String templateBody) {
        final SettableFuture<Void> resultFuture = SettableFuture.create();

        if (isNullOrWhiteSpace(pnsHandle)) {
            resultFuture.setException(new IllegalArgumentException("pnsHandle"));
            return resultFuture;
        }

        if (isNullOrWhiteSpace(templateName)) {
            resultFuture.setException(new IllegalArgumentException("templateName"));
            return resultFuture;
        }

        if (isNullOrWhiteSpace(templateBody)) {
            resultFuture.setException(new IllegalArgumentException("body"));
            return resultFuture;
        }

        JsonObject templateObject = GetTemplateObject(templateName, templateBody);

        ListenableFuture<Void> registerInternalFuture = createOrUpdateInstallation(pnsHandle, templateObject);

        Futures.addCallback(registerInternalFuture, new FutureCallback<Void>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(Void v) {
                resultFuture.set(v);
            }
        }, MoreExecutors.directExecutor());

        return resultFuture;
    }

    private JsonObject GetTemplateObject(String templateName, String templateBody) {
        JsonObject templateDetailObject = new JsonObject();
        templateDetailObject.addProperty("body", templateBody);

        JsonObject templateObject = new JsonObject();
        templateObject.add(templateName, templateDetailObject);

        return templateObject;
    }

    /**
     * Registers the client for template notifications
     *
     * @param pnsHandle    PNS specific identifier
     * @param templateName The template name
     * @param template     The template body
     * @param callback     The operation callback
     * @deprecated use {@link #registerTemplate(String pnsHandle, String templateName, String template)} instead
     */
    public void registerTemplate(String pnsHandle, String templateName, String template, final RegistrationCallback callback) {
        ListenableFuture<Void> registerFuture = registerTemplate(pnsHandle, templateName, template);

        Futures.addCallback(registerFuture, new FutureCallback<Void>() {
            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onRegister((Exception) exception);
                }
            }

            @Override
            public void onSuccess(Void v) {
                callback.onRegister(null);
            }
        }, MoreExecutors.directExecutor());
    }

    /**
     * Unregisters the client for native notifications
     *
     * @return Future with TemplateRegistration Information
     */
    public ListenableFuture<Void> unregister() {
        return deleteInstallation();
    }

    /**
     * Unregisters the client for native notifications
     *
     * @param callback The operation callback
     * @deprecated use {@link #unregister()} instead
     */
    public void unregister(final UnregisterCallback callback) {
        ListenableFuture<Void> deleteInstallationFuture = deleteInstallation();

        Futures.addCallback(deleteInstallationFuture, new FutureCallback<Void>() {
            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onUnregister((Exception) exception);
                }
            }

            @Override
            public void onSuccess(Void v) {
                callback.onUnregister(null);
            }
        }, MoreExecutors.directExecutor());
    }

    private ListenableFuture<Void> deleteInstallation() {
        final SettableFuture<Void> resultFuture = SettableFuture.create();

        String installationId = MobileServiceApplication.getInstallationId(mHttpClient.getClient().getContext());

        String path = PNS_API_URL + "/installations/" + Uri.encode(installationId);

        ListenableFuture<ServiceFilterResponse> serviceFilterFuture = mHttpClient.request(path, null, HttpConstants.DeleteMethod, null, null);

        Futures.addCallback(serviceFilterFuture, new FutureCallback<ServiceFilterResponse>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(ServiceFilterResponse response) {

                resultFuture.set(null);
            }
        }, MoreExecutors.directExecutor());

        return resultFuture;
    }

    private JsonObject convertInstallationToJson(Installation installation) {

        // installationId, pushChannel and platform are the required properties in Installation object.
        // Make sure installation object is valid.
        if (installation == null || isNullOrWhiteSpace(installation.installationId) || isNullOrWhiteSpace(installation.pushChannel) || isNullOrWhiteSpace(installation.platform)) {
            return null;
        }

        JsonObject installationJson = new JsonObject();

        installationJson.addProperty("installationId", installation.installationId);
        installationJson.addProperty("pushChannel", installation.pushChannel);
        installationJson.addProperty("platform", installation.platform);

        // expirationTime is a read-only property that can be set at NotificationHubs level create or update.
        // Ignore expirationTime in create or update installation.
        installationJson.addProperty("expirationTime", "");

        if (installation.pushVariables != null) {
            JsonObject pushVariables = new JsonObject();
            for (Map.Entry<String, String> entry : installation.pushVariables.entrySet()) {
                pushVariables.addProperty(entry.getKey(), entry.getValue());
            }
            installationJson.add("pushVariables", pushVariables);
        }
        if (installation.tags != null) {
            JsonArray tagsArray = new JsonArray();
            for (String tag : installation.tags) {
                tagsArray.add(new JsonPrimitive(tag));
            }
            installationJson.add("tags", tagsArray);
        }

        if (installation.templates != null) {
            JsonObject templates = new JsonObject();
            for (Map.Entry<String, InstallationTemplate> entry : installation.templates.entrySet()) {
                JsonObject template = new JsonObject();
                if (entry.getValue().body != null) {
                    template.addProperty("body", entry.getValue().body);
                }
                if (entry.getValue().tags != null) {
                    JsonArray tagsArray = new JsonArray();
                    for (String tag : entry.getValue().tags) {
                        tagsArray.add(new JsonPrimitive(tag));
                    }
                    template.add("tags", tagsArray);
                }
                templates.add(entry.getKey(), template);
            }
            installationJson.add("templates", templates);
        }

        return installationJson;
    }

    private ListenableFuture<Void> createOrUpdateInstallation(Installation installation) {

        SettableFuture<Void> resultFuture = SettableFuture.create();

        JsonObject installationJson = convertInstallationToJson(installation);
        if (installationJson == null) {
            resultFuture.setException(new MobileServiceException("Error with create or update installation. installationId, pushChannel or platform of installation can't be NULL."));
            return resultFuture;
        }
        String installationJsonString = installationJson.toString();

        return createOrUpdateInstallationInternal(installationJsonString, resultFuture);
    }

    private ListenableFuture<Void> createOrUpdateInstallation(String pnsHandle, JsonElement templates) {

        SettableFuture<Void> resultFuture = SettableFuture.create();

        JsonObject installation = new JsonObject();
        installation.addProperty("pushChannel", pnsHandle);
        installation.addProperty("platform", PNS_PLATFORM);

        if (templates != null) {
            installation.add("templates", templates);
        }

        String installationJsonString = installation.toString();

        return createOrUpdateInstallationInternal(installationJsonString, resultFuture);
    }

    private ListenableFuture<Void> createOrUpdateInstallationInternal(String installationJsonString, final SettableFuture<Void> resultFuture) {

        String installationId = MobileServiceApplication.getInstallationId(mHttpClient.getClient().getContext());

        String path = PNS_API_URL + "/installations/" + Uri.encode(installationId);

        List<Pair<String, String>> requestHeaders = new ArrayList<>();

        requestHeaders.add(new Pair<>(HttpConstants.ContentType, MobileServiceConnection.JSON_CONTENTTYPE));

        ListenableFuture<ServiceFilterResponse> serviceFilterFuture = mHttpClient.request(path, installationJsonString, HttpConstants.PutMethod, requestHeaders, null, EnumSet.noneOf(MobileServiceFeatures.class));

        Futures.addCallback(serviceFilterFuture, new FutureCallback<ServiceFilterResponse>() {
            @Override
            public void onFailure(Throwable exception) {

                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(ServiceFilterResponse response) {

                resultFuture.set(null);
            }
        }, MoreExecutors.directExecutor());

        return resultFuture;
    }
}
