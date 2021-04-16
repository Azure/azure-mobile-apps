var fs = require('fs'),
    path = require('path')

module.exports = function(sourcePath) {
    return fs.readdirSync(sourcePath).reduce(function (target, filename) {
        if(isTargetFile())
            addContentToTarget(target, filename, fs.readFileSync(path.join(sourcePath, filename), 'utf8'))
        return target;

        function isTargetFile() {
            var file = path.parse(filename)
            return (file.ext === '.js' || file.ext === '.json')
                && file.name !== '__fxutil'
                && fs.statSync(path.join(sourcePath, filename)).isFile()
        }
    }, {})
}

function addContentToTarget(target, filename, content) {
    var parsed = path.parse(filename),
        fileName = getFileName(),
        itemName = fileName.toLowerCase()

    if(!target[itemName])
        target[itemName] = { operations: {} }

    if(parsed.ext === '.json') {
        // mobile services only preserves casing on the .json file
        target[itemName].fileName = fileName
        target[itemName].permissions = JSON.parse(content)
    } else {
        target[itemName].fileName = target[itemName].fileName || fileName
        target[itemName].operations[operationName()] = content
    }

    function getFileName() {
        var index = parsed.name.indexOf('.')
        return (index === -1 ? parsed.name : parsed.name.substring(0, index))
    }

    function operationName() {
        var index = parsed.name.indexOf('.')
        return parsed.name.indexOf('.') > -1 ? parsed.name.substring(index + 1) : 'api'
    }
}
