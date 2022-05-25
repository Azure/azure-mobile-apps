// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;

namespace Microsoft.Azure.Mobile.Server.Swagger
{
    public class MobileAppApiExplorer : ApiExplorer
    {
        public MobileAppApiExplorer(HttpConfiguration config)
            : base(config)
        {
        }

        public override bool ShouldExploreController(string controllerVariableValue, HttpControllerDescriptor controllerDescriptor, IHttpRoute route)
        {
            if (route == null)
            {
                throw new ArgumentNullException("route");
            }

            bool result = base.ShouldExploreController(controllerVariableValue, controllerDescriptor, route);

            object constraint;

            if (route.Constraints.TryGetValue("controller", out constraint))
            {
                IHttpRouteConstraint typedConstraint = constraint as IHttpRouteConstraint;
                return result && typedConstraint.Match(null, route, "controller", new Dictionary<string, object> { { "controller", controllerVariableValue } }, HttpRouteDirection.UriResolution);
            }
            return result;
        }
    }
}