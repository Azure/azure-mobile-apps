// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using DeviceTests.Android.Services;
using DeviceTests.Shared;
using DeviceTests.Shared.Helpers;
using DeviceTests.Shared.TestPlatform;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DeviceTests.Android.Tests
{
    public class Push_Tests : E2ETestBase
    {
        readonly IPushTestUtility pushTestUtility = new PushTestUtility();

        [SkippableFact]
        public async Task InitialDeleteRegistrationsAsync()
        {
            Skip.IfNot(EnablePushTests);
            string registrationId = pushTestUtility.GetPushHandle();
            Dictionary<string, string> channelUriParam = new Dictionary<string, string>()
            {
                {"channelUri", registrationId}
            };
            await GetClient().InvokeApiAsync("deleteRegistrationsForChannel", HttpMethod.Delete, channelUriParam);
        }

        [SkippableFact]
        public async Task RegisterAsync()
        {
            Skip.IfNot(EnablePushTests);
            string registrationId = pushTestUtility.GetPushHandle();
            var push = GetClient().GetPush();
            await push.RegisterAsync(registrationId);
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                {"channelUri", registrationId}
            };
            await VerifyRegistration(parameters, push);
        }

        [SkippableFact]
        public async Task LoginRegisterAsync()
        {
            Skip.IfNot(EnablePushTests);
            MobileServiceUser user = await Utilities.GetDummyUser(GetClient());
            GetClient().CurrentUser = user;
            string registrationId = pushTestUtility.GetPushHandle();
            var push = GetClient().GetPush();
            await push.RegisterAsync(registrationId);

            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                {"channelUri", registrationId}
            };
            await VerifyRegistration(parameters, push);
        }

        [SkippableFact]
        public async Task UnregisterAsync()
        {
            Skip.IfNot(EnablePushTests);
            var push = GetClient().GetPush();
            await push.UnregisterAsync();
            await GetClient().InvokeApiAsync("verifyUnregisterInstallationResult", HttpMethod.Get, null);
        }

        [SkippableFact]
        public async Task RegisterAsyncTemplatesAndOverride()
        {
            Skip.IfNot(EnablePushTests);
            string registrationId = pushTestUtility.GetPushHandle();
            JObject templates = GetTemplates("bar");
            JObject expectedTemplates = GetTemplates("testGcmTemplate");
            var push = GetClient().GetPush();
            try
            {
                await push.RegisterAsync(registrationId, templates);
                var parameters = new Dictionary<string, string>()
                {
                    {"channelUri", registrationId},
                    {"templates", JsonConvert.SerializeObject(expectedTemplates)}
                };
                await GetClient().InvokeApiAsync("verifyRegisterInstallationResult", HttpMethod.Get, parameters);

                await push.RegisterAsync(registrationId);
                parameters = new Dictionary<string, string>()
                {
                    {"channelUri", registrationId},
                };
                await GetClient().InvokeApiAsync("verifyRegisterInstallationResult", HttpMethod.Get, parameters);
            }
            finally
            {
                push.UnregisterAsync().Wait();
            }

            await GetClient().LogoutAsync();
        }

        [SkippableFact]
        public async Task RegisterAsyncTemplatesWithTemplateBodyJson()
        {
            Skip.IfNot(EnablePushTests);
            string registrationId = pushTestUtility.GetPushHandle();
            JObject templates = GetTemplates("bar", true);
            JObject expectedTemplates = GetTemplates("testGcmTemplate");
            var push = GetClient().GetPush();
            try
            {
                await push.RegisterAsync(registrationId, templates);
                var parameters = new Dictionary<string, string>()
                {
                    {"channelUri", registrationId},
                    {"templates", JsonConvert.SerializeObject(expectedTemplates)}
                };
                await GetClient().InvokeApiAsync("verifyRegisterInstallationResult", HttpMethod.Get, parameters);
            }
            finally
            {
                push.UnregisterAsync().Wait();
            }
        }

        [SkippableFact]
        public async Task PushGcmTest()
        {
            Skip.IfNot(EnablePushTests);
            var push = GetClient().GetPush();
            string registrationId = ((PushTestUtility)pushTestUtility).GetPushHandle();

            //Build push payload
            JObject body = new JObject();
            body["method"] = "send";
            body["type"] = "gcm";
            body["payload"] = "{\"data\":{\"message\":\"Notification Hub test notification\"}}";
            body["token"] = "dummy";

            try
            {
                // Register for Push
                await push.RegisterAsync(registrationId);

                // Invoke API to send push & Wait for push receive
                var send = GetClient().InvokeApiAsync("push", body);
                var receive = this.WaitForPush(TimeSpan.FromSeconds(20));
                Task.WaitAll(new Task[] { send, receive }, TimeSpan.FromSeconds(25));

                Assert.Equal(TaskStatus.RanToCompletion, receive.Status);
                Assert.Equal("Notification Hub test notification", receive.Result);
            }
            finally
            {
                push.UnregisterAsync().Wait();
            }
        }

        private static JObject GetTemplates(string tag, bool templateBodyJson = false)
        {
            JObject msg = new JObject();
            msg["msg"] = "$(message)";
            JObject data = new JObject();
            data["data"] = msg;
            JObject templateBody = new JObject();
            templateBody["body"] = data.ToString();
            if (templateBodyJson)
            {
                templateBody["body"] = data;
            }
            if (tag != null)
            {
                JArray tags = new JArray() { tag };
                templateBody["tags"] = tags;
            }
            JObject templates = new JObject();
            templates["testGcmTemplate"] = templateBody;
            return templates;
        }

        private async Task VerifyRegistration(Dictionary<string, string> parameters, Push push)
        {
            try
            {
                await GetClient().InvokeApiAsync("verifyRegisterInstallationResult", HttpMethod.Get, parameters);
            }
            finally
            {
                push.UnregisterAsync().Wait();
            }

            await GetClient().LogoutAsync();
        }

        private async Task<string> WaitForPush(TimeSpan maximumWait)
        {
            DateTime start = DateTime.UtcNow;
            while (DateTime.UtcNow.Subtract(start) < maximumWait)
            {
                if (GcmService.PushesReceived.Count > 0)
                {
                    return GcmService.PushesReceived.Dequeue();
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            return null;
        }
    }
}