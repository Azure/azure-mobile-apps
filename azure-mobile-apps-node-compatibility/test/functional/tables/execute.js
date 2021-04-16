var wrap = require('../../..').wrap,
    table = require('azure-mobile-apps').table();

table.read(wrap.read(function (tables, push, request, user, statusCodes) {
    return function(query, user, request) {
        // ensure async works
        setTimeout(function () {
            request.execute()
        })
    }
}));

table.insert(wrap.insert(function (tables, push, request, user, statusCodes) {
    return function(item, user, request) {
        request.execute()
    }
}));

table.update(wrap.update(function (tables, push, request, user, statusCodes) {
    return function(item, user, request) {
        request.execute()
    }
}));

table.delete(wrap.delete(function (tables, push, request, user, statusCodes) {
    return function(item, user, request) {
        request.execute()
    }
}));


module.exports = table;
