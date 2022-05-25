// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Web.Http.Controllers;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    /// <summary>
    /// Provides an abstraction for performing configuration customizations for <see cref="TableController{TData}"/> derived
    /// controllers. An implementation can be registered via the <see cref="System.Web.Http.HttpConfiguration"/>.
    /// </summary>
    public interface ITableControllerConfigProvider
    {
        /// <summary>
        /// Configures the settings specific for controllers of type <see cref="TableController{TData}"/>.
        /// </summary>
        /// <param name="controllerSettings">The <see cref="HttpControllerSettings"/> for this controller type.</param>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor"/> for this controller type.</param>
        void Configure(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor);
    }
}