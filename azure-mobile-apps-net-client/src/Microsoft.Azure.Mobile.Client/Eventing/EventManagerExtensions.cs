﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Eventing
{
    internal static class EventManagerExtensions
    {
        internal static void BackgroundPublish(this IMobileServiceEventManager manager, IMobileServiceEvent mobileServiceEvent)
        {
            manager.PublishAsync(mobileServiceEvent)
                .ContinueWith(t => t.Exception.Handle(e => true), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
