// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@typedef tableDefinition
@description Specifies options for table access
@property {boolean} authorize=false - Execute the {@link module:azure-mobile-apps/src/express/middleware/authorize authorize middleware} for each table operation
@property {string} access - Specify access constraints for the table ('anonymous', 'authenticated' or 'disabled')
@property {boolean} autoIncrement=false - Automatically increment the id column on each insert
@property {boolean} dynamicSchema=true - Dynamically create table schema
@property {string} name - Name of the table
@property {object} columns - Object containing column definitions; property key defines the column name, the value should be one of 'number', 'string', 'boolean' or 'datetime'
@property {string} schema=dbo - SQL Server schema name to use for the table
@property {string} [databaseTableName] - The name of the database table to use. If unspecified, the name property is used
@property {integer} maxTop - Limit the maximum number of rows a client can request
@property {boolean} softDelete=false Turn on soft deletion for the table
@property {boolean} perUser=false Restrict records to those created by the current user
@property {duration} recordsExpire Prevent access to records older than the specified duration
@property {webhook} webhook Send a POST http request to the specified URL at the completion of each table operation
@property {function[]} filters An array of filter functions
@property {function[]} transforms An array of transform functions
@property {function[]} hooks An array of post-operation functions
*/


/**
@typedef duration
@description Specifies a duration in time
@property {number} milliseconds The number of milliseconds 
@property {number} seconds The number of seconds 
@property {number} minutes The number of minutes 
@property {number} hours The number of hours 
@property {number} days The number of days 
@property {number} weeks The number of weeks 
@property {number} months The number of months 
@property {number} years The number of years 
*/

/**
@typedef webhook
@description Specifies options for calling into a HTTP endpoint
@property {string} url The URL of the endpoint to call
*/