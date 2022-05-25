// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.SqlServer;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    /// <summary>
    /// Updates the SQL DB definition to take into account columns marked by <see cref="TableColumnType"/>.
    /// The <see cref="EntityTableSqlGenerator"/> can be enabled either by using the scaffolded <see cref="System.Data.Entity.DbContext"/>
    /// or by deriving from the <see cref="EntityContext"/> base class.
    /// </summary>
    public class EntityTableSqlGenerator : SqlServerMigrationSqlGenerator
    {
        private static readonly char[] Escapes = new char[] { '[', ']' };

        protected override string BuildColumnType(ColumnModel columnModel)
        {
            if (columnModel == null)
            {
                throw new ArgumentNullException("columnModel");
            }

            TableColumnType tableColumnType = this.GetTableColumnType(columnModel);
            if (tableColumnType != TableColumnType.None)
            {
                this.UpdateTableColumn(columnModel, tableColumnType);
            }

            return base.BuildColumnType(columnModel);
        }

        protected virtual void UpdateTableColumn(ColumnModel columnModel, TableColumnType tableColumnType)
        {
            if (columnModel == null)
            {
                throw new ArgumentNullException("columnModel");
            }

            // We don't try to set default value if the user has already set it or the store type has been set explicitly.
            if (columnModel == null || columnModel.ClrType == null || columnModel.DefaultValueSql != null || columnModel.StoreType != null)
            {
                return;
            }

            switch (tableColumnType)
            {
                case TableColumnType.Id:
                    columnModel.DefaultValueSql = "NEWID()";
                    break;

                case TableColumnType.CreatedAt:
                    columnModel.DefaultValueSql = "SYSUTCDATETIME()";
                    break;
            }
        }

        protected override void Generate(CreateTableOperation createTableOperation)
        {
            if (createTableOperation == null)
            {
                throw new ArgumentNullException("createTableOperation");
            }

            bool createUpdatedAtTrigger = false;
            string updatedAtColumnName = null;
            string idColumnName = null;
            foreach (ColumnModel column in createTableOperation.Columns)
            {
                TableColumnType tableColumnType = this.GetTableColumnType(column);
                switch (tableColumnType)
                {
                    case TableColumnType.Id:
                        idColumnName = column.Name;
                        break;

                    case TableColumnType.UpdatedAt:
                        createUpdatedAtTrigger = true;
                        updatedAtColumnName = column.Name;
                        break;

                    case TableColumnType.CreatedAt:
                        // If we have this column then use that to create a clustered index for instead of the Id column
                        createTableOperation.PrimaryKey.IsClustered = false;
                        break;
                }
            }

            base.Generate(createTableOperation);

            if (createUpdatedAtTrigger && !string.IsNullOrEmpty(idColumnName) && !string.IsNullOrEmpty(updatedAtColumnName))
            {
                string trigger = this.GetTrigger(createTableOperation.Name, idColumnName, updatedAtColumnName);
                this.Statement(trigger);
            }
        }

        protected virtual TableColumnType GetTableColumnType(ColumnModel columnModel)
        {
            if (columnModel == null)
            {
                throw new ArgumentNullException("columnModel");
            }

            AnnotationValues tableAnnotations;
            if (columnModel.Annotations.TryGetValue(TableColumnAttribute.TableColumnAnnotation, out tableAnnotations))
            {
                TableColumnType tableColumnType;
                return Enum.TryParse<TableColumnType>(tableAnnotations.NewValue as string, out tableColumnType) ? tableColumnType : TableColumnType.None;
            }

            return TableColumnType.None;
        }

        protected virtual string GetTrigger(string tableName, string idColumnName, string updatedAtColumnName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            if (idColumnName == null)
            {
                throw new ArgumentNullException("idColumnName");
            }

            if (updatedAtColumnName == null)
            {
                throw new ArgumentNullException("updatedAtColumnName");
            }

            string escapedTableName = this.Name(tableName);
            string triggerName = this.GetTriggerName(tableName);
            string qualifiedUpdatedColumName = this.GetColumnName(escapedTableName, updatedAtColumnName);
            string qualifiedIdColumnName = this.GetColumnName(escapedTableName, idColumnName);
            string escapedIdColumnName = this.Name(idColumnName);
            string msg = "CREATE TRIGGER {0} ON {1} AFTER INSERT, UPDATE, DELETE AS BEGIN UPDATE {1} SET {2} = CONVERT(DATETIMEOFFSET, SYSUTCDATETIME()) FROM INSERTED WHERE inserted.{3} = {4} END"
                .FormatInvariant(triggerName, escapedTableName, qualifiedUpdatedColumName, escapedIdColumnName, qualifiedIdColumnName);
            return msg;
        }

        protected virtual string GetTriggerName(string tableName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            string normalizedTableName = tableName.Replace(".", "_").Trim(Escapes);
            string triggerName = "TR_{0}_InsertUpdateDelete".FormatInvariant(normalizedTableName);
            return this.Name(triggerName);
        }

        protected virtual string GetColumnName(string escapedTableName, string columnName)
        {
            if (escapedTableName == null)
            {
                throw new ArgumentNullException("escapedTableName");
            }

            if (columnName == null)
            {
                throw new ArgumentNullException("columnName");
            }

            string escapedColumnName = this.Name(columnName);
            return "{0}.{1}".FormatInvariant(escapedTableName, escapedColumnName);
        }
    }
}
