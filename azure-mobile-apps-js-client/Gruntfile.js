module.exports = function(grunt) {
    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        files: {
            // Entry points common to all platforms
            core: [
                'sdk/src/index.js',
            ],
            // List of Web entry points
            web: [
                '<%= files.core %>',
            ],
            // List of Cordova entry points
            cordova: [
                'sdk/src/Platforms/cordova/MobileServiceSqliteStore.js',
                '<%= files.core %>',
            ],
            // Entry points common to tests for all platforms
            testcore: [
                'sdk/test/misc/**/*.js',
                'sdk/test/tests/shared/**/*.js'
            ],
            // List of all javascript files that we want to validate and watch
            // i.e. all javascript files except those that are installed, generated during build, third party files, etc
            all: [
                'Gruntfile.js',
                'sdk/src/**/*.js',
                'sdk/test/**/*.js',
                '!**/[Gg]enerated/**',
                '!**/[Ee]xternal/**',
                '!sdk/test/app/cordova/platforms/**',
                '!sdk/test/app/cordova/merges/**',
                '!sdk/test/**/bin/**',
                '!sdk/test/**/plugins/**'
            ]
        },        
        jshint: {
            all: '<%= files.all %>'
        },
        uglify: {
            options: {
                banner: '//! Copyright (c) Microsoft Corporation. All rights reserved. <%= pkg.name %> v<%= pkg.version %>\n',
                mangle: false
            },
            web: {
                src: 'sdk/src/generated/azure-mobile-apps-client.js',
                dest: 'sdk/src/generated/azure-mobile-apps-client.min.js'
            },
            cordova: {
                src: 'sdk/src/generated/azure-mobile-apps-client-cordova.js',
                dest: 'sdk/src/generated/azure-mobile-apps-client-cordova.min.js'
            }
        },
        browserify: {
            options: {
                browserifyOptions: {
                    standalone: 'WindowsAzure'
                },
                plugin: [
                    [ 'browserify-derequire' ]
                ],
                transform: [ 'package-json-versionify' ],
                banner: header
            },
            web: {
                src: '<%= files.web %>',
                dest: './sdk/src/generated/azure-mobile-apps-client.js'
            },
            cordova: {
                src: '<%= files.cordova %>',
                dest: './sdk/src/generated/azure-mobile-apps-client-cordova.js'
            },
            webTest: {
                src: [
                    '<%= files.web %>',
                    '<%= files.testcore %>'
                ],
                dest: './sdk/test/app/browser/generated/tests.js'
            },
            cordovaTest: {
                src: [
                    '<%= files.testcore %>',
                    '<%= files.cordova %>',
                    './sdk/test/tests/target/cordova/**/*.js'
                ],
                dest: './sdk/test/app/cordova/www/scripts/generated/tests.js'
            },
        },
        copy: {
            web: {
                src: 'azure-mobile-apps-client.*js',
                dest: 'dist/',
                expand: true,
                cwd: 'sdk/src/generated/'
            },
            cordova: {
                src: 'azure-mobile-apps-client-cordova.*js',
                dest: 'dist/',
                expand: true,
                cwd: 'sdk/src/generated/'
            },
            webTest: {
                src: '*',
                dest: 'sdk/test/app/browser/external/qunit/',
                expand: true,
                cwd: 'node_modules/qunitjs/qunit'
            },
            cordovaTest: {
                // Copy qunit css and js files to the Cordova unit test app directory
                src: ['*'],
                dest: 'sdk/test/app/cordova/www/external/qunit/',
                expand: true,
                cwd: 'node_modules/qunitjs/qunit'
            },
            hostCordovaTest: {
                // Copy the test bundle to the Cordova unit test app's android directory.
                // This is needed to host the Cordova bits so that the Cordova app can refresh on the fly.
                src: ['tests.js'],
                dest: __dirname + '/sdk/test/app/cordova/platforms/android/assets/www/scripts/generated',
                expand: true,
                cwd: __dirname + '/sdk/test/app/cordova/www/scripts/generated'
            },
            e2etestShared: {
                src: [
                    'misc/**',
                    'tests/**'
                ],
                //dest: refer multidest task
                expand: true,
                cwd: __dirname + '/e2etest'
            },
            e2etestDist: {
                src: 'dist/**',
                //dest: refer multidest task
                expand: true,
                cwd: __dirname
            }
        },
        watch: {
            all: {
                files: '<%= files.all %>',
                tasks: ['browserify', 'copy']
            },
            web: {
                files: '<%= files.all %>',
                tasks: ['browserify:webTest', 'copy:webTest']
            },
            cordova: {
                files: '<%= files.all %>',
                tasks: ['browserify:cordovaTest', 'copy:cordovaTest', 'copy:hostCordovaTest']
            }
        },
        multidest: {
            e2etest: {
                tasks: [
                    'copy:e2etestShared',
                    'copy:e2etestDist'
                ],
                dest: [
                    'e2etest/app/browser-bundle-using-script-tag/generated',
                    'e2etest/app/browser-bundle-as-commonjs/generated',
                    'e2etest/app/browser-bundle-as-amd/generated',
                    'e2etest/app/browser-package-browserify/generated',
                    'e2etest/app/browser-package-webpack/generated',
                    'e2etest/app/cordova/www/generated'
                ]
            }
        }
    });

    // Load the plugins
    grunt.loadNpmTasks('grunt-contrib-uglify');
    grunt.loadNpmTasks('grunt-contrib-jshint');
    grunt.loadNpmTasks('grunt-browserify');
    grunt.loadNpmTasks('grunt-contrib-watch');
    grunt.loadNpmTasks('grunt-contrib-copy');
    grunt.loadNpmTasks('grunt-multi-dest');
        
    // Define build tasks
    grunt.registerTask('build', ['buildbrowserfull', 'buildcordovafull', 'jshint']);
    grunt.registerTask('buildbrowsermin', ['browserify:web', 'browserify:webTest', 'copy:web', 'copy:webTest']);
    grunt.registerTask('buildcordovamin', ['browserify:cordova', 'browserify:cordovaTest', 'copy:cordova', 'copy:cordovaTest']);
    grunt.registerTask('buildbrowserfull', ['browserify:web', 'browserify:webTest', 'uglify:web', 'copy:web', 'copy:webTest']);
    grunt.registerTask('buildcordovafull', ['browserify:cordova', 'browserify:cordovaTest', 'uglify:cordova', 'copy:cordova', 'copy:cordovaTest']);

    grunt.registerTask('e2esetup', ['copy:e2etestDist', 'multidest:e2etest']);

    // Define the default task
    grunt.registerTask('default', ['build']);
};

var header = '// ----------------------------------------------------------------------------\n' +
             '// Copyright (c) Microsoft Corporation. All rights reserved\n' +
             '// <%= pkg.name %> - v<%= pkg.version %>\n' +
             '// ----------------------------------------------------------------------------\n';

