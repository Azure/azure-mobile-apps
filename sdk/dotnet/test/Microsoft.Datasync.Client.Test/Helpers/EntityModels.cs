// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class IdEntity : IEquatable<IdEntity>
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string StringValue { get; set; }
        public bool Equals(IdEntity other) => Id == other.Id && StringValue == other.StringValue;
        public override bool Equals(object obj) => obj is IdEntity ide && Equals(ide);
        public override int GetHashCode() => Id.GetHashCode() + StringValue.GetHashCode();
    }

    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class KeyEntity
    {
        [Key]
        public string KeyId { get; set; }
        [ConcurrencyCheck]
        public string KeyVersion { get; set; }
    }

    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class NoIdEntity
    {
        public string Test { get; set; }
    }

    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class NonStringIdEntity
    {
        public bool Id { get; set; }
        public bool Version { get; set; }
    }
}
