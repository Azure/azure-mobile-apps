var fs = require('fs'),
    path = require('path')

module.exports = function (targetPath) {
    targetPath = targetPath || __dirname
    return obtainFiles(targetPath, '')
}

function obtainFiles(targetPath, childPath, target) {
    return fs.readdirSync(path.join(targetPath, childPath)).reduce(function (files, filename) {
        // npm renames .gitignore to .npmignore - safer to treat specially
        var targetFilename = filename === 'gitignore' ? '.gitignore' : filename;

        // recurse through directories
        if(fs.statSync(path.join(targetPath, childPath, filename)).isDirectory())
            obtainFiles(targetPath, filename, files)

        // ignore this file and cruft files in a default mobile service shared folder
        else if(!(filename === 'index.js' && targetPath === __dirname)
            && ['__fxutil.js', 'placeholder', 'readme.md'].indexOf(filename.toLowerCase()) === -1)
            files[path.join(childPath, targetFilename)] = fs.readFileSync(path.join(targetPath, childPath, filename))

        return files
    }, target || {})
}
