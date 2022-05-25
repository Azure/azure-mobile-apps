// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Azure.Mobile.Server.Tables;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class EntityDataTests
    {
        public static TheoryDataCollection<Expression<Func<EntityData, object>>, TableColumnType> ColumnTypeData
        {
            get
            {
                return new TheoryDataCollection<Expression<Func<EntityData, object>>, TableColumnType>
                {
                    { s => s.Id, TableColumnType.Id }, 
                    { s => s.Version, TableColumnType.Version }, 
                    { s => s.CreatedAt, TableColumnType.CreatedAt }, 
                    { s => s.UpdatedAt, TableColumnType.UpdatedAt },
                    { s => s.Deleted, TableColumnType.Deleted }, 
                };
            }
        }

        [Theory]
        [MemberData("ColumnTypeData")]
        public void Property_IsMarkedWithColumnType<TProperty>(Expression<Func<EntityData, TProperty>> property, TableColumnType expected)
        {
            // Arrange
            PropertyInfo propertyInfo = PropertyAssert.GetPropertyInfo<EntityData, TProperty>(property);

            // Act
            TableColumnAttribute attr = propertyInfo.GetCustomAttributes<TableColumnAttribute>().Single();

            // Assert
            Assert.Equal(attr.ColumnType, expected);
        }

        [Fact]
        public void Serialization_IsConsistent()
        {
            // Arrange
            EntityDataMock data = new EntityDataMock()
            {
                Id = "你好世界",
                Version = Encoding.UTF8.GetBytes("version"),
                CreatedAt = DateTimeOffset.Parse("2013-10-07T21:55:45.8677285Z"),
                UpdatedAt = DateTimeOffset.Parse("2013-11-07T21:55:45.8677285Z"),
                Deleted = true
            };

            // Act/Assert
            SerializationAssert.VerifySerialization(data, "{\"id\":\"你好世界\",\"version\":\"dmVyc2lvbg==\",\"createdAt\":\"2013-10-07T21:55:45.867Z\",\"updatedAt\":\"2013-11-07T21:55:45.867Z\",\"deleted\":true}");
        }

        private class EntityDataMock : EntityData
        {
        }
    }
}
