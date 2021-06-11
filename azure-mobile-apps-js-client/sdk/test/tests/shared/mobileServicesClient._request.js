// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var Platform = require('../../../src/Platform'),
    MobileServiceClient = require('../../../src/MobileServiceClient');

$testGroup('MobileServiceClient._request',

    $test('get')
    .description('Verify get uses the right method.')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            callback(null, { status: 200, responseText: null });
        });

        return Platform.async(client._request).call(client, 'GET', 'foo', null);
    }),

    $test('post')
    .description('Verify post uses the right method.')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            callback(null, { status: 200, responseText: null });
        });

        return Platform.async(client._request).call(client, 'POST', 'foo', null);
    }),

    $test('patch')
    .description('Verify patch uses the right method.')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'PATCH');
            callback(null, { status: 200, responseText: null });
        });

        return Platform.async(client._request).call(client, 'PATCH', 'foo', null);
    }),

    $test('Delete')
    .description('Verify WebRequest.Delete uses the right method.')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'DELETE');
            callback(null, { status: 200, responseText: null });
        });

        return Platform.async(client._request).call(client, 'DELETE', 'foo', null);
    }),

    $test('no content')
    .description('Verify _request sets no Content-Type header for no content')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers['Content-Type'], undefined);
            $assert.areEqual(req.data, null);
            callback(null, { status: 200, responseText: null });
        });

        return Platform.async(client._request).call(client, 'POST', 'foo', null);
    }),

    $test('content')
    .description('Verify WebRequest.requestAsync sets a Content-Type header')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers['Content-Type'], 'application/json');
            $assert.areEqual(req.data, '{"a":1}');
            callback(null, { status: 200, responseText: null });
        });

        return Platform.async(client._request).call(client, 'POST', 'foo', { a: 1 });
    }),

    $test('url')
    .description('Verify WebRequest.requestAsync creates an absolute url')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.url, 'http://www.windowsazure.com/foo');
            callback(null, { status: 200, responseText: null });
        });

        return Platform.async(client._request).call(client, 'POST', 'foo', null);
    }),

    $test('installation id header')
    .description('Verify WebRequest.requestAsync provides an installation ID header')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers['X-ZUMO-INSTALLATION-ID'], MobileServiceClient._applicationInstallationId);
            callback(null, { status: 200, responseText: null });
        });

        return Platform.async(client._request).call(client, 'POST', 'foo', null);
    }),

    $test('VersionHeader')
    .description('Verify WebRequest.requestAsync provides an X-ZUMO-VERSION header')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client = client.withFilter(function (req, next, callback) {
            var isWinJs = Platform.getSdkInfo().language === "WinJS",
                isCordova = Platform.getSdkInfo().language === "Cordova";

            if (isWinJs) {
                $assert.areEqual(0, req.headers['X-ZUMO-VERSION'].indexOf("ZUMO/2.0 (lang=WinJS; os=Windows 8; os_version=--; arch=Neutral; version=2.0"));
            } else if (isCordova) {
                $assert.areEqual(0, req.headers['X-ZUMO-VERSION'].indexOf("ZUMO/2.0 (lang=Cordova; os=--; os_version=--; arch=--; version=2.0"));
            } else {
                $assert.areEqual(0, req.headers['X-ZUMO-VERSION'].indexOf("ZUMO/2.0 (lang=Web; os=--; os_version=--; arch=--; version=2.0"));
            }
            callback(null, { status: 200, responseText: null });
        });

        return Platform.async(client._request).call(client, 'POST', 'foo', null);
    }),

    $test('auth header')
    .description('Verify WebRequest.requestAsync provides an auth header when logged in')
    .checkAsync(function () {
        var auth = '123';
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client.currentUser = { mobileServiceAuthenticationToken: auth };
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers['X-ZUMO-AUTH'], auth);
            callback(null, { status: 200, responseText: null });
        });

        return Platform.async(client._request).call(client, 'POST', 'foo', null);
    }),

    $test('no auth header')
    .description('Verify WebRequest.requestAsync provides no auth header when not logged in')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers['X-ZUMO-AUTH'], undefined);
            callback(null, { status: 200, responseText: null });
        });

        return Platform.async(client._request).call(client, 'POST', 'foo', null);
    }),

    $test('no api version header')
    .description('Verify WebRequest.requestAsync has no Zumo API Version header')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/", "http://www.gateway.com/", "123456abcdefg");
        client = client.withFilter(function (req, next, callback) {
            $assert.isNull(req.headers['ZUMO-API-VERSION']);
            callback(null, { status: 200, responseText: null });
        });

        return Platform.async(client._request).call(client, 'POST', 'foo', null);
    }),

    $test('Passes back response')
    .description('Verify WebRequest.requestAsync passes back the response.')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 200, responseText: '{"message":"test"}' });
        });

        return Platform.async(client._request).call(client, 'POST', 'foo', null).then(
            function (response, complete, error) {
                $assert.areEqual(response.responseText, '{"message":"test"}');
            });
    }),

    $test('400 throws')
    .description('Verify WebRequest.requestAsync throws on a 400 response.')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.windowsazure.com/");
        client = client.withFilter(function (req, next, callback) {
            callback(null, { status: 400, responseText: '{"error":"test"}' });
        });

        return Platform.async(client._request).call(client, 'POST', 'foo', null).then(
            function (response) { $assert.fail('Should have called success continuation!'); },
            function (err) { $assert.areEqual(err.message, 'test'); });
    })
);
