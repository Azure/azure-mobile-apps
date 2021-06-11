// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module queryjs/Query
@description Provides an API for defining queries against arbitrary data sources.
*/
/**
@function select
@description Select specified columns / properties from the source objects
@param {string} properties A comma delimited list of properties to select
*/
/**
@function where
@description Filter results by the specified query, dictionary, or function
@param {module:queryjs/Query|object|function} query Specifies how to filter items
*/
/**
@function orderBy
@description Order results by the specified columns / properties
@param {string} properties A comma delimited list of properties to order by
*/
/**
@function orderByDescending
@description Order results by the specified columns / properties in descending order
@param {string} properties A comma delimited list of properties to order by
*/
/**
@function skip
@description Skip the specified number of items
@param {integer} count Number of items to skip
*/
/**
@function take
@description Limit the number of items returned to the specified number
@param {integer} count Number of items to take
*/
/**
@name includeTotalCount
@description Include the total number of results matched by the where clause, i.e. ignoring skip and take clauses
@type {boolean}
*/
