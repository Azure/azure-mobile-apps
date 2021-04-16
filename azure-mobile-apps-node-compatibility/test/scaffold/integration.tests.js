var scaffold = require('../../scaffold'),
    expect = require('chai').use(require('chai-subset')).expect,
    rimraf = require('rimraf').sync,
    fs = require('fs'),

    inputPath = __dirname + '/files',
    outputPath = __dirname + '/output'

describe('azure-mobile-apps-compatibility.scaffold.integration', function () {
    before(function () {
        scaffold(inputPath, outputPath)
    })

    after(function () {
        rimraf(outputPath)
    })

    it("creates correct files", function () {
        expect(fs.readdirSync(outputPath)).to.deep.equal([
            '.gitignore',
            'api',
            'app.js',
            'azureMobile.js',
            'createViews.sql',
            'package.json',
            'shared',
            'tables',
            'web.config'
        ])

        expect(fs.readdirSync(outputPath + '/tables')).to.deep.equal([
            'friends.js',
            'friends.json',
            'messages.js',
            'messages.json'
        ])

        expect(fs.readdirSync(outputPath + '/api')).to.deep.equal([
            'custom.js',
            'custom.json'
        ])

        expect(fs.readdirSync(outputPath + '/shared')).to.deep.equal([
            'shared.js',
            'subfolder'
        ])

        expect(fs.readdirSync(outputPath + '/shared/subfolder')).to.deep.equal([
            'child.js'
        ])
    });
});
