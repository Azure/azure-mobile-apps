// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.Azure.Mobile.Server.Properties;

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// Configures the specified <see cref="System.Web.Http.HttpConfiguration" /> with settings used by
    /// Azure Mobile Apps.
    /// </summary>
    public class MobileAppConfiguration : AppConfiguration
    {
        private bool enableApiControllers = false;
        private IMobileAppControllerConfigProvider configProvider = new MobileAppControllerConfigProvider();

        /// <summary>
        /// Initializes a new instance of the <see cref="Microsoft.Azure.Mobile.Server.Config.MobileAppConfiguration" /> class.
        /// </summary>
        public MobileAppConfiguration()
        {
            this.BaseRouteConstraints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        internal HashSet<string> BaseRouteConstraints { get; set; }

        /// <inheritdoc />
        public override void ApplyTo(HttpConfiguration config)
        {
            if (config.GetMobileAppConfiguration() != null)
            {
                throw new InvalidOperationException(RResources.ApplyTo_CalledTwice);
            }

            config.SetMobileAppConfiguration(this);
            config.SetMobileAppControllerConfigProvider(this.configProvider);

            base.ApplyTo(config);

            if (this.enableApiControllers)
            {
                this.MapApiControllers(config);
            }
        }

        /// <summary>
        /// Maps all controllers with the <see cref="Microsoft.Azure.Mobile.Server.Config.MobileAppControllerAttribute"/> to the route
        /// template "api/{controller}/{id}".
        /// </summary>
        /// <returns>The current <see cref="Microsoft.Azure.Mobile.Server.Config.MobileAppConfiguration"/>.</returns>
        public MobileAppConfiguration MapApiControllers()
        {
            this.enableApiControllers = true;
            return this;
        }

        /// <summary>
        /// Registers the specified <see cref="IMobileAppControllerConfigProvider" /> with the <see cref="HttpConfiguration"/>.
        /// Use this to override the default controller configuration.
        /// </summary>
        /// <param name="provider">The provider to register.</param>
        /// <returns>The current <see cref="Microsoft.Azure.Mobile.Server.Config.MobileAppConfiguration"/>.</returns>
        public MobileAppConfiguration WithMobileAppControllerConfigProvider(IMobileAppControllerConfigProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            this.configProvider = provider;
            return this;
        }

        /// <summary>
        /// Adds a controller name to be excluded from the default route of 'api/{controller}/{id}'.
        /// </summary>
        /// <param name="exclusion">The controller name to exclude from the route. Do not include 'Controller' at the end of the string.</param>
        public void AddBaseRouteExclusion(string exclusion)
        {
            if (string.IsNullOrWhiteSpace(exclusion))
            {
                throw new ArgumentNullException("exclusion");
            }

            this.BaseRouteConstraints.Add(exclusion);
        }

        private void MapApiControllers(HttpConfiguration config)
        {
            HashSet<string> tableControllerNames = config.GetMobileAppControllerNames();
            tableControllerNames.RemoveWhere(n => this.BaseRouteConstraints.Contains(n));

            SetRouteConstraint<string> apiControllerConstraint = new SetRouteConstraint<string>(tableControllerNames, matchOnExcluded: false);

            HttpRouteCollectionExtensions.MapHttpRoute(
                config.Routes,
                name: RouteNames.Apis,
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: new { controller = apiControllerConstraint });
        }
    }
}