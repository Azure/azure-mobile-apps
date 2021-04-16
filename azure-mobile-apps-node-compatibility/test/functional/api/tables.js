var wrap = require('../../..').wrap;

module.exports = wrap.api(function (exports, statusCodes) {
    exports.post = function(request, response) {
        request.service.tables.getTable('api').insert({ id: request.query.id }, {
            success: function (results) {
                response.send(201, "Created")
            }
        })
    }

    exports.get = function(request, response) {
        // ensure a new query is created each time table.where is called
        var table = request.service.tables.getTable('api')
        table.where({ id: 'non-existent' }).read({
            success: function (results) {
                table.where({ id: request.query.id }).read({
                    success: function (results) {
                        response.send(results)
                    }
                })
            }
        })
    }
})
