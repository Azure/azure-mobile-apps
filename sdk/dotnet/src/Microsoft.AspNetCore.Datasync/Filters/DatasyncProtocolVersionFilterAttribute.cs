// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Microsoft.AspNetCore.Datasync.Filters
{
    /// <summary>
    /// A resource filter that implements the <c>ZUMO-API-VERSION</c> check and adds the
    /// check to the <see cref="HttpContext"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal class DatasyncProtocolVersionFilterAttribute : Attribute, IResourceFilter
    {
        private const string ProtocolVersionHeader = "ZUMO-API-VERSION";

        /// <summary>
        /// Executes the resource filter.  Called before the execution of the remainder of the
        /// pipeline. Determines whether <c>ZUMO-API-VERSION</c> has been submitted, and returns
        /// a well-formed error message if it has not been submitted.
        /// </summary>
        /// <param name="resourceContext"></param>
        public void OnResourceExecuting(ResourceExecutingContext resourceContext)
        {
            var request = resourceContext.HttpContext.Request;
            ProtocolVersion protocolVersion = ProtocolVersion.Invalid;
            ProblemDetails errorDocument = null;

            // Query takes priority over header
            if (request.Query.ContainsKey(ProtocolVersionHeader))
            {
                bool isValidQuery = request.Query[ProtocolVersionHeader][0].TryParseProtocolVersion(out protocolVersion);
                if (!isValidQuery)
                {
                    errorDocument = new ProblemDetails
                    {
                        Title = $"Invalid {ProtocolVersionHeader} Query Parameter",
                        Detail = $"The value of the {ProtocolVersionHeader} query parameter does not match a known version"
                    };
                }
            }
            else if (request.Headers.ContainsKey(ProtocolVersionHeader))
            {
                bool isValidHeader = request.Headers[ProtocolVersionHeader][0].TryParseProtocolVersion(out protocolVersion);
                if (!isValidHeader)
                {
                    errorDocument = new ProblemDetails
                    {
                        Title = $"Invalid {ProtocolVersionHeader} Header",
                        Detail = $"The value of the {ProtocolVersionHeader} header does not match a known version"
                    };
                }
            }
            else
            {
                errorDocument = new ProblemDetails
                {
                    Title = $"Missing {ProtocolVersionHeader}",
                    Detail = $"The value of {ProtocolVersionHeader} is not specified in headers or query parameters"
                };
            }

            if (errorDocument != null)
            {
                resourceContext.HttpContext.Response.ContentType = "application/problem+json";
                resourceContext.Result = new BadRequestObjectResult(errorDocument);
            }
            else
            {
                resourceContext.HttpContext.Items["ProtocolVersion"] = protocolVersion;
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // Although part of the IResourceFilter interface, this is not required.
        }
    }
}
