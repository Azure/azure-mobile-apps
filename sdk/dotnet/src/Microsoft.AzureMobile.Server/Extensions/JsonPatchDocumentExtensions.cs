// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch;

namespace Microsoft.AzureMobile.Server.Extensions
{
    internal static class JsonPatchDocumentExtensions
    {
        /// <summary>
        /// Returns true if one of the operations is the operation specified.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="patch">The patch document</param>
        /// <param name="op">The operation</param>
        /// <param name="path">The path</param>
        /// <param name="value">The value</param>
        /// <returns></returns>
        internal static bool Contains<TEntity>(this JsonPatchDocument<TEntity> patch, string op, string path, object value) where TEntity : class, ITableData
        {
            foreach (var operation in patch.Operations)
            {
                if (operation.op == op && operation.path == path && operation.value.Equals(value))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if the JSON patch document is a valid document. It is not allowed
        /// to adjust system properties, and only replace operations are allowed.
        /// </summary>
        /// <param name="patch">The patch document</param>
        internal static bool ModifiesSystemProperties<TEntity>(this JsonPatchDocument<TEntity> patch) where TEntity : class, ITableData
        {
            string[] systemProperties = new string[] { "/id", "/updatedat", "/version" };
            string[] allowsOperations = new string[] { "replace", "test" };

            foreach (var operation in patch.Operations)
            {
                if (!allowsOperations.Contains(operation.op) || systemProperties.Contains(operation.path.ToLowerInvariant()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
