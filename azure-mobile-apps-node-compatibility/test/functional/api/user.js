var wrap = require('../../..').wrap;

module.exports = wrap.api(function (exports, statusCodes) {
    exports.get = function(request, response) {
        response.json(request.user)
    }
})
