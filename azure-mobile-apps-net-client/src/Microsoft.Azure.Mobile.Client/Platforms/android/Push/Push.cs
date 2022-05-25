//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Define a class help to create/delete notification registrations
    /// </summary>
    public sealed class Push
    {
        internal readonly PushHttpClient PushHttpClient;
        private IMobileServiceClient Client { get; set; }

        internal Push(IMobileServiceClient client)
        {
            Arguments.IsNotNull(client, nameof(client));

            if (!(client is MobileServiceClient internalClient))
            {
                throw new ArgumentException("Client must be a MobileServiceClient object", nameof(client));
            }

            PushHttpClient = new PushHttpClient(internalClient);
            Client = client;
        }

        /// <summary>
        /// Installation Id used to register the device with Notification Hubs
        /// </summary>
        public string InstallationId => this.Client.InstallationId;

        /// <summary>
        /// Register an Installation with particular registrationId
        /// </summary>
        /// <param name="registrationId">The registrationId to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(string registrationId) => RegisterAsync(registrationId, null);

        /// <summary>
        /// Register an Installation with particular registrationId and templates
        /// </summary>
        /// <param name="registrationId">The registrationId to register</param>
        /// <param name="templates">JSON with one more templates to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(string registrationId, JObject templates)
        {
            Arguments.IsNotNullOrWhiteSpace(registrationId, nameof(registrationId));

            JObject installation = new JObject
            {
                [PushInstallationProperties.PUSHCHANNEL] = registrationId,
                [PushInstallationProperties.PLATFORM] = Platform.Instance.PushUtility.GetPlatform()
            };
            if (templates != null)
            {
                JObject templatesWithStringBody = templates;
                foreach (JProperty template in templates.Properties())
                {
                    //Notification hub requires template body to be a string.Convert to string from JObject
                    var templateBody = template.Value["body"];
                    if (templateBody != null && templateBody.GetType() == typeof(JObject))
                    {
                        templatesWithStringBody[template.Name]["body"] = templateBody.ToString();
                    }
                }
                installation[PushInstallationProperties.TEMPLATES] = templatesWithStringBody;
            }
            return PushHttpClient.CreateOrUpdateInstallationAsync(installation);
        }

        /// <summary>
        /// Unregister any installations for a particular app
        /// </summary>
        /// <returns>Task that completes when unregister is complete</returns>
        public Task UnregisterAsync()
        {
            return PushHttpClient.DeleteInstallationAsync();
        }
    }
}
