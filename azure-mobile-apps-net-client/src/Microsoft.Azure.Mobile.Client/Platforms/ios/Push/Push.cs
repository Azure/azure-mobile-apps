//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using Foundation;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Define a class help to create/update/delete notification registrations
    /// </summary>
    public sealed class Push
    {
        internal readonly PushHttpClient PushHttpClient;
        private IMobileServiceClient Client { get; set; }

        internal Push(IMobileServiceClient client)
        {
            Arguments.IsNotNull(client, nameof(client));

            Client = client;
            if (!(client is MobileServiceClient internalClient))
            {
                throw new ArgumentException("Client must be a MobileServiceClient object");
            }
            this.PushHttpClient = new PushHttpClient(internalClient);
        }

        /// <summary>
        /// Installation Id used to register the device with Notification Hubs
        /// </summary>
        public string InstallationId => Client.InstallationId;

        /// <summary>
        /// Register an Installation with particular deviceToken
        /// </summary>
        /// <param name="deviceToken">The deviceToken to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(NSData deviceToken) => RegisterAsync(deviceToken, null);

        /// <summary>
        /// Register an Installation with particular deviceToken and templates
        /// </summary>
        /// <param name="deviceToken">The deviceToken to register</param>
        /// <param name="templates">JSON with one more templates to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(NSData deviceToken, JObject templates)
        {
            string channelUri = ParseDeviceToken(deviceToken);
            JObject installation = new JObject
            {
                [PushInstallationProperties.PUSHCHANNEL] = channelUri,
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
        public Task UnregisterAsync() => PushHttpClient.DeleteInstallationAsync();

        internal static string ParseDeviceToken(NSData deviceToken)
        {
            if (deviceToken == null)
            {
                throw new ArgumentNullException(nameof(deviceToken));
            }
            byte[] byteArray = deviceToken.ToArray();
            if (byteArray.Length == 0)
            {
                throw new ArgumentException($"{nameof(deviceToken)} cannot be empty.", nameof(deviceToken));
            }
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }
}
