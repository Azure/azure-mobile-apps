// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

module.exports = {
    cordova: {
        // If this flag is set to true, plugins, platforms are left untouched and
        // only the source files are refreshed. Useful during development.
        refreshOnly: false,
    
        azureMobilePlugin: {
            github: 'https://github.com/azure/azure-mobile-apps-cordova-client.git',
            npm: 'cordova-plugin-ms-azure-mobile-apps'
        },
        
        // Platform to add to the Cordova project and build. The platforms will be built only if the 
        // host machine supports building that platform.
        platforms: {
            windows: false,
            android: true,
            ios: false,
            wp8: false
        }

    }
};
