var wrap = require('azure-mobile-apps-compatibility').wrap,
    table = require('azure-mobile-apps').table();

<% Object.keys(operations).forEach(function (operationName) { %>
table.<%= operationName %>(wrap.<%= operationName %>(function (tables, push, request, user, statusCodes, context) {
<%= operations[operationName] %>

return <%= operationName %>;
}));
<% }) %>

module.exports = table;
