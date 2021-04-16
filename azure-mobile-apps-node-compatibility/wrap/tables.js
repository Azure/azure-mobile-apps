var table = require('./table')

module.exports = function (context) {
    return {
        getTable: function (tableName) {
            return table(context, context.configuration.tables[tableName])
        },
        current: context.table && table(context, context.table)
    }
}
