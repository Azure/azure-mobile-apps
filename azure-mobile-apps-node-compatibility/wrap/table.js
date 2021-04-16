var promise = require('./promise'),
    queries = require('azure-mobile-apps/src/query')

module.exports = function (context, table) {
    var data = context.data(table)

    return attachOperators(table.name, {
        read: function (options) {
            promise(data.read(queries.create(table.name)), options, context.logger)
        },
        del: function (itemOrId, options) {
            var query = queries.create(table.name).where({ id: typeof itemOrId === 'object' ? itemOrId.id : itemOrId })
            promise(data.delete(query), options, context.logger)
        },
        insert: function (item, options) {
            promise(data.insert(item), options, context.logger)
        },
        update: function (item, options) {
            promise(data.update(item), options, context.logger)
        }
    })

    function attachOperators(name, table) {
        ['where', 'select', 'orderBy', 'orderByDescending', 'skip', 'take', 'includeTotalCount'].forEach(attachOperator)

        return table;

        function attachOperator(operator) {
            table[operator] = function () {
                var query = queries.create(name)
                query.read = function (options) {
                    promise(data.read(query), options, context.logger)
                }
                query[operator].apply(query, arguments)
                return query
            }
        }
    }
}
