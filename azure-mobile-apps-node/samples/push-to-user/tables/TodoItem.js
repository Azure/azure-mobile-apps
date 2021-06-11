var azureMobileApps = require('azure-mobile-apps'),
    promises = require('azure-mobile-apps/src/utilities/promises'),
    logger = require('azure-mobile-apps/src/logger');

var table = azureMobileApps.table();

// enable authentication for the table
table.access = 'authenticated';

// When adding record, send a push notification via WNS
// For this to work, you must have a WNS Hub already configured
table.insert(function (context) {
    // For details of the Notification Hubs JavaScript SDK,
    // see https://azure.microsoft.com/en-us/documentation/articles/notification-hubs-nodejs-how-to-use-notification-hubs/
    logger.silly('Running TodoItem.insert');

    // This push uses a template mechanism, so we need a template
    var payload = '<toast><visual><binding template="Toast01"><text id="1">INSERT</text></binding></visual></toast>';

    // attach the userId to the record
    context.item.userId = context.user.id;

    // Execute the insert.  The insert returns the results as a Promise,
    // Do the push as a post-execute action within the promise flow.
    return context.execute()
        .then(function (results) {
            // Only do the push if configured
            if (context.push) {
                // Mobile Apps adds a user tag when registering for push notifications
                var userTag = '_UserId:' + context.user.id;

                context.push.wns.send(userTag, payload, 'wns/toast', function (error) {
                    if (error) {
                        logger.error('Error while sending push notification: ', error);
                    } else {
                        logger.silly('Push notification sent successfully!');
                    }
                });
            }
            // Don't forget to return the results from the context.execute()
            return results;
        })
        .catch(function (error) {
            logger.error('Error while running context.execute: ', error);
        });
});

// Limit the viewable records to those that the user created.  This
// is used in the individual read, update and delete operations to
// ensure that one user cannot touch another users records
function limitToAuthenticatedUser(context) {
    context.query.where({ userId: context.user.id });
    return context.execute();
}

// Attach the limitation to each of the affected operations
table.read(limitToAuthenticatedUser);
table.update(limitToAuthenticatedUser);
table.delete(limitToAuthenticatedUser);

module.exports = table;
