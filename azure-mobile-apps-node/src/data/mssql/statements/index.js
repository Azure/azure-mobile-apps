// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
module.exports = {
    insert: require('./insert'),
    update: require('./update'),
    delete: require('./delete'),
    read: require('./read'),
    truncate: require('./truncate'),
    createTable: require('./createTable'),
    getColumns: require('./getColumns'),
    updateSchema: require('./updateSchema'),
    createTrigger: require('./createTrigger'),
    undelete: require('./undelete'),
    createIndex: require('./createIndex'),
    getIndexes: require('./getIndexes'),
};
