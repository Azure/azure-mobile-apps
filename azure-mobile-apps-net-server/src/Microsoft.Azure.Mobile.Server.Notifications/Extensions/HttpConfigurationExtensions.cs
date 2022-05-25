// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Mobile.Server.Notifications;

namespace System.Web.Http
{
    public static class HttpConfigurationExtensions
    {
        private const string PushClientKey = "MS_PushClientKey";

        public static PushClient GetPushClient(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            PushClient client;
            if (!config.Properties.TryGetValue(PushClientKey, out client))
            {
                client = new PushClient(config);
                config.Properties[PushClientKey] = client;
            }

            return client;
        }

        public static void SetPushClient(this HttpConfiguration config, PushClient client)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            config.Properties[PushClientKey] = client;
        }
    }
}