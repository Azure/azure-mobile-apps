var generateFiles = require('./generateFiles'),
    writer = require('./writer'),
    static = require('../static'),
    path = require('path')

module.exports = function (inputPath, outputPath) {
    var tables = generateFiles('table', path.join(inputPath, 'table')),
        apis = generateFiles('api', path.join(inputPath, 'api'), '*'),
        root = static(),
        shared = static(path.join(inputPath, 'shared')),
        count = Object.keys(tables).length + Object.keys(apis).length + Object.keys(root).length

    writer(outputPath, root)
    writer(path.join(outputPath, '/tables'), tables)
    writer(path.join(outputPath, '/api'), apis)
    writer(path.join(outputPath, '/shared'), shared)

    console.log("Wrote " + count + " files")
}
