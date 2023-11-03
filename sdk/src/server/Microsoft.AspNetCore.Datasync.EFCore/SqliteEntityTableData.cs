// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.EFCore;

/// <summary>
/// A version of the <see cref="EntityTableData"/> that does not do automatic timestamping.
/// Use this version when the repository should be responsible for updating the timestamp.
/// The classic example of this is Sqlite.
/// </summary>
public class SqliteEntityTableData : BaseEntityTableData
{
}
