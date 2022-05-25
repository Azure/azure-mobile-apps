// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Internal;
using MobileClient.Tests.Helpers;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MobileClient.Tests
{
    public class AuthTestInfo
    {
        public MobileServiceClient Client { get; set; }
        public TestHttpHandler Hijack { get; set; }
    }

    public class MobileServiceTokenAuthentication_Tests
    {
        readonly string loginAsyncUriFragment = ".auth/login";
        readonly string legacyLoginAsyncUriFragment = "login";
        readonly string validAlternateLoginUrl = "https://www.testalternatelogin.com/";
        readonly string validAlternateLoginUrlWithoutTrailingSlash = "https://www.testalternatelogin.com";

        private AuthTestInfo Initialize_Client(string appUrl = null, string loginPrefix = null, string alternateLoginUri = null)
        {
            if (string.IsNullOrEmpty(appUrl))
            {
                appUrl = MobileAppUriValidator.DummyMobileApp;
            }
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent(String.Empty);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            var client = new MobileServiceClient(appUrl) //,hijack
            {
                LoginUriPrefix = loginPrefix
            };
            if (!string.IsNullOrEmpty(alternateLoginUri))
            {
                client.AlternateLoginHost = new Uri(alternateLoginUri);
            }
            return new AuthTestInfo() { Client = client, Hijack = hijack };
        }

        private void TestStartUriForParameters(Dictionary<string, string> parameters, string uri, string loginPrefix = null,
            string alternateLoginUri = null, string appUrl = null)
        {
            var authTestInfo = Initialize_Client(appUrl, loginPrefix, alternateLoginUri);
            var auth = new MobileServiceTokenAuthentication(authTestInfo.Client, "MicrosoftAccount", new Newtonsoft.Json.Linq.JObject(), parameters);
            Assert.Equal(auth.StartUri.OriginalString, uri);
        }

        private async Task TestLoginAsyncForParameters(Dictionary<string, string> parameters, string uri, string loginPrefix = null,
            string alternateLoginUrl = null, string appUrl = null)
        {
            var authTestInfo = Initialize_Client(appUrl, loginPrefix, alternateLoginUrl);
            var auth = new MobileServiceTokenAuthentication(authTestInfo.Client, "MicrosoftAccount", new JObject(), parameters);
            await auth.LoginAsync();
            Assert.Equal(authTestInfo.Hijack.Request.RequestUri.OriginalString, uri);
        }

        [Fact]
        public void StartUri_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday");
        }

        [Fact]
        public void StartUri_Leagcy_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", "login");
        }


        [Fact]
        public void StartUri_AlternateLoginUri_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, validAlternateLoginUrl + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", null, validAlternateLoginUrl);
        }

        [Fact]
        public void StartUri_Legacy_AlternateLoginUri_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", "/login", validAlternateLoginUrl);
        }

        [Fact]
        public void StartUri_MobileAppWithFolder_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", null, null, MobileAppUriValidator.DummyMobileAppUriWithFolder);
        }

        [Fact]
        public void StartUri_Legacy_MobileAppUriWithFolder_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", "login");
        }

        [Fact]
        public void StartUri_WithNullParameters()
        {
            TestStartUriForParameters(null, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount");
        }

        [Fact]
        public void StartUri_Legacy_WithNullParameters()
        {
            TestStartUriForParameters(null, MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount", "/login");
        }

        [Fact]
        public void StartUri_MobileAppUriWihtoutTrailingSlash_WithNullParameters()
        {
            TestStartUriForParameters(null, MobileAppUriValidator.DummyMobileAppWithoutTrailingSlash + "/" + legacyLoginAsyncUriFragment + "/microsoftaccount", "login", null, MobileAppUriValidator.DummyMobileAppWithoutTrailingSlash);
        }

        [Fact]
        public void StartUri_AlternateLoginUri_WithNullParameters()
        {
            TestStartUriForParameters(null, validAlternateLoginUrl + loginAsyncUriFragment + "/microsoftaccount", null, validAlternateLoginUrl);
        }

        [Fact]
        public void StartUri_Legacy_AlternateLoginUri_WithNullParameters()
        {
            TestStartUriForParameters(null, validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount", "login", validAlternateLoginUrl);
        }

        [Fact]
        public void StartUri_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount");
        }

        [Fact]
        public void StartUri_Legacy_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount", "login");
        }

        [Fact]
        public void StartUri_AlternateLoginUrl_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), validAlternateLoginUrlWithoutTrailingSlash + "/" + loginAsyncUriFragment + "/microsoftaccount", null, validAlternateLoginUrlWithoutTrailingSlash);
        }

        [Fact]
        public void StartUri_Legacy_AlternateLoginUrl_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount", "/login", validAlternateLoginUrl);
        }

        [Fact]
        public void StartUri_Legacy_MobileAppUriWithFolder_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount", "login", null, MobileAppUriValidator.DummyMobileAppUriWithFolder);
        }

        [Fact]
        public void StartUri_ThrowsInvalidAlternateLoginHost()
            => Assert.Throws<ArgumentException>(() => new MobileServiceClient(MobileAppUriValidator.DummyMobileApp) 
            { AlternateLoginHost = new Uri(MobileAppUriValidator.DummyMobileAppUriWithFolder) });

        [Fact]
        public void StartUri_ThrowsInvalidAlternateLoginHostTS()
            => Assert.Throws<ArgumentException>(() => new MobileServiceClient(MobileAppUriValidator.DummyMobileApp) 
            { AlternateLoginHost = new Uri(MobileAppUriValidator.DummyMobileAppUriWithFolderWithoutTrailingSlash) });

        [Fact]
        public void StartUri_ThrowsInvalidAlternateLoginHostHTTP()
            => Assert.Throws<ArgumentException>(() => new MobileServiceClient(MobileAppUriValidator.DummyMobileApp)
            { AlternateLoginHost = new Uri("http://www.testalternatelogin.com/") });

        [Fact]
        public void AlternateLoginUri_Null()
        {
            var client = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp)
            {
                AlternateLoginHost = null
            };
            Assert.Equal(client.AlternateLoginHost, client.MobileAppUri);
        }

        [Fact]
        public Task LoginAsync_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday");
        }

        [Fact]
        public Task LoginAsync_Legacy_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", "login");
        }

        [Fact]
        public Task LoginAsync_AlternateLoginUri_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, validAlternateLoginUrl + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", null, validAlternateLoginUrl);
        }

        [Fact]
        public Task LoginAsync_Legacy_AlternateLoginUri_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", "/login", validAlternateLoginUrl);
        }

        [Fact]
        public Task LoginAsync_MobileAppUriWithFolder_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", null, null, MobileAppUriValidator.DummyMobileAppUriWithFolder);
        }

        [Fact]
        public Task LoginAsync_WithNullParameters()
        {
            return TestLoginAsyncForParameters(null, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount");
        }

        [Fact]
        public Task LoginAsync_WithEmptyParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount");
        }

        [Fact]
        public Task LoginAsync_MobileAppUriWithoutTrailingSlash_WithEmptyParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount", null, null, MobileAppUriValidator.DummyMobileAppWithoutTrailingSlash);
        }

        [Fact]
        public Task LoginAsync_Legacy_WithEmptyParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(),
                MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount", "login");
        }

        [Fact]
        public Task LoginAsync_AlternateLoginUri_WithEmptyParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(), validAlternateLoginUrl + loginAsyncUriFragment + "/microsoftaccount", null, validAlternateLoginUrl);
        }

        [Fact]
        public Task LoginAsync_Legacy_AlternateLoginUri_WithEmptyParameters()
        {
            
                return TestLoginAsyncForParameters(new Dictionary<string, string>(), validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount", "login", validAlternateLoginUrl);
        }
    }
}
