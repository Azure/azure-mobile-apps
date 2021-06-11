// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;

namespace Microsoft.AspNetCore.Datasync.Extensions
{
    internal static class JsonPatchDocumentExtensions
    {
        private const string UniversalTimeFormat = "yyyyMMddTHHmmss.fff";

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
        internal static bool ModifiesSystemProperties<TEntity>(this JsonPatchDocument<TEntity> patch, TEntity entity) where TEntity : class, ITableData
        {
            foreach (var operation in patch.Operations)
            {
                switch (operation.OperationType)
                {
                    case OperationType.Replace:
                        if (operation.path.Equals("/id", StringComparison.OrdinalIgnoreCase) && !entity.Id.Equals(operation.value))
                            return true;

                        if (operation.path.Equals("/version", StringComparison.OrdinalIgnoreCase))
                        {
                            string stored = entity.GetETag().Trim('"');
                            if (!stored.Equals(operation.value))
                                return true;
                        }

                        if (operation.path.Equals("/updatedAt", StringComparison.OrdinalIgnoreCase) && operation.value is DateTime dt)
                        {
                            string stored = entity.UpdatedAt.ToUniversalTime().ToString(UniversalTimeFormat);
                            string dtzval = dt.ToUniversalTime().ToString(UniversalTimeFormat);
                            if (stored != dtzval)
                                return true;
                        }

                        break;
                    case OperationType.Test:
                        break;
                    default:
                        return true;
                }
            }
            return false;
        }
    }
}
