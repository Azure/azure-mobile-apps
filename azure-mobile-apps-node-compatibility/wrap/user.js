var promise = require('./promise')

module.exports = function (context) {
    if(!context.user)
        return {
            getIdentities: function (options) {
                if(options && options.success)
                    options.success({})
                return {};
            },
            accessTokens: {},
            level: 'anonymous',
            userId: undefined
        }

    var tokens = {}
    tokens[context.user.claims.provider] = context.user.tokens

    return {
        getIdentities: function (options) {
            promise(context.user.getIdentities(), options, context.logger)
        },
        accessTokens: tokens,
        level: 'authenticated',

        // to get old user id, see https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-net-upgrading-from-mobile-services/#authentication
        userId: context.user.id
    }
}
