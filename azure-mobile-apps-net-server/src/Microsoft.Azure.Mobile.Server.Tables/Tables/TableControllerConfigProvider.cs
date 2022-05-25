// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.Azure.Mobile.Server.Serialization;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    /// <summary>
    /// Customizes settings for <see cref="TableController{TData}"/> derived controllers.
    /// </summary>
    public class TableControllerConfigProvider : ITableControllerConfigProvider
    {
        /// <inheritdoc />
        public virtual void Configure(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerSettings == null)
            {
                throw new ArgumentNullException("controllerSettings");
            }

            if (controllerDescriptor == null)
            {
                throw new ArgumentNullException("controllerDescriptor");
            }

            // We need to remove the xml formatter because it cannot handle the wrapped
            // results this controller produces for inline count, etc.
            controllerSettings.Formatters.Remove(controllerSettings.Formatters.XmlFormatter);

            // Add additional query related filters for the same actions with a QueryableAttribute
            // The Filter Provider ensures that the additional filters are always *after* the query filter as we
            // want the IQueryable to have been set up before we do additional work on it.
            controllerSettings.Services.Add(typeof(IFilterProvider), new TableFilterProvider());

            // Register a ContractResolver with the JSON formatter that can handle Delta<T> correctly
            JsonMediaTypeFormatter jsonFormatter = controllerSettings.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new TableContractResolver(jsonFormatter);
        }
    }
}