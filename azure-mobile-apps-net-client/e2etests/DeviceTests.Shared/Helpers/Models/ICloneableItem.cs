// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace DeviceTests.Shared.Helpers.Models
{
    public interface ICloneableItem<T> where T : ICloneableItem<T>
    {
        T Clone();
        object Id { get; set; }
    }
}
