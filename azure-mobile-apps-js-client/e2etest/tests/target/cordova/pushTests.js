// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

function definePushTestsNamespace() {
    var tests = [],
        deviceToken,
    	templateName = 'myPushTemplate',
        waitTime = 30000,
        GCM_SENDER_ID = '13925784256';

    // TODO: See if we can check if we are running on a simulator and abort the tests

    // Validate native push registration
    tests.push(new zumo.Test('Request Device Token/Id', function (test, done) {
        console.log("Running " + test.name);
        if (!runTest('')) {
            done(true);
            return;   
        }

        console.log("Getting device token");
        GetDeviceToken(test, function (error, token) {
            if (error) {
                console.log('Error: Requesting token: ' + error);
                test.addLog('Error: Requesting token: ', error);
            } else {
                console.log('Success: Got token: ' + token);
                test.addLog('Success: Got token: ', token);
            }
            done(!error);
        });
    }, ['nhPushEnabled']));

    // Simple alert test
    tests.push(new zumo.Test('Alert', function (test, done) {
        console.log("Running " + test.name);
        if (!runTest('iOS')) {
            done(true);
            return;
        }

        NativeRegistration(deviceToken).then(function () {
            return SendNotification(deviceToken, { 'aps': {'alert': 'push received'} });
        }).then(function () {
            waitForNotification(waitTime, function(event) {
                if (event && event.alert && event.alert == 'push received') {
                    test.addLog('Success: ', event);
                    done(true);
                } else {
                    test.addLog('Push failed or timed out: ', event);
                    done(false);
                }
            });
        }, function (error) {
            test.addLog('Error: ', error);
            done(false);            
        });
    }, ['nhPushEnabled']));

    // Simple badge test
    tests.push(new zumo.Test('Badge', function (test, done) {
        console.log("Running " + test.name);
        if (!runTest('iOS')) {
            done(true);
            return;
        }

        NativeRegistration(deviceToken).then(function () {
            return SendNotification(deviceToken, { 'aps': {'badge': 9} });
        }).then(function () {
            waitForNotification(waitTime, function(event) {
                if (event && event.badge && event.badge == 9) {
                    test.addLog('Success: ', event);
                    done(true);
                } else {
                    test.addLog('Push failed or timed out:', event);
                    done(false);
                }
            });
        }, function (error) {
            test.addLog('Error: ', error);
            done(false);            
        });
    }, ['nhPushEnabled']));

    tests.push(new zumo.Test('Alert and Sound', function (test, done) {
        console.log("Running " + test.name);
        if (!runTest('iOS')) {
            done(true);
            return;
        }

        NativeRegistration(deviceToken).then(function () {
            return SendNotification(deviceToken, { 'aps': {'alert':'push received', 'sound': 'default'} });
        }).then(function () {
            waitForNotification(waitTime, function(event) {
                if (event && event.alert && event.alert == 'push received' &&
                     event.sound && event.sound == 'default') {
                    test.addLog('Success: ', event);
                    done(true);
                } else {
                    test.addLog('Push failed or timed out:', event);
                    done(false);
                }
            });
        }, function (error) {
            test.addLog('Error: ', error);
            done(false);            
        });
    }, ['nhPushEnabled']));

    /* 

    // This test is NOT supported current with the pushPlugin 
    // Should be addressed with: https://github.com/phonegap-build/PushPlugin/pull/290

    tests.push(new zumo.Test('Loc info and parameters', function (test, done) {
        NativeRegistration(deviceToken).then(function () {
            return SendNotification(deviceToken, {'alert':{'loc-key':'LOC_STRING','loc-args':['first', 'second']}});
        }).then(function () {
            waitForNotification(waitTime, function(event) {
                if (event && event.alert && event.alert['loc-key'] == 'LOC_STRING' && event.alert['loc-args'] == '(first, second)') {
                    test.addLog('Success: ', event);
                    done(true);
                } else {
                    test.addLog('Push failed or timed out:', event);
                    done(false);
                }
            });
        }, function (error) {
            test.addLog('Error: ', error);
            done(false);            
        });
    }, ['apns']));

    */

    tests.push(new zumo.Test('Push with only custom info', function (test, done) {
        console.log("Running " + test.name);
        if (!runTest('iOS')) {
            done(true);
            return;
        }

        NativeRegistration(deviceToken).then(function () {
            return SendNotification(deviceToken, {'aps':{},'foo':'bar'});
        }).then(function () {
            waitForNotification(waitTime, function(event) {
                if (event && event.foo && event.foo == 'bar') {
                    test.addLog('Success: ', event);
                    done(true);
                } else {
                    test.addLog('Push failed or timed out:', event);
                    done(false);
                }
            });
        }, function (error) {
            test.addLog('Error: ', error);
            done(false);            
        });
    }, ['nhPushEnabled']));

    tests.push(new zumo.Test('Push with alert, badge and sound', function (test, done) {
        console.log("Running " + test.name);
        if (!runTest('iOS')) {
            done(true);
            return;
        }

        NativeRegistration(deviceToken).then(function () {
            return SendNotification(deviceToken, { 'aps': {'alert':'simple alert', 'badge': 37, 'sound': 'default', 'custom': 'value'} });
        }).then(function () {
            waitForNotification(waitTime, function(event) {
                if (event && event.alert && event.alert == 'simple alert' &&
                     event.sound && event.sound == 'default' &&
                     event.badge && event.badge == 37 &&
                     event.custom && event.custom == 'value') {
                    test.addLog('Success: ', event);
                    done(true);
                } else {
                    test.addLog('Push failed or timed out:', event);
                    done(false);
                }
            });
        }, function (error) {
            test.addLog('Error: ', error);
            done(false);            
        });
    }, ['nhPushEnabled']));

    tests.push(new zumo.Test('Push with alert with non-ASCII characters', function (test, done) {
        console.log("Running " + test.name);
        if (!runTest('iOS')) {
            done(true);
            return;
        }

        NativeRegistration(deviceToken).then(function () {
            return SendNotification(deviceToken, { 'aps': {'alert':'Latin-ãéìôü ÇñÑ, arabic-لكتاب على الطاولة, chinese-这本书在桌子上'} });
        }).then(function () {
            waitForNotification(waitTime, function(event) {
                if (event && event.alert && event.alert == 'Latin-ãéìôü ÇñÑ, arabic-لكتاب على الطاولة, chinese-这本书在桌子上') {
                    test.addLog('Success: ', event);
                    done(true);
                } else {
                    test.addLog('Push failed or timed out:', event);
                    done(false);
                }
            });
        }, function (error) {
            test.addLog('Error: ', error);
            done(false);            
        });
    }, ['nhPushEnabled']));

    // GCM Tests

    tests.push(new zumo.Test('GCM Push: name and age', function (test, done) {
        console.log("Running " + test.name);
        if (!runTest('android')) {
            done(true);
            return;
        }

        NativeRegistration(deviceToken).then(function () {
            return SendNotification(deviceToken, { 'data' : {'name':'John Doe','age':'33'} }, 'gcm');
        }).then(function () {
            waitForNotification(waitTime, function(event) {
                console.log('event: ' + JSON.stringify(event));                
                if (event && event.payload && event.payload.name && event.payload.name == 'John Doe' && 
                    event.payload.age && event.payload.age == '33') {
                    test.addLog('Success: ', event);
                    done(true);
                } else {
                    test.addLog('Push failed or timed out:', event);
                    done(false);
                }
            });
        }, function (error) {
            test.addLog('Error: ', error);
            done(false);            
        });
    }, ['nhPushEnabled']));

    tests.push(new zumo.Test('GCM Push: message', function (test, done) {
        console.log("Running " + test.name);
        if (!runTest('android')) {
            done(true);
            return;
        }

        NativeRegistration(deviceToken).then(function () {
            return SendNotification(deviceToken, { 'data': {'message' : 'MSFT'} }, 'gcm');
        }).then(function () {
            waitForNotification(waitTime, function(event) {
                if (event && event.message && event.message == 'MSFT') {
                    test.addLog('Success: ', event);
                    done(true);
                } else {
                    test.addLog('Push failed or timed out:', event);
                    done(false);
                }
            });
        }, function (error) {
            test.addLog('Error: ', error);
            done(false);            
        });
    }, ['nhPushEnabled']));

    tests.push(new zumo.Test('GCM Push: non-ASCII characters', function (test, done) {
        console.log("Running " + test.name);
        if (!runTest('android')) {
            done(true);
            return;
        }

        NativeRegistration(deviceToken).then(function () {
            return SendNotification(deviceToken, { 'data': {'non-ASCII':'Latin-ãéìôü ÇñÑ, arabic-لكتاب على الطاولة, chinese-这本书在桌子上'} }, 'gcm');
        }).then(function () {
            waitForNotification(waitTime, function(event) {
                console.log('event: ' + JSON.stringify(event));                
                if (event && event.payload && event.payload['non-ASCII'] && 
                    event.payload['non-ASCII'] == 'Latin-ãéìôü ÇñÑ, arabic-لكتاب على الطاولة, chinese-这本书在桌子上') {
                    test.addLog('Success: ', event);
                    done(true);
                } else {
                    test.addLog('Push failed or timed out:', event);
                    done(false);
                }
            });
        }, function (error) {
            test.addLog('Error: ', error);
            done(false);            
        });
    }, ['nhPushEnabled']));

    // Template

    tests.push(new zumo.Test('Template alert', function (test, done) {
        console.log("Running " + test.name);
        if (!runTest('')) {
            done(true);
            return;
        }

        TemplateRegistration(deviceToken).then(function () {
            return SendNotification(deviceToken, {'News_French':'Bonjour', 'News_English':'Hello'}, 'template');
        }).then(function () {
            waitForNotification(waitTime, function(event) {
                if (event && (
                    (event.alert && event.alert == 'Bonjour') /* iOS */ || 
                    (event.message && event.message == 'Bonjour')  /* Android */)) {
                    test.addLog('Success: ', event);
                    done(true);
                } else {
                    test.addLog('Push failed or timed out:', event);
                    done(false);
                }
            });
        }, function (error) {
            test.addLog('Error: ', error);
            done(false);            
        });
    }, ['nhPushEnabled']));

    // unregister tests

    tests.push(new zumo.Test('Unregister native', function (test, done) {
        console.log("Running " + test.name);

        if (!runTest('')) {
            done(true);
            return;
        }

        NativeUnregister().then(function () {
            console.log('Success');
            test.addLog('Success');
            done(true);
        }, function (error) {
            console.log('Error' + error);            
            test.addLog('Error: ', error);
            done(false);
        });
    }, ['nhPushEnabled']));

    tests.push(new zumo.Test('Unregister template', function (test, done) {
        console.log("Running " + test.name);
        if (!runTest('')) {
            done(true);
            return;
        }

        TemplateUnregister(templateName).then(function () {
            test.addLog('Success');
            done(true);
        }, function (error) {
            console.log('Error' + error);
            done(false);
        });
    }, ['nhPushEnabled']));

    return {
        name: 'Push',
        tests: tests
    };

    function GetDeviceToken(test, callback) {
        var pushNotification = window.plugins.pushNotification;

        // If we already have a token return it
        if (deviceToken) {
            callback(null, deviceToken);
            return;
        }

        // Ask for a token instead
        if (device.platform == 'iOS') {
            // Register with APNS for iOS apps.         
            pushNotification.register(
                function (newDeviceToken) {                    
                    test.addLog('APNS Result: ', newDeviceToken);
                    deviceToken = newDeviceToken;
                    callback(null, newDeviceToken);
                }, callback, {
                    "badge":"true",
                    "sound":"true",
                    "alert":"true",
                    "ecb": "app.onNotification"
                });
        } else { //if (device.platform.toLowerCase() == 'android') {
            pushNotification.register(
                function (result) {
                    test.addLog('GCM Result: ', result);
                }, callback, { 
                    "senderID": GCM_SENDER_ID, 
                    "ecb": "app.onNotification" 
                });

            // GCM registration id comes back in as an event
            waitForNotification(waitTime, function(event) {
                if (event && event.event == 'registered' && event.regid && event.regid.length > 0) {
                    deviceToken = event.regid;
                    callback(null, deviceToken);
                } else {
                    callback('Timed out or no GCM Id received');
                }
            });            
        }
    }

    function NativeRegistration(pushHandle) {
        var push = zumo.getClient().push;

        console.log('registering ' + pushHandle.substr(0, 100));
        if (device.platform == 'iOS') {
            return push.apns.registerNative(pushHandle, [pushHandle.substr(0, 100)]);            
        } else { //if (device.platform.toLowerCase() == 'android') {
            return push.gcm.registerNative(pushHandle, [pushHandle.substr(0, 100)]);
        }
    }

    function NativeUnregister(pushHandle) {
        var push = zumo.getClient().push;
        if (device.platform == 'iOS') {
            return push.apns.unregisterNative();            
        } else { //if (device.platform.toLowerCase() == 'android') {
            return push.gcm.unregisterNative();
        }
    }

    function TemplateRegistration(pushHandle) {
        var push = zumo.getClient().push;

        if (device.platform == 'iOS') {
            return push.apns.registerTemplate(pushHandle, 'myPushTemplate', { aps: { alert: '$(News_French)' } }, null, ['World']);            
        } else {
            return push.gcm.registerTemplate(pushHandle, 'myPushTemplate', '{"data":{"message":"$(News_French)"}}', ['World']);
        }
    }

    function TemplateUnregister(templateName) {
        var push = zumo.getClient().push;

        if (device.platform == 'iOS') {
            return push.apns.unregisterTemplate(templateName);            
        } else {
            return push.gcm.unregisterTemplate(templateName);            
        }
    }

    function SendNotification(pushHandle, payload, type) {
        var item = {
                method: 'send',
                payload: payload,
                token: pushHandle,
                type: type || 'apns',
                tag: 'World'
            };

        app.pushNotificationQueue = [];
        return zumo.getClient().invokeApi('push', { body: item });
    }

    function waitForNotification(timeout, timeAfterPush, continuation) {
        /// <param name="timeout" type="Number">Time to wait for push notification in milliseconds</param>
        /// <param name="timeAfterPush" type="Number">Time to sleep after a push is received. Used to prevent
        ///            blasting the push notification service.</param>
        /// <param name="continuation" type="function(Object)">Function called when the timeout expires.
        ///            If there was a push notification, it will be passed; otherwise null will be passed
        ///            to the function.</param>
        if (typeof timeAfterPush === 'function') {
            continuation = timeAfterPush;
            timeAfterPush = 3000; // default to 3 seconds
        }

        var start = Date.now(),
            waitForPush = function () {
                var now = Date.now();
                if (app.pushNotificationQueue.length > 0) {
                    var notification = app.pushNotificationQueue.pop();
                    setTimeout(function () {
                        continuation(notification);
                    }, timeAfterPush);
                } else {
                    if ((now - start) > timeout) {
                        continuation(null); // Timed out
                    } else {
                        setTimeout(waitForPush, 500); // try it again in 500ms
                    }
                }
            };

        waitForPush();
    }

    function runTest(platform) {
        // This is a short term hack as the current implementation of tags
        // does not allow an or expression, nor does it allow for client
        // level information to be in it without some rework to the framework

        if (!device || !device.platform) {
            console.log('no device or platform defined');
            return false;
        }

        if (platform === '') {
            return true;
        }

        if (platform.toLowerCase() == device.platform.toLowerCase()) {
            // Add in simulator check if iOS?
            if (device.platform == 'iOS') { 
                // Check if on simulator
                // return false;
            }

            return true;
        }

        console.log('Not a match: ' + platform + ' == ' + device.platform);
    }

    return {
        name: 'Push',
        tests: tests
    };
}

zumo.tests.push = definePushTestsNamespace();