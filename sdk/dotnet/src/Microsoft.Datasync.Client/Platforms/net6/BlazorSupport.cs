// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.JSInterop;
using System;

namespace Microsoft.Datasync.Client
{
    public static class BlazorSupport
    {
        private const string installationIdLocalStorageKey = "ms-datasync-installation-id";

        /// <summary>
        /// Gets the installation ID in a JavaScript / Blazor WASM context.
        /// </summary>
        /// <param name="jsRuntime">The Javascript Runtime.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the <see cref="IJSInProcessRuntime"/> is not available.</exception>
        /// <exception cref="ApplicationException">If localstorage is not available.</exception>
        public static string GetInstallationId(IJSRuntime jsRuntime)
        {
            if (jsRuntime is not IJSInProcessRuntime jsproc)
            {
                throw new InvalidOperationException("IJSInProcessRuntime is not available");
            }
            try
            {
                string installationId = jsproc.Invoke<string>("localStorage.getItem", installationIdLocalStorageKey);
                if (installationId == null)
                {
                    installationId = Guid.NewGuid().ToString("D");
                    jsproc.InvokeVoid("localStorage.setItem", installationIdLocalStorageKey, installationId);
                }
                return installationId;
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("Failed to read the 'localStorage' property from 'Window'"))
                {
                    throw new ApplicationException("Unable to access the browser storage. This is most likely due to the browser settings.");
                }
                throw;
            }
        }
    }
}
