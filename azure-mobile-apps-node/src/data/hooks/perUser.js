// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

module.exports = {
    filter: function (query, context) {
        return query.where(perUserPredicate(context));
    },
    transform: function (item, context) {
        item[context.table.userIdColumn] = context.user && context.user.id;
    }
};

function perUserPredicate(context) {
    var predicate = {};
    predicate[context.table.userIdColumn] = context.user && context.user.id;
    return predicate;
}