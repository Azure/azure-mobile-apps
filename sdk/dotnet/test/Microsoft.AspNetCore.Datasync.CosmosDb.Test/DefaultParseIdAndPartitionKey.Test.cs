// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.CosmosDb.Test;

[ExcludeFromCodeCoverage]
public class DefaultParseIdAndPartitionKey_Tests
{
    [Fact]
    public void ShouldThrowBadRequestWhenInputIsEmpty()
    {
        // Arrange
        var input = "";

        // Act & Assert
        Assert.Throws<BadRequestException>(() => CosmosUtils.DefaultParseIdAndPartitionKey(input));
    }

    [Fact]
    public void ShouldThrowBadRequestWhenInputIsNull()
    {
        // Arrange
        string input = null;

        // Act & Assert
        Assert.Throws<BadRequestException>(() => CosmosUtils.DefaultParseIdAndPartitionKey(input));
    }

    [Fact]
    public void ShouldThrowBadRequestWhenInputIsAColon()
    {
        // Arrange
        var input = ":";

        // Act & Assert
        Assert.Throws<BadRequestException>(() => CosmosUtils.DefaultParseIdAndPartitionKey(input));
    }

    [Fact]
    public void ShouldThrowBadRequestWhenStartsWithAColon()
    {
        // Arrange
        var input = ":customerId";

        // Act & Assert
        Assert.Throws<BadRequestException>(() => CosmosUtils.DefaultParseIdAndPartitionKey(input));
    }

    [Fact]
    public void ShouldThrowBadRequestWhenEndingInColon()
    {
        // Arrange
        var input = "id:";

        // Act & Assert
        Assert.Throws<BadRequestException>(() => CosmosUtils.DefaultParseIdAndPartitionKey(input));
    }

    [Fact]
    public void ShouldReturnIdAndSamePartitionKeyWhenNoColon()
    {
        // Arrange
        var input = "id";

        // Act
        var (id, partitionKey) = CosmosUtils.DefaultParseIdAndPartitionKey(input);

        // Assert
        Assert.Equal("id", id);
        Assert.Equal(new PartitionKey("id"), partitionKey);
    }

    [Fact]
    public void ShouldReturnIdAndSinglePartitionKeyWhenOneColon()
    {
        // Arrange
        var input = "id:customerId";

        // Act
        var (id, partitionKey) = CosmosUtils.DefaultParseIdAndPartitionKey(input);

        // Assert
        Assert.Equal("id", id);
        Assert.Equal(new PartitionKey("customerId"), partitionKey);
    }

    [Fact]
    public void ShouldReturnIdAndCompositePartitionKeyWhenMultipleKeys()
    {
        // Arrange
        var input = "id:customerId+employeeId";
        var expectedPartitionKey = new PartitionKeyBuilder()
            .Add("customerId")
            .Add("employeeId")
            .Build();

        // Act
        var (id, partitionKey) = CosmosUtils.DefaultParseIdAndPartitionKey(input);

        // Assert
        Assert.Equal("id", id);
        Assert.Equal(expectedPartitionKey, partitionKey);
    }
}