// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Azure.Mobile.Server.Content
{
    /// <summary>
    /// Generates an HTML-formatted <see cref="System.Web.Http.IHttpActionResult"/> from an embedded resource.
    /// </summary>
    public class StaticHtmlActionResult : IHttpActionResult
    {
        private const string HtmlMediaType = "text/html";
        private string resourceName;
        private object[] replacements;
        private Assembly assembly;

        /// <summary>
        /// Gets an embedded resource string from the calling assembly and returns it as HTML.
        /// Optionally uses <see cref="string.Format(string, object[])"/> to replace values in the resource.
        /// </summary>
        /// <param name="resourceName">The resource name to return as Html.</param>
        /// <param name="replacements">Optional replacement values to pass to <see cref="string.Format(string, object[])"/>.</param>
        public StaticHtmlActionResult(string resourceName, params object[] replacements)
        {
            if (resourceName == null)
            {
                throw new ArgumentNullException("resourceName");
            }

            this.resourceName = resourceName;
            this.replacements = replacements;
            this.assembly = Assembly.GetCallingAssembly();
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This code is resilient to this scenario")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response will be disposed by caller.")]
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string result;
            using (Stream stream = this.assembly.GetManifestResourceStream(this.resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            if (this.replacements.Length > 0)
            {
                result = result.FormatInvariant(this.replacements);
            }

            response.Content = new StringContent(result, Encoding.UTF8, HtmlMediaType);

            return Task.FromResult(response);
        }
    }
}