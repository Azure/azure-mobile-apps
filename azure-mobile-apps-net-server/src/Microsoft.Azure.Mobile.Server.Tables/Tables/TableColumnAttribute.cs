// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    /// <summary>
    /// The <see cref="TableColumnAttribute"/> can be used to annotate data model properties that represent system properties
    /// used by the <see cref="TableController{T}"/>. By indicating which columns are the id, version, createdAt, etc. columns,
    /// the various domain managers can leverage that information to provide the best possible mapping to any particular 
    /// backend store.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class TableColumnAttribute : Attribute
    {
        private const string AnnotationName = "ServiceTableColumn";

        /// <summary>
        /// Initializes a new instance of the <see cref="TableColumnAttribute"/> with a given <paramref name="columnType"/>.
        /// </summary>
        /// <param name="columnType">The <see cref="TableColumnType"/> for property this attribute is applied to.</param>
        public TableColumnAttribute(TableColumnType columnType)
        {
            this.ColumnType = columnType;
        }

        /// <summary>
        /// The <see cref="TableColumnType"/> for property this attribute is applied to.
        /// </summary>
        public TableColumnType ColumnType { get; private set; }

        /// <summary>
        /// When registering the <see cref="TableColumnAttribute"/> with Entity Framework using a model builder
        /// code first convention, use this name as the table column annotation name. See 
        /// <c>https://entityframework.codeplex.com/wikipage?title=Code%20First%20Annotations</c> for more 
        /// information about Entity Framework code first conventions and code annotations.
        /// </summary>
        public static string TableColumnAnnotation
        {
            get
            {
                return AnnotationName;
            }
        }
    }
}
