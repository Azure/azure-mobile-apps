// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// Optional log categories which can be used by the service when logging messages.
    /// </summary>
    public static class LogCategories
    {
        /// <summary>
        /// Provides a common prefix for all log categories defined by <see cref="LogCategories"/>.
        /// </summary>
        public static readonly string Prefix = "App.";

        /// <summary>
        /// Log category identifying logs coming from an <see cref="System.Web.Http.ApiController"/> action.
        /// </summary>
        public static readonly string Action = Prefix + "Action";

        /// <summary>
        /// Log category identifying logs coming from a configuration component.
        /// </summary>
        public static readonly string Config = Prefix + "Configuration";

        /// <summary>
        /// Log category identifying logs coming from an <see cref="System.Web.Http.ApiController"/>.
        /// </summary>
        public static readonly string Controllers = Prefix + "Controllers";

        /// <summary>
        /// Log category identifying logs coming from the <see cref="JobsController"/>.
        /// </summary>
        public static readonly string JobsController = Prefix + "Controllers.Jobs";

        /// <summary>
        /// Log category identifying logs coming from an ASP.NET Web API Filter (see <see cref="System.Web.Http.Filters.IFilter"/>).
        /// </summary>
        public static readonly string Filters = Prefix + "Filters";

        /// <summary>
        /// Log category identifying logs coming from an ASP.NET Web API <see cref="System.Net.Http.Formatting.MediaTypeFormatter"/>.
        /// </summary>
        public static readonly string Formatting = Prefix + "Formatting";

        /// <summary>
        /// Log category identifying logs coming from the hosting layer.
        /// </summary>
        public static readonly string Hosting = Prefix + "Hosting";

        /// <summary>
        /// Log category identifying logs coming from an ASP.NET Web API Message Handler (see <see cref="System.Net.Http.DelegatingHandler"/>).
        /// </summary>
        public static readonly string MessageHandlers = Prefix + "MessageHandlers";

        /// <summary>
        /// Log category identifying logs coming from an ASP.NET Web API Model Binding.
        /// </summary>
        public static readonly string ModelBinding = Prefix + "ModelBinding";

        /// <summary>
        /// Log category identifying logs coming from the installation registration controller for Notification Hub.
        /// </summary>
        public static readonly string NotificationControllers = Prefix + "Controllers.Notification";

        /// <summary>
        /// Log category identifying logs coming from a part of the ASP.NET Web API Request flow.
        /// </summary>
        public static readonly string Request = Prefix + "Request";

        /// <summary>
        /// Log category identifying logs coming from an ASP.NET Web API Routing.
        /// </summary>
        public static readonly string Routing = Prefix + "Routing";

        /// <summary>
        /// Log category identifying logs coming from security related processing.
        /// </summary>
        public static readonly string Security = Prefix + "Security";

        /// <summary>
        /// Log category identifying logs coming from a Table Controller.
        /// </summary>
        public static readonly string TableControllers = Prefix + "Controllers.Tables";

        /// <summary>
        /// Log category identifying logs coming from a Health Reporter.
        /// </summary>
        public static readonly string HealthReporter = Prefix + "HealthReporter";
    }
}
