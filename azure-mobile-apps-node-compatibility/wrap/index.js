var query = require('./query'),
    apiRequest = require('./request.api'),
    tableRequest = require('./request.table'),
    user = require('./user'),
    statusCodes = require('./statusCodes'),
    promises = require('azure-mobile-apps/src/utilities/promises')

module.exports = {
    read: tableWrapper(function (context) {
        return [query(context), user(context), tableRequest(context)]
    }),
    insert: tableWrapper(function (context) {
        return [context.item, user(context), tableRequest(context)]
    }),
    update: tableWrapper(function (context) {
        return [context.item, user(context), tableRequest(context)]
    }),
    delete: tableWrapper(function (context) {
        return [context.id, user(context), tableRequest(context)]
    }),
    api: apiWrapper
}

function tableWrapper(argumentFactory) {
    return function (generatedHandler) {
        return function (context) {
            var userHandler = generatedHandler(context.tables, context.push, tableRequest(context), user(context), statusCodes, context),
                promise = promises.create(function (resolve, reject) {
                    context.setExecutePromise = function (promise) {
                        promise.then(resolve).catch(reject)
                    }
                })

            userHandler.apply(null, argumentFactory(context))
            return promise
        }
    }
}

function apiWrapper(generatedHandler) {
    var methods = {}
    generatedHandler(methods, statusCodes)

    return Object.keys(methods).reduce(function (definition, method) {
        definition[method] = function (req, res, next) {
            methods[method](apiRequest(req.azureMobile), res)
        }
        return definition
    }, {})
}
