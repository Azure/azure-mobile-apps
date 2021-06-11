// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var log = require('../../logger'),
    request = require('../../utilities/request');

module.exports = { hook: executeWebhook };

function executeWebhook(results, context) {
    var url = (context.table.webhook && context.table.webhook.url)
        || (context.configuration.webhook && context.configuration.webhook.url);
    
    if(!url)
        log.error("Unable to connect to webhook: URL not specified");

    request(url, {
        operation: context.operation,
        item: context.operation === 'read' ? undefined : results,
        table: context.table && context.table.name,
        userId: context.user && context.user.id,
        id: context.id
    });
}
