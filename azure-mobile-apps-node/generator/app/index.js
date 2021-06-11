'use strict';

var generators = require('yeoman-generator'),
    chalk = require('chalk'),
    yosay = require('yosay');

module.exports = generators.Base.extend({
    constructor: function() {
        var testLocal;

        generators.Base.apply(this, arguments);

        this.option('skip-install-message', {
            desc: 'Skips the message after the installation of dependencies',
            type: Boolean
        });
        this.option('test-framework', {
            desc: 'Test framework to be invoked',
            type: String,
            defaults: 'mocha'
        });

        if (this.options['test-framework'] === 'mocha') {
            testLocal = require.resolve('generator-mocha/generators/app/index.js');
        } else if (this.options['test-framework'] === 'jasmine') {
            testLocal = require.resolve('generator-jasmine/generators/app/index.js');
        }

        this.composeWith(this.options['test-framework']+':app', {
            options: {
                'skip-install': this.options['skip-install']
            }
        }, {
            local: testLocal
        });
    },

    initializing: function() {
        this.pkg = require('../package.json');
    },

    prompting: function() {
        var done = this.async();

        this.log(yosay(
            'Welcome to the ' + chalk.red('Azure App Service Mobile Apps') + ' generator!'
        ));

        // We have nothing more to ask for right now.
        done();
    },

    writing: {
        'files': function() {
            var self = this;

            // Copy the following files as-is
            var fileList = [
                '.deployment',
                '.editorconfig',
                '.eslintrc.js',
                '.gitignore',
                'deploy.cmd',
                'jsconfig.json',
                'bin/www',
                'config/custom-environment-variables.json',
                'config/default.json',
                'public/index.html',
                'server/api/example.js',
                'server/tables/example.js',
                'server/app.js',
                'server/logger.js',
                'test/logger.js',
                'test/spec/.eslintrc.js'
            ];

            fileList.forEach(function copyFile(filename) {
                // Initial dots get replaced by underscores in the template directory
                // This is to avoid the .gitignore problem where certain files are not
                // kept because they are in the .gitignore.
                let srcfile = filename.replace(/^\./, '_');
                self.fs.copy(
                    self.templatePath(srcfile),
                    self.destinationPath(filename)
                );
            });
        },

        'templateFiles': function() {
            var self = this;

            // Copy the following files using template strings
            var fileList = [
                'test/.eslintrc.js',
                'test/app.js'
            ];

            fileList.forEach(function copyFile(filename) {
                self.fs.copyTpl(
                    self.templatePath(filename),
                    self.destinationPath(filename),
                    {
                        testFramework: self.options['test-framework']
                    }
                );
            });
        },

        // Special case package.json
        'package-json': function() {
            this.fs.copyTpl(
                this.templatePath('_package.json'),
                this.destinationPath('package.json'),
                {
                    testFramework: this.options['test-framework']
                }
            );
        }
    },

    install: function() {
        this.installDependencies({
            npm: true,
            bower: false,
            skipInstall: this.options['skip-install'],
            skipMessage: this.options['skip-install-message']
        });
    },

    end: function() {
        var howToInstall = '\nAfter running ' + chalk.yellow.bold('npm install') + ', publish your service to Azure App Service.';

        if (this.options['skip-install']) {
            this.log(howToInstall);
            return;
        }
    }
});
