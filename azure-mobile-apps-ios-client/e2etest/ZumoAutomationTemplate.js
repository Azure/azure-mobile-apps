var MICROSOFT = 'microsoft';
var FACEBOOK = 'facebook';
var TWITTER = 'twitter';
var GOOGLE = 'google';
var AAD = 'aad';

var userNames = {};
userNames[MICROSOFT] = 'zumotestuser@hotmail.com';
userNames[FACEBOOK] = 'zumotestuser@hotmail.com';
userNames[TWITTER] = 'zumotestuser';
userNames[GOOGLE] = 'zumotestuser@hotmail.com';
userNames[AAD] = 'zumoaaduser@azuremobile.onmicrosoft.com';

var password = '--AUTH_PASSWORD--';

var mobileServiceName = '--APPLICATION_URL--';
var blobUrl = '--BLOB_URL--';
var blobToken = '--BLOB_TOKEN--';

var target = UIATarget.localTarget();
var app = target.frontMostApp();
var window = app.mainWindow();

var done = false;

UIATarget.onAlert = function(alert) {
	var title = alert.name();
	UIALogger.logDebug("Alert with title: '" + title + "'");

	if (title == 'Tests Complete') {
		UIALogger.logPass('All tests');
		done = true;
	} 
	return false;
}

setMobileService(app, window, mobileServiceName, blobUrl, blobToken);

startTests();

while (!done) {
	var provider = isLoginPage();
	if (provider) {
		UIALogger.logMessage('Performing log in for \'' + provider + '\'');
		var userName = userNames[provider];
		try {
			doLogin(target, app, userName, password, provider);
		}catch(error) {
			UIALogger.logMessage('Login failed: ' + error);
			UIATarget.localTarget().logElementTree();
			throw error;
		}
	}
	UIALogger.logMessage('Waiting for login or done');
	target.delay(3);
}

backToStart();

function setMobileService(app, window, appUrl, blobUrl, blobToken) {
	var values = {
		MobileServiceURL: appUrl,
		BlobURL: blobUrl,
		BlobToken: blobToken,
	};

	for (var key in values) {
		if (values.hasOwnProperty(key)) {
			var textField = window.textFields()[key];
			if (textField.isValid()) {
				textField.setValue(values[key]);
			}
			app.keyboard().typeString("\n");
		}
	}

	// Start testing the application
	window.buttons()["BeginTesting"].tap();
}

function startTests() {
	UIATarget.localTarget().pushTimeout(600);

	var testGroups = window.tableViews()[0].cells();
	var lastTestGroup = testGroups.length - 1;
	testGroups[lastTestGroup].tap();

	UIATarget.localTarget().popTimeout();
	UIALogger.logStart('All tests');
}

function backToStart() {
	app.navigationBar().leftButton().tap();
}

function getWebView() {
	return window.scrollViews()[0].webViews()[0];
}

function isLoginPage() {
	var webView = getWebView();
	if (!webView.isValid()) {
		return null;
	}

	var alltags = webView.staticTexts();

	if (alltags.withPredicate('name contains[cd] "Facebook"').length > 0) {
		return FACEBOOK;
	}

	if (alltags.withPredicate('name contains[cd] "Work"').length > 0) {
		return AAD;
	}

	if (alltags.withPredicate('name contains[cd] "Microsoft"').length > 0) {
		return MICROSOFT;
	}

	if (alltags.withPredicate('name contains[cd] "Twitter"').length > 0) {
		return TWITTER;
	}

	if (alltags.withPredicate('name contains[cd] "Google"').length > 0) {
		return GOOGLE;
	}

	return null;
}

function doLogin(target, app, userName, password, provider) {
	var webView = getWebView();
	var userTextField = webView.textFields()[0];
	target.delay(1);
    if(userTextField){
        userTextField.tap();
        target.delay(1);
        if (!userTextField.hasKeyboardFocus()) {
            userTextField.tap();
            target.delay(1);
        }
        app.keyboard().typeString(userName);
        target.delay(3);
    }
	
    if(provider===GOOGLE){
        var btnNext = webView.buttons()[0];
		target.delay(1);
        if(btnNext){
            btnNext.tap();
            target.delay(1);
        }
    }
    
	if(provider===AAD){
        var rememberMeCheckbox = webView.switches()[0];
        target.delay(1);
        if(rememberMeCheckbox){
            rememberMeCheckbox.setValue(true);
            target.delay(1);
        }
    }
	var passwordTextField = webView.secureTextFields()[0];
	target.delay(1);
    if(passwordTextField){
        passwordTextField.tap();
        target.delay(1);
        if (!passwordTextField.hasKeyboardFocus()) {
            passwordTextField.tap();
            target.delay(1);
        }
        app.keyboard().typeString(password);
        target.delay(3);
    }

    var btnLogin = webView.buttons()[0];
	target.delay(1);
    if(btnLogin){
        btnLogin.tap();
        target.delay(1);
    }
}
