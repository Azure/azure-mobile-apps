// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var errors = require('../../../utilities/errors');

var helpers = module.exports = {
    translateVersion: function (items) {
        if(items) {
            if(items.constructor === Array)
                return items.map(helpers.translateVersion);

            if(items.version)
                items.version = items.version.toString('base64');

            return items;
        }
    },
    combineStatements: function (statements, transform) {
        return statements.reduce(function(target, statement) {
            target.sql += statement.sql + '; ';

            if (statement.parameters)
                target.parameters = target.parameters.concat(statement.parameters);

            return target;
        }, { sql: '', parameters: [], multiple: true, transform: transform });
    },
    checkConcurrencyAndTranslate: function (results, originalItem, undeleting) {
        var recordsAffected = results[0][0].recordsAffected,
            records = results[1],
            item;

        if (records.length === 0) {
            // the record was updated to no longer be returned with the specified filters
            // just return the original item - if there's more than one, this will result in weird behavior
            if(recordsAffected > 0)
                item = originalItem;
        } else if (records.length === 1)
            item = records[0];
        else
            item = records;
            
        item = helpers.translateVersion(item);

        // there is a big assumption here that if no records were affected, it is because of a concurrency violation
        // it is possible to pass in a query to an update or delete operation that results in no affected records
        // this would not strictly be a concurrency violation, but there is no simple way to determine if it was because
        // the version column didn't match or the rest of the query filtered out all records
        if(recordsAffected === 0) {
            // we want to 404 if the item didn't exist or was filtered, 
            // or if the item has been soft deleted (unless we're undeleting)
            if(!item || (item.deleted && !undeleting))
                throw errors.notFound('No records were updated');

            var error = errors.concurrency('No records were updated');
            error.item = item;
            throw error;
        }

        return item;
    }
}
