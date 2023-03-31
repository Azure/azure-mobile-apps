// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Datasync.Common.Test.Models;

/// <summary>
/// It's pretty hard to construct a <see cref="JsonPatchDocument"/>, so this
/// class is used to provide the same functionality more easily for testing
/// purposes.
/// </summary>
[ExcludeFromCodeCoverage]
public class PatchOperation
{
    public PatchOperation(string op, string path, object value = null)
    {
        Op = op;
        Path = path.StartsWith("/") ? path : $"/{path}";
        Value = value;
    }

    public string Op { get; set; }
    public string Path { get; set; }
    public object Value { get; set; }
}
