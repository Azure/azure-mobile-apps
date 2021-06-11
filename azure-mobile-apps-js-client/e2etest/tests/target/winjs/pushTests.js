// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="../testFramework.js" />

function definePushTestsNamespace() {
    var tests = [],
        channelUri,
        notification = {
            type: 'toast',
            notificationType: Windows.Networking.PushNotifications.PushNotificationType.toast,
            payload: '<?xml version="1.0"?><toast><visual><binding template="ToastText01"><text id="1">hello world</text></binding></visual></toast>'
        },
        receivedNotification;

    tests.push(new zumo.Test('InitialDeleteRegistrations', function (test, done) {
        testPlatform.getPushChannel()
            .then(function (channel) {
                channelUri = channel.uri;
                return zumo.getClient().invokeApi('deleteRegistrationsForChannel', { method: 'DELETE', parameters: { channelUri: channelUri } });
            })
            .done(done, fail(test, done));
    }));

    tests.push(new zumo.Test('Register', function (test, done) {
        testPlatform.getPushChannel()
            .then(function (channel) {
                channelUri = channel.uri;
                return zumo.getClient().push.register('wns', channelUri);
            })
            .then(function () {
                return zumo.getClient().invokeApi('verifyRegisterInstallationResult', { method: 'GET', parameters: { channelUri: channelUri } });
            })
            .then(function () {
                return zumo.getClient().push.unregister(channelUri);
            })
            .done(done, fail(test, done));
    }));

    tests.push(new zumo.Test('Unregister', function (test, done) {
        testPlatform.getPushChannel()
            .then(function (channel) {
                channelUri = channel.uri;
                return zumo.getClient().push.unregister(channelUri)
            })
            .then(function () {
                return zumo.getClient().invokeApi('verifyUnregisterInstallationResult', { method: 'GET' });
            })
            .done(done, fail(test, done));
    }));

    tests.push(new zumo.Test('RegisterWithTemplates', function (test, done) {
        testPlatform.getPushChannel()
            .then(function (channel) {
                channelUri = channel.uri;
                return zumo.getClient().push.register('wns', channelUri, createTemplates(['foo']))
            })
            .then(function () {
                return zumo.getClient().invokeApi('verifyRegisterInstallationResult', { method: 'GET', parameters: { channelUri: channelUri, templates: createTemplates() } });
            })
            .then(function () {
                return zumo.getClient().push.unregister(channelUri);
            })
            .done(done, fail(test, done));
    }));

    tests.push(new zumo.Test('RegisterWithTemplatesAndSecondaryTiles', function (test, done) {
        testPlatform.getPushChannel()
            .then(function (channel) {
                channelUri = channel.uri;
                return zumo.getClient().push.register('wns', channelUri, createTemplates(['bar']), createSecondaryTiles(channelUri, ['foo']))
            })
            .then(function () {
                return zumo.getClient().invokeApi('verifyRegisterInstallationResult', { method: 'GET', parameters: { channelUri: channelUri, templates: createTemplates(), secondaryTiles: createSecondaryTiles(channelUri, undefined, true) } });
            })
            .then(function () {
                return zumo.getClient().push.unregister(channelUri);
            })
            .done(done, fail(test, done));
    }));

    tests.push(new zumo.Test('RegisterMultiple', function (test, done) {
        testPlatform.getPushChannel()
            .then(function (channel) {
                channelUri = channel.uri;
                return zumo.getClient().push.register('wns', channelUri)
            })
            .then(function () {
                return zumo.getClient().push.register('wns', channelUri, createTemplates(['foo']));
            })
            .then(function () {
                return zumo.getClient().push.register('wns', channelUri);
            })
            .then(function () {
                return zumo.getClient().invokeApi('verifyRegisterInstallationResult', { method: 'GET', parameters: { channelUri: channelUri } });
            })
            .then(function () {
                return zumo.getClient().push.unregister(channelUri);
            })
            .done(done, fail(test, done));
    }));

    tests.push(new zumo.Test('ToastPush', function (test, done) {
        testPlatform.getPushChannel()
            .then(function (channel) {
                channelUri = channel.uri;
                channel.onpushnotificationreceived = function (e) {
                    receivedNotification = e;
                };
                receivedNotification = undefined;
                return zumo.getClient().push.register('wns', channelUri);
            })
            .then(function () {
                return zumo.getClient().invokeApi('push',
                {
                    body: {
                        method: 'send',
                        type: 'wns',
                        payload: notification.payload,
                        token: 'dummy',
                        wnsType: notification.type
                    }
                });
            })
            .then(function () {
                return WinJS.Promise.timeout(3000);
            })
            .then(function () {
                if (receivedNotification === undefined) {
                    throw 'No push notification received within allotted timeout';
                } else if (receivedNotification.notificationType !== notification.notificationType) {
                    throw 'Incorrect push notification type\nexpected ' + notification.notificationType + '\nactual ' + receivedNotification.notificationType;
                } else if (receivedNotification.toastNotification.content.getXml() !== notification.payload) {
                    throw 'Incorrect push notification content\nexpected ' + notification.payload + '\nactual ' + receivedNotification.toastNotification.content.getXml();
                }
                return true;
            })
            .then(function () {
                return zumo.getClient().push.unregister(channelUri);
            })
            .done(done, fail(test, done));
    }));

    return {
        name: 'Push',
        tests: tests
    };
}

function createTemplates(tags) {
    return {
        testTemplate: {
            body: '<toast><visual><binding template="ToastText01"><text id="1">$(message)</text></binding></visual></toast>',
            headers: { 'X-WNS-Type': 'wns/toast' },
            tags: tags
        }
    }
}

function createSecondaryTiles(channelUri, tags, expectedTiles) {
    // the ordering of this is significant as the comparison performed on the server is done by serialising to JSON. 
    // If it's flaky, a more robust object comparison should be implemented
    return {
        testSecondaryTiles: {
            pushChannel: channelUri,
            pushChannelExpired: expectedTiles ? false : undefined,
            templates: createTemplates(tags)
        }
    };
}

function fail(test, done) {
    return function (error) {
        test.addLog('Error occurred: ', error);
        done(false);
    }
}

zumo.tests.push = definePushTestsNamespace();