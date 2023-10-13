using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Datasync.CosmosDb.Test;

[ExcludeFromCodeCoverage]
public class BuildPartitionKey_Tests
{
    public class Entity : CosmosTableData
    {
        public string StringProperty { get; set; } = Guid.NewGuid().ToString("N");
        public Guid GuidProperty { get; set; } = Guid.NewGuid();
        public double DoubleProperty { get; set; } = 999;
        public bool BoolProperty { get; set; } = true;
        public string NullProperty { get; set; }
        public DateTime DateTimeProperty { get; set; } = DateTime.Now;
        public int IntProperty { get; set; } = 4;
    }

    [Fact]
    public void WithValidData_ReturnsExpectedPartitionKey()
    {
        // Arrange
        Entity entity = new();
        var propertyNames = new List<string> { nameof(entity.StringProperty), nameof(entity.BoolProperty), nameof(entity.DoubleProperty) };
        var expectedPartitionKey = new PartitionKeyBuilder()
            .Add(entity.StringProperty)
            .Add(entity.BoolProperty)
            .Add(entity.DoubleProperty)
            .Build();

        // Act
        var partitionKey = entity.BuildPartitionKey(propertyNames);

        // Assert
        Assert.Equal(expectedPartitionKey, partitionKey);
    }
    
    [Fact]
    public void WithNonStringDoubleBool_ReturnsExpectedPartitionKey()
    {
        // Arrange
        Entity entity = new();
        var propertyNames = new List<string> { nameof(entity.GuidProperty), nameof(entity.IntProperty), nameof(entity.DateTimeProperty) };
        var expectedPartitionKey = new PartitionKeyBuilder()
            .Add(entity.GuidProperty.ToString())
            .Add(entity.IntProperty.ToString())
            .Add(entity.DateTimeProperty.ToString())
            .Build();

        // Act
        var partitionKey = entity.BuildPartitionKey(propertyNames);

        // Assert
        Assert.Equal(expectedPartitionKey, partitionKey);
    }

    [Fact]
    public void WithNullPropertyNames_ThrowsArgumentNullException()
    {
        // Arrange
        Entity entity = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.BuildPartitionKey(null));
    }

    [Fact]
    public void WithEmptyPropertyNames_ThrowsArgumentException()
    {
        // Arrange
        Entity entity = new();
        var propertyNames = new List<string>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => entity.BuildPartitionKey(propertyNames));
    }

    [Fact]
    public void WithInvalidPropertyName_ThrowsArgumentException()
    {
        // Arrange
        Entity entity = new();
        var propertyNames = new List<string> { "InvalidProperty" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => entity.BuildPartitionKey(propertyNames));
    }

    [Fact]
    public void WithNullPropertyValue_ThrowsArgumentNullException()
    {
        // Arrange
        Entity entity = new();
        List<string> propertyNames = new() { nameof(entity.NullProperty) };


        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entity.BuildPartitionKey(propertyNames));
    }
}