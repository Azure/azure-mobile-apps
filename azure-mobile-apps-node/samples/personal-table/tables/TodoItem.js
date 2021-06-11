var azureMobileApps = require('azure-mobile-apps');

var table = azureMobileApps.table();

// Defines the list of columns
table.columns = {
    "userId": "string",
    "text": "string",
    "complete": "boolean"
};
// Turns off dynamic schema
table.dynamicSchema = false;

// Must be authenticated for this to work
table.access = 'authenticated';

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

// When adding a new record, overwrite the userId with the
// id of the user so that the read/update/delete limitations
// will work properly
table.insert(function (context) {
    context.item.userId = context.user.id;
    return context.execute();
});

module.exports = table;
