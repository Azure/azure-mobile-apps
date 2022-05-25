// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    /// <summary>
    /// Marker interface identifying various notification payloads that can be sent to the Notification Hub.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "Marker interface used for compile time detection.")]
    public interface IPushMessage
    {
    }
}
