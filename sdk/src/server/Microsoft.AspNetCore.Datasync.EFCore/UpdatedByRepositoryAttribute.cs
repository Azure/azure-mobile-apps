// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.EFCore;

/// <summary>
/// The <c>pUpdatedByRepository</c> attribute is used to signify
/// that the property should be updated by the repository.  It is
/// only valid on <c>UpdatedAt</c> and <c>Version</c> properties.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class UpdatedByRepositoryAttribute : Attribute
{
}
