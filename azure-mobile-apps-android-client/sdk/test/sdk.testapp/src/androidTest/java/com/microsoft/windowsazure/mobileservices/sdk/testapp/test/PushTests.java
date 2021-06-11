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
package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import android.net.Uri;
import android.test.InstrumentationTestCase;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceApplication;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.http.HttpConstants;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.notifications.Installation;
import com.microsoft.windowsazure.mobileservices.notifications.InstallationTemplate;
import com.microsoft.windowsazure.mobileservices.notifications.MobileServicePush;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterRequestMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterResponseMock;
import okhttp3.Protocol;
import okhttp3.internal.http.StatusLine;

import junit.framework.Assert;

import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.concurrent.ExecutionException;

public class PushTests extends InstrumentationTestCase {

    final String appUrl = "http://myapp.com/";
    final String pnsApiUrl = "push";
    final String pnsApiPlatform = "gcm";

    public void testUnregister() throws Throwable {

        final Container container = new Container();

        MobileServiceClient client = null;

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        final String expectedUrl = appUrl + pnsApiUrl + "/installations/" + Uri.encode(installationId);

        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());

            client = client.withFilter(new ServiceFilter() {

                @Override
                public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                    container.requestUrl = request.getUrl();
                    container.requestMethod = request.getMethod();

                    ServiceFilterResponseMock mockResponse = new ServiceFilterResponseMock();
                    mockResponse.setStatus(new StatusLine(Protocol.HTTP_2, 204, ""));

                    ServiceFilterRequestMock mockRequest = new ServiceFilterRequestMock(mockResponse);

                    return nextServiceFilterCallback.onNext(mockRequest);
                }
            });

            final MobileServicePush push = client.getPush();
            push.unregister().get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            fail(container.exception.getMessage());
        }

        // Asserts
        Assert.assertEquals(expectedUrl, container.requestUrl);
        Assert.assertEquals(HttpConstants.DeleteMethod, container.requestMethod);
    }

    public void testRegister() throws Throwable {

        final Container container = new Container();

        MobileServiceClient client = null;
        final String handle = "handle";

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        final String expectedUrl = appUrl + pnsApiUrl + "/installations/" + Uri.encode(installationId);
        final String expectedContent = "{\"pushChannel\":\"handle\",\"platform\":\""+pnsApiPlatform+"\"}";
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());

            client = client.withFilter(new ServiceFilter() {

                @Override
                public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                    container.requestUrl = request.getUrl();
                    container.requestContent = request.getContent();
                    container.requestMethod = request.getMethod();

                    ServiceFilterResponseMock mockResponse = new ServiceFilterResponseMock();
                    mockResponse.setStatus(new StatusLine(Protocol.HTTP_2, 204, ""));

                    ServiceFilterRequestMock mockRequest = new ServiceFilterRequestMock(mockResponse);

                    return nextServiceFilterCallback.onNext(mockRequest);
                }
            });

            final MobileServicePush push = client.getPush();
            push.register(handle).get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            fail(container.exception.getMessage());
        }

        // Asserts
        Assert.assertEquals(expectedUrl, container.requestUrl);
        Assert.assertEquals(expectedContent, container.requestContent);
        Assert.assertEquals(HttpConstants.PutMethod, container.requestMethod);
    }

    public void testRegisterTemplate() throws Throwable {

        final Container container = new Container();

        MobileServiceClient client = null;
        final String handle = "handle";

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        final String expectedUrl = appUrl + pnsApiUrl + "/installations/" + Uri.encode(installationId);
        final String expectedContent =
                "{\"pushChannel\":\"handle\",\"platform\":\""+pnsApiPlatform+"\",\"templates\":{\"template1\":{\"body\":\"{\\\"data\\\":\\\"abc\\\"}\"}}}";
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());

            client = client.withFilter(new ServiceFilter() {

                @Override
                public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                    container.requestUrl = request.getUrl();
                    container.requestContent = request.getContent();
                    container.requestMethod = request.getMethod();

                    ServiceFilterResponseMock mockResponse = new ServiceFilterResponseMock();
                    mockResponse.setStatus(new StatusLine(Protocol.HTTP_2, 204, ""));

                    ServiceFilterRequestMock mockRequest = new ServiceFilterRequestMock(mockResponse);

                    return nextServiceFilterCallback.onNext(mockRequest);
                }
            });

            final MobileServicePush push = client.getPush();
            push.registerTemplate(handle, "template1", "{\"data\":\"abc\"}").get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            fail(container.exception.getMessage());
        }

        // Asserts
        Assert.assertEquals(expectedUrl, container.requestUrl);
        Assert.assertEquals(expectedContent, container.requestContent);
        Assert.assertEquals(HttpConstants.PutMethod, container.requestMethod);
    }

    public void testRegisterJsonObjectTemplate() throws Throwable {

        final Container container = new Container();

        MobileServiceClient client = null;
        final String handle = "handle";

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        final String expectedUrl = appUrl + pnsApiUrl + "/installations/" + Uri.encode(installationId);
        final String expectedContent = "{\"pushChannel\":\"handle\",\"platform\":\""+pnsApiPlatform+"\",\"templates\":" + createTemplateObject(true).toString() + "}";

        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());

            client = client.withFilter(new ServiceFilter() {

                @Override
                public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                    container.requestUrl = request.getUrl();
                    container.requestContent = request.getContent();
                    container.requestMethod = request.getMethod();

                    ServiceFilterResponseMock mockResponse = new ServiceFilterResponseMock();
                    mockResponse.setStatus(new StatusLine(Protocol.HTTP_2, 204, ""));

                    ServiceFilterRequestMock mockRequest = new ServiceFilterRequestMock(mockResponse);

                    return nextServiceFilterCallback.onNext(mockRequest);
                }
            });

            JsonObject templates = createTemplateObject(false);

            final MobileServicePush push = client.getPush();

            push.register(handle, templates).get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            fail(container.exception.getMessage());
        }

        // Asserts
        Assert.assertEquals(expectedUrl, container.requestUrl);
        Assert.assertEquals(expectedContent, container.requestContent);
        Assert.assertEquals(HttpConstants.PutMethod, container.requestMethod);
    }

    public void testRegisterTemplateBodyAsJsonObject() throws Throwable {

        final Container container = new Container();

        MobileServiceClient client = null;
        final String handle = "handle";

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        final String expectedUrl = appUrl + pnsApiUrl + "/installations/" + Uri.encode(installationId);
        final String expectedContent = "{\"pushChannel\":\"handle\",\"platform\":\""+pnsApiPlatform+"\",\"templates\":" + createTemplateObject(true).toString() + "}";

        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());

            client = client.withFilter(new ServiceFilter() {

                @Override
                public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                    container.requestUrl = request.getUrl();
                    container.requestContent = request.getContent();
                    container.requestMethod = request.getMethod();

                    ServiceFilterResponseMock mockResponse = new ServiceFilterResponseMock();
                    mockResponse.setStatus(new StatusLine(Protocol.HTTP_2, 204, ""));

                    ServiceFilterRequestMock mockRequest = new ServiceFilterRequestMock(mockResponse);

                    return nextServiceFilterCallback.onNext(mockRequest);
                }
            });

            JsonObject templates = createTemplateObject(true);

            final MobileServicePush push = client.getPush();

            push.register(handle, templates).get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            fail(container.exception.getMessage());
        }

        // Asserts
        Assert.assertEquals(expectedUrl, container.requestUrl);
        Assert.assertEquals(expectedContent, container.requestContent);
        Assert.assertEquals(HttpConstants.PutMethod, container.requestMethod);
    }

    public void testRegisterInstallationWithTagNoTemplate() throws Throwable {

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        String handle = "handle";
        ArrayList<String> tags = new ArrayList<>();
        tags.add("topics:my-first-tag");
        tags.add("topics:my-second-tag");

        Installation installation = new Installation(installationId, pnsApiPlatform, handle, null, tags, null);

        String expectedContent =
                "{\"installationId\":\"" + installationId + "\",\"pushChannel\":\"handle\",\"platform\":\""+pnsApiPlatform+"\",\"expirationTime\":\"\",\"tags\":[\"topics:my-first-tag\",\"topics:my-second-tag\"]}";

        this.testRegisterInstallation(installation, expectedContent);
    }

    public void testRegisterInstallationIgnoresExpirationTimeAndPushChannelExpired() throws Throwable {

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        String handle = "handle";
        ArrayList<String> tags = new ArrayList<>();
        tags.add("topics:my-first-tag");
        tags.add("topics:my-second-tag");

        Installation installation = new Installation(installationId, pnsApiPlatform, handle, null, tags, null, new Date(), true);

        String expectedContent =
                "{\"installationId\":\"" + installationId + "\",\"pushChannel\":\"handle\",\"platform\":\""+pnsApiPlatform+"\",\"expirationTime\":\"\",\"tags\":[\"topics:my-first-tag\",\"topics:my-second-tag\"]}";

        this.testRegisterInstallation(installation, expectedContent);
    }

    public void testRegisterInstallationWithPushVariables() throws Throwable {
        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        String handle = "handle";
        ArrayList<String> tags = new ArrayList<>();
        tags.add("topics:my-first-tag");
        tags.add("topics:my-second-tag");
        HashMap<String, String> pushVariables = new LinkedHashMap<>();
        pushVariables.put("key1", "value1");
        pushVariables.put("key2", "value2");
        HashMap<String, InstallationTemplate> templates = new HashMap<>();
        InstallationTemplate template = new InstallationTemplate("{\"data\":\"abc\"}", tags);
        templates.put("templateName", template);

        Installation installation = new Installation(installationId, pnsApiPlatform, handle, pushVariables, tags, templates);

        String expectedContent =
                "{\"installationId\":\"" + installationId + "\",\"pushChannel\":\"handle\",\"platform\":\""+pnsApiPlatform+"\",\"expirationTime\":\"\",\"pushVariables\":{\"key1\":\"value1\",\"key2\":\"value2\"},\"tags\":[\"topics:my-first-tag\",\"topics:my-second-tag\"],\"templates\":{\"templateName\":{\"body\":\"{\\\"data\\\":\\\"abc\\\"}\",\"tags\":[\"topics:my-first-tag\",\"topics:my-second-tag\"]}}}";

        this.testRegisterInstallation(installation, expectedContent);
    }

    public void testRegisterInstallationWithTagAndTagInsideTemplate() throws Throwable {

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        String handle = "handle";
        ArrayList<String> tags = new ArrayList<>();
        tags.add("topics:my-first-tag");
        tags.add("topics:my-second-tag");
        HashMap<String, InstallationTemplate> templates = new HashMap<>();
        InstallationTemplate template = new InstallationTemplate("{\"data\":\"abc\"}", tags);
        templates.put("templateName", template);

        Installation installation = new Installation(installationId, pnsApiPlatform, handle, null, tags, templates);

        String expectedContent =
                "{\"installationId\":\"" + installationId + "\",\"pushChannel\":\"handle\",\"platform\":\""+pnsApiPlatform+"\",\"expirationTime\":\"\",\"tags\":[\"topics:my-first-tag\",\"topics:my-second-tag\"],\"templates\":{\"templateName\":{\"body\":\"{\\\"data\\\":\\\"abc\\\"}\",\"tags\":[\"topics:my-first-tag\",\"topics:my-second-tag\"]}}}";

        this.testRegisterInstallation(installation, expectedContent);
    }

    public void testRegisterInstallationWithTagInsideTemplate() throws Throwable {

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        String handle = "handle";
        ArrayList<String> tags = new ArrayList<>();
        tags.add("topics:my-first-tag");
        tags.add("topics:my-second-tag");
        HashMap<String, InstallationTemplate> templates = new HashMap<>();
        InstallationTemplate template = new InstallationTemplate("{\"data\":\"abc\"}", tags);
        templates.put("templateName", template);

        Installation installation = new Installation(installationId, pnsApiPlatform, handle, null, null, templates);

        String expectedContent =
                "{\"installationId\":\"" + installationId + "\",\"pushChannel\":\"handle\",\"platform\":\""+pnsApiPlatform+"\",\"expirationTime\":\"\",\"templates\":{\"templateName\":{\"body\":\"{\\\"data\\\":\\\"abc\\\"}\",\"tags\":[\"topics:my-first-tag\",\"topics:my-second-tag\"]}}}";

        this.testRegisterInstallation(installation, expectedContent);
    }

    private void testRegisterInstallation(Installation installation, String expectedContent) {

        final Container container = new Container();

        MobileServiceClient client;

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        final String expectedUrl = appUrl + pnsApiUrl + "/installations/" + Uri.encode(installationId);
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());

            client = client.withFilter(new ServiceFilter() {

                @Override
                public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                    container.requestUrl = request.getUrl();
                    container.requestContent = request.getContent();
                    container.requestMethod = request.getMethod();

                    ServiceFilterResponseMock mockResponse = new ServiceFilterResponseMock();
                    mockResponse.setStatus(new StatusLine(Protocol.HTTP_2, 204, ""));

                    ServiceFilterRequestMock mockRequest = new ServiceFilterRequestMock(mockResponse);

                    return nextServiceFilterCallback.onNext(mockRequest);
                }
            });

            MobileServicePush push = client.getPush();

            push.register(installation).get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            fail(container.exception.getMessage());
        }

        // Asserts
        Assert.assertEquals(expectedUrl, container.requestUrl);
        Assert.assertEquals(expectedContent, container.requestContent);
        Assert.assertEquals(HttpConstants.PutMethod, container.requestMethod);
    }

    public void testRegisterInstallationWhenInstallationIsNull() throws Throwable {
        this.testRegisterInstallationOnInvalidInstallation(null);
    }

    public void testRegisterInstallationWhenInstallationHasNullPushChannel() throws Throwable {

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        ArrayList<String> tags = new ArrayList<>();
        tags.add("topics:my-first-tag");
        tags.add("topics:my-second-tag");
        HashMap<String, InstallationTemplate> templates = new HashMap<>();
        InstallationTemplate template = new InstallationTemplate("{\"data\":\"abc\"}", tags);
        templates.put("templateName", template);
        Installation installation = new Installation(installationId, pnsApiPlatform, null, null, tags, templates);

        this.testRegisterInstallationOnInvalidInstallation(installation);
    }

    public void testRegisterInstallationWhenInstallationHasNullInstallationId() throws Throwable {

        String handle = "handle";
        ArrayList<String> tags = new ArrayList<>();
        tags.add("topics:my-first-tag");
        tags.add("topics:my-second-tag");
        HashMap<String, InstallationTemplate> templates = new HashMap<>();
        InstallationTemplate template = new InstallationTemplate("{\"data\":\"abc\"}", tags);
        templates.put("templateName", template);
        Installation installation = new Installation(null, pnsApiPlatform, handle, null, tags, templates);

        this.testRegisterInstallationOnInvalidInstallation(installation);
    }

    public void testRegisterInstallationWhenInstallationHasNullPlatform() throws Throwable {

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        String handle = "handle";
        ArrayList<String> tags = new ArrayList<>();
        tags.add("topics:my-first-tag");
        tags.add("topics:my-second-tag");
        HashMap<String, InstallationTemplate> templates = new HashMap<>();
        InstallationTemplate template = new InstallationTemplate("{\"data\":\"abc\"}", tags);
        templates.put("templateName", template);
        Installation installation = new Installation(installationId, null, handle, null, tags, templates);

        this.testRegisterInstallationOnInvalidInstallation(installation);
    }

    private void testRegisterInstallationOnInvalidInstallation(Installation installation) throws Throwable {

        MobileServiceClient client;

        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());

            MobileServicePush push = client.getPush();

            push.register(installation).get();

        } catch (Exception exception) {
            assertTrue(exception instanceof ExecutionException);
            assertTrue(exception.getCause() instanceof MobileServiceException);
            assertEquals("Error with create or update installation. installationId, pushChannel or platform of installation can't be NULL.", exception.getCause().getMessage());
        }
    }

    private JsonObject createTemplateObject(Boolean isTemplateBodyString) {
        JsonObject templateBody = new JsonObject();
        templateBody.addProperty("data", "abc");

        JsonObject templateDetailObject = new JsonObject();
        if (isTemplateBodyString) {
            templateDetailObject.addProperty("body", templateBody.toString());
        } else {
            templateDetailObject.add("body", templateBody);
        }

        JsonObject templateObject = new JsonObject();
        templateObject.add("template1", templateDetailObject);
        templateObject.add("template2", templateDetailObject);

        return templateObject;
    }

    private class Container {

        public String requestContent;
        public String requestUrl;
        public String requestMethod;

        public Exception exception;
    }
}