var fs = require('fs'),
    _ = require('underscore')

module.exports = function (name, data) {
    template = fs.readFileSync(__dirname + '/' + name + '.js', 'utf8')
    return _.template(template)(data)
}
