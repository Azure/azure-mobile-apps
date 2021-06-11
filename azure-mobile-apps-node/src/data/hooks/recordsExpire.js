// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var duration = require('../../utilities/duration');

module.exports = {
    filter: function (query, context) {
        if(context.table.recordsExpire)
            return query.where(function (expiry) {
                return this.createdAt > expiry;
            }, duration.subtract(new Date(), context.table.recordsExpire));
        return query;
    }
};
