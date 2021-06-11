// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

function definePushTestsNamespace() {
    var gcmSenderId = 'REPLACE_THIS_WITH_SENDER_ID_FROM_GOOGLE_CONSOLE',
        tests = [],
        pushRegistrationId,
        pushNotification,
        pushNotificationQueue = [],
        pushTimeout = 30 * 1000, // time (milli seconds) to wait for a push event or push registration completion
        sleepTimeAfterReceivingPush = 3 * 1000; // time (milli seconds) to wait after a push event is received to avoid blasting the push service

    tests.push(new zumo.Test('Push register', function (test, done) {
        initializePush(test, function(error) {
            done(!error);
        });
    }));

    tests.push(new zumo.Test('Basic push', function (test, done) {
        initializePush(test, function(error) {
            if (error) {
                return done(false);
            }

            sendPushNotification(function(error, notification) {
                done(!error);
            });
        });
    }));

    tests.push(new zumo.Test('Push unregister', function (test, done) {
        initializePush(test, function(error) {
            if (error) {
                return done(false);
            }

            // Set pushRegistrationId to undefined before calling unregister
            var registrationId = pushRegistrationId;
            pushRegistrationId = undefined;
            zumo.getClient().push.unregister(registrationId)
                .then(function() {
                    done(true);
                }, function(error) {
                    test.log('Failed to unregister. Eror: ' + error.message)
                    done(false);
                });
        });
    }));

    function sendPushNotification(completion) {
        var notificationServiceName = getNotificationServiceName();
        zumo.getClient().push.register(notificationServiceName, pushRegistrationId)
            .then(function () {
                var body = {
                    method: 'send',
                    token: 'dummy'
                };
                if (device.platform === 'Android') {
                    body.type = notificationServiceName;
                    body.payload = '{"data":{"message":"gcm test"}}';
                } else if (device.platform === 'windows') {
                    body.type = notificationServiceName;
                    body.wnsType = 'toast';
                    body.payload = '<?xml version="1.0"?><toast><visual><binding template="ToastText01"><text id="1">wns test</text></binding></visual></toast>';
                } else if (device.platform === 'iOS') {
                    body.type = notificationServiceName;
                    body.payload = '{"aps":{"alert":"apns test"}}';
                } else {
                    throw new Error('Unsupported platform: ' + device.platform)
                }

                return zumo.getClient().invokeApi('push', { body: body });
            })
            .then(function() {
                waitForNotification(completion);
            }, function(error) {
                test.addLog('Failed to send push notification: ' + error.message);
                completion(error);
            });
    }

    function getNotificationServiceName() {
        if (device.platform === 'Android') {
            return 'gcm';
        } if (device.platform === 'windows') {
            return 'wns';
        } else if (device.platform === 'iOS') {
            return 'apns';
        } else {
            window.alert('TODO');
        }
    }

    function initializePush(test, completion) {
        // If pushRegistrationId is already defined, no need to initialize again
        // For now ignore other cases of initializePush being called once before.. 
        if (pushRegistrationId) {
            completion();
            return;
        }

        pushNotification = PushNotification.init({
            android: {
                senderID: gcmSenderId
            },
            ios: {
                alert: "true",
                badge: "true",
                sound: "true"
            },
            windows: {}
        });

        setTimeout(function() {
            if (!pushRegistrationId) {
                var errorMsg = 'Push registration failed to complete within allotted time';
                test.addLog(errorMsg);
                completion(errorMsg);
            }
        }, pushTimeout)

        pushNotification.on('registration', function (data) {
            test.addLog('Push registration successful. Registration ID: ', data.pushRegistrationId);
            pushRegistrationId = data.registrationId
            completion();
        });

        pushNotification.on('notification', function (data) {
            test.addLog('Received notification: ', JSON.stringify(data));
            pushNotificationQueue.push({
                data: data
            });
        });

        pushNotification.on('error', function (error) {
            test.addLog('Received error: ', JSON.stringify(error));
            pushNotificationQueue.push({
                error: error
            });
        });
    }

    function waitForNotification(completion) {
        var start = Date.now();
        var waitForPush = function () {
            var now = Date.now();
            if (pushNotificationQueue.length) {
                var notification = pushNotificationQueue.pop();
                setTimeout(function () {
                    completion(undefined, notification);
                }, sleepTimeAfterReceivingPush);
            } else {
                if ((now - start) > pushTimeout) {
                    completion(new Error('Timeout. Notification not received within the allotted time')); // Timed out
                } else {
                    setTimeout(waitForPush, 500); // try it again in 500ms
                }
            }
        }

        waitForPush();
    }

    return {
        name: 'Push',
        tests: tests
    }
}

zumo.tests.push = definePushTestsNamespace();
