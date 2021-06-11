// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/// <reference path="platformSpecificFunctions.js" />
/// <reference path="testFramework.js" />
/// <reference path="storageFramework.js" />
/// <reference path="~/js/lib/MobileServices.Web.js" />
if (!testPlatform.IsHTMLApplication) { // Call UpdateTestListHeight() if WinJS application is running
    function updateTestListHeight() {
        var tableScroll = document.getElementById('table-scroll');
        var tableHead = document.getElementById('tblTestsHead');
        var tableHeight = document.getElementById('testGroupsTableCell').getBoundingClientRect().height;
        var padding = 30;
        var headerHeight = tableHead.getBoundingClientRect().height;
        var bodyHeight = tableHeight - headerHeight - padding;
        tableScroll.style.height = bodyHeight + "px";
    }
    updateTestListHeight();
}

function setDefaultButtonEventHandler() {
    var buttons = document.getElementsByTagName('button');
    for (var i = 0; i < buttons.length; i++) {
        var btn = buttons[i];
        btn.onclick = function (evt) {
            var name = evt.target.innerText;
            testPlatform.alert('Operation ' + name + ' not implemented');
        };
    }
}

setDefaultButtonEventHandler();

function saveLastUsedAppInfo() {
    var lastAppUrl = document.getElementById('txtAppUrl').value;

    testPlatform.saveAppInfo(lastAppUrl);
}

function getTestDisplayColor(test) {
    if (test.status === zumo.TSFailed) {
        return 'Red';
    } else if (test.status == zumo.TSPassed) {
        return 'Lime';
    } else if (test.status == zumo.TSRunning) {
        return 'Gray';
    } else if (test.status == zumo.TSSkipped) {
        return 'Orange';
    } else {
        return 'White';
    }
}

function btnRunTestClick(evt) {
    if (zumo.currentGroup < 0) {
        testPlatform.alert('Please select a test group to run');
        return;
    }
    def = $.Deferred();
    var currentGroup = zumo.testGroups[zumo.currentGroup];
    var appUrl = document.getElementById('txtAppUrl').value;

    if (zumo.initializeClient(appUrl)) {

        saveLastUsedAppInfo();

        var groupDone = function (testsPassed, testsFailed) {
            var logs = 'Test group finished';
            logs = logs + '\n=-=-=-=-=-=-=-=-=-=-=-=-=-=-=\n';
            logs = logs + 'Tests passed: ' + testsPassed + '\n';
            logs = logs + 'Tests failed: ' + testsFailed;
            if (currentGroup.name.indexOf(zumo.AllTestsGroupName) === 0) {
                if (testsFailed === 0) {
                    if (currentGroup.name == zumo.AllTestsGroupName) {
                        btnRunAllTests.textContent = "Passed";
                    }
                    else {
                        btnRunAllUnattendedTests.textContent = "Passed";
                    }
                }
            }

            def.resolve();

            if (showAlerts) {
                testPlatform.alert(logs);
            }
        };

        var updateTest = function (test, index) {
            var tblTests = document.getElementById('tblTestsBody');
            var tr = tblTests.childNodes[index];
            var td = tr.firstChild;
            td.style.color = getTestDisplayColor(test);
            td.innerText = "" + (index + 1) + ". " + test.displayText();
        };
        var testFinished = updateTest;
        var testStarted = updateTest;
        currentGroup.runTests(testStarted, testFinished, groupDone);
    }
    return def.promise();
}

document.getElementById('btnRunTests').onclick = btnRunTestClick;

document.getElementById('btnResetTests').onclick = function (evt) {
    if (zumo.currentGroup < 0) {
        testPlatform.alert('Please select a test group to reset its tests');
        return;
    }

    var currentGroup = zumo.testGroups[zumo.currentGroup];
    var tests = currentGroup.tests;
    var tblTests = document.getElementById('tblTestsBody');
    tests.forEach(function (test, index) {
        test.reset();
        var tr = tblTests.childNodes[index];
        var td = tr.firstChild;
        td.style.color = getTestDisplayColor(test);
        td.innerText = "" + (index + 1) + ". " + test.displayText();
    });
};

var testGroups = zumo.testGroups;

var btnRunAllTests = document.getElementById('btnRunAllTests');
var btnRunAllUnattendedTests = document.getElementById('btnRunAllUnattendedTests');
var showAlerts = true;

if (btnRunAllTests) btnRunAllTests.onclick = handlerForAllTestsButtons(false);

if (btnRunAllUnattendedTests) btnRunAllUnattendedTests.onclick = handlerForAllTestsButtons(true);

function handlerForAllTestsButtons(unattendedOnly) {
    return function (evt) {
        showAlerts = false;
        for (var i = 0; i < testGroups.length; i++) {
            var groupName = testGroups[i].name;
            if (!unattendedOnly && groupName === zumo.AllTestsGroupName) {
                testGroupSelected(i);
                break;
            } else if (unattendedOnly && groupName == zumo.AllTestsUnattendedGroupName) {
                testGroupSelected(i);
                break;
            }
        }
    };
}

function highlightSelectedGroup(groupIndex) {
    var testsGroupBody = document.getElementById('tblTestsGroupBody');
    for (var i = 0; i < testsGroupBody.children.length; i++) {
        var tr = testsGroupBody.children[i];
        var td = tr.firstElementChild;
        td.style.fontWeight = i == groupIndex ? 'bold' : 'normal';
    }
}

function testGroupSelected(index) {
    highlightSelectedGroup(index);
    var group = testGroups[index];
    zumo.currentGroup = index;
    document.getElementById('testsTitle').innerText = 'Tests for group: ' + group.name;
    var tblTests = document.getElementById('tblTestsBody');
    for (var i = tblTests.childElementCount - 1; i >= 0; i--) {
        tblTests.removeChild(tblTests.children[i]);
    }

    function viewTestLogs(groupIndex, testIndex) {
        var test = zumo.testGroups[groupIndex].tests[testIndex];
        var logs = test.getLogs();
        testPlatform.alert(logs);
    }

    group.tests.forEach(function (test, index) {
        var tr = document.createElement('tr');
        var td = document.createElement('td');
        td.innerText = "" + (index + 1) + ". " + test.displayText();
        tr.appendChild(td);
        td.style.color = getTestDisplayColor(test);
        td.ondblclick = function () {
            viewTestLogs(zumo.currentGroup, index);
        };
        tblTests.appendChild(tr);
    });

    if (group.name === zumo.AllTestsGroupName || group.name === zumo.AllTestsUnattendedGroupName) {
        btnRunTestClick().then(function () {
            return testPlatform.getAppConfig();
        })
        .then(function (appConfig) {
            // The WebIntent plugin has trouble reading booleans. Work around it by comparing the string value to 'true' and '1'
            if (appConfig.generateReport === 'true' || appConfig.generateReport === '1') {
                storage.setConfig(appConfig);
                storage.ReportResults(zumo.testGroups, index);
            }
        });
    }
}


function addAttribute(element, name, value) {
    var attr = document.createAttribute(name);
    attr.value = value.toString();
    element.attributes.setNamedItem(attr);
}

function addTestGroups() {
    var tblTestsGroup = document.getElementById('tblTestsGroupBody');

    for (var index = 0; index < testGroups.length; index++) {
        var item = testGroups[index];
        var name = "" + (index + 1) + ". " + item.name + " tests";
        var tr = document.createElement('tr');
        var td = document.createElement('td');
        tr.appendChild(td);
        var a = document.createElement('a');
        td.appendChild(a);
        addAttribute(a, 'href', '#');
        addAttribute(a, 'class', 'testGroupItem');

        var attachEvent = function (a, index) {
            if (a.attachEvent) {

                a.attachEvent('onclick', function () {
                    testGroupSelected(index);
                });
                a.innerText = toStaticHTML(name);
            }
            else {
                a.addEventListener('click', function () {
                    testGroupSelected(index);
                }, false);
                a.textContent = name;
            }
        };

        attachEvent(a, index);

        tblTestsGroup.appendChild(tr);
    }

}
addTestGroups();