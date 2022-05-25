// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.Internal;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides basic access to a Microsoft Azure Mobile Service.
    /// </summary>
    public class MobileServiceClient : IMobileServiceClient, IDisposable
    {
        /// <summary>
        /// Name of the config setting that stores the installation ID.
        /// </summary>
        private const string ConfigureAsyncInstallationConfigPath = "MobileServices.Installation.config";

        /// <summary>
        /// Name of the JSON member in the config setting that stores the
        /// installation ID.
        /// </summary>
        private const string ConfigureAsyncApplicationIdKey = "applicationInstallationId";

        /// <summary>
        /// Relative URI fragment of the refresh user endpoint.
        /// </summary>
        private const string RefreshUserAsyncUriFragment = "/.auth/refresh";

        private static readonly HttpMethod defaultHttpMethod = HttpMethod.Post;

        /// <summary>
        /// Default empty array of HttpMessageHandlers.
        /// </summary>
        private static readonly HttpMessageHandler[] EmptyHttpMessageHandlers = new HttpMessageHandler[0];

        /// <summary>
        /// Absolute URI of the Microsoft Azure Mobile App.
        /// </summary>
        public Uri MobileAppUri { get; private set; }

        /// <summary>
        /// The current authenticated user provided after a successful call to
        /// MobileServiceClient.Login().
        /// </summary>
        public MobileServiceUser CurrentUser { get; set; }

        private string loginUriPrefix;

        /// <summary>
        /// Prefix for the login endpoints. If not set defaults to /.auth/login
        /// </summary>
        public string LoginUriPrefix
        {
            get
            {
                return loginUriPrefix;
            }
            set
            {
                loginUriPrefix = value;
                if (!string.IsNullOrEmpty(value))
                {
                    loginUriPrefix = MobileServiceUrlBuilder.AddLeadingSlash(value);
                }
            }
        }

        private Uri alternateLoginHost;

        /// <summary>
        /// Alternate URI for login
        /// </summary>
        public Uri AlternateLoginHost
        {
            get
            {
                return alternateLoginHost;
            }
            set
            {
                if (value == null)
                {
                    alternateLoginHost = MobileAppUri;
                }
                else if (value.IsAbsoluteUri && value.Segments.Length == 1 && value.Scheme == "https")
                {
                    alternateLoginHost = value;
                }
                else
                {
                    throw new ArgumentException("Invalid AlternateLoginHost", nameof(value));
                }

                this.AlternateAuthHttpClient = new MobileServiceHttpClient(EmptyHttpMessageHandlers, alternateLoginHost, this.InstallationId);
            }
        }

        /// <summary>
        /// The id used to identify this installation of the application to
        /// provide telemetry data.
        /// </summary>
        public string InstallationId { get; private set; }

        /// <summary>
        /// The event manager that exposes and manages the event stream used by the mobile services types to
        /// publish and consume events.
        /// </summary>
        public IMobileServiceEventManager EventManager { get; private set; }

        /// <summary>
        /// The location of any files we need to create for offline-sync
        /// </summary>
        public static string DefaultDatabasePath
        {
            get
            {
                return Platform.Instance.DefaultDatabasePath;
            }
        }

        /// <summary>
        /// Ensures that a file exists, creating it if necessary
        /// </summary>
        /// <param name="path">The fully-qualified pathname to check</param>
        public static void EnsureFileExists(string path)
        {
            Platform.Instance.EnsureFileExists(path);
        }

        /// <summary>
        /// Gets or sets the settings used for serialization.
        /// </summary>
        public MobileServiceJsonSerializerSettings SerializerSettings
        {
            get
            {
                return this.Serializer.SerializerSettings;
            }

            set
            {
                this.Serializer.SerializerSettings = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Instance of <see cref="IMobileServiceSyncContext"/>
        /// </summary>
        public IMobileServiceSyncContext SyncContext
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the serializer that is used with the table.
        /// </summary>
        internal MobileServiceSerializer Serializer { get; set; }

        /// <summary>
        /// Gets the <see cref="MobileServiceHttpClient"/> associated with the Azure Mobile App.
        /// </summary>
        internal MobileServiceHttpClient HttpClient { get; private set; }

        /// <summary>
        /// Gets the <see cref="MobileServiceHttpClient"/> associated with the Alternate login
        /// Azure Mobile App.
        /// </summary>
        internal MobileServiceHttpClient AlternateAuthHttpClient { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MobileServiceClient class.
        /// </summary>
        /// <param name="mobileAppUri">
        /// Absolute URI of the Microsoft Azure Mobile App.
        /// </param>
        /// <param name="handlers">
        /// Chain of <see cref="HttpMessageHandler" /> instances.
        /// All but the last should be <see cref="DelegatingHandler"/>s.
        /// </param>
        public MobileServiceClient(string mobileAppUri, params HttpMessageHandler[] handlers)
            : this(new Uri(mobileAppUri, UriKind.Absolute), handlers)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MobileServiceClient class.
        /// </summary>
        /// <param name="mobileAppUri">
        /// Absolute URI of the Microsoft Azure Mobile App.
        /// </param>
        /// <param name="handlers">
        /// Chain of <see cref="HttpMessageHandler" /> instances.
        /// All but the last should be <see cref="DelegatingHandler"/>s.
        /// </param>
        public MobileServiceClient(Uri mobileAppUri, params HttpMessageHandler[] handlers)
        {
            Arguments.IsNotNull(mobileAppUri, nameof(mobileAppUri));

            if (mobileAppUri.IsAbsoluteUri)
            {
                // Trailing slash in the MobileAppUri is important. Fix it right here before we pass it on further.
                MobileAppUri = new Uri(MobileServiceUrlBuilder.AddTrailingSlash(mobileAppUri.AbsoluteUri), UriKind.Absolute);
            }
            else
            {
                throw new ArgumentException($"'{mobileAppUri}' is not an absolute Uri", nameof(mobileAppUri));
            }

            this.InstallationId = GetApplicationInstallationId();

            handlers ??= EmptyHttpMessageHandlers;
            this.HttpClient = new MobileServiceHttpClient(handlers, this.MobileAppUri, this.InstallationId);
            this.Serializer = new MobileServiceSerializer();
            this.EventManager = new MobileServiceEventManager();
            this.SyncContext = new MobileServiceSyncContext(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileServiceClient"/> class.
        /// </summary>
        /// <param name="options">the connection options.</param>
        public MobileServiceClient(IMobileServiceClientOptions options) : this(options.MobileAppUri, null)
        {
            AlternateLoginHost = options.AlternateLoginHost;
            LoginUriPrefix = options.LoginUriPrefix;

            var handlers = options.GetDefaultMessageHandlers(this) ?? EmptyHttpMessageHandlers;
            if (handlers.Any())
            {
                HttpClient = new MobileServiceHttpClient(handlers, MobileAppUri, InstallationId);
            }
        }

        /// <summary>
        ///  This is for unit testing only
        /// </summary>
        protected MobileServiceClient()
        {
        }

        /// <summary>
        /// Returns a <see cref="IMobileServiceTable"/> instance, which provides
        /// untyped data operations for that table.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table.
        /// </param>
        /// <returns>
        /// The table.
        /// </returns>
        public IMobileServiceTable GetTable(string tableName)
        {
            ValidateTableName(tableName);

            return new MobileServiceTable(tableName, this);
        }


        /// <summary>
        /// Returns a <see cref="IMobileServiceSyncTable"/> instance, which provides
        /// untyped data operations for that table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The table.</returns>
        public IMobileServiceSyncTable GetSyncTable(string tableName)
        {
            return GetSyncTable(tableName, MobileServiceTableKind.Table);
        }

        internal MobileServiceSyncTable GetSyncTable(string tableName, MobileServiceTableKind kind)
        {
            ValidateTableName(tableName);

            return new MobileServiceSyncTable(tableName, kind, this);
        }

        /// <summary>
        /// Returns a <see cref="IMobileServiceTable{T}"/> instance, which provides
        /// strongly typed data operations for that table.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the instances in the table.
        /// </typeparam>
        /// <returns>
        /// The table.
        /// </returns>
        public IMobileServiceTable<T> GetTable<T>()
        {
            string tableName = this.SerializerSettings.ContractResolver.ResolveTableName(typeof(T));
            return new MobileServiceTable<T>(tableName, this);
        }


        /// <summary>
        /// Returns a <see cref="IMobileServiceSyncTable{T}"/> instance, which provides
        /// strongly typed data operations for local table.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the instances in the table.
        /// </typeparam>
        /// <returns>
        /// The table.
        /// </returns>
        public IMobileServiceSyncTable<T> GetSyncTable<T>()
        {
            string tableName = this.SerializerSettings.ContractResolver.ResolveTableName(typeof(T));
            return new MobileServiceSyncTable<T>(tableName, MobileServiceTableKind.Table, this);
        }

        /// <summary>
        /// Logs a user into a Windows Azure Mobile Service with the provider and optional token object.
        /// </summary>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="token">
        /// Provider specific object with existing OAuth token to log in with.
        /// </param>
        /// <remarks>
        /// The token object needs to be formatted depending on the specific provider. These are some
        /// examples of formats based on the providers:
        /// <list type="bullet">
        ///   <item>
        ///     <term>MicrosoftAccount</term>
        ///     <description><code>{"authenticationToken":"&lt;the_authentication_token&gt;"}</code></description>
        ///   </item>
        ///   <item>
        ///     <term>Facebook</term>
        ///     <description><code>{"access_token":"&lt;the_access_token&gt;"}</code></description>
        ///   </item>
        ///   <item>
        ///     <term>Google</term>
        ///     <description><code>{"access_token":"&lt;the_access_token&gt;"}</code></description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider, JObject token)
        {
            if (!Enum.IsDefined(typeof(MobileServiceAuthenticationProvider), provider))
            {
                throw new ArgumentOutOfRangeException(nameof(provider));
            }
            return this.LoginAsync(provider.ToString(), token);
        }

        /// <summary>
        /// Logs a user into a Microsoft Azure Mobile Service with the provider and optional token object.
        /// </summary>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="token">
        /// Provider specific object with existing OAuth token to log in with.
        /// </param>
        /// <remarks>
        /// The token object needs to be formatted depending on the specific provider. These are some
        /// examples of formats based on the providers:
        /// <list type="bullet">
        ///   <item>
        ///     <term>MicrosoftAccount</term>
        ///     <description><code>{"authenticationToken":"&lt;the_authentication_token&gt;"}</code></description>
        ///   </item>
        ///   <item>
        ///     <term>Facebook</term>
        ///     <description><code>{"access_token":"&lt;the_access_token&gt;"}</code></description>
        ///   </item>
        ///   <item>
        ///     <term>Google</term>
        ///     <description><code>{"access_token":"&lt;the_access_token&gt;"}</code></description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public Task<MobileServiceUser> LoginAsync(string provider, JObject token)
        {
            Arguments.IsNotNull(token, nameof(token));

            MobileServiceTokenAuthentication auth = new MobileServiceTokenAuthentication(this, provider, token, parameters: null);
            return auth.LoginAsync();
        }

        /// <summary>
        /// Log a user out.
        /// </summary>
        public Task LogoutAsync()
        {
            this.CurrentUser = null;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Refreshes access token with the identity provider for the logged in user.
        /// </summary>
        /// <returns>
        /// Task that will complete when the user has finished refreshing access token
        /// </returns>
        public async Task<MobileServiceUser> RefreshUserAsync()
        {
            if (this.CurrentUser == null || string.IsNullOrEmpty(this.CurrentUser.MobileServiceAuthenticationToken))
            {
                throw new InvalidOperationException("MobileServiceUser must be set before calling refresh");
            }

            MobileServiceHttpClient client = this.HttpClient;
            if (this.AlternateLoginHost != null)
            {
                client = this.AlternateAuthHttpClient;
            }
            string response;
            try
            {
                response = await client.RequestWithoutHandlersAsync(HttpMethod.Get, RefreshUserAsyncUriFragment, this.CurrentUser, null, MobileServiceFeatures.RefreshToken);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                if (ex.Response != null)
                {
                    string message = ex.Response.StatusCode switch
                    {
                        HttpStatusCode.BadRequest => "Refresh failed with a 400 Bad Request error. The identity provider does not support refresh, or the user is not logged in with sufficient permission.",
                        HttpStatusCode.Unauthorized => "Refresh failed with a 401 Unauthorized error. Credentials are no longer valid.",
                        HttpStatusCode.Forbidden => "Refresh failed with a 403 Forbidden error. The refresh token was revoked or expired.",
                        _ => "Refresh failed due to an unexpected error.",
                    };
                    throw new MobileServiceInvalidOperationException(message, innerException: ex, request: ex.Request, response: ex.Response);
                }
                throw;
            }

            if (!string.IsNullOrEmpty(response))
            {
                JToken authToken = JToken.Parse(response);

                this.CurrentUser.MobileServiceAuthenticationToken = (string)authToken["authenticationToken"];
            }

            return this.CurrentUser;
        }

        /// <summary>
        /// Invokes a user-defined custom API of a Microsoft Azure Mobile Service using an HTTP POST.
        /// </summary>
        /// <typeparam name="T">The type of instance returned from the Microsoft Azure Mobile Service.</typeparam>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe</param>
        /// <returns>The response content from the custom api invocation.</returns>
        public Task<T> InvokeApiAsync<T>(string apiName, CancellationToken cancellationToken = default)
        {
            return this.InvokeApiAsync<string, T>(apiName, null, null, null, cancellationToken);
        }

        /// <summary>
        /// Invokes a user-defined custom API of a Microsoft Azure Mobile Service using an HTTP POST with
        /// support for sending HTTP content.
        /// </summary>
        /// <typeparam name="T">The type of instance sent to the Microsoft Azure Mobile Service.</typeparam>
        /// <typeparam name="U">The type of instance returned from the Microsoft Azure Mobile Service.</typeparam>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="body">The value to be sent as the HTTP body.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe</param>
        /// <returns>The response content from the custom api invocation.</returns>
        public Task<U> InvokeApiAsync<T, U>(string apiName, T body, CancellationToken cancellationToken = default)
        {
            return this.InvokeApiAsync<T, U>(apiName, body, null, null, cancellationToken);
        }

        /// <summary>
        /// Invokes a user-defined custom API of a Microsoft Azure Mobile Service using the specified HTTP Method.
        /// Additional data can be passed using the query string.
        /// </summary>
        /// <typeparam name="T">The type of instance sent to the Microsoft Azure Mobile Service.</typeparam>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe</param>
        /// <returns>The response content from the custom api invocation.</returns>
        public Task<T> InvokeApiAsync<T>(string apiName, HttpMethod method, IDictionary<string, string> parameters, CancellationToken cancellationToken = default)
        {
            return this.InvokeApiAsync<string, T>(apiName, null, method, parameters, cancellationToken);
        }

        /// <summary>
        /// Invokes a user-defined custom API of a Microsoft Azure Mobile Service using the specified HTTP Method.
        /// Additional data can be sent though the HTTP content or the query string.
        /// </summary>
        /// <typeparam name="T">The type of instance sent to the Microsoft Azure Mobile Service.</typeparam>
        /// <typeparam name="U">The type of instance returned from the Microsoft Azure Mobile Service.</typeparam>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="body">The value to be sent as the HTTP body.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe</param>
        /// <returns>The response content from the custom api invocation.</returns>
        public async Task<U> InvokeApiAsync<T, U>(string apiName, T body, HttpMethod method, IDictionary<string, string> parameters, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNullOrWhiteSpace(apiName, nameof(apiName));

            MobileServiceSerializer serializer = this.Serializer;
            string content = null;
            if (body != null)
            {
                content = serializer.Serialize(body).ToString();
            }

            string response = await this.InternalInvokeApiAsync(apiName, content, method, parameters, MobileServiceFeatures.TypedApiCall, cancellationToken);
            if (string.IsNullOrEmpty(response))
            {
                return default;
            }
            return serializer.Deserialize<U>(JToken.Parse(response));
        }

        /// <summary>
        /// Invokes a user-defined custom API of a Microsoft Azure Mobile Service using an HTTP POST.
        /// </summary>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe</param>
        /// <returns></returns>
        public Task<JToken> InvokeApiAsync(string apiName, CancellationToken cancellationToken = default)
        {
            return this.InvokeApiAsync(apiName, null, null, null, cancellationToken);
        }

        /// <summary>
        /// Invokes a user-defined custom API of a Microsoft Azure Mobile Service using an HTTP POST, with
        /// support for sending HTTP content.
        /// </summary>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="body">The value to be sent as the HTTP body.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe</param>
        /// <returns>The response content from the custom api invocation.</returns>
        public Task<JToken> InvokeApiAsync(string apiName, JToken body, CancellationToken cancellationToken = default)
        {
            return this.InvokeApiAsync(apiName, body, defaultHttpMethod, null, cancellationToken);
        }

        /// <summary>
        /// Invokes a user-defined custom API of a Microsoft Azure Mobile Service using the specified HTTP Method.
        /// Additional data will sent to through the query string.
        /// </summary>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe</param>
        /// <returns>The response content from the custom api invocation.</returns>
        public Task<JToken> InvokeApiAsync(string apiName, HttpMethod method, IDictionary<string, string> parameters, CancellationToken cancellationToken = default)
        {
            return this.InvokeApiAsync(apiName, null, method, parameters, cancellationToken);
        }

        /// <summary>
        /// Invokes a user-defined custom API of a Microsoft Azure Mobile Service using the specified HTTP method.
        /// Additional data can be sent though the HTTP content or the query string.
        /// </summary>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="body">The value to be sent as the HTTP body.</param>
        /// <param name="method">The HTTP Method.</param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe</param>
        /// <returns>The response content from the custom api invocation.</returns>
        public async Task<JToken> InvokeApiAsync(string apiName, JToken body, HttpMethod method, IDictionary<string, string> parameters, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNullOrWhiteSpace(apiName, nameof(apiName));

            string content = null;
            if (body != null)
            {
                content = body.Type switch
                {
                    JTokenType.Null => "null",
                    JTokenType.Boolean => body.ToString().ToLowerInvariant(),
                    _ => body.ToString(),
                };
            }

            string response = await this.InternalInvokeApiAsync(apiName, content, method, parameters, MobileServiceFeatures.JsonApiCall, cancellationToken);
            return response.ParseToJToken(this.SerializerSettings);
        }

        /// <summary>
        /// Invokes a user-defined custom API of a Windows Azure Mobile Service using the specified HttpMethod.
        /// Additional data can be sent though the HTTP content or the query string.
        /// </summary>
        /// <param name="apiName">The name of the custom AP.</param>
        /// <param name="content">The HTTP content.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestHeaders">
        /// A dictionary of user-defined headers to include in the HttpRequest.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe</param>
        /// <returns>The HTTP Response from the custom api invocation.</returns>
        public async Task<HttpResponseMessage> InvokeApiAsync(string apiName, HttpContent content, HttpMethod method, IDictionary<string, string> requestHeaders, IDictionary<string, string> parameters, CancellationToken cancellationToken = default)
        {
            method ??= defaultHttpMethod;
            HttpResponseMessage response = await this.HttpClient.RequestAsync(method, CreateAPIUriString(apiName, parameters), this.CurrentUser, content, requestHeaders: requestHeaders, features: MobileServiceFeatures.GenericApiCall, cancellationToken: cancellationToken);
            return response;
        }

        /// <summary>
        /// Invokes a user-defined custom API of a Microsoft Azure Mobile Service.
        /// </summary>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="content">The HTTP content, as a string, in json format.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <param name="features">
        /// Value indicating which features of the SDK are being used in this call. Useful for telemetry.
        /// </param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe</param>
        /// <returns>The response content from the custom api invocation.</returns>
        private async Task<string> InternalInvokeApiAsync(string apiName, string content, HttpMethod method, IDictionary<string, string> parameters, MobileServiceFeatures features, CancellationToken cancellationToken = default)
        {
            method ??= defaultHttpMethod;
            if (parameters != null && parameters.Count > 0)
            {
                features |= MobileServiceFeatures.AdditionalQueryParameters;
            }

            MobileServiceHttpResponse response = await this.HttpClient.RequestAsync(method, CreateAPIUriString(apiName, parameters), this.CurrentUser, content, false, features: features, cancellationToken: cancellationToken);
            return response.Content;
        }

        /// <summary>
        /// Helper function to assemble the Uri for a given custom api.
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string CreateAPIUriString(string apiName, IDictionary<string, string> parameters = null)
        {
            string uriFragment = apiName.StartsWith("/") ? apiName : $"api/{apiName}";
            string queryString = MobileServiceUrlBuilder.GetQueryString(parameters, useTableAPIRules: false);
            return MobileServiceUrlBuilder.CombinePathAndQuery(uriFragment, queryString);
        }

        /// <summary>
        /// Invokes a user-defined custom API of a Windows Azure Mobile Service using the specified HttpMethod.
        /// Additional data can be sent though the HTTP content or the query string.
        /// </summary>
        /// <param name="apiName">The name of the custom AP.</param>
        /// <param name="content">The HTTP content.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestHeaders">
        /// A dictionary of user-defined headers to include in the HttpRequest.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>The HTTP Response from the custom api invocation.</returns>
        public async Task<HttpResponseMessage> InvokeApiAsync(string apiName, HttpContent content, HttpMethod method, IDictionary<string, string> requestHeaders, IDictionary<string, string> parameters)
        {
            method ??= defaultHttpMethod;
            HttpResponseMessage response = await this.HttpClient.RequestAsync(method, CreateAPIUriString(apiName, parameters), this.CurrentUser, content, requestHeaders: requestHeaders, features: MobileServiceFeatures.GenericApiCall);
            return response;
        }

        /// <summary>
        /// Implemenation of <see cref="IDisposable"/>
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implemenation of <see cref="IDisposable"/> for
        /// derived classes to use.
        /// </summary>
        /// <param name="disposing">
        /// Indicates if being called from the Dispose() method
        /// or the finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((MobileServiceSyncContext)this.SyncContext).Dispose();
                // free managed resources
                this.HttpClient.Dispose();
            }
        }

        private static void ValidateTableName(string tableName)
        {
            Arguments.IsNotNullOrWhiteSpace(tableName, nameof(tableName));
        }

        /// <summary>
        /// Gets the ID used to identify this installation of the
        /// application to provide telemetry data.  It will either be retrieved
        /// from local settings or generated fresh.
        /// </summary>
        /// <returns>
        /// An installation ID.
        /// </returns>
        private string GetApplicationInstallationId()
        {
            // Try to get the AppInstallationId from settings
            string installationId = null;
            IApplicationStorage applicationStorage = Platform.Instance.ApplicationStorage;

            if (applicationStorage.TryReadSetting(ConfigureAsyncInstallationConfigPath, out object setting))
            {
                try
                {
                    JToken config = JToken.Parse(setting as string);
                    installationId = (string)config[ConfigureAsyncApplicationIdKey];
                }
                catch (Exception)
                {
                }
            }

            // Generate a new AppInstallationId if we failed to find one
            if (installationId == null)
            {
                installationId = Guid.NewGuid().ToString();
                JObject jobject = new JObject
                {
                    [ConfigureAsyncApplicationIdKey] = installationId
                };
                string configText = jobject.ToString();
                applicationStorage.WriteSetting(ConfigureAsyncInstallationConfigPath, configText);
            }

            return installationId;
        }

    }
}
