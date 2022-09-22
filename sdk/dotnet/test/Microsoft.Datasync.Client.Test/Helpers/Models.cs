// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    [ExcludeFromCodeCoverage]
    public class IdEntity : IEquatable<IdEntity>
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string StringValue { get; set; }

        public string StringField;

        public bool Equals(IdEntity other)
            => Id == other.Id && StringValue == other.StringValue;

        public override bool Equals(object obj)
            => obj is IdEntity ide && Equals(ide);

        public override int GetHashCode()
            => Id.GetHashCode() + StringValue.GetHashCode();
    }

    [ExcludeFromCodeCoverage]
    public class IdOnly
    {
        public string Id { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BadEntityNoId
    {
        public string NotAnId { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BadEntityIntId
    {
        public int Id { get; set; }
    }
}
