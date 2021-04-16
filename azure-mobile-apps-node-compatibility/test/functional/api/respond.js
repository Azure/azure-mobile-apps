var wrap = require('../../..').wrap;

module.exports = wrap.api(function (exports, statusCodes) {
    exports.get = function(request, response) {
        request.respond(203, { message : 'get' })
    }

    exports.post = function(request, response) {
        response.send(statusCodes.ACCEPTED, { message : 'post' })
    }
})
