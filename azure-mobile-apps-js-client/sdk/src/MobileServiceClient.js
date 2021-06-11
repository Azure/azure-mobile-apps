// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var _ = require('./Utilities/Extensions'),
    constants = require('./constants'),
    Validate = require('./Utilities/Validate'),
    Platform = require('./Platform'),
    MobileServiceSyncContext = require('./sync/MobileServiceSyncContext'),
    MobileServiceSyncTable = require('./sync/MobileServiceSyncTable').MobileServiceSyncTable,
    MobileServiceTable = require('./MobileServiceTable'),
    MobileServiceLogin = require('./MobileServiceLogin');

var Push;
try {
    Push = require('./Push/Push').Push;
} catch (e) { }

var _alternateLoginHost = null;
Object.defineProperties(MobileServiceClient.prototype, {
    alternateLoginHost: {
        get: function () {
            return this._alternateLoginHost;
        },
        set: function (value) {
            if (_.isNullOrEmpty(value)) {
                this._alternateLoginHost = this.applicationUrl;
            }else if (_.url.isAbsoluteUrl(value) && _.url.isHttps(value)) {
                this._alternateLoginHost = value;
            } else {
                throw new Error(value + ' is not valid. Expected Absolute Url with https scheme');
            }
        }
    }
});
var _loginUriPrefix = null;
Object.defineProperties(MobileServiceClient.prototype, {
    loginUriPrefix: {
        get: function () {
            return this._loginUriPrefix;
        },
        set: function (value) {
            if (_.isNullOrEmpty(value)) {
                this._loginUriPrefix = ".auth/login";
            } else {
                _.isString(value);
                this._loginUriPrefix = value;
            }
        }
    }
});

/**
 * @class
 * @classdesc Client for connecting to the Azure Mobile Apps backend.
 * @protected
 * 
 * @param {string} applicationUrl The URL of the Azure Mobile backend.
 */
function MobileServiceClient(applicationUrl) {

    Validate.isString(applicationUrl, 'applicationUrl');
    Validate.notNullOrEmpty(applicationUrl, 'applicationUrl');

    this.applicationUrl = applicationUrl;

    var sdkInfo = Platform.getSdkInfo();
    var osInfo = Platform.getOperatingSystemInfo();
    var sdkVersion = sdkInfo.fileVersion.split(".").slice(0, 2).join(".");
    this.version = "ZUMO/" + sdkVersion + " (lang=" + sdkInfo.language + "; " +
                                            "os=" + osInfo.name + "; " +
                                            "os_version=" + osInfo.version + "; " +
                                            "arch=" + osInfo.architecture + "; " +
                                            "version=" + sdkInfo.fileVersion + ")";
    this.currentUser = null;
    this._serviceFilter = null;
    this._login = new MobileServiceLogin(this);

    var _syncContext = new MobileServiceSyncContext(this);

    /**
     * Get the associated {@link MobileServiceSyncContext} instance.
     * 
     * @returns {MobileServiceSyncContext} The associated {@link MobileServiceSyncContext}.
     */
    this.getSyncContext = function() {
        return _syncContext;
    };

    /**
     * Gets a reference to the specified backend table.
     * 
     * @param {string} tableName The name of the backend table. 
     * 
     * @returns {MobileServiceTable} A reference to the specified table in the backend.
     */
    this.getTable = function (tableName) {

        Validate.isString(tableName, 'tableName');
        Validate.notNullOrEmpty(tableName, 'tableName');
        return new MobileServiceTable(tableName, this);
    };

    /**
     * Gets a reference to the specified local table.
     * 
     * @param {string} tableName The name of the table in the local store. 
     * 
     * @returns {MobileServiceSyncTable} A refence to the specified table in the local store.
     */
    this.getSyncTable = function (tableName) {

        Validate.isString(tableName, 'tableName');
        Validate.notNullOrEmpty(tableName, 'tableName');
		
        return new MobileServiceSyncTable(tableName, this);
    };

    if (Push) {
        /**
         * @member {Push} push Push registration manager.
         * @instance
         * @memberof MobileServiceClient
         */
        this.push = new Push(this, MobileServiceClient._applicationInstallationId);
    }
    
}

/**
 * @callback MobileServiceClient.Next
 * @param {Request} request The outgoing request.
 * @param {MobileServiceClient.Completion} callback Completion callback.
 */

/**
 * @callback MobileServiceClient.Completion
 * @param {Error} error Error object.
 * @param {Response} response Server response.
 */

/**
 * @callback MobileServiceClient.Filter
 * @param {Request} request The outgoing request.
 * @param {MobileServiceClient.Next} next Next {@link MobileServiceClient.Filter} in the chain of filters.
 * @param {MobileServiceClient.Completion} callback Completion callback.
 */

/**
 * Create a new {@link MobileServiceClient} with a filter used to process all
 * of its network requests and responses.
 * 
 * @param {MobileServiceClient.Filter} serviceFilter The filter to use on the
 * {@link MobileServiceClient} instance's network requests and responses.
 * 
 * The Mobile Services HTTP pipeline is a chain of filters composed
 * together by giving each the next operation which it can invoke
 * (zero, one, or many times as necessary).
 * 
 * Filters are composed just like standard function composition.  If
 * we had the following:
 * 
 *     new MobileServiceClient().withFilter(F1).withFilter(F2),withFilter(F3),
 * 
 * it is conceptually equivalent to saying:
 * 
 *     var response = F3(F2(F1(next(request)));
 * 
 * @returns {MobileServiceClient} A client whose HTTP requests and responses will be
 * filtered as desired.
 * 
 * Here's a sample filter that will automatically retry request that fails with status code >= 400.
 * 
 * @example
 * function(req, next, callback) {
 *     next(req, function(err, rsp) {
 *         if (rsp.statusCode >= 400) {
 *             next(req, callback);
 *         } else {
 *             callback(err, rsp);
 *         }
 *     });
 * }
 *
 */
MobileServiceClient.prototype.withFilter = function (serviceFilter) {

    Validate.notNull(serviceFilter, 'serviceFilter');

    // Clone the current instance
    var client = new MobileServiceClient(this.applicationUrl);
    client.currentUser = this.currentUser;

    // Chain the service filter with any existing filters
    var existingFilter = this._serviceFilter;
    client._serviceFilter = _.isNull(existingFilter) ?
        serviceFilter :
        function (req, next, callback) {
            // compose existingFilter with next so it can be used as the next
            // of the new serviceFilter
            var composed = function (req, callback) {
                existingFilter(req, next, callback);
            };
            serviceFilter(req, composed, callback);
        };

    return client;
};

MobileServiceClient.prototype._request = function (method, uriFragment, content, ignoreFilters, headers, features, callback) {
    /// <summary>
    /// Perform a web request and include the standard Mobile Services headers.
    /// </summary>
    /// <param name="method" type="string">
    /// The HTTP method used to request the resource.
    /// </param>
    /// <param name="uriFragment" type="String">
    /// URI of the resource to request (relative to the Mobile Services
    /// runtime).
    /// </param>
    /// <param name="content" type="Object">
    /// Optional content to send to the resource.
    /// </param>
    /// <param name="ignoreFilters" type="Boolean" mayBeNull="true">
    /// Optional parameter to indicate if the client filters should be ignored
    /// and the request should be sent directly. Is false by default.
    /// </param>
    /// <param name="headers" type="Object">
    /// Optional request headers
    /// </param>
    /// <param name="features" type="Array">
    /// Codes for features which are used in this request, sent to the server for telemetry.
    /// </param>
    /// <param name="callback" type="function(error, response)">
    /// Handler that will be called on the response.
    /// </param>

    // Account for absent optional arguments
    if (_.isNull(callback) && (typeof features === 'function')) {
        callback = features;
        features = null;
    }

    if (_.isNull(callback) && (typeof headers === 'function')) {
        callback = headers;
        headers = null;
    }

    if (_.isNull(callback) && (typeof ignoreFilters === 'function')) {
        callback = ignoreFilters;
        ignoreFilters = false;
    }

    if (_.isNull(callback) && (typeof content === 'function')) {
        callback = content;
        content = null;
    }

    Validate.isString(method, 'method');
    Validate.notNullOrEmpty(method, 'method');
    Validate.isString(uriFragment, 'uriFragment');
    Validate.notNull(uriFragment, 'uriFragment');
    Validate.notNull(callback, 'callback');

    // Create the absolute URI
    var options = { type: method.toUpperCase() };
    if (_.url.isAbsoluteUrl(uriFragment)) {
        options.url = uriFragment;
    } else {
        options.url = _.url.combinePathSegments(this.applicationUrl, uriFragment);
    }

    // Set MobileServices authentication, application, User-Agent and telemetry headers
    options.headers = {};
    if (!_.isNull(headers)) {
        _.extend(options.headers, headers);
    }
    options.headers["X-ZUMO-INSTALLATION-ID"] = MobileServiceClient._applicationInstallationId;
    if (this.currentUser && !_.isNullOrEmpty(this.currentUser.mobileServiceAuthenticationToken)) {
        options.headers["X-ZUMO-AUTH"] = this.currentUser.mobileServiceAuthenticationToken;
    }
    if (!_.isNull(MobileServiceClient._userAgent)) {
        options.headers["User-Agent"] = MobileServiceClient._userAgent;
    }
    if (!_.isNullOrEmpty["X-ZUMO-VERSION"]) {
        options.headers["X-ZUMO-VERSION"] = this.version;
    }

    if (_.isNull(options.headers[constants.featuresHeaderName]) && features && features.length) {
        options.headers[constants.featuresHeaderName] = features.join(',');
    }

    // Add any content as JSON
    if (!_.isNull(content)) {
        if (!_.isString(content)) {
            options.data = _.toJson(content);
        } else {
            options.data = content;
        }

        if (!_.hasProperty(options.headers, ['Content-Type', 'content-type', 'CONTENT-TYPE', 'Content-type'])) {
            options.headers['Content-Type'] = 'application/json';
        }
    } else {
        // options.data must be set to null if there is no content or the xhr object
        // will set the content-type to "application/text" for non-GET requests.
        options.data = null;
    }

    // Treat any >=400 status codes as errors.  Also treat the status code 0 as
    // an error (which indicates a connection failure).
    var handler = function (error, response) {
        if (!_.isNull(error)) {
            error = _.createError(error);
        } else if (!_.isNull(response) && (response.status >= 400 || response.status === 0)) {
            error = _.createError(null, response);
            response = null;
        }
        callback(error, response);
    };

    // Make the web request
    if (!_.isNull(this._serviceFilter) && !ignoreFilters) {
        this._serviceFilter(options, Platform.webRequest, handler);
    } else {
        Platform.webRequest(options, handler);
    }
};

/**
 * Log a user into an Azure Mobile Apps backend.
 * 
 * @function
 * 
 * @param {string} provider Name of the authentication provider to use; one of _'facebook'_, _'twitter'_, _'google'_,
 *                          _'aad'_ (equivalent to _'windowsazureactivedirectory'_) or _'microsoftaccount'_.
 * @param {object} options Contains additional parameter information.
 * @param {object} options.token provider specific object with existing OAuth token to log in with.
 * @param {boolean} options.useSingleSignOn Indicates if single sign-on should be used. This parameter only applies to Windows clients 
 *                                  and is ignored on other platforms. Single sign-on requires that the 
 *                                  application's Package SID be registered with the Microsoft Azure Mobile Apps backend,
 *                                  but it provides a better experience as HTTP cookies are supported so that users 
 *                                  do not have to login in everytime the application is launched.
 * @param {object} options.parameters Any additional provider specific query string parameters.
 * @returns {Promise} A promise that is either resolved with the logged in user or rejected with the error.
 */
MobileServiceClient.prototype.loginWithOptions = Platform.async(
     function (provider, options, callback) {
         this._login.loginWithOptions(provider, options, callback);
     });

/**
 * Log a user into an Azure Mobile Apps backend.
 * 
 * @function
 * 
 * @param {string} provider Name of the authentication provider to use; one of _'facebook'_, _'twitter'_, _'google'_,
 *                          _'aad'_ (equivalent to _'windowsazureactivedirectory'_) or _'microsoftaccount'_. If no
 *                          provider is specified, the 'token' parameter is considered a Microsoft Account
 *                          authentication token. If a provider is specified, the 'token' parameter is 
 *                          considered a provider-specific authentication token.
 * @param {object} token provider specific object with existing OAuth token to log in with.  
 * @param {boolean} useSingleSignOn Indicates if single sign-on should be used. This parameter only applies to Windows clients 
 *                                  and is ignored on other platforms. Single sign-on requires that the 
 *                                  application's Package SID be registered with the Microsoft Azure Mobile Apps backend,
 *                                  but it provides a better experience as HTTP cookies are supported so that users 
 *                                  do not have to login in everytime the application is launched.
 * @returns {Promise} A promise that is either resolved with the logged in user or rejected with the error.
 */
MobileServiceClient.prototype.login = Platform.async(
    function (provider, token, useSingleSignOn, callback) {
        this._login.login(provider, token, useSingleSignOn, callback);
    });

/**
 * Log a user out of the Mobile Apps backend.
 * 
 * @function
 * 
 * @returns {Promise} A promise that is either resolved or rejected with the error. 
 */
MobileServiceClient.prototype.logout = Platform.async(function(callback) {
    this.currentUser = null;
    callback();
});

/**
 * Invokes the specified custom api and returns a response object.
 * 
 * @function
 * 
 * @param {string} apiName The custom api to invoke.
 * @param {object} options Additional parameter information.
 * @param {object} options.body The body of the HTTP request.
 * @param {string} options.method The HTTP method to use in the request, with the default being 'POST'.
 * @param {object} options.parameters Additional query string parameters, if any, with property names and
 *                                    values as property keys and values respectively. 
 * @param {object} options.headers HTTP request headers.
 * 
 * @returns {Promise} A promise that is resolved with an _XMLHttpRequest_ object if the API is invoked succesfully.
 *                    If the server response is JSON, it is deserialized into _XMLHttpRequest.result_.
 *                    If _invokeApi_ fails, the promise is rejected with the error.
 */
MobileServiceClient.prototype.invokeApi = Platform.async(
    function (apiName, options, callback) {

        Validate.isString(apiName, 'apiName');

        // Account for absent optional arguments
        if (_.isNull(callback)) {
            if (typeof options === 'function') {
                callback = options;
                options = null;
            }
        }
        Validate.notNull(callback, 'callback');

        var parameters, method, body, headers;
        if (!_.isNull(options)) {
            parameters = options.parameters;
            if (!_.isNull(parameters)) {
                Validate.isValidParametersObject(options.parameters);
            }

            method = options.method;
            body = options.body;
            headers = options.headers;
        }

        headers = headers || {};

        if (_.isNull(method)) {
            method = "POST";
        }

        // if not specified, default to return results in JSON format
        if (_.isNull(headers.accept)) {
            headers.accept = 'application/json';
        }

        // Add version header on API requests
        if (_.isNull(headers[constants.apiVersionHeaderName])) {
            headers[constants.apiVersionHeaderName] = constants.apiVersion;
        }

        // Construct the URL
        var url;
        if (_.url.isAbsoluteUrl(apiName)) {
            url = apiName;
        } else {
            url = _.url.combinePathSegments("api", apiName);
        }
        if (!_.isNull(parameters)) {
            var queryString = _.url.getQueryString(parameters);
            url = _.url.combinePathAndQuery(url, queryString);
        }

        var features = [];
        if (!_.isNullOrEmpty(body)) {
            features.push(_.isString(body) ?
                constants.features.GenericApiCall :
                constants.features.JsonApiCall);
        }

        if (!_.isNull(parameters)) {
            features.push(constants.features.AdditionalQueryParameters);
        }

        // Make the request
        this._request(
            method,
            url,
            body,
            null,
            headers,
            features,
            function (error, response) {
                if (!_.isNull(error)) {
                    callback(error, null);
                } else {
                    var contentType;
                    if (typeof response.getResponseHeader !== 'undefined') { // (when not using IframeTransport, IE9)
                        contentType = response.getResponseHeader('Content-Type');
                    }

                    // If there was no header / can't get one, try json
                    if (!contentType) {
                        try {
                            response.result = _.fromJson(response.responseText);
                        } catch (e) {
                            // Do nothing, since we don't know the content-type, failing may be ok
                        }
                    } else if (contentType.toLowerCase().indexOf('json') !== -1) {
                        response.result = _.fromJson(response.responseText);
                    }

                    callback(null, response);
                }
            });

    });

function getApplicationInstallationId() {
    /// <summary>
    /// Gets or creates the static application installation ID.
    /// </summary>
    /// <returns type="string">
    /// The application installation ID.
    /// </returns>

    // Get or create a new installation ID that can be passed along on each
    // request to provide telemetry data
    var applicationInstallationId = null;

    // Check if the config settings exist
    var path = "MobileServices.Installation.config";
    var contents = Platform.readSetting(path);
    if (!_.isNull(contents)) {
        // Parse the contents of the file as JSON and pull out the
        // application's installation ID.
        try {
            var config = _.fromJson(contents);
            applicationInstallationId = config.applicationInstallationId;
        } catch (ex) {
            // Ignore any failures (like invalid JSON, etc.) which will allow
            // us to fall through to and regenerate a valid config below
        }
    }

    // If no installation ID was found, generate a new one and save the config
    // settings.  This is pulled out as a separate function because we'll do it
    // even if we successfully read an existing config but there's no
    // installation ID.
    if (_.isNullOrEmpty(applicationInstallationId)) {
        applicationInstallationId = _.createUniqueInstallationId();

        // TODO: How many other settings should we write out as well?
        var configText = _.toJson({ applicationInstallationId: applicationInstallationId });
        Platform.writeSetting(path, configText);
    }

    return applicationInstallationId;
}

/// <summary>
/// Get or set the static _applicationInstallationId by checking the settings
/// and create the value if necessary.
/// </summary>
MobileServiceClient._applicationInstallationId = getApplicationInstallationId();

/// <summary>
/// Get or set the static _userAgent by calling into the Platform.
/// </summary>
MobileServiceClient._userAgent = Platform.getUserAgent();

// Define the module exports
module.exports = MobileServiceClient;
