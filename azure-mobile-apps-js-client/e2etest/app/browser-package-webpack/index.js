
// All tests depend on the WindowsAzure namespace.
// Initialize the WindowsAzure namespace using the bundle.
// Just to be sure, it is not already initialized, we undefine it and do nothing
// if it is initialized. That way, we can ascertain the tests will fail.
if (typeof WindowsAzure === 'undefined') {
    WindowsAzure = require('azure-mobile-apps-client');
} else {
    WindowsAzure = undefined;
}
