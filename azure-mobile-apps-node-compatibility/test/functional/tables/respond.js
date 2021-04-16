var wrap = require('../../..').wrap,
    table = require('azure-mobile-apps').table();

table.read(wrap.read(function (tables, push, request, user, statusCodes) {
    return function read(query, user, request) {
        request.respond()
    }
}));

table.insert(wrap.insert(function (tables, push, request, user, statusCodes) {
    return function insert(item, user, request) {
        request.respond(new Error("test"))
    }
}));

table.update(wrap.update(function (tables, push, request, user, statusCodes) {
    return function update(item, user, request) {
        request.respond(statusCodes.SERVICE_UNAVAILABLE, "unavailable")
    }
}));

module.exports = table;
