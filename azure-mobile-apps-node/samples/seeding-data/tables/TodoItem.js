var azureMobileApps = require('azure-mobile-apps');

var table = azureMobileApps.table();

// Defines the list of columns
table.columns = {
    "text": "string",
    "complete": "boolean"
};
// Turns off dynamic schema
table.dynamicSchema = false;

// Seed data into the table
table.seed = [
    { text: "Example 1", complete: true },
    { text: "Example 2", complete: false }
];

// Read-only table - turn off write operations
table.insert.access = 'disabled';
table.update.access = 'disabled';
table.delete.access = 'disabled';

module.exports = table;