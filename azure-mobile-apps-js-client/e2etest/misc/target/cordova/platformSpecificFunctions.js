// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

function createPlatformSpecificFunctions() {

    // Currently only implemented for Android
    function getApplicationParam(name) {
        var config = {};

        var def = $.Deferred();

        if (device.platform === 'Android') {
            window.plugins.webintent.getExtra(name,
                function (value) {
                    def.resolve(value);
                }, function () {
                    def.resolve();
                }
            );
        } else {
            def.resolve();
        }

        return def;
    }

    function getAppConfig() {
        return $.when(
            getApplicationParam('serverPlatform'),
            getApplicationParam('appUrl'),
            getApplicationParam('containerUrl'),
            getApplicationParam('storageAccessToken'),
            getApplicationParam('generateReport'))
            .then(function (serverPlatform, appUrl, containerUrl, storageAccessToken, generateReport) {
                return {
                    serverPlatform: serverPlatform,
                    appUrl: appUrl,
                    containerUrl: containerUrl,
                    storageAccessToken: storageAccessToken,
                    generateReport: generateReport
                };
            });
    }

    var alertFunction;
    if (typeof alert === 'undefined') {
        alertFunction = function (text, done) {
            var dialog = new Windows.UI.Popups.MessageDialog(text);
            dialog.showAsync().done(function () {
                if (typeof done === 'function') {
                    done();
                }
            });
        }
    } else {
        alertFunction = function (text, done) {
            window.alert(text);
            if (done) {
                done();
            }
        };
    }

    var saveAppInfo = function (lastAppUrl) {
        /// <param name="lastAppUrl" type="String">The last value used in the application URL text box</param>
        var state = {
            lastAppUrl: lastAppUrl
        };
    };

    return {
        alert: alertFunction,
        saveAppInfo: saveAppInfo,
        IsHTMLApplication: true,
        getAppConfig: getAppConfig
    };
}

var testPlatform = createPlatformSpecificFunctions();
