﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AzureMobile.Server.Tables
{
    /// <summary>
    /// A resource filter that implements the X-ZUMO-Version check and adds the
    /// check to the <see cref="HttpContext"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal class ZumoVersionFilterAttribute : Attribute, IResourceFilter
    {
        private const string ZumoVersionHeader = "X-ZUMO-Version";

        /// <summary>
        /// Executes the resource filter.  Called before the execution of the remainder of the
        /// pipeline. Determines whether <c>X-ZUMO-Version</c> has been submitted, and returns
        /// a well-formed error message if it has not been submitted.
        /// </summary>
        /// <param name="resourceContext"></param>
        public void OnResourceExecuting(ResourceExecutingContext resourceContext)
        {
            var request = resourceContext.HttpContext.Request;
            ZumoVersion zumoVersion = ZumoVersion.Invalid;
            ProblemDetails errorDocument = null;

            // Query takes priority over header
            if (request.Query.ContainsKey(ZumoVersionHeader))
            {
                bool isValidQuery = request.Query[ZumoVersionHeader][0].TryParseZumoVersion(out zumoVersion);
                if (!isValidQuery)
                {
                    errorDocument = new ProblemDetails
                    {
                        Title = "Invalid X-ZUMO-Version Query Parameter",
                        Detail = "The value of the X-ZUMO-Version query parameter does not match a known version"
                    };
                }
            }
            else if (request.Headers.ContainsKey(ZumoVersionHeader))
            {
                bool isValidHeader = request.Headers[ZumoVersionHeader][0].TryParseZumoVersion(out zumoVersion);
                if (!isValidHeader)
                {
                    errorDocument = new ProblemDetails
                    {
                        Title = "Invalid X-ZUMO-Version Header",
                        Detail = "The value of the X-ZUMO-Version header does not match a known version"
                    };
                }
            }
            else
            {
                errorDocument = new ProblemDetails
                {
                    Title = "Missing X-ZUMO-Version",
                    Detail = "The value of X-ZUMO-Version is not specified in headers or query parameters"
                };
            }

            if (errorDocument != null)
            {
                resourceContext.HttpContext.Response.ContentType = "application/problem+json";
                resourceContext.Result = new BadRequestObjectResult(errorDocument);
            }
            else
            {
                resourceContext.HttpContext.Items["ZumoVersion"] = zumoVersion;
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // Although part of the IResourceFilter interface, this is not required.
        }
    }
}
