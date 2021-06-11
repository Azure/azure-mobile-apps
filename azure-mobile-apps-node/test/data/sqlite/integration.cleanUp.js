var provider = require('../../../src/data/sqlite');

module.exports = function (configuration, table) {
    var data = provider(configuration);
    
    table.sqliteColumns = undefined;

    return data.execute({ sql: 'DROP TABLE __types' })
        .catch(function () {})
        .then(function () {
            return data.execute({ sql: 'DROP TABLE ' + table.name });
        })
        .then(function () {})
        .catch(function () {});
};
