// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Tracing;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Notifications;
using Microsoft.Azure.NotificationHubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZumoE2EServerApp.Controllers
{
    [MobileAppController]
    public class PushApiController : ApiController
    {
        private ITraceWriter traceWriter;
        private PushClient pushClient;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            this.traceWriter = this.Configuration.Services.GetTraceWriter();
            this.pushClient = new PushClient(this.Configuration);
        }

        [Route("api/push")]
        public async Task<HttpResponseMessage> Post()
        {
            var data = await this.Request.Content.ReadAsAsync<JObject>();
            var method = (string)data["method"];

            if (method == null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            if (method == "send")
            {
                var serialize = new JsonSerializer();

                var token = (string)data["token"];


                if (data["payload"] == null || token == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }

                // Payload could be a string or a dictionary
                var payloadString = data["payload"].ToString();

                var type = (string)data["type"];
                var tag = (string)data["tag"];


                if (type == "template")
                {
                    TemplatePushMessage message = new TemplatePushMessage();
                    var payload = JObject.Parse(payloadString);
                    var keys = payload.Properties();
                    foreach (JProperty key in keys)
                    {
                        this.traceWriter.Info("Key: " + key.Name);

                        message.Add(key.Name, (string)key.Value);
                    }
                    var result = await this.pushClient.SendAsync(message);
                }
                else if (type == "gcm")
                {
                    GooglePushMessage message = new GooglePushMessage();
                    message.JsonPayload = payloadString;
                    if (tag != null)
                    {
                        await this.pushClient.SendAsync(message, tag);
                    }
                    else
                    {
                        await this.pushClient.SendAsync(message);
                    }
                }
                else if (type == "apns")
                {
                    ApplePushMessage message = new ApplePushMessage();
                    this.traceWriter.Info(payloadString.ToString());
                    message.JsonPayload = payloadString.ToString();
                    if (tag != null)
                    {
                        await this.pushClient.SendAsync(message, tag);
                    }
                    else
                    {
                        await this.pushClient.SendAsync(message);
                    }
                }
                else if (type == "wns")
                {
                    var wnsType = (string)data["wnsType"];
                    WindowsPushMessage message = new WindowsPushMessage();
                    message.XmlPayload = payloadString;
                    message.Headers.Add("X-WNS-Type", type + '/' + wnsType);
                    if (tag != null)
                    {
                        await this.pushClient.SendAsync(message, tag);
                    }
                    else
                    {
                        await this.pushClient.SendAsync(message);
                    }
                }
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [Route("api/verifyRegisterInstallationResult")]
        public async Task<bool> GetVerifyRegisterInstallationResult(string channelUri, string templates = null, string secondaryTiles = null)
        {
            var nhClient = this.GetNhClient();
            HttpResponseMessage msg = new HttpResponseMessage();
            msg.StatusCode = HttpStatusCode.InternalServerError;
            IEnumerable<string> installationIds;
            if (this.Request.Headers.TryGetValues("X-ZUMO-INSTALLATION-ID", out installationIds))
            {
                return await Retry(async () =>
                {
                    var installationId = installationIds.FirstOrDefault();

                    Installation nhInstallation = await nhClient.GetInstallationAsync(installationId);
                    string nhTemplates = null;
                    string nhSecondaryTiles = null;

                    if (nhInstallation.Templates != null)
                    {
                        nhTemplates = JsonConvert.SerializeObject(nhInstallation.Templates);
                        nhTemplates = Regex.Replace(nhTemplates, @"\s+", String.Empty);
                        templates = Regex.Replace(templates, @"\s+", String.Empty);
                    }
                    if (nhInstallation.SecondaryTiles != null)
                    {
                        nhSecondaryTiles = JsonConvert.SerializeObject(nhInstallation.SecondaryTiles);
                        nhSecondaryTiles = Regex.Replace(nhSecondaryTiles, @"\s+", String.Empty);
                        secondaryTiles = Regex.Replace(secondaryTiles, @"\s+", String.Empty);
                    }
                    if (nhInstallation.PushChannel.ToLower() != channelUri.ToLower())
                    {
                        msg.Content = new StringContent(string.Format("ChannelUri did not match. Expected {0} Found {1}", channelUri, nhInstallation.PushChannel));
                        throw new HttpResponseException(msg);
                    }
                    if (templates != nhTemplates)
                    {
                        msg.Content = new StringContent(string.Format("Templates did not match. Expected {0} Found {1}", templates, nhTemplates));
                        throw new HttpResponseException(msg);
                    }
                    if (secondaryTiles != nhSecondaryTiles)
                    {
                        msg.Content = new StringContent(string.Format("SecondaryTiles did not match. Expected {0} Found {1}", secondaryTiles, nhSecondaryTiles));
                        throw new HttpResponseException(msg);
                    }
                    bool tagsVerified = await VerifyTags(channelUri, installationId, nhClient);
                    if (!tagsVerified)
                    {
                        msg.Content = new StringContent("Did not find installationId tag");
                        throw new HttpResponseException(msg);
                    }
                    return true;
                });
            }
            msg.Content = new StringContent("Did not find X-ZUMO-INSTALLATION-ID header in the incoming request");
            throw new HttpResponseException(msg);
        }

        [Route("api/verifyUnregisterInstallationResult")]
        public async Task<bool> GetVerifyUnregisterInstallationResult()
        {
            IEnumerable<string> installationIds;
            string responseErrorMessage = null;
            if (this.Request.Headers.TryGetValues("X-ZUMO-INSTALLATION-ID", out installationIds))
            {
                return await Retry(async () =>
                {
                    var installationId = installationIds.FirstOrDefault();
                    try
                    {
                        Installation nhInstallation = await this.GetNhClient().GetInstallationAsync(installationId);
                    }
                    catch (Exception)
                    {
                        return true;
                    }
                    responseErrorMessage = string.Format("Found deleted Installation with id {0}", installationId);
                    return false;
                });
            }

            HttpResponseMessage msg = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(responseErrorMessage)
            };
            throw new HttpResponseException(msg);
        }

        [Route("api/deleteRegistrationsForChannel")]
        public async Task<HttpResponseMessage> DeleteRegistrationsForChannel(string channelUri)
        {
            var result = await Retry(async () =>
            {
                await this.GetNhClient().DeleteRegistrationsByChannelAsync(channelUri);
                return true;
            });

            if (result)
            {
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(new { result = true })) };
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    result = false,
                    error = "Can't delete Registrations For Channel"
                }))
            };
        }

        [Route("api/register")]
        public void Register(string data)
        {
            var installation = JsonConvert.DeserializeObject<Installation>(data);
            new PushClient(this.Configuration).HubClient.CreateOrUpdateInstallation(installation);
        }

        private NotificationHubClient GetNhClient()
        {
            var settings = this.Configuration.GetMobileAppSettingsProvider().GetMobileAppSettings();
            string notificationHubName = settings.NotificationHubName;
            string notificationHubConnection = settings.Connections[MobileAppSettingsKeys.NotificationHubConnectionString].ConnectionString;
            return NotificationHubClient.CreateClientFromConnectionString(notificationHubConnection, notificationHubName);
        }

        private async Task<bool> VerifyTags(string channelUri, string installationId, NotificationHubClient nhClient)
        {
            IPrincipal user = this.User;
            int expectedTagsCount = 1;
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                expectedTagsCount = 2;
            }

            string continuationToken = null;
            do
            {
                CollectionQueryResult<RegistrationDescription> regsForChannel = await nhClient.GetRegistrationsByChannelAsync(channelUri, continuationToken, 100);
                continuationToken = regsForChannel.ContinuationToken;
                foreach (RegistrationDescription reg in regsForChannel)
                {
                    RegistrationDescription registration = await nhClient.GetRegistrationAsync<RegistrationDescription>(reg.RegistrationId);
                    if (registration.Tags == null || registration.Tags.Count() < expectedTagsCount)
                    {
                        return false;
                    }
                    if (!registration.Tags.Contains("$InstallationId:{" + installationId + "}"))
                    {
                        return false;
                    }

                    ClaimsIdentity identity = user.Identity as ClaimsIdentity;
                    Claim userIdClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    string userId = (userIdClaim != null) ? userIdClaim.Value : string.Empty;

                    if (user.Identity != null && user.Identity.IsAuthenticated && !registration.Tags.Contains("_UserId:" + userId))
                    {
                        return false;
                    }
                }
            } while (continuationToken != null);
            return true;
        }

        private async Task<bool> Retry(Func<Task<bool>> target)
        {
            var sleepTimes = new int[3] { 1000, 3000, 5000 };

            for (var i = 0; i < sleepTimes.Length; i++)
            {
                System.Threading.Thread.Sleep(sleepTimes[i]);

                try
                {
                    // if the call succeeds, return the result
                    return await target();
                }
                catch (Exception)
                {
                    // if an exception was thrown and we've already retried three times, rethrow
                    if (i == 2)
                        throw;
                }
            }
            return false;
        }
    }
}
