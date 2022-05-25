// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    public class EntityTableSqlGeneratorTests
    {
        private EntityTableSqlGeneratorMock generatorMock;

        public EntityTableSqlGeneratorTests()
        {
            this.generatorMock = new EntityTableSqlGeneratorMock();
        }

        public static TheoryDataCollection<string, string, string> GetColumnNames
        {
            get
            {
                return new TheoryDataCollection<string, string, string>
                {
                    { "schema.table", "column", "schema.table.[column]" },
                    { "SCHEMA.TABLE", " & ", "SCHEMA.TABLE.[&]" },
                    { "table", "你好世界", "table.[你好世界]" },
                    { "table", "[你好]", "table.[你好]" },
                };
            }
        }

        public static TheoryDataCollection<string, string> GetTriggerNames
        {
            get
            {
                return new TheoryDataCollection<string, string>
                {
                    { "schema.table", "[TR_schema_table_InsertUpdateDelete]" },
                    { "SCHEMA.TABLE", "[TR_SCHEMA_TABLE_InsertUpdateDelete]" },
                    { "你好世界", "[TR_你好世界_InsertUpdateDelete]" },
                    { "[你好]", "[TR_你好_InsertUpdateDelete]" },
                };
            }
        }

        public static TheoryDataCollection<string, string, string, string> GetTriggerData
        {
            get
            {
                return new TheoryDataCollection<string, string, string, string>
                {
                    { 
                        "schema.Table", 
                        "Id", 
                        "UpdatedAt", 
                        "CREATE TRIGGER [TR_schema_Table_InsertUpdateDelete] ON [schema].[Table] AFTER INSERT, UPDATE, DELETE AS BEGIN UPDATE [schema].[Table] SET [schema].[Table].[UpdatedAt] = CONVERT(DATETIMEOFFSET, SYSUTCDATETIME()) FROM INSERTED WHERE inserted.[Id] = [schema].[Table].[Id] END" 
                    },
                    { 
                        "你好世界", 
                        "Id", 
                        "UpdatedAt", 
                        "CREATE TRIGGER [TR_你好世界_InsertUpdateDelete] ON [你好世界] AFTER INSERT, UPDATE, DELETE AS BEGIN UPDATE [你好世界] SET [你好世界].[UpdatedAt] = CONVERT(DATETIMEOFFSET, SYSUTCDATETIME()) FROM INSERTED WHERE inserted.[Id] = [你好世界].[Id] END" 
                    },
                    { 
                        "你好.世界", 
                        "你好", 
                        "世界", 
                        "CREATE TRIGGER [TR_你好_世界_InsertUpdateDelete] ON [你好].[世界] AFTER INSERT, UPDATE, DELETE AS BEGIN UPDATE [你好].[世界] SET [你好].[世界].[世界] = CONVERT(DATETIMEOFFSET, SYSUTCDATETIME()) FROM INSERTED WHERE inserted.[你好] = [你好].[世界].[你好] END" 
                    },
                    { 
                        "你好.世'界", 
                        "你好", 
                        "世界", 
                        "CREATE TRIGGER [TR_你好_世'界_InsertUpdateDelete] ON [你好].[世'界] AFTER INSERT, UPDATE, DELETE AS BEGIN UPDATE [你好].[世'界] SET [你好].[世'界].[世界] = CONVERT(DATETIMEOFFSET, SYSUTCDATETIME()) FROM INSERTED WHERE inserted.[你好] = [你好].[世'界].[你好] END" 
                    },
                };
            }
        }

        [Theory]
        [MemberData("GetTriggerData")]
        public void GetTrigger_CreatesValidTriggers(string tableName, string idColumnName, string updatedAtColumnName, string expected)
        {
            // Act
            string actual = this.generatorMock.GetTrigger(tableName, idColumnName, updatedAtColumnName);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("GetColumnNames")]
        public void GetColumnName_CreatesValidNames(string escapedTableName, string columnName, string expected)
        {
            // Act
            string actual = this.generatorMock.GetColumnName(escapedTableName, columnName);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("GetTriggerNames")]
        public void GetTriggerName_CreatesValidNames(string tableName, string expected)
        {
            // Act
            string actual = this.generatorMock.GetTriggerName(tableName);

            // Assert
            Assert.Equal(expected, actual);
        }

        private class EntityTableSqlGeneratorMock : EntityTableSqlGenerator
        {
            public new string GetColumnName(string escapedTableName, string columnName)
            {
                return base.GetColumnName(escapedTableName, columnName);
            }

            public new string GetTriggerName(string tableName)
            {
                return base.GetTriggerName(tableName);
            }

            public new string GetTrigger(string tableName, string idColumnName, string updatedAtColumnName)
            {
                return base.GetTrigger(tableName, idColumnName, updatedAtColumnName);
            }
        }
    }
}
