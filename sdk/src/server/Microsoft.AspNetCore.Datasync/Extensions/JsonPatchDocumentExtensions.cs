// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System.Globalization;

namespace Microsoft.AspNetCore.Datasync.Extensions;

internal static class JsonPatchDocumentExtensions
{
    private const string timeFormat = "yyyy-MM-dd'T'HH:mm:ss.fff";

    /// <summary>
    /// Returns <c>true</c> if one of the operations is the operation specified.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being modified by the patch document.</typeparam>
    /// <param name="patch">The patch document to check.</param>
    /// <param name="op">The operation for comparison.</param>
    /// <param name="path">The path for comparison.</param>
    /// <param name="value">The value for comparison.</param>
    /// <returns><c>true</c> if one of the operations match, <c>false</c> otherwise.</returns>
    internal static bool Contains<TEntity>(this JsonPatchDocument<TEntity> patch, string op, string path, object value) where TEntity : class, ITableData
        => patch.Operations.Any(o => o.op.Equals(op, StringComparison.InvariantCultureIgnoreCase) && o.path.Equals(path, StringComparison.InvariantCultureIgnoreCase) && o.value.Equals(value));

    /// <summary>
    /// Determines if the JSON patch document is a valid document.  It is not allowed to adjust system properties
    /// and can only replace data, not delete or add data.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being modified by the patch document.</typeparam>
    /// <param name="patch">The patch document to check.</param>
    /// <param name="entity">The entity being patched.</param>
    /// <param name="validationErrors">On completion, the validation errors.</param>
    /// <returns><c>true</c> if there are validation errors; <c>false</c> otherwise.</returns>
    internal static bool ModifiesSystemProperties<TEntity>(this JsonPatchDocument<TEntity> patch, TEntity entity, out Dictionary<string, string[]> validationErrors) where TEntity : class, ITableData
    {
        validationErrors = new Dictionary<string, string[]>();
        bool returnValue = false;

        foreach (Operation operation in patch.Operations)
        {
            switch (operation.OperationType)
            {
                case OperationType.Replace:
                    if (operation.path.Equals("/id", StringComparison.InvariantCultureIgnoreCase) && !entity.Id.Equals(operation.value))
                    {
                        validationErrors.Add("entity.Id", new string[] { entity.Id });
                        validationErrors.Add("patch./id", new string[] { (string)operation.value });
                        returnValue = true;
                    }

                    if (operation.path.Equals("/version", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string stored = entity.Version.ToEntityTagValue();
                        if (!stored.Equals(operation.value))
                        {
                            validationErrors.Add("entity.Version", new string[] { stored });
                            validationErrors.Add("patch./version", new string[] { (string)operation.value });
                            returnValue = true;
                        }
                    }

                    if (operation.path.Equals("/updatedAt", StringComparison.InvariantCultureIgnoreCase) && operation.value is DateTime dt)
                    {
                        string? stored = entity.UpdatedAt?.ToUniversalTime().ToString(timeFormat, CultureInfo.InvariantCulture) ?? "";
                        string dtzval = dt.ToUniversalTime().ToString(timeFormat, CultureInfo.InvariantCulture);
                        if (stored != dtzval)
                        {
                            validationErrors.Add("entity.UpdatedAt", new string[] { stored });
                            validationErrors.Add("patch./updatedAt", new string[] { dtzval });
                            returnValue = true;
                        }
                    }
                    break;
            }
        }

        return returnValue;
    }
}
