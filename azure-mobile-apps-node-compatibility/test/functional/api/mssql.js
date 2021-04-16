var wrap = require('../../..').wrap;

module.exports = wrap.api(function (exports, statusCodes) {
    exports.get = function(request, response) {
        request.service.mssql.query('select ? as value', [1], {
            success: function (results) {
                response.send(200, results)
            }
        })
    }
})
