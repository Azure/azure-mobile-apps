// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Android.App;
using Android.Content;
using Gcm.Client;
using System.Collections.Generic;

namespace DeviceTests.Android.Services
{
    [Service]
    public class GcmService : GcmServiceBase
    {
        internal static string RegistrationId = string.Empty;
        internal static Queue<string> PushesReceived = new Queue<string>();

        public GcmService() : base(GcmBroadcastReceiver.SENDER_IDS)
        {
        }

        protected override void OnError(Context context, string errorId) 
        { 
        }

        protected override void OnMessage(Context context, Intent intent)
        {
            var message = "Empty Message";
            if (intent != null && intent.Extras != null)
            {
                message = intent.Extras.GetString("message");
            }
            GcmService.PushesReceived.Enqueue(message);
        }

        protected override void OnRegistered(Context context, string registrationId)
        {
            GcmService.RegistrationId = registrationId;
        }

        protected override void OnUnRegistered(Context context, string registrationId)
        {
            GcmService.RegistrationId = string.Empty;
        }
    }
}