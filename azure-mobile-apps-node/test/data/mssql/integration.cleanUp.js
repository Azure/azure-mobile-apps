var execute = require('../../../src/data/mssql/execute');

module.exports = function (config, table) {
    return execute(config, { sql: 'DROP TABLE ' + table.name });
};
