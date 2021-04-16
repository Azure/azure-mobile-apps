var mkdirp = require('mkdirp').sync,
    path = require('path'),
    fs = require('fs')

module.exports = function (outputPath, files) {
    Object.keys(files).forEach(function (filename) {
        var fullPath = path.join(outputPath, filename)            
        mkdirp(path.dirname(fullPath))
        fs.writeFile(fullPath, files[filename])
    })
}
