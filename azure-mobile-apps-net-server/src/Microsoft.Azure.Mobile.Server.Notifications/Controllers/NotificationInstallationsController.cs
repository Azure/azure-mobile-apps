// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Tracing;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Notifications;
using Microsoft.Azure.Mobile.Server.Properties;
using Microsoft.Azure.NotificationHubs;

namespace Microsoft.Azure.Mobile.Server.Controllers
{
    /// <summary>
    /// The <see cref="NotificationInstallationsController"/> defines an endpoint for managing installation registrations for a
    /// Notification Hub associated with this service.
    /// </summary>
    [MobileAppController]
    public class NotificationInstallationsController : ApiController
    {
        private const string UserIdTagPrefix = "_UserId:";
        private const string UserIdTagPlaceholder = UserIdTagPrefix + "{0}";
        private const string InstallationIdTagPrefix = "$InstallationId:";
        private const string InstallationIdTagPlaceholder = InstallationIdTagPrefix + "{{{0}}}";

        private const int RegistrationPageSize = 1000;

        private const int MaxDeviceRegistrationsSize = 128;
        private const string WindowsStorePlatform = "wns";
        private const string ApplePlatform = "apns";
        private const string MicrosoftPushPlatform = "mpns";
        private const string GooglePlatform = "gcm";

        private PushClient pushClient = null;

        private PushClient PushClient
        {
            get
            {
                if (this.pushClient == null)
                {
                    this.pushClient = this.Configuration.GetPushClient();
                }

                return this.pushClient;
            }
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
        }

        /// <summary>
        /// Enables the registration of a device installation for push notifications via PUT request.
        /// If the specified installationId does not exist on the notification hub, a new installation is created and registered.
        /// If the specified installationId already exists, the installation is modified.
        /// </summary>
        /// <param name="installationId">The installation id to register or modify.</param>
        /// <param name="notificationInstallation">The <see cref="NotificationInstallation"/> object to register.</param>
        /// <returns><see cref="HttpResponseMessage"/> describing the result of the operation.</returns>
        public async Task<HttpResponseMessage> PutInstallation(string installationId, NotificationInstallation notificationInstallation)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, this.ModelState));
            }

            this.ValidateInstallationId(installationId);
            this.ValidateNotificationInstallation(installationId, notificationInstallation);

            ITraceWriter traceWriter = this.Configuration.Services.GetTraceWriter();

            // The installation object that will be sent to NH.
            Installation installation = this.CreateInstallation(notificationInstallation);
            HashSet<string> tagsAssociatedWithInstallationId = await this.GetTagsAssociatedWithInstallationId(notificationInstallation.InstallationId);
            ClaimsPrincipal serviceUser = this.User as ClaimsPrincipal;
            if (tagsAssociatedWithInstallationId.Count == 0)
            {
                // Installation does not exist on NH.  Add it.
                if (installation.Tags == null)
                {
                    installation.Tags = new List<string>();
                }

                // Tag the installation with the UserId if authenticated.
                if (serviceUser != null && serviceUser.Identity.IsAuthenticated)
                {
                    Claim userIdClaim = serviceUser.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null)
                    {
                        string incomingUserTag = string.Format(UserIdTagPlaceholder, userIdClaim.Value);
                        installation.Tags.Add(incomingUserTag);
                    }
                }

                try
                {
                    await this.UpsertInstallationAsync(installation);
                }
                catch (HttpResponseException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    string error = RResources.NotificationHub_CreateOrUpdateInstallationFailed.FormatForUser(installationId, ex.Message);

                    traceWriter.Error(error, ex, this.Request, ServiceLogCategories.NotificationControllers);
                    traceWriter.Error(error, ex, this.Request, LogCategories.NotificationControllers);

                    // We return 4xx status code on error as it is impossible to know whether it is bad input or a
                    // server error from NH. As a result we err on the bad request side.
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
            else
            {
                // Installation already existed on NH.
                if (serviceUser != null && serviceUser.Identity.IsAuthenticated)
                {
                    // Because the user is authenticated, copy all previous tags except UserId.
                    CopyTagsToInstallation(installation, tagsAssociatedWithInstallationId, false);

                    // Add the incoming UserId.
                    Claim userIdClaim = serviceUser.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null)
                    {
                        string incomingUserTag = string.Format(UserIdTagPlaceholder, userIdClaim.Value);
                        AddTagToInstallation(installation, incomingUserTag);
                    }
                }
                else
                {
                    // Because the request is anonymous, copy all previous tags to the installation object, including the previous user tag.
                    CopyTagsToInstallation(installation, tagsAssociatedWithInstallationId, true);
                }

                try
                {
                    await this.UpsertInstallationAsync(installation);
                }
                catch (HttpResponseException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    string error = RResources.NotificationHub_CreateOrUpdateInstallationFailed.FormatForUser(installationId, ex.Message);

                    traceWriter.Error(error, ex, this.Request, ServiceLogCategories.NotificationControllers);
                    traceWriter.Error(error, ex, this.Request, LogCategories.NotificationControllers);

                    // We return 4xx status code on error as it is impossible to know whether it is bad input or a
                    // server error from NH. As a result we err on the bad request side.
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Enables the deletion of the specified device installation registration via DELETE request.
        /// </summary>
        /// <param name="installationId">The installation id to deregister from Notification Hub.</param>
        /// <returns><see cref="HttpResponseMessage"/> describing the result of the operation.</returns>
        public async Task<HttpResponseMessage> DeleteInstallation(string installationId)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, this.ModelState));
            }

            this.ValidateInstallationId(installationId);

            ITraceWriter traceWriter = this.Configuration.Services.GetTraceWriter();

            // Submit the delete
            try
            {
                await this.DeleteInstallationAsync(installationId);
                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string error = RResources.NotificationHub_DeleteInstallationFailed.FormatForUser(installationId, ex.Message);

                traceWriter.Error(error, ex, this.Request, ServiceLogCategories.NotificationControllers);
                traceWriter.Error(error, ex, this.Request, LogCategories.NotificationControllers);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }
        }

        /// <summary>
        /// Copies the tags from the specified set to the Installation object, excluding InstallationId and UserId (conditionally).
        /// </summary>
        /// <param name="installation">The Installation object to copy tags to.</param>
        /// <param name="tags">The set of tags to copy.</param>
        /// <param name="copyUserId">True to copy the UserId tag. False, otherwise.</param>
        internal static void CopyTagsToInstallation(Installation installation, HashSet<string> tags, bool copyUserId)
        {
            if (tags == null)
            {
                installation.Tags = null;
                return;
            }

            if (tags.Count < 1)
            {
                installation.Tags = new List<string>();
            }

            foreach (string tag in tags)
            {
                bool isInstallationTag = tag.StartsWith(InstallationIdTagPrefix, StringComparison.OrdinalIgnoreCase);
                bool isUserIdTag = tag.StartsWith(UserIdTagPrefix, StringComparison.OrdinalIgnoreCase);

                // Do not copy installation ID.
                if (!isInstallationTag)
                {
                    if (isUserIdTag)
                    {
                        // Copy userId only if copyUserId is true.
                        if (copyUserId)
                        {
                            AddTagToInstallation(installation, tag);
                        }
                    }
                    else
                    {
                        // Copy all other tags.
                        AddTagToInstallation(installation, tag);
                    }
                }
            }
        }

        internal static void AddTagToInstallation(Installation installation, string tag)
        {
            if (installation.Tags == null)
            {
                installation.Tags = new List<string>();
            }

            installation.Tags.Add(tag);
        }

        /// <summary>
        /// Gets a set of unique tags associated with the specified installation id from all the associated registrations.
        /// </summary>
        /// <param name="installationId">The installation id to get tags for.</param>
        /// <returns>A set of tags associated with the specified installation Id.  If installation id doesn't exist, set will have zero elements.</returns>
        internal virtual async Task<HashSet<string>> GetTagsAssociatedWithInstallationId(string installationId)
        {
            HashSet<string> tagsAssociatedWithInstallationId = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            string installationIdAsTag = string.Format(InstallationIdTagPlaceholder, installationId);

            string continuationToken = null;
            do
            {
                CollectionQueryResult<RegistrationDescription> registrations = await this.PushClient.GetRegistrationsByTagAsync(installationIdAsTag, continuationToken, RegistrationPageSize);
                continuationToken = registrations.ContinuationToken;
                foreach (RegistrationDescription registration in registrations)
                {
                    if (registration.Tags != null)
                    {
                        // Copy the tags
                        foreach (string tag in registration.Tags)
                        {
                            tagsAssociatedWithInstallationId.Add(tag);
                        }
                    }
                }
            }
            while (continuationToken != null);
            return tagsAssociatedWithInstallationId;
        }

        /// <summary>
        /// Gets the UserId tag, if present, from the specified collection.
        /// </summary>
        /// <param name="tagsCollection">The collection to search for the UserId substring.</param>
        /// <returns>Null if no UserId tag was found; else the UserId tag.</returns>
        internal static string GetUserIdTagFromHashset(HashSet<string> tagsCollection)
        {
            foreach (string tag in tagsCollection)
            {
                if (tag.StartsWith(UserIdTagPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return tag;
                }
            }

            return null;
        }

        /// <summary>
        /// Invokes the <see cref="NotificationHubClient"/> create/update operation.
        /// </summary>
        /// <param name="installation">The <see cref="Installation"/> to create or update.</param>
        /// <returns>A <see cref="Task"/> representing the operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Upsert", Justification = "Upsert is in our vocabulary.")]
        protected virtual async Task UpsertInstallationAsync(Installation installation)
        {
            if (installation == null)
            {
                throw new ArgumentNullException("installation");
            }

            if (installation.InstallationId == null)
            {
                throw new ArgumentNullException("installation.InstallationId");
            }

            if (installation.PushChannel == null)
            {
                throw new ArgumentNullException("installation.PushChannel");
            }

            // NH client will validate the Installation.InstallationId value.
            await this.PushClient.CreateOrUpdateInstallationAsync(installation);
        }

        /// <summary>
        /// Invokes the <see cref="NotificationHubClient"/> delete operation.
        /// </summary>
        /// <param name="installationId">The installation id to delete.</param>
        /// <returns>A <see cref="Task"/> representing the operation.</returns>
        protected internal virtual Task DeleteInstallationAsync(string installationId)
        {
            if (installationId == null)
            {
                throw new ArgumentNullException("installationId");
            }

            return this.PushClient.DeleteInstallationAsync(installationId);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "notificationInstallation", Justification = "Variable name is notificationInstallation"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpResponseMessage is returned to caller")]
        internal Installation CreateInstallation(NotificationInstallation notificationInstallation)
        {
            if (notificationInstallation == null)
            {
                throw new ArgumentNullException("notificationInstallation");
            }

            if (notificationInstallation.Platform == null)
            {
                throw new ArgumentException("notificationInstallation.Platform is null");
            }

            Installation installation = new Installation();
            installation.InstallationId = notificationInstallation.InstallationId;
            installation.PushChannel = notificationInstallation.PushChannel;

            // Set the Platform. Create and add WnsSecondaryTile objects to the installation if WNS.
            if (notificationInstallation.Platform.Equals(WindowsStorePlatform, StringComparison.OrdinalIgnoreCase))
            {
                installation.Platform = NotificationPlatform.Wns;

                // Parse each Secondary Tile passed in request and add to installation object.
                foreach (string tileName in notificationInstallation.SecondaryTiles.Keys)
                {
                    NotificationSecondaryTile notificationTile = notificationInstallation.SecondaryTiles[tileName];
                    if (installation.SecondaryTiles == null)
                    {
                        installation.SecondaryTiles = new Dictionary<string, WnsSecondaryTile>();
                    }

                    installation.SecondaryTiles[tileName] = CreateWnsSecondaryTile(notificationTile);
                }
            }
            else if (notificationInstallation.Platform.Equals(ApplePlatform, StringComparison.OrdinalIgnoreCase))
            {
                installation.Platform = NotificationPlatform.Apns;
            }
            else if (notificationInstallation.Platform.Equals(GooglePlatform, StringComparison.OrdinalIgnoreCase))
            {
                installation.Platform = NotificationPlatform.Gcm;
            }
            else
            {
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_UnsupportedPlatform.FormatForUser(notificationInstallation.Platform)));
            }

            // Create and add InstallationTemplate objects to the installation.
            foreach (string templateName in notificationInstallation.Templates.Keys)
            {
                NotificationTemplate template = notificationInstallation.Templates[templateName];
                if (installation.Templates == null)
                {
                    installation.Templates = new Dictionary<string, InstallationTemplate>();
                }

                installation.Templates[templateName] = CreateInstallationTemplate(template, installation.Platform);
            }

            return installation;
        }

        internal static WnsSecondaryTile CreateWnsSecondaryTile(NotificationSecondaryTile notificationSecondaryTile)
        {
            WnsSecondaryTile secondaryTile = new WnsSecondaryTile();

            // Strip the tags.
            secondaryTile.Tags = new List<string>();
            secondaryTile.PushChannelExpired = null;
            secondaryTile.Templates = new Dictionary<string, InstallationTemplate>();
            if (notificationSecondaryTile != null)
            {
                secondaryTile.PushChannel = notificationSecondaryTile.PushChannel;

                // Parse each template for the Secondary Tile and add to installation object.
                foreach (string templateName in notificationSecondaryTile.Templates.Keys)
                {
                    NotificationTemplate template = notificationSecondaryTile.Templates[templateName];
                    secondaryTile.Templates[templateName] = CreateInstallationTemplate(template, NotificationPlatform.Wns);
                }
            }
            else
            {
                secondaryTile.PushChannel = string.Empty;
            }

            return secondaryTile;
        }

        internal static InstallationTemplate CreateInstallationTemplate(NotificationTemplate notificationTemplate, NotificationPlatform platform)
        {
            if (notificationTemplate != null)
            {
                InstallationTemplate installationTemplate = new InstallationTemplate();

                // strip tags
                installationTemplate.Tags = new List<string>();
                installationTemplate.Body = notificationTemplate.Body;
                if (platform == NotificationPlatform.Wns)
                {
                    installationTemplate.Headers = notificationTemplate.Headers;
                }
                else
                {
                    // Headers is not meaningful for all other platforms
                    installationTemplate.Headers = null;
                }

                return installationTemplate;
            }

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpResponseMessage is returned to caller")]
        internal void ValidateSecondaryTiles(NotificationInstallation notificationInstallation)
        {
            if (notificationInstallation == null)
            {
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_NotificationInstallationWasNull.FormatForUser()));
            }

            foreach (NotificationSecondaryTile secondaryTile in notificationInstallation.SecondaryTiles.Values)
            {
                if (secondaryTile == null)
                {
                    throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_SecondaryTileWasNull.FormatForUser()));
                }

                this.ValidateSecondaryTile(secondaryTile);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpResponseMessage is returned to caller")]
        internal void ValidateSecondaryTile(NotificationSecondaryTile secondaryTile)
        {
            if (secondaryTile == null)
            {
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_SecondaryTileWasNull.FormatForUser()));
            }

            if (string.IsNullOrEmpty(secondaryTile.PushChannel))
            {
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_EmptyPushChannelInSecondaryTile.FormatForUser()));
            }

            if (secondaryTile.Templates == null || secondaryTile.Templates.Count < 1)
            {
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_SecondaryTileTemplatesWasNullOrEmpty.FormatForUser()));
            }

            foreach (NotificationTemplate template in secondaryTile.Templates.Values)
            {
                this.ValidateTemplate(template, WindowsStorePlatform);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpResponseMessage is returned to caller"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "this is used in the exception.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "installationId", Justification = "We are validating it.")]
        internal void ValidateInstallationId(string installationId)
        {
            if (installationId == null)
            {
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_InstallationIdNotSpecifiedInUrl.FormatForUser()));
            }

            Guid idAsGuid;
            bool isGuid = Guid.TryParse(installationId, out idAsGuid);
            if (!isGuid)
            {
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_InstallationIdNotValidGuid.FormatForUser()));
            }
        }

        /// <summary>
        /// Validates the specified parameters provided in the request.  Throws HttpResponseException if there are errors.
        /// </summary>
        /// <param name="installationId">The installation id being registered, parse from URI.</param>
        /// <param name="notificationInstallation">The installation object parsed from body.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpResponseMessage is returned to caller.")]
        internal void ValidateNotificationInstallation(string installationId, NotificationInstallation notificationInstallation)
        {
            if (notificationInstallation == null)
            {
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_NotificationInstallationWasNull.FormatForUser()));
            }

            if (notificationInstallation.InstallationId != null && !installationId.ToString().Equals(notificationInstallation.InstallationId, StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_InstallationIdDoesntMatchUri.FormatForUser()));
            }

            // Store the installation id into NotificationInstallation object.
            notificationInstallation.InstallationId = installationId.ToString();

            // Check the required fields of the NotificationInstallation object.  If failed, return Bad Request.
            if (string.IsNullOrEmpty(notificationInstallation.PushChannel))
            {
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_MissingPropertyInRequestBody.FormatForUser("pushChannel")));
            }

            if (string.IsNullOrEmpty(notificationInstallation.Platform))
            {
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_MissingPropertyInRequestBody.FormatForUser("platform")));
            }

            if (string.Equals(notificationInstallation.Platform, WindowsStorePlatform, StringComparison.OrdinalIgnoreCase))
            {
                if (notificationInstallation.SecondaryTiles != null)
                {
                    this.ValidateSecondaryTiles(notificationInstallation);
                }
            }

            foreach (NotificationTemplate template in notificationInstallation.Templates.Values)
            {
                this.ValidateTemplate(template, notificationInstallation.Platform);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpResponseMessage is returned to caller")]
        internal void ValidateTemplate(NotificationTemplate template, string platform)
        {
            if (template == null)
            {
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_TemplateWasNull.FormatForUser()));
            }

            if (template.Body == null)
            {
                template.Body = string.Empty;
            }

            if (platform.Equals(WindowsStorePlatform, StringComparison.OrdinalIgnoreCase))
            {
                if (template.Headers != null)
                {
                    foreach (string headerName in template.Headers.Keys)
                    {
                        if (string.IsNullOrEmpty(template.Headers[headerName]))
                        {
                            throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_EmptyHeaderValueInSecondaryTileForHeaderName.FormatForUser(headerName)));
                        }
                    }
                }
            }
            else if (platform.Equals(ApplePlatform, StringComparison.OrdinalIgnoreCase) || platform.Equals(GooglePlatform, StringComparison.OrdinalIgnoreCase))
            {
                if (template.Headers != null)
                {
                    throw new HttpResponseException(this.Request.CreateBadRequestResponse(RResources.NotificationHub_DoesNotSupportTemplateHeaders.FormatForUser(platform)));
                }
            }
        }
    }
}