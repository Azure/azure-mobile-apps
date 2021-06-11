// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var wd = require('wd'),
    path = require('path'),
    request = require('request'),
    argv =  require('yargs').argv,
    fs = require('fs'),
    Promise = require('es6-promise').Promise;

var loginWindowWaitTime = 20 /* seconds */ * 1000 /* milli seconds */,
    loginWindowLoadTime = 1 /* minute(s) */ * 60 /* seconds */ * 1000 /* milli seconds */,
    briefWaitTime = 3 /* seconds */ * 1000 /* milli seconds */,
    asyncScriptTimeout = 5 /* minute(s) */ * 60 /* seconds */ * 1000 /* milli seconds */,
    implicitWaitTimeout = 1 /* minute(s) */ * 60 /* seconds */ * 1000 /* milli seconds */,
    resultPollInterval =  30 /* seconds */ * 1000 /* milli seconds */,
    appUrlTextBoxId = 'txtAppUrl',
    webContextName = 'WEBVIEW_com.microsoft.azuremobile.e2etest',
    loginCount = 0,
    appiumServer = {
        host: "localhost",
        port: 4723
    },
    // Actions to be performed on each login prompt
    actions = {
        facebook: {
            titleSelector: 'facebook',
            emailInputSelector: '[name=email]',
            emailSubmitSelector: undefined,
            passwordInputSelector: '[name=pass]',
            passwordSubmitSelector: '[value="Log In"]'
        },
        google: {
            titleSelector: 'google',
            emailInputSelector: '[name=Email]',
            emailSubmitSelector: '[type=submit]',
            passwordInputSelector: '[name=Passwd]',
            passwordSubmitSelector: '[id=signIn]'
        },
        twitter: {
            titleSelector: 'twitter',
            emailInputSelector: '[type=text]',
            emailSubmitSelector: undefined,
            passwordInputSelector: '[type=password]',
            passwordSubmitSelector: '[type=submit]'
        },
        microsoftaccount: {
            titleSelector: 'microsoft',
            emailInputSelector: '[type=email]',
            emailSubmitSelector: undefined,
            passwordInputSelector: '[type=password]',
            passwordSubmitSelector: '[type=submit]'
        },
        aad: {
            titleSelector: 'sign in to your account',
            emailInputSelector: '[name=login]',
            emailSubmitSelector: undefined,
            passwordInputSelector: '[name=passwd]',
            passwordSubmitSelector: '[id=cred_sign_in_button]'
        }
    },
    storageAccessToken = fs.readFileSync(argv.storageTokenPath, 'utf8').trim(),
    password = fs.readFileSync(argv.passwordPath, 'utf8').trim(),
    visitedWindows = {},
    capabilities = {
      platformName: 'Android',
      deviceName: 'some name', // name does not matter, but deviceName can't be undefined
      autoWebview: false, /* **DO NOT USE** autoWebView. It will lead to spurious crashes */
      app: argv.e2etestApkPath,
      optionalIntentArguments: ' --es serverPlatform ' + argv.serverPlatform +
                               ' --es appUrl ' + argv.appUrl +
                               ' --es containerUrl ' + argv.containerUrl +
                               ' --es storageAccessToken ' + storageAccessToken +
                               ' --es generateReport ' + argv.generateReport,
      newCommandTimeout: 20 /* minute(s) */ * 60 /* seconds */ 
    },
    noNewWindowError = 'no new window',
    unknownProviderError = 'unknown provider',
    driver;

// Stop the automation and exit with appropriate status code
function shutdown(error) {
    driver.quit();

    if (error) {
        console.log('ERROR: Test execution could not be completed. Error: ' + error);
        process.exit(1);
    } else {
        console.log('SUCCESS: Test execution completed');
        process.exit(0);
    }
}

// Poll the specified URL for the test results.
function pollResult(resultUrl) {
    setTimeout(function() {
        console.log('Checking if results are available');
        request(resultUrl, function(error, response) {
            if (!error && response.statusCode === 200) {
                console.log('Test results available. Time to exit!');
                shutdown();
            } else {
                console.log('Test results not yet available');
                pollResult(resultUrl);
            }
        });
    }, resultPollInterval);
};

// Waits for a login window, enters credentials and submits the form
function login() {
    
    var provider,
        currentWindow;
        
    driver
        .windowHandles()
        .then(function(windowHandles) {
            
            console.log('Window count: ' + windowHandles.length);
            console.log('Windows : ' + JSON.stringify(windowHandles));
            
            for (var i in windowHandles) {
                var windowHandle = windowHandles[i];

                if (!visitedWindows[windowHandle]) {

                    driver.window(windowHandle);
                    visitedWindows[windowHandle] = true;
                    currentWindow = windowHandle;
                    console.log('Switched to window ' + JSON.stringify(windowHandle));
                    return;
                }
            }
            
            throw noNewWindowError;
        })
        
        // Wait for the window to load
        .then(function() {
            console.log('Waiting for the window to load');
        })
        .delay(briefWaitTime)
        .waitForConditionInBrowser("document.querySelectorAll('title').length > 0", loginWindowLoadTime)
        
        // Detect the provider
        .title()
        .then(function(title) {
            console.log('Title: ' + title);
            
            for (var i in actions) {
                if (title.toLowerCase().indexOf(actions[i].titleSelector) != -1) {
                    provider = i;
                    return;
                }
            }
    
            throw unknownProviderError;
        })
        
        // Enter the user name
        .then(function() {
            console.log('Inputting username');
            return driver.elementByCss(actions[provider].emailInputSelector);
        })
        .delay(briefWaitTime)
        .clear()
        .sendKeys(argv.username)
        
        // If we need to hit submit to get to the password text box, hit the submit button 
        .delay(briefWaitTime)
        .then(function() {
            if (actions[provider].emailSubmitSelector) {
                return driver.elementByCss(actions[provider].emailSubmitSelector);
            }
        })
        .then(function(emailSubmitButton) {
            
            if (emailSubmitButton) {
                console.log('Sumitting username');
                emailSubmitButton.click();
            }
        })
        
        // Enter the password and hit submit
        .delay(briefWaitTime)
        .then(function() {
            console.log('Inputting password');
            return driver.elementByCss(actions[provider].passwordInputSelector);
        })
        .delay(briefWaitTime)
        .clear()
        .sendKeys(password)
        
        // Submit the credentials
        .then(function() {
            console.log('Submitting form');
        })
        .then(function() {
            return driver.elementByCss(actions[provider].passwordSubmitSelector);
        })
        .delay(briefWaitTime)
        .click()
        .then(function() {
            console.log('Submitted form');
        })
        
        .then(function() {
            ++loginCount;
            
            console.log(provider + ' login successful!');
            console.log('Providers processed so far: ' + loginCount);
            
            setTimeout(function() {
                login();
            }, loginWindowWaitTime);
        }, function(error) {
            // If there is no new window corresponding to a provider we recognize, retry.
            var retry;
            if (error === unknownProviderError) {
                console.log('Selected window is not a supported provider window.');
                retry = true;
            } else if (error === noNewWindowError) {
                console.log('No unprocessed provider window. Retrying..');
                retry = true;
            } else {
                error = provider + ' login failed with error : ' + error;
                console.log(error);
                shutdown(error);
            }
            
            if (retry) {
                setTimeout(function() {
                    login();
                }, loginWindowWaitTime);
            }
        });
}

console.log('capabilities : ' + JSON.stringify(capabilities));

// Poll the test results URL to detect test execution completion
var resultUrl = argv.containerUrl;
if (resultUrl.slice(-1) !== '/') {
    resultUrl = resultUrl + '/';
}
resultUrl += argv.serverPlatform + '-Cordova.Android' + '-master.json?' + new Buffer(storageAccessToken, 'base64').toString();
pollResult(resultUrl);

// Setup a callback to cancel the automation after the specified timeout
if (argv.timeout) {
    console.log('Timeout: ' + argv.timeout);
    setTimeout(function() {
        shutdown('Timed out! Force shutting down.')
    }, argv.timeout);
} else {
    console.log('no timeout specified');
}

// Define the automation chain
driver = wd.promiseChainRemote(appiumServer);
driver
    .chain()
    .then(function() {
        console.log('Initializing Appium');
    })

    // Initialize the driver
    .init(capabilities)
    .then(function() {
        console.log('Initialized Appium successfully. Waiting for a few seconds..');
    })
    
    // Wait for a few seconds before initiating the automation to avoid spurious failures
    .delay(briefWaitTime)
    
    // Switch to webview context    
    .contexts()
    .then(function (contexts) {
        console.log('Contexts available ' + JSON.stringify(contexts) );
        
        for (var i in contexts) {
            if (contexts[i] === webContextName) {
                console.log('Switching to context : ' + contexts[i]);
                return driver.context(contexts[i]);
            }
        }
        
        throw new Error('Missing context ' + webContextName);
    })
    
    // Set defualt timeout values
    .setAsyncScriptTimeout(asyncScriptTimeout)
    .setImplicitWaitTimeout(implicitWaitTimeout)
    
    .delay(briefWaitTime)
    .windowHandles()
    .then(function(windows) {
        console.log('Initial window count: ' + windows.length);
        console.log('Initial windows : ' + JSON.stringify(windows));
        
        if (windows.length !== 1) {
            throw 'Expected exactly 1 window. Something is wrong';
        }
        
        visitedWindows[windows[0]] = true;
    })


    // Set the Mobile App url
    .elementById(appUrlTextBoxId)
    .clear()    
    .elementById(appUrlTextBoxId)
    .sendKeys(argv.appUrl)
    
    // Run the tests
    .delay(briefWaitTime)
    .elementById('btnRunAllTests')
    .then(function(el) {
        console.log('Beginning test execution..');
    })
    .elementById('btnRunAllTests')
    .click()

    // Setup login handler    
    .then(function() {
        console.log('Setting up login handler');
        setTimeout(function() {
            login();
        }, loginWindowWaitTime);
    })

    .then(function() {
        console.log('Test execution started successfully');
    }, function(error) {
        console.log('Failed to start test execution. Error : ' + error);
        shutdown('Failed to start test execution. Error : ' + error);
    });

