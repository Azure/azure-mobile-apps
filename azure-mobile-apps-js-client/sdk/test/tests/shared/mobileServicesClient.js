// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

// revert isvalidate, isarray, etc from validate.js, extensions.js and corresponding 2 test files and *.resjson

var Platform = require('../../../src/Platform'),
    MobileServiceClient = require('../../../src/MobileServiceClient');

$testGroup('MobileServiceClient.js',

    $test('constructor')
    .description('Verify the constructor correctly initializes the client.')
    .check(function () {
        $assertThrows(function () { new MobileServiceClient(); });
        $assertThrows(function () { new MobileServiceClient(null); });
        $assertThrows(function () { new MobileServiceClient(''); });
        $assertThrows(function () { new MobileServiceClient(2); });

        var uri = "http://www.test.com",
            client = new MobileServiceClient(uri);

        $assert.areEqual(uri, client.applicationUrl);
        $assert.isTrue(client.getTable);
    }),

    $test('withFilter chaining')
    .description('Verify withFilter correctly chains filters')
    .checkAsync(function () {
        var descend = '';
        var rise = '';
        var createFilter = function (letter) {
            return function (req, next, callback) {
                descend += letter;
                next(req, function (err, resp) {
                    rise += letter;
                    callback(err, resp);
                });
            };
        };

        var client = new MobileServiceClient('http://something');

        client = client
                    .withFilter(createFilter('should not be called'))
                    .withFilter((function (req, next, callback) {
                        callback(null, { status: 200, responseText: '' });
                    }))
                    .withFilter(createFilter('A'))
                    .withFilter(createFilter('B'))
                    .withFilter(createFilter('C'));

        return Platform.async(client._request).call(client, 'GET', 'http://anything', null).then(function (rsp) {
            $assert.areEqual(descend, 'CBA');
            $assert.areEqual(rise, 'ABC');
        });
    }),

    $test('withFilter')
    .description('Verify withFilter intercepts calls')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            callback(null, { status: 200, responseText: '{"authenticationToken":"zumo","user":{"userId":"bob"}}' });
        });

        client._login.ignoreFilters = false;

        return client.login('token.a.b').then(function (result) {
            $assert.areEqual(result.userId, 'bob');
        });
    }),

    $test('login_Verify_login_mechanics')
    .tag('login')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.contains(req.url, ".auth/login");
            $assert.areEqual(req.data, '{"authenticationToken":"token.a.b"}');
            callback(null, { status: 200, responseText: '{"authenticationToken":"zumo","user":{"userId":"bob"}}' });
        });

        client._login.ignoreFilters = false;

        return client.login('token.a.b').then(function (currentUser) {
            $assert.areEqual(client.currentUser.userId, 'bob');
            $assert.areEqual(client.currentUser.mobileServiceAuthenticationToken, 'zumo');
        });
    }),

    $test('loginWithOptions_token')
    .tag('login')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.contains(req.url, ".auth/login");
            $assert.areEqual(req.data, '{"authenticationToken":"token.a.b"}');
            callback(null, { status: 200, responseText: '{"authenticationToken":"zumo","user":{"userId":"bob"}}' });
        });

        client._login.ignoreFilters = false;

        return client.loginWithOptions('token.a.b').then(function (currentUser) {
            $assert.areEqual(client.currentUser.userId, 'bob');
            $assert.areEqual(client.currentUser.mobileServiceAuthenticationToken, 'zumo');
        });
    }),

    $test('testLoginWithToken_withoutAlternateLoginHost_withoutLoginUriPrefix')
    .tag('login')
    .checkAsync(function () {
        return testLoginWithToken(['facebook', { access_token: 'zumo' }],
                                  'https://www.test.com/test/',
                                  null,
                                  null,
                                  'https://www.test.com/test/.auth/login/facebook');
    }),

    $test('testLoginWithToken_withoutAlternateLoginHost_withLoginUriPrefix')
    .tag('login')
    .checkAsync(function () {
        return testLoginWithToken(['aad', { access_token: 'zumo' }],
                                  'https://www.test.com/test',
                                  undefined,
                                  'custom/prefix',
                                  'https://www.test.com/test/custom/prefix/aad');
    }),

    $test('testLoginWithToken_withAlternateLoginHost_withoutLoginUriPrefix')
    .tag('login')
    .checkAsync(function () {
        return testLoginWithToken(['google', { access_token: 'zumo' }],
                                  'https://www.test.com/test',
                                  'https://www.alternateloginhost.com/test/',
                                  undefined,
                                  'https://www.alternateloginhost.com/test/.auth/login/google');
    }),

    $test('testLoginWithToken_withAlternateLoginHost_withLoginUriPrefix')
    .tag('login')
    .checkAsync(function () {
        return testLoginWithToken(['microsoftaccount', { access_token: 'zumo' }],
                                  'https://www.test.com/test/',
                                  'https://www.alternateloginhost.com/test',
                                  'custom/prefix',
                                  'https://www.alternateloginhost.com/test/custom/prefix/microsoftaccount');
    }),

    $test('loginWithOptions_provider_parameters')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { parameters: { display: 'popup' } }],
            "http://www.test.com/.auth/login/facebook?display=popup&session_mode=token",
            "http://www.test.com/.auth/login/done");
    }),

    $test('loginWithOptions_provider_singlesignon_parameters')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { useSingleSignOn: true, parameters: { display: 'popup' } }],
                                  "http://www.test.com/.auth/login/facebook?display=popup&session_mode=token",
                                  null);
    }),

    $test('loginWithOptions_provider_singlesignon')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { useSingleSignOn: true }],
                                  "http://www.test.com/.auth/login/facebook?session_mode=token",
                                  null);
    }),

    $test('loginWithOptions_provider_sessionmode_token_parameter')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { parameters: { display: 'popup', session_mode: 'token' } }],
            "http://www.test.com/.auth/login/facebook?display=popup&session_mode=token",
            "http://www.test.com/.auth/login/done");
    }),

    $test('loginWithOptions_provider_sessionmode_other_than_token_parameter')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { parameters: { display: 'popup', session_mode: 'some_session_mode' } }],
            "http://www.test.com/.auth/login/facebook?display=popup&session_mode=token",
            "http://www.test.com/.auth/login/done");
    }),

    $test('loginWithOptions_provider')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook'],
                                  "http://www.test.com/.auth/login/facebook?session_mode=token",
                                  "http://www.test.com/.auth/login/done");
    }),

      $test('loginWithOptions_provider_parameters_alternateLoginHost')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { parameters: { display: 'popup' } }],
                                  "https://www.testalternateloginhost.com/.auth/login/facebook?display=popup&session_mode=token",
                                  "https://www.testalternateloginhost.com/.auth/login/done", "https://www.testalternateloginhost.com/");
    }),

    $test('loginWithOptions_provider_singlesignon_parameters_alternateLoginHost')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { useSingleSignOn: true, parameters: { display: 'popup' } }],
                                  "https://www.testalternateloginhost.com/.auth/login/facebook?display=popup&session_mode=token",
                                  null, "https://www.testalternateloginhost.com/");
    }),

    $test('loginWithOptions_provider_singlesignon_alternateLoginHost')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { useSingleSignOn: true }],
                                  "https://www.testalternateloginhost.com/.auth/login/facebook?session_mode=token",
                                  null, "https://www.testalternateloginhost.com/");
    }),

    $test('loginWithOptions_provider_alternateLoginHost')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook'],
                                  "https://www.testalternateloginhost.com/.auth/login/facebook?session_mode=token",
                                  "https://www.testalternateloginhost.com/.auth/login/done", "https://www.testalternateloginhost.com/");
    }),


     $test('loginWithOptions_provider_parameters_loginPrefix')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { parameters: { display: 'popup' } }],
                                  "http://www.test.com/login/facebook?display=popup&session_mode=token",
                                  "http://www.test.com/login/done", null, "login");
    }),

    $test('loginWithOptions_provider_singlesignon_parameters_loginPrefix')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { useSingleSignOn: true, parameters: { display: 'popup' } }],
                                  "http://www.test.com/login/facebook?display=popup&session_mode=token",
                                  null, null, "login");
    }),

    $test('loginWithOptions_provider_singlesignon_loginPrefix')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { useSingleSignOn: true }],
                                  "http://www.test.com/login/facebook?session_mode=token",
                                  null, null, "login");
    }),

    $test('loginWithOptions_provider_loginPrefix')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook'],
                                  "http://www.test.com/login/facebook?session_mode=token",
                                  "http://www.test.com/login/done", null, "login");
    }),

    $test('loginWithOptions_provider_parameters_alternateLoginHost_loginPrefix')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { parameters: { display: 'popup' } }],
                                  "https://www.testalternateloginhost.com/login/facebook?display=popup&session_mode=token",
                                  "https://www.testalternateloginhost.com/login/done", "https://www.testalternateloginhost.com/", "login");
    }),

    $test('loginWithOptions_provider_singlesignon_parameters_alternateLoginHost_loginPrefix')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { useSingleSignOn: true, parameters: { display: 'popup' } }],
                                  "https://www.testalternateloginhost.com/login/facebook?display=popup&session_mode=token",
                                  null, "https://www.testalternateloginhost.com/", "login");
    }),

    $test('loginWithOptions_provider_singlesignon_alternateLoginHost_loginPrefix')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook', { useSingleSignOn: true }],
                                  "https://www.testalternateloginhost.com/login/facebook?session_mode=token",
                                  null, "https://www.testalternateloginhost.com/", "login");
    }),

    $test('loginWithOptions_provider_alternateLoginHost_loginPrefix')
    .tag('login')
    .checkAsync(function () {
        return testLoginParameters(['facebook'],
                                  "https://www.testalternateloginhost.com/login/facebook?session_mode=token",
                                  "https://www.testalternateloginhost.com/login/done", "https://www.testalternateloginhost.com/", "login");
    }),

     $test('invalid_alternateLoginHost')
            .tag('login')
            .check(function () {
                $assertThrows(function () {
                    var client = new MobileServiceClient("http://www.test.com");
                    client.alternateLoginHost = "invalidUrl";
                });
                $assertThrows(function () {
                    var client = new MobileServiceClient("http://www.test.com");
                    client.alternateLoginHost = "http://www.alternateloginHostHttp.com";
                });
            }),

     $test('default_loginendpoint_if_alternateloginhost_isnull')
            .tag('login')
            .checkAsync(function () {
                return testLoginParameters(['facebook'],
                                          "http://www.test.com/.auth/login/facebook?session_mode=token",
                                          "http://www.test.com/.auth/login/done", null, null);
            }),
    

    $test('logout')
    .description('Verify Authentication.logout clears currentUser')
    .checkAsync(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client.currentUser = { userId: 'bob', mobileServiceAuthenticationToken: 'abcd' };

        return client.logout().then(function () {
            $assert.areEqual(client.currentUser, null);
        });
    }),

    $test('static initialization of appInstallId')
    .description('Verify the app installation id is created statically.')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com"),
            settingsKey = "MobileServices.Installation.config",
            settings = typeof Windows === "object" ? Windows.Storage.ApplicationData.current.localSettings.values[settingsKey]
                                                   : Platform.readSetting(settingsKey);
        $assert.isTrue(settings);
    }),

    $test('CustomAPI - error response as json object')
    .description('Verify the custom API error messages')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/checkins/post');
            callback(null, { status: 400, responseText: '{"error":"bad robot"}', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("checkins/post").done(function (response) {
            $assert.fail("api call failed");
        },
        function (error) {
            $assert.areEqual(error.message, "bad robot");
        });
    }),

    $test('CustomAPI - error response as json object without content-type')
    .description('Verify the custom API error messages')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/checkins/post');
            callback(null, { status: 400, responseText: '{"error":"bad robot"}', getResponseHeader: function () { return null; } });
        });
        client.invokeApi("checkins/post").done(function (response) {
            $assert.fail("api call failed");
        },
        function (error) {
            $assert.areEqual(error.message, "bad robot");
        });
    }),

    $test('CustomAPI - error response as json string')
    .description('Verify the custom API error messages')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/checkins/post');
            callback(null, { status: 400, responseText: '"bad robot"', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("checkins/post").done(function (response) {
            $assert.fail("api call failed");
        },
        function (error) {
            $assert.areEqual(error.message, "bad robot");
        });
    }),

    $test('CustomAPI - error as text')
    .description('Verify the custom API error messages')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/checkins/post');
            callback(null, { status: 400, responseText: 'bad robot', getResponseHeader: function () { return 'text/html'; } });
        });
        client.invokeApi("checkins/post").done(function (response) {
            $assert.fail("api call failed");
        },
        function (error) {
            $assert.areEqual(error.message, "bad robot");
        });
    }),

    $test('CustomAPI - error as text without content type')
    .description('Verify the custom API error messages')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/checkins/post');
            callback(null, { status: 400, responseText: 'bad robot' });
        });
        client.invokeApi("checkins/post").done(function (response) {
            $assert.fail("api call failed");
        },
        function (error) {
            $assert.areEqual(error.message, "Unexpected failure.");
        });
    }),

    $test('CustomAPI - just api name')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/checkins/post');
            $assert.areEqual(req.headers['ZUMO-API-VERSION'], "2.0.0");
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("checkins/post").done(function (response) {
            $assert.areEqual(response.result.result, 3);
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - absolute http:// URI ')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("https://abc");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/checkins/post');
            $assert.areEqual(req.headers['ZUMO-API-VERSION'], "2.0.0");
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("http://www.test.com/api/checkins/post").done(function (response) {
            $assert.areEqual(response.result.result, 3);
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - absolute https:// URI ')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("http://abc");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'https://www.test.com/api/checkins/post');
            $assert.areEqual(req.headers['ZUMO-API-VERSION'], "2.0.0");
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("https://www.test.com/api/checkins/post").done(function (response) {
            $assert.areEqual(response.result.result, 3);
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - name and content')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/scenarios/verifyRequestAccess');
            $assert.areEqual(req.data, '{\"data\":\"one\"}');
            callback(null, { status: 200, getResponseHeader: function () { return 'application/xml'; }, responseText: '' });
        });

        client.invokeApi("scenarios/verifyRequestAccess", { body: { 'data': 'one' } }).done(function (response) {
        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - string content')
    .description('Verify sending string content')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.data, 'apples');
            callback(null, { status: 200, responseText: '{"result":3}', getResponseHeader: function () { return 'application/json'; } });
        });

        client.invokeApi("scenarios/verifyRequestAccess", { body: "apples" }).done(function (response) {
        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - boolean content')
    .description('Verify sending boolean content')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.data, "true");
            callback(null, { status: 200, responseText: '{"result":3}', getResponseHeader: function () { return 'application/json'; } });
        });

        client.invokeApi("scenarios/verifyRequestAccess", { body: true }).done(function (response) {
        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - date object')
    .description('Verify sending date object')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.data, "\"2013-04-14T06:01:59.000Z\"");
            callback(null, { status: 200, responseText: '{"result":3}', getResponseHeader: function () { return 'application/json'; } });
        });

        var date = new Date(Date.UTC(2013, 3, 14, 6, 1, 59));
        client.invokeApi("scenarios/verifyRequestAccess", { body: date }).done(function (response) {
        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - array content')
    .description('Verify sending array content')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.data, '[\"a\",\"b\",\"c\"]');
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });

        client.invokeApi("scenarios/verifyRequestAccess", { body: ['a', 'b', 'c'] }).done(function (response) {
        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - name with querystring and method')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/api/calculator/add?a=1&b=2');
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("calculator/add?a=1&b=2", { method: "GET" }).done(function (response) {
            $assert.areEqual(response.result.result, 3);
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - name and method with param')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/api/calculator/add?a=1&b=2');
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("calculator/add", { method: "GET", parameters: { 'a': 1, 'b': 2 } }).done(function (response) {
            $assert.areEqual(response.result.result, 3);
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - http:// URI with query string, method and param')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("https://abc");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/api/calculator/add?q=1&a=1&b=2');
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("http://www.test.com/api/calculator/add?q=1", { method: "GET", parameters: { 'a': 1, 'b': 2 } }).done(function (response) {
            $assert.areEqual(response.result.result, 3);
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - http:// URI without query string, method and param')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("https://abc");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/api/calculator/add?a=1&b=2');
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("http://www.test.com/api/calculator/add", { method: "GET", parameters: { 'a': 1, 'b': 2 } }).done(function (response) {
            $assert.areEqual(response.result.result, 3);
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - https:// URI with query string, method and param')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("http://abc");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'https://www.test.com/api/calculator/add?q=1&a=1&b=2');
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("https://www.test.com/api/calculator/add?q=1", { method: "GET", parameters: { 'a': 1, 'b': 2 } }).done(function (response) {
            $assert.areEqual(response.result.result, 3);
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - https:// URI without query string, method and param')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("http://abc");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'https://www.test.com/api/calculator/add?a=1&b=2');
            callback(null, { status: 200, responseText: '{"result":3 }', getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("https://www.test.com/api/calculator/add", { method: "GET", parameters: { 'a': 1, 'b': 2 } }).done(function (response) {
            $assert.areEqual(response.result.result, 3);
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - Return XML')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'GET');
            $assert.areEqual(req.url, 'http://www.test.com/api/scenarios/getXmlResponse');
            callback(null, { status: 200, responseText: '<foo>bar</foo>', getResponseHeader: function () { return 'application/xml'; } });
        });
        client.invokeApi("scenarios/getXmlResponse", { method: "GET" }).done(function (response) {
            $assert.areEqual(response.responseText, "<foo>bar</foo>");
        },
        function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - name, body, method, and headers')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/scenarios/verifyRequestAccess');
            $assert.areEqual(req.headers['x-zumo-testing'], 'test');
            $assert.areEqual(req.data, '{\"data\":\"one\"}');
            $assert.areEqual(req.headers['ZUMO-API-VERSION'], "2.0.0");
            callback(null, { status: 200, getResponseHeader: function () { return 'application/xml'; }, responseText: '' });
        });
        var headers = { 'x-zumo-testing': 'test' };
        client.invokeApi("scenarios/verifyRequestAccess", { body: { 'data': 'one' }, method: "POST", headers: headers }).done(function (response) {
            $assert.isNull(headers['X-ZUMO-VERSION']);
        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - Custom headers and return XML not JSON')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/scenarios/verifyRequestAccess');
            $assert.areEqual(req.headers['Content-Type'], 'application/xml');
            $assert.areEqual(req.data, '<foo>bar</foo>'); //no json encoded...
            callback(null, { status: 200, getResponseHeader: function () { return 'application/xml'; } });
        });
        client.invokeApi("scenarios/verifyRequestAccess", { body: '<foo>bar</foo>', method: "POST", headers: { 'Content-Type': 'application/xml' } }).done(function (response) {

        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - Send content-type instead of Content-Type')
    .description('Verify the custom API url formatting')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.type, 'POST');
            $assert.areEqual(req.url, 'http://www.test.com/api/scenarios/verifyRequestAccess');
            $assert.areEqual(req.headers['content-type'], 'application/xml');
            $assert.areEqual(req.data, '<foo>bar</foo>'); //no json encoded...
            callback(null, { status: 200, getResponseHeader: function () { return 'application/xml'; } });
        });
        client.invokeApi("scenarios/verifyRequestAccess", { body: '<foo>bar</foo>', method: "POST", headers: { 'content-type': 'application/xml' } }).done(function (response) {

        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - specifies accept: application/json header by default')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers.accept, 'application/json');
            callback(null, { status: 200 });
        });
        client.invokeApi("someApi").done(function (response) { }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - specifies accept: application/json header by default when options are passed')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers.accept, 'application/json');
            callback(null, { status: 200 });
        });
        client.invokeApi("someApi", {}).done(function (response) { }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - specifies accept: application/json header by default when headers are passed')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers.accept, 'application/json');
            callback(null, { status: 200 });
        });
        client.invokeApi("someApi", { headers: { someHeader: 'test' } }).done(function (response) { }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('CustomAPI - Does not override existing accept headers')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers.accept, 'application/xml');
            callback(null, { status: 200 });
        });
        client.invokeApi("someApi", { headers: { 'accept': 'application/xml' } }).done(function (response) { }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('Features - CustomAPI - Call with object (JSON-ified)')
    .description('Verify the features headers for custom calls')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers["X-ZUMO-FEATURES"], "AJ");
            callback(null, { status: 200, getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("scenarios/verifyRequestAccess", {
            body: { hello: "world" },
            method: "POST"
        }).done(function (response) {

        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('Features - CustomAPI - Call with object (JSON-ified) and parameters')
    .description('Verify the features headers for custom calls')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers["X-ZUMO-FEATURES"], "AJ,QS");
            callback(null, { status: 200, getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("scenarios/verifyRequestAccess", {
            body: { hello: "world" },
            method: "POST",
            parameters: { 'a': 1, 'b': 2 }
        }).done(function (response) {

        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('Features - CustomAPI - Call with non-object (not JSON-ified)')
    .description('Verify the features headers for custom calls')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers["X-ZUMO-FEATURES"], "AG");
            callback(null, { status: 200, getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("scenarios/verifyRequestAccess", {
            body: "<hello>world</hello>",
            method: "POST",
            headers: { 'Content-Type': 'application/xml' }
        }).done(function (response) {

        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('Features - CustomAPI - Call with object (JSON-ified)')
    .description('Verify the features headers for custom calls')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers["X-ZUMO-FEATURES"], "AG,QS");
            callback(null, { status: 200, getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("scenarios/verifyRequestAccess", {
            body: "<hello>world</hello>",
            method: "POST",
            headers: { 'Content-Type': 'application/xml' },
            parameters: { 'a': 1, 'b': 2 }
        }).done(function (response) {

        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('Features - CustomAPI - Call with no body')
    .description('Verify the features headers for custom calls')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers["X-ZUMO-FEATURES"], "QS");
            callback(null, { status: 200, getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("scenarios/verifyRequestAccess", {
            method: "GET",
            parameters: { a: 1, b: 2 }
        }).done(function (response) {

        }, function (error) {
            $assert.fail("api call failed");
        });
    }),

    $test('Features - CustomAPI - Headers parameters is not modified')
    .description('Verify the features headers for custom calls')
    .check(function () {
        var client = new MobileServiceClient("http://www.test.com");
        var reqHeaders = { 'Content-Type': 'application/xml' };
        client = client.withFilter(function (req, next, callback) {
            $assert.areEqual(req.headers["X-ZUMO-FEATURES"], "AG");
            callback(null, { status: 200, getResponseHeader: function () { return 'application/json'; } });
        });
        client.invokeApi("scenarios/verifyRequestAccess", {
            method: "POST",
            body: "<hello>world</hello>"
        }).done(function (response) {
            $assert.isNull(reqHeaders["X-ZUMO-FEATURES"]);
        }, function (error) {
            $assert.fail("api call failed");
        });
    })
);

function testLoginParameters(args, expectedStartUri, expectedEndUri, alternateLoginHost, loginUriPrefix) {
    var client = new MobileServiceClient("http://www.test.com");
    client.alternateLoginHost = alternateLoginHost;
    client.loginUriPrefix = loginUriPrefix;

    var _login = Platform.login;

    Platform.login = function (startUri, endUri, callback) {
        Platform.login = _login;

        $assert.areEqual(startUri, expectedStartUri);
        $assert.areEqual(endUri, expectedEndUri);
        callback(null, {
            authenticationToken: "zumo",
            user: {
                "userId": "bob"
            }
        });
    };

    var originalArgs = JSON.stringify(args);
    return client.loginWithOptions.apply(client, args).then(function(currentUser) {
        $assert.areEqual(client.currentUser.userId, 'bob');
        $assert.areEqual(client.currentUser.mobileServiceAuthenticationToken, 'zumo');

        // Make sure args haven't changed.
        $assert.areEqual(JSON.stringify(args), originalArgs);
    });
}

function testLoginWithToken(args, serviceUrl, alternateLoginHost, loginUriPrefix, expectedLoginUri) {

    var client = new MobileServiceClient(serviceUrl);

    client = client.withFilter(function (req, next, callback) {
        $assert.areEqual(req.url, expectedLoginUri);
        callback(null, { status: 200, responseText: '{"authenticationToken":"zumo","user":{"userId":"bob"}}' });
    });

    client.alternateLoginHost = alternateLoginHost;
    client.loginUriPrefix = loginUriPrefix;

    client._login.ignoreFilters = false;

    return client.login.apply(client, args);
}

