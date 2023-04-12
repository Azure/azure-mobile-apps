// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;

namespace Microsoft.Datasync.Client.Test.Offline.Queue;

[ExcludeFromCodeCoverage]
public abstract class BaseOperationTest : BaseTest
{
    /// <summary>
    /// A set of invalid IDs for testing
    /// </summary>
    public static IEnumerable<object[]> GetInvalidIds() => new List<object[]>
    {
        new object[] { "" },
        new object[] { " " },
        new object[] { "\t" },
        new object[] { "abcdef gh" },
        new object[] { "!!!" },
        new object[] { "?" },
        new object[] { ";" },
        new object[] { "{EA235ADF-9F38-44EA-8DA4-EF3D24755767}" },
        new object[] { "###" }
    };

    /// <summary>
    /// A set of invalid table names for testing
    /// </summary>
    /// <remarks>
    /// Tests for this directory do not allow the system tables.
    /// </remarks>
    public static IEnumerable<object[]> GetInvalidTableNames() => new List<object[]>
    {
        new object[] { "" },
        new object[] { " " },
        new object[] { "\t" },
        new object[] { "abcdef gh" },
        new object[] { "!!!" },
        new object[] { "?" },
        new object[] { ";" },
        new object[] { "{EA235ADF-9F38-44EA-8DA4-EF3D24755767}" },
        new object[] { "###" },
        new object[] { "1abcd" },
        new object[] { "true.false" },
        new object[] { "a-b-c-d" },
        new object[] { "__queue" },
        new object[] { "__errors" },
        new object[] { "__delta" }
    };
}
