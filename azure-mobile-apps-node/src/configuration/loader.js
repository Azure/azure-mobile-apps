// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var path = require('path'),
    fs = require('fs'),
    logger = require('../logger'),
    merge = require('../utilities/merge'),

    supportedExtensions = ['.js', '.json'];

module.exports = {
    loadPath: function (targetPath, basePath) {
        basePath = basePath || path.dirname(module.parent.filename);
        var fullPath = path.resolve(basePath, targetPath);

        // this won't work with other extensions (e.g. .ts, .coffee)
        // perhaps we should use require.resolve here instead - also enables loading modules in other packages
        if (!fs.existsSync(fullPath) || !fs.statSync(fullPath).isDirectory()) {
            // remove path extension
            var filesPath = fullPath;
            if(path.extname(fullPath))
                filesPath = fullPath.slice(0, -path.extname(fullPath).length)

            // get all files with supported extensions
            var filePaths = getFilePaths(filesPath);

            if (filePaths.length) {
                return loadFiles({}, filePaths);
            } else {
                logger.warn('Requested configuration path (' + fullPath + ') does not exist');
                return {};
            }
        }
        else
            return loadDirectory({}, fullPath);
    }
}

function loadModule(target, targetPath) {
    var extension = path.extname(targetPath),
        moduleName = path.basename(targetPath, extension),
        targetModule = target[moduleName] || {},
        loadedModule;

    if(supportedExtensions.indexOf(extension) > -1) {
        try {
            loadedModule = require(targetPath);

            logger.silly('Loaded ' + targetPath);

            merge.getConflictingProperties(targetModule, loadedModule).forEach(function (conflict) {
                logger.warn('Property \'' + conflict + '\' in module ' + moduleName + ' overwritten by JSON configuration');
            });
            // due to lexicographic ordering, .js is loaded before .json
            target[moduleName] = merge.mergeObjects(targetModule, loadedModule);
        } catch (err) {
            logger.error('Unable to load ' + targetPath, err);
            // throw err;
        }
    }

    return target;
}

function loadDirectory(target, targetPath) {
    fs.readdirSync(targetPath).forEach(function (iterationPath) {
        var fullPath = path.join(targetPath, iterationPath);
        if (fs.statSync(fullPath).isDirectory())
            loadDirectory(target, fullPath);
        else
            loadModule(target, fullPath);
    });
    return target;
}

function getFilePaths(targetPath) {
    return supportedExtensions.map(function (extension) {
            return targetPath + extension;
        })
        .filter(function (path) {
            return fs.existsSync(path);
        });
}

function loadFiles(target, targetPaths) {
    targetPaths.forEach(function (path) {
        loadModule(target, path);
    });
    return target;
}
