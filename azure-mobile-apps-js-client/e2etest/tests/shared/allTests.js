﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

(function () {
    var setupGroup = new zumo.Group('Tests setup', [
        new zumo.Test('Identify enabled runtime features',
            function (test, done) {
                var client = zumo.getClient();
                client.invokeApi('runtimeInfo', {
                    method: 'GET'
                }).done(function (response) {
                    var runtimeInfo = response.result;
                    test.addLog('Runtime features: ', runtimeInfo);
                    var features = runtimeInfo.features;
                    zumo.util.globalTestParams[zumo.constants.RUNTIME_FEATURES_KEY] = features;
                    done(true);
                }, function (err) {
                    test.addLog('Error retrieving runtime info: ', err);
                    done(false);
                });
            })
    ]);
    zumo.testGroups.push(setupGroup);

    zumo.testGroups.push(new zumo.Group(zumo.tests.roundTrip.name, zumo.tests.roundTrip.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.query.name, zumo.tests.query.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.tableGenericFunctional.name, zumo.tests.tableGenericFunctional.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.blog.name, zumo.tests.blog.tests));
    
    ////Add addistional Win JS scenario if user run WinJS application
    //if (!testPlatform.IsHTMLApplication) {
    //    zumo.testGroups.push(new zumo.Group(zumo.tests.query.name + ' (server side)', zumo.tests.query.serverSideTests));
    //}
    zumo.testGroups.push(new zumo.Group(zumo.tests.updateDelete.name, zumo.tests.updateDelete.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.login.name, zumo.tests.login.tests));
    zumo.testGroups.push(new zumo.Group(zumo.tests.misc.name, zumo.tests.misc.tests));
    // Add push tests only if they are defined.
    if (zumo.tests.push) {
        zumo.testGroups.push(new zumo.Group(zumo.tests.push.name, zumo.tests.push.tests));
    }
    zumo.testGroups.push(new zumo.Group(zumo.tests.api.name, zumo.tests.api.tests));

    var allTests = [];
    var allUnattendedTests = [];
    for (var i = 0; i < zumo.testGroups.length; i++) {
        var group = zumo.testGroups[i];
        for (var j = 0; j < group.tests.length; j++) {
            var test = group.tests[j];
            allTests.push(test);
            if (test.canRunUnattended) {
                allUnattendedTests.push(test);
            }
        }
    }

    zumo.testGroups.push(new zumo.Group(zumo.AllTestsUnattendedGroupName, allUnattendedTests));
    zumo.testGroups.push(new zumo.Group(zumo.AllTestsGroupName, allTests));
})();
