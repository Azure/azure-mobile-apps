// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.Azure.Mobile.Server.Properties;

namespace System.Net.Http
{
    /// <summary>
    /// Extension methods for <see cref="HttpRequestMessage"/> providing various utilities.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Gets the first HTTP header value as a single value or <c>null</c> if not present.
        /// </summary>
        /// <param name="request">The request for where to look for the header.</param>
        /// <param name="name">The name of the header.</param>
        /// <returns>The first HTTP header value or <c>null</c> if not present.</returns>
        public static string GetHeaderOrDefault(this HttpRequestMessage request, string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(name);
            }

            if (request == null || request.Headers == null)
            {
                return null;
            }

            IEnumerable<string> values;
            if (request.Headers.TryGetValues(name, out values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Checks if the request is conditional having a <c>If-None-Match</c> HTTP header field with a value that matches the
        /// <paramref name="current"/> value. In the case of <c>true</c> this can be used to indicate that a
        /// 304 (Not Modified) or a 412 (Precondition Failed) status code should be used.
        /// </summary>
        /// <param name="request">The request to match.</param>
        /// <param name="current">The current etag for the resource. If there is no current etag (i.e. the resource does not yet
        /// exist) then use <c>null</c>.</param>
        /// <returns>Returns <c>true</c> if one of the <c>If-None-Match</c> values match the current etag; or the
        /// <c>If-None-Match</c> value is "*" and <paramref name="current"/> is not null; otherwise false.</returns>
        public static bool IsIfNoneMatch(this HttpRequestMessage request, EntityTagHeaderValue current)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.Headers.IfNoneMatch != null)
            {
                foreach (EntityTagHeaderValue etag in request.Headers.IfNoneMatch)
                {
                    if (current != null && (etag == EntityTagHeaderValue.Any || current.Equals(etag)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Creates an <see cref="HttpResponseMessage"/> with an <see cref="HttpStatusCode.NotFound"/> status code and a default
        /// <see cref="System.Web.Http.HttpError"/> as HTTP response body.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <returns>An initialized <see cref="HttpResponseMessage"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Parameters are validated by Create due to how extension methods are resolved.")]
        public static HttpResponseMessage CreateNotFoundResponse(this HttpRequestMessage request)
        {
            return request.Create(HttpStatusCode.NotFound, RResources.HttpNotFound.FormatForUser(request.RequestUri.AbsoluteUri));
        }

        /// <summary>
        /// Creates an <see cref="HttpResponseMessage"/> with an <see cref="HttpStatusCode.NotFound"/> status code and a
        /// <see cref="System.Web.Http.HttpError"/> containing the provided message as HTTP response body.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>An initialized <see cref="HttpResponseMessage"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Parameters are validated by Create due to how extension methods are resolved.")]
        public static HttpResponseMessage CreateNotFoundResponse(this HttpRequestMessage request, string format, params object[] args)
        {
            return request.Create(HttpStatusCode.NotFound, format, args);
        }

        /// <summary>
        /// Creates an <see cref="HttpResponseMessage"/> with an <see cref="HttpStatusCode.Unauthorized"/> status code and a  default
        /// <see cref="System.Web.Http.HttpError"/> as HTTP response body.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <returns>An initialized <see cref="HttpResponseMessage"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Parameters are validated by Create due to how extension methods are resolved.")]
        public static HttpResponseMessage CreateUnauthorizedResponse(this HttpRequestMessage request)
        {
            return request.Create(HttpStatusCode.Unauthorized, RResources.HttpUnauthorized);
        }

        /// <summary>
        /// Creates an <see cref="HttpResponseMessage"/> with an <see cref="HttpStatusCode.Unauthorized"/> status code and a
        /// <see cref="System.Web.Http.HttpError"/> containing the provided message as HTTP response body.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>An initialized <see cref="HttpResponseMessage"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Parameters are validated by Create due to how extension methods are resolved.")]
        public static HttpResponseMessage CreateUnauthorizedResponse(this HttpRequestMessage request, string format, params object[] args)
        {
            return request.Create(HttpStatusCode.Unauthorized, format, args);
        }

        /// <summary>
        /// Creates an <see cref="HttpResponseMessage"/> with an <see cref="HttpStatusCode.BadRequest"/> status code and a  default
        /// <see cref="System.Web.Http.HttpError"/> as HTTP response body.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <returns>An initialized <see cref="HttpResponseMessage"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Parameters are validated by Create due to how extension methods are resolved.")]
        public static HttpResponseMessage CreateBadRequestResponse(this HttpRequestMessage request)
        {
            return request.Create(HttpStatusCode.BadRequest, RResources.HttpBadRequest);
        }

        /// <summary>
        /// Creates an <see cref="HttpResponseMessage"/> with an <see cref="HttpStatusCode.BadRequest"/> status code and a
        /// <see cref="System.Web.Http.HttpError"/> containing the provided message as HTTP response body.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>An initialized <see cref="HttpResponseMessage"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Parameters are validated by Create due to how extension methods are resolved.")]
        public static HttpResponseMessage CreateBadRequestResponse(this HttpRequestMessage request, string format, params object[] args)
        {
            return request.Create(HttpStatusCode.BadRequest, format, args);
        }

        private static HttpResponseMessage Create(this HttpRequestMessage request, HttpStatusCode status, string format, params object[] args)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (format == null)
            {
                throw new ArgumentNullException("format");
            }

            string msg = (args != null && args.Length > 0) ? string.Format(CultureInfo.CurrentCulture, format, args) : format;

            return request.CreateErrorResponse(status, msg);
        }
    }
}