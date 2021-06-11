# Creating Data Providers for the Azure Mobile Apps SDK for Node

## API

Data providers should implement and export the following interface:

````
module.exports = function (configuration) {
    return function(table) {
        return {
            read: function (query) { },
            update: function (item, query) { },
            insert: function (item) { },
            delete: function (query, version) { },
            undelete: function (query, version) { },
            truncate: function () { },
            initialize: function () { },
            schema: function () { }
        };
    };
};
````

The top level function should accept a dataConfiguration object and return a factory function that 
accepts a tableConfiguration object and returns a table access object as described.

All functions should return a promise, as constructed by the `create` function in the 
`azure-mobile-apps/src/utilities/promises` module.

### read

    function (query) { }

The `query` parameter is a [queryjs Query object][queryjs]. It exposes a LINQ style API and 
exposes a comprehensive expression tree through the `getComponents` function.

Support for conversion to other formats is currently limited to an OData object representation. 
This can be done using the `toOData` function exposed by the `azure-mobile-apps/src/query` module. 
See [the source][toOData].

The read function should resolve to an array of results.

- When the provided query has the `includeTotalCount` property set, the array should have an
  additional property `totalCount` set to the total number of records that would be returned
  without a result size limit.
- When the provided query has the `includeDeleted` property set, the results should include
  soft deleted items.
- All version values, when retrieved should be Base64 encoded.

### insert

    function (item) { }

The insert function should insert a new record into the database and resolve to the inserted item.

- If an item with the same `id` property already exists, an `Error` should be thrown with
  the `duplicate` property set.
- The `createdAt` and `updatedAt` properties should be set to the current date and time.
- The `version` property should be set to a unique value.

### update

    function (item, query) { }

The update function should update the item with the corresponding `id` in the database
and resolve to the updated item.

- If the `version` property is specified, it should only update the record if the `version`
  property matches.
- If the `query` parameter is specified, it should only update the record if the query
  returns the record being updated.
- If the `version` property does not match or the `query` does not return the record, an
  `Error` should be thrown with the `concurrency` property set to true.
- The `updatedAt` property should be updated to the current date and time.
- The `version` property should be updated to a new unique value.

The `query` parameter is optional and allows filters such as user IDs to be applied to update
operations. The query is in the format described in the read section.

### delete

    function (query, version) { }

The delete function should delete records matching the provided query.

- If a single item is deleted, it should resolve to the deleted item. This is the behavior
  that is exposed to the client.
- If multiple items are deleted, it should resolve to an array of those items.
- If the `version` parameter is specified, it should only delete records if the `version`
  property matches.
- If no records are deleted, either because the `version` property does not match or the query
  returns no records, an `Error` should be thrown with the `concurrency` property set to true.
- If the `softDelete` option is specified on the table configuration, the record should be
  recoverable by calling undelete, and should be queryable by specifying the `includeDeleted`
  option on read queries.

The query object is in the format described in the read section. For simple data provider 
scenarios, the query object has an `id` property corresponding with the value passed in the 
querystring of delete requests.

### undelete

    function (query, version) { }

The undelete function should restore records matching the provided query.

- If a single item is undeleted, it should resolve to the restored item. This is the behavior
  that is exposed to the client.
- If multiple items are undeleted, it should resolve to an array of those items.
- If the `version` parameter is specified, it should only restore records if the `version`
  property matches.
- If no records are restored, either because the `version` property does not match or the query
  returns no records, an `Error` should be thrown with the `concurrency` property set to true.
- If the `softDelete` option is not specified on the table configuration, this function
  should have no effect and resolve to undefined.

The query object is in the format described in the read section. For simple data provider 
scenarios, the query object has an `id` property corresponding with the value passed in the 
querystring of undelete requests.

### truncate

    function () { }

The truncate function should clear all items from the table and resolve when complete.

### initialize

    function () { }

The initialize function should

- create appropriate schema as specified by the `columns` property of the table configuration,
- insert items into the table specified by the `seed` property of the table configuration,
- perform any other table initialization, such as index creation

### schema

    function () { }

The schema function should resolve to an object with the following example structure:

    {
        name: 'table1',
        properties: [
            { name: "stringColumn", type: "string" },
            { name: "numberColumn", type: "number" },
            { name: "booleanColumn", type: "boolean" },
            { name: "datetimeColumn", type: "datetime" }
        ]
    }

The schema function is currently only required to support swagger functionality.

## Consuming the Data Provider

Set the top level factory function to the `data.provider` configuration option. The `data` 
object is passed as the `configuration` parameter to the factory function to allow additional 
configuration options to be specified.

## Testing

A suite of integration tests to ensure compatibility will be provided soon.


[queryjs]: https://github.com/Azure/queryjs
[toOData]: https://github.com/Azure/azure-mobile-apps-node/blob/master/src/query/index.js
