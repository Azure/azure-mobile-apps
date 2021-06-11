var azureMobileApps = require('azure-mobile-apps');

var table = azureMobileApps.table();

// Dynamic Schema is the default setting
// table.dynamicSchema = true;

module.exports = table;