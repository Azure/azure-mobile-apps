var load = require('./load'),
    mapPermissions = require('./mapPermissions'),
    templates = require('../templates')

module.exports = function (template, path, route) {
    var items = load(path)

    return Object.keys(items).reduce(function (files, itemName) {
        try {
            var item = items[itemName]

            files[item.fileName + '.json'] = JSON.stringify(mapPermissions(item.permissions, route), null, 2)
            files[item.fileName + '.js'] = templates(template, item)
        } catch(ex) {
            console.log("Failed to process item " + itemName + " from " + path)
        }

        return files;
    }, {})
}
