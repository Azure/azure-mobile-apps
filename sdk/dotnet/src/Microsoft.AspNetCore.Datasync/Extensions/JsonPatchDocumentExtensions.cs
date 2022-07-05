// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.AspNetCore.Datasync.Extensions
{
    internal static class JsonPatchDocumentExtensions
    {
        private static string timeFormat = "yyyy-MM-dd'T'HH:mm:ss.fff";

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
                if (operation.op.Equals(op, StringComparison.InvariantCultureIgnoreCase)
                    && operation.path.Equals(path, StringComparison.InvariantCultureIgnoreCase)
                    && operation.value.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if the JSON patch document is a valid document. It is not allowed
        /// to adjust system properties, and only replace operations are allowed.
        /// </summary>
        /// <param name="patch">The patch document</param>
        internal static bool ModifiesSystemProperties<TEntity>(this JsonPatchDocument<TEntity> patch, TEntity entity, out Dictionary<string, string[]> validationErrors) where TEntity : class, ITableData
        {
            validationErrors = new Dictionary<string, string[]>();
            bool returnValue = false;
            foreach (var operation in patch.Operations)
            {
                switch (operation.OperationType)
                {
                    case OperationType.Replace:
                        if (operation.path.Equals("/id", StringComparison.OrdinalIgnoreCase) && !entity.Id.Equals(operation.value))
                        {
                            validationErrors.Add("entity.Id", new string[] { entity.Id });
                            validationErrors.Add("patch./id", new string[] { (string)operation.value });
                            returnValue = true;
                        }

                        if (operation.path.Equals("/version", StringComparison.OrdinalIgnoreCase))
                        {
                            string stored = entity.GetETag().Trim('"');
                            if (!stored.Equals(operation.value))
                            {
                                validationErrors.Add("entity.Version", new string[] { stored });
                                validationErrors.Add("patch./version", new string[] { (string)operation.value });
                                returnValue = true;
                            }
                        }

                        if (operation.path.Equals("/updatedAt", StringComparison.OrdinalIgnoreCase) && operation.value is DateTime dt)
                        {
                            string stored = entity.UpdatedAt.ToUniversalTime().ToString(timeFormat, CultureInfo.InvariantCulture);
                            string dtzval = dt.ToUniversalTime().ToString(timeFormat, CultureInfo.InvariantCulture);
                            if (stored != dtzval)
                            {
                                validationErrors.Add("entity.UpdatedAt", new string[] { stored });
                                validationErrors.Add("patch./updatedAt", new string[] { dtzval });
                                returnValue = true;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return returnValue;
        }
    }
}
