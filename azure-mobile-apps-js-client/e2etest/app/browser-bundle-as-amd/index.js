// To use an officially published bundle, download it to a local folder and point to it.
define(['./generated/dist/azure-mobile-apps-client.js'], function(azureMobileApps) {
	
	// All tests depend on the WindowsAzure namespace.
	// Initialize the WindowsAzure namespace using the bundle.
	// Just to be sure, it is not already initialized, we undefine it and do nothing
	// if it is initialized. That way, we can ascertain the tests will fail.
	if (typeof WindowsAzure === 'undefined') {
		WindowsAzure = azureMobileApps;
	} else {
		WindowsAzure = undefined;
	}

});
