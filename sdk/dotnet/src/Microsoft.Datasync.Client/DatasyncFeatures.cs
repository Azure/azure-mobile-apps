// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// The list of features exposed in the HTTP Features Header for tracking usage.
    /// </summary>
    [Flags]
    internal enum DatasyncFeatures
    {
        None = 0x00,

        // Table operation - typed and untyped variants
        [EnumValue("TT")] TypedTable = 0x01,
        [EnumValue("TU")] UntypedTable = 0x02,

        // Offline initiated requests
        [EnumValue("OL")] Offline = 0x80
    }
}
