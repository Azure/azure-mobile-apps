// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace Microsoft.Azure.Mobile.Server.Tables
{
    /// <summary>
    /// Provides an indication of type of table column a given property is. The <see cref="TableColumnType"/> is used 
    /// in connection with the <see cref="TableControllerConfigAttribute"/> which can be used to decorate a data type.
    /// </summary>
    public enum TableColumnType
    {
        /// <summary>
        /// Not a table column
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents a unique ID property.
        /// </summary>
        Id,
        
        /// <summary>
        /// Represents a unique version identifier property which is updated every time the entity is updated.
        /// </summary>
        Version,
        
        /// <summary>
        /// Represents the date and time the entity was created.
        /// </summary>
        CreatedAt,
        
        /// <summary>
        /// Represents the date and time the entity was last modified.
        /// </summary>
        UpdatedAt,
        
        /// <summary>
        /// Represents a value indicating whether the entity has been deleted.
        /// </summary>
        Deleted
    }
}
