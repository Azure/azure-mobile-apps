// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var rimraf = require('rimraf'),
    path = require('path'),
    fs = require('fs-sync'),
    execSync = require('child_process').execSync,
    config = require('./config');
    
var e2eTestRoot = path.join(__dirname, '..');

function run(command) {
    
    console.log('Running : ' + command);
    var result = execSync(command, {
        encoding: 'utf8'
    });
    
    console.log(result);
}

function setupCordovaTests() {

    console.log('Starting Cordova setup');
    process.chdir(e2eTestRoot);

    // Clean up previous builds
    rimraf.sync('./cordova/www/generated');
    
    fs.copy('./shared', './cordova/www/generated/shared');
    fs.copy('./shared', './cordova/www/generated/shared');
    
    process.chdir('./cordova');
    
    if (!config.cordova.refreshOnly) {
        
        rimraf.sync('./platforms');
        rimraf.sync('./plugins');

        // The Azure Mobile App Cordova plugin can be saved in config.xml, but this way of installation
        // gives us control over where to install the plugins from (github / npm registry / custom path).
        var azureMobilePlugin;
        if (process.argv[2] === 'github') {
            azureMobilePlugin = config.cordova.azureMobilePlugin.github;
        } else if (process.argv[2] === 'npm') {
            azureMobilePlugin = config.cordova.azureMobilePlugin.npm;
        } else if (process.argv[2]) {
            azureMobilePlugin = process.argv[2];
        } else {
            throw new Error('Azure Mobile Apps Cordova plugin path missing');
        }

        console.log('Installing Azure Mobile Apps Cordova plugin : ' + azureMobilePlugin);
        run('cordova plugin add ' + azureMobilePlugin);

        console.log('Installing Cordova platforms..');
        
        if (config.cordova.platforms.windows && process.platform === 'win32') {
            console.log('Preparing for windows..');
            run('cordova platform add windows');
            run('cordova build windows');
        }
        
        if (config.cordova.platforms.android) {
            console.log('Preparing for android..');
            run('cordova platform add android');
            run('cordova build android');
        }
        
        if (config.cordova.platforms.ios && process.platform === 'darwin') {
            console.log('Preparing for ios..');
            run('cordova platform add ios');
            run('cordova build ios');
        }
        
        if (config.cordova.platforms.wp8 && process.platform === 'win32') {
            console.log('Preparing for wp8..');
            run('cordova platform add wp8');
            run('cordova build wp8');
        }

        console.log('Cordova platforms installation and build done!');

    } else {
        console.log('Skipping plugin and platform installation!');
    }
    
    console.log('Cordova setup done!');
}

function setupBrowserTests() {

    console.log('Starting Browser setup');
    
    process.chdir(e2eTestRoot);

    // Clean up previous builds
    rimraf.sync('./browser/generated');
    
    fs.copy('../dist', './browser/generated/dist');
    fs.copy('./shared', './browser/generated/shared');

    console.log('Browser setup done!');
}

function setupWinjsTests() {

    console.log('Starting WinJS setup..');

    process.chdir(e2eTestRoot);

    // Clean up previous builds
    rimraf.sync('./winjs/WinjsEndToEndTests/generated');
    
    fs.copy('../sdk/src/Generated/MobileServices.js', './winjs/WinjsEndToEndTests/generated/dist/MobileServices.js');
    fs.copy('./shared', './winjs/WinjsEndToEndTests/generated/shared');

    console.log('WinJS setup done!');
}


setupCordovaTests();
setupWinjsTests();
setupBrowserTests();
