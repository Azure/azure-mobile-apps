// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.EFCore;

namespace ThreadedPushTest.Service
{
    /// <summary>
    /// The fields in this class must match the fields in Models/TodoItem.cs
    /// for the TodoApp.Data project.
    /// </summary>
    public class Entity : EntityTableData
    {
        public bool BoolProp { get; set; }
        public int IntProp { get; set; }
        public double DoubleProp { get; set; }
        public string? StringProp { get; set; }
        public DateTimeOffset TimestampProp { get; set; }
        public Guid GuidProp { get; set; }
    }
}
