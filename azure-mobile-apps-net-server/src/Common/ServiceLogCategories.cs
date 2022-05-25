// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Azure.Mobile
{
    /// <summary>
    /// Category names traced by the service infrastructure. By separating it out from the default Web API 
    /// categories and from other user categories it is possible to filter them out in the tracing
    /// logic.
    /// </summary>
    internal static class ServiceLogCategories
    {
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate", Justification = "readonly is preferable here.")]
        public static readonly string Prefix = "Service.";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string Action = Prefix + "Action";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string Config = Prefix + "Configuration";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string Controllers = Prefix + "Controllers";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string Diagnostics = Prefix + "Diagnostics";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string Filters = Prefix + "Filters";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string Formatting = Prefix + "Formatting";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string Hosting = Prefix + "Hosting";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string JobsController = Prefix + "Controllers.Jobs";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string MessageHandlers = Prefix + "MessageHandlers";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string ModelBinding = Prefix + "ModelBinding";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string NotificationControllers = Prefix + "Controllers.Notification";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string Request = Prefix + "Request";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string Routing = Prefix + "Routing";

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Not all members are used in all contexts.")]
        public static readonly string TableControllers = Prefix + "Controllers.Tables";
    }
}
