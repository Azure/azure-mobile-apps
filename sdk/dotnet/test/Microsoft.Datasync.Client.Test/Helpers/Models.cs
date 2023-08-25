// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Test.Helpers;

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

public interface IDatasyncEntity
{
    string Id { get; set; }
    bool Deleted { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    string Version { get; set; }
}

[ExcludeFromCodeCoverage]
public class TestDatasyncEntity : IDatasyncEntity
{
    public string Id { get; set; }
    public bool Deleted { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string Version { get; set; }
}

[ExcludeFromCodeCoverage]
public class DerivedTestDatasyncEntity : TestDatasyncEntity
{
    public string StringValue { get; set; }
}
