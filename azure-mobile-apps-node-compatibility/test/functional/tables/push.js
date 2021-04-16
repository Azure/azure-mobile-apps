var wrap = require('../../..').wrap,
    table = require('azure-mobile-apps').table();

table.read(wrap.read(function (tables, push, request, user, statusCodes) {
    return function read(query, user, request) {
        request.respond(200, { sendExists: !!(push && push.send) })
    }
}));

module.exports = table
