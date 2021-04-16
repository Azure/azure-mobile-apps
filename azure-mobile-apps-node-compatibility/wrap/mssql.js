var mssql = require('msnodesqlv8'),
    format = require('util').format

module.exports = function (context) {
    var config = context.configuration.data,
        connectionString = format('Driver={SQL Server Native Client 11.0};Database=%s;Server=%s;uid=%s;pwd=%s', config.database, config.server, config.user, config.password)

    return {
        query: function (sql, params, options) {
            mssql.query(connectionString, sql, params, executeCallback(options))
        },
        queryRaw: function (sql, params, options) {
            mssql.queryRaw(connectionString, sql, params, executeCallback(options))
        },
        open: function (options) {
            mssql.query(connectionString, executeCallback(options))
        }
    }

    function executeCallback(options) {
        return function (err, results) {
            if(err)
                if(options && options.err) {
                    options.error(err)
                } else {
                    context.logger.error("Unhandled SQL Error", err)
                }
            if(results && options && options.success)
                options.success(results)
        }
    }
}
