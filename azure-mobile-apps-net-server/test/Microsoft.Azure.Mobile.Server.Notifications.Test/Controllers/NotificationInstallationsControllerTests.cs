// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Controllers;
using Microsoft.Azure.Mobile.Server.Login;
using Microsoft.Azure.Mobile.Server.Notifications;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using Moq;
using Xunit;

namespace Microsoft.WindowsAzure.Mobile.Service
{
    public class NotificationInstallationsControllerTests
    {
        private const string TestGuidValid = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
        private const string ApnsTokenValid = "2ed202ac08ea9033665e853a3dc8bc4c5e78f7a6cf8d55910df230567037dcc4";
        private const string AppleNotificationServiceString = "apns";
        private const string WindowsNotificationServiceString = "wns";
        private const string UserIdTagPrefix = "_UserId:";
        private const string UserIdTagPlaceholder = UserIdTagPrefix + "{0}";
        private const string InstallationIdTagPrefix = "$InstallationId:";
        private const string InstallationIdTagPlaceholder = InstallationIdTagPrefix + "{{{0}}}";
        private const string WnsPushChannelValid = "https://bn1.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d";
        private const string GcmToken = "someGcmToken";
        private const string GcmNotificationServiceString = "gcm";

        [Fact]
        public void CopyTagsToInstallation_IsConsistent()
        {
            Installation installation = MakeTestInstallation();
            HashSet<string> tags = new HashSet<string>
            {
                "tag1",
                "tag2",
                string.Format(UserIdTagPlaceholder, "myId"),
                string.Format(InstallationIdTagPlaceholder, Guid.NewGuid().ToString())
            };
            NotificationInstallationsControllerMock.CopyTagsToInstallation(installation, tags, true);
            IList<string> tagsOutput = installation.Tags;
            foreach (string tag in tags)
            {
                if (tag.StartsWith(InstallationIdTagPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    Assert.False(tagsOutput.Contains(tag));
                }
                else
                {
                    Assert.True(tagsOutput.Contains(tag));
                }
            }
        }

        [Fact]
        public void CopyTagsToInstallation_IsConsistent_CopyUserFalse()
        {
            Installation installation = MakeTestInstallation();
            HashSet<string> tags = new HashSet<string>
            {
                "tag1",
                "tag2",
                string.Format(UserIdTagPlaceholder, "myId"),
                string.Format(InstallationIdTagPlaceholder, Guid.NewGuid().ToString())
            };
            NotificationInstallationsControllerMock.CopyTagsToInstallation(installation, tags, false);
            IList<string> tagsOutput = installation.Tags;
            foreach (string tag in tags)
            {
                if (tag.StartsWith(InstallationIdTagPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    Assert.False(tagsOutput.Contains(tag));
                }
                else if (tag.StartsWith(UserIdTagPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    Assert.False(tagsOutput.Contains(tag));
                }
                else
                {
                    Assert.True(tagsOutput.Contains(tag));
                }
            }
        }

        [Fact]
        public void CopyTagsToInstallation_DoesNotThrowForEmptyTagsSet()
        {
            Installation installation = MakeTestInstallation();
            HashSet<string> tags = new HashSet<string>();
            NotificationInstallationsControllerMock.CopyTagsToInstallation(installation, tags, true);
            IList<string> tagsOutput = installation.Tags;
            Assert.NotNull(tagsOutput);
            Assert.True(tagsOutput.Count == 0);
        }

        [Fact]
        public void CopyTagsToInstallation_DoesNotThrowForNullTags()
        {
            Installation installation = MakeTestInstallation();
            NotificationInstallationsControllerMock.CopyTagsToInstallation(installation, null, true);
            IList<string> tagsOutput = installation.Tags;
            Assert.Null(tagsOutput);
        }

        private static Installation MakeTestInstallation()
        {
            return new Installation
            {
                InstallationId = TestGuidValid,
                Platform = NotificationPlatform.Wns,
                PushChannel = WnsPushChannelValid,
                SecondaryTiles = new Dictionary<string, WnsSecondaryTile>
                {
                    {
                        "tileName",
                        new WnsSecondaryTile
                        {
                            PushChannel = WnsPushChannelValid,
                            Tags = new List<string>
                            {
                                "tag1",
                                "tag2"
                            },
                            Templates = new Dictionary<string, InstallationTemplate>
                            {
                                {
                                    "templateName",
                                    new InstallationTemplate
                                    {
                                        Body = "someBody",
                                        Headers = new Dictionary<string, string>
                                        {
                                            {
                                                "headerName",
                                                "headerValue"
                                            }
                                        },
                                        Tags = new List<string>
                                        {
                                            "tagA",
                                            "tagB"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        [Theory]
        [InlineData("apns")]
        [InlineData("gcm")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wns", Justification = "Wns is fine")]
        public void ValidateTemplate_NonWnsPlatformThrowsWhenHeadersNonNull(string platform)
        {
            // Pass condition = exception thrown.
            bool testPass = false;
            NotificationTemplate template = new NotificationTemplate()
            {
                Body = "someString",
                Tags = new List<string>
                {
                    "tag1",
                    "tag2"
                },
                Headers = new Dictionary<string, string>()
            };
            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            try
            {
                mock.ValidateTemplate(template, platform);
            }
            catch
            {
                testPass = true;
            }

            Assert.True(testPass);
        }

        [Theory]
        [InlineData("apns")]
        [InlineData("wns")]
        [InlineData("gcm")]
        public void ValidateTemplate_DoesNotThrowWhenHeadersIsNull(string platform)
        {
            NotificationTemplate template = MakeTestNotificationTemplate();
            template.Headers = null;
            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            mock.ValidateTemplate(template, platform);
        }

        [Theory]
        [InlineData("wns")]
        [InlineData("apns")]
        [InlineData("gcm")]
        public void ValidateTemplate_DoesNotThrowOnNullBody(string platform)
        {
            NotificationTemplate template = MakeTestNotificationTemplate();
            template.Headers = null;
            template.Body = null;
            if (platform.Equals("wns", StringComparison.OrdinalIgnoreCase))
            {
                template.Headers = new Dictionary<string, string> { { "header1", "value1" } };
            }

            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            mock.ValidateTemplate(template, platform);
        }

        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wns", Justification = "Wns is fine")]
        public void ValidateTemplate_WnsPlatformDoesNotThrowOnEmptyHeaders()
        {
            NotificationTemplate template = MakeTestNotificationTemplate();
            template.Headers = new Dictionary<string, string>();
            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            mock.ValidateTemplate(template, "wns");
        }

        [Theory]
        [InlineData("wns")]
        [InlineData("apns")]
        [InlineData("gcm")]
        public void ValidateTemplate_DoesNotThrowOnNullTags(string platform)
        {
            NotificationTemplate template = MakeTestNotificationTemplate();
            template.Tags = null;
            if (!platform.Equals("wns", StringComparison.OrdinalIgnoreCase))
            {
                template.Headers = null;
            }

            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            mock.ValidateTemplate(template, platform);
        }

        [Theory]
        [InlineData("wns")]
        [InlineData("apns")]
        [InlineData("gcm")]
        public void ValidateTemplate_DoesNotThrowOnEmptyTags(string platform)
        {
            NotificationTemplate template = new NotificationTemplate()
            {
                Body = "someString",
                Tags = new List<string>(),
                Headers = null
            };
            if (platform.Equals("wns", StringComparison.OrdinalIgnoreCase))
            {
                template.Headers = new Dictionary<string, string>
                {
                    { "header1", "value1" }
                };
            }

            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            mock.ValidateTemplate(template, platform);
        }

        [Theory]
        [InlineData("wns")]
        [InlineData("apns")]
        [InlineData("gcm")]
        public void ValidateTemplate_ThrowsOnNullTemplate(string platform)
        {
            bool testPass = false;
            NotificationTemplate template = null;
            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            try
            {
                mock.ValidateTemplate(template, platform);
            }
            catch
            {
                testPass = true;
            }

            Assert.True(testPass);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("12345678-1234-1234-1234-1234567890123")]
        [InlineData("abcdefg")]
        public void ValidateInstallationId_ThrowsOnNonGuid(string id)
        {
            bool testPass = false;
            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            try
            {
                mock.ValidateInstallationId(id);
            }
            catch
            {
                testPass = true;
            }

            Assert.True(testPass);
        }

        [Theory]
        [InlineData("12345678-1234-1234-1234-123456789012")]
        [InlineData("abcdef12-abcd-abcd-abcd-abcdef123456")]
        [InlineData("abcdefab-abcd-abcd-abcd-abcdefabcdef")]
        public void ValidateInstallationId_PassesOnGuid(string id)
        {
            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            mock.ValidateInstallationId(id);
        }

        [Fact]
        public void ValidateSecondaryTile_ThrowsOnNullPushChannel()
        {
            string pushChannel = null;
            bool testPass = false;
            NotificationSecondaryTile tile = MakeTestNotificationSecondaryTile();
            tile.PushChannel = pushChannel;
            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            try
            {
                mock.ValidateSecondaryTile(tile);
            }
            catch
            {
                testPass = true;
            }

            Assert.True(testPass);
        }

        [Fact]
        public void ValidateSecondaryTile_ThrowsOnEmptyPushChannel()
        {
            string pushChannel = string.Empty;
            bool testPass = false;
            NotificationSecondaryTile tile = MakeTestNotificationSecondaryTile();
            tile.PushChannel = pushChannel;
            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            try
            {
                mock.ValidateSecondaryTile(tile);
            }
            catch
            {
                testPass = true;
            }

            Assert.True(testPass);
        }

        [Fact]
        public void ValidateSecondaryTile_ThrowsOnNullTemplates()
        {
            bool testPass = false;
            NotificationSecondaryTile tile = MakeTestNotificationSecondaryTile();
            tile.Templates = null;
            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            try
            {
                mock.ValidateSecondaryTile(tile);
            }
            catch
            {
                testPass = true;
            }

            Assert.True(testPass);
        }

        [Fact]
        public void ValidateSecondaryTile_ThrowsOnEmptyTemplates()
        {
            bool testPass = false;
            NotificationSecondaryTile tile = MakeTestNotificationSecondaryTile();
            tile.Templates = new Dictionary<string, NotificationTemplate>();
            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            try
            {
                mock.ValidateSecondaryTile(tile);
            }
            catch
            {
                testPass = true;
            }

            Assert.True(testPass);
        }

        [Fact]
        public void ValidateSecondaryTile_ThrowsOnNullTemplateValue()
        {
            bool testPass = false;
            NotificationSecondaryTile tile = MakeTestNotificationSecondaryTile();

            tile.Templates = new Dictionary<string, NotificationTemplate>
            {
                {
                    "templateName",
                    null
                }
            };

            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            try
            {
                mock.ValidateSecondaryTile(tile);
            }
            catch
            {
                testPass = true;
            }

            Assert.True(testPass);
        }

        [Fact]
        public void ValidateSecondaryTile_ThrowsOnNull()
        {
            bool testPass = false;
            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();
            try
            {
                mock.ValidateSecondaryTile(null);
            }
            catch
            {
                testPass = true;
            }

            Assert.True(testPass);
        }

        [Theory]
        [InlineData(WnsPushChannelValid, WindowsNotificationServiceString, NotificationPlatform.Wns)]
        [InlineData(ApnsTokenValid, AppleNotificationServiceString, NotificationPlatform.Apns)]
        [InlineData(GcmToken, GcmNotificationServiceString, NotificationPlatform.Gcm)]
        public void CreateInstallationObject_ParsingIsConsistent(string pushChannel, string platformAsString, NotificationPlatform platformAsEnum)
        {
            string installationId = "12345678-1234-1234-1234-123456789012";
            string tileName = "myTile";
            string tileTemplateName = "templateNameTile";
            string tileTemplateBody = "myTemplateBodyTile";
            string tileTemplateHeaderName = "templateHeaderNameTile";
            string tileTemplateHeaderValue = "templateHeaderValueTile";
            string installationTemplateName = "installationTemplateName";
            string installationTemplateBody = "installationTemplateBody";
            string installationTemplateHeaderName = "installationTemplateHeaderName";
            string installationTemplateHeaderValue = "installationTemplateHeaderBody";

            // Arrange
            Dictionary<string, string> tileTemplateHeaders = new Dictionary<string, string>();
            tileTemplateHeaders[tileTemplateHeaderName] = tileTemplateHeaderValue;

            Dictionary<string, NotificationTemplate> tileTemplates = new Dictionary<string, NotificationTemplate>();
            tileTemplates[tileTemplateName] = new NotificationTemplate
            {
                Body = tileTemplateBody,
                Headers = tileTemplateHeaders
            };

            Dictionary<string, NotificationSecondaryTile> tiles = new Dictionary<string, NotificationSecondaryTile>();
            tiles[tileName] = new NotificationSecondaryTile
            {
                PushChannel = pushChannel,
                Tags = new List<string> { "tag1", "tag2" },
                Templates = tileTemplates
            };

            Dictionary<string, string> installationTemplateHeaders = new Dictionary<string, string>();
            installationTemplateHeaders[installationTemplateHeaderName] = installationTemplateHeaderValue;

            Dictionary<string, NotificationTemplate> installationTemplates = new Dictionary<string, NotificationTemplate>();
            installationTemplates[installationTemplateName] = new NotificationTemplate
            {
                Body = installationTemplateBody,
                Headers = installationTemplateHeaders
            };

            NotificationInstallation notificationInstallation = new NotificationInstallation
            {
                Platform = platformAsString,
                PushChannel = pushChannel,
                SecondaryTiles = tiles,
                Tags = new List<string> { "tagA", "tagB" },
                Templates = installationTemplates
            };

            notificationInstallation.InstallationId = installationId;

            NotificationInstallationsControllerMock mock = new NotificationInstallationsControllerMock();

            // Act
            Installation testInstallation = mock.CreateInstallation(notificationInstallation);

            // Assert
            Assert.NotNull(testInstallation);
            Assert.Equal(installationId, testInstallation.InstallationId);
            Assert.Equal(platformAsEnum, testInstallation.Platform);
            Assert.Equal(pushChannel, testInstallation.PushChannel);
            if (platformAsEnum == NotificationPlatform.Wns)
            {
                Assert.NotNull(testInstallation.SecondaryTiles);
                Assert.NotNull(testInstallation.SecondaryTiles[tileName]);
                Assert.Equal(pushChannel, testInstallation.SecondaryTiles[tileName].PushChannel);
                Assert.Equal(0, testInstallation.SecondaryTiles[tileName].Tags.Count);
                Assert.NotNull(testInstallation.SecondaryTiles[tileName].Templates);
                Assert.NotNull(testInstallation.SecondaryTiles[tileName].Templates[tileTemplateName]);

                // Tags were stripped within the tile
                Assert.Equal(tileTemplateBody, testInstallation.SecondaryTiles[tileName].Templates[tileTemplateName].Body);
                Assert.Equal(installationTemplateHeaderValue, testInstallation.Templates[installationTemplateName].Headers[installationTemplateHeaderName]);
            }
            else
            {
                Assert.Null(testInstallation.SecondaryTiles);
                Assert.Null(testInstallation.Templates[installationTemplateName].Headers);
            }

            Assert.NotNull(testInstallation.Templates[installationTemplateName]);
            Assert.Equal(installationTemplateBody, testInstallation.Templates[installationTemplateName].Body);

            // Tags were stripped within the template
            Assert.Equal(0, testInstallation.Templates[installationTemplateName].Tags.Count);
        }

        [Fact]
        public async Task PutInstallations_RegistersUserId_IfAuthenticated()
        {
            // Arrange
            NotificationInstallationsController controller = InitializeAuthenticatedController();
            HttpConfiguration config = controller.Configuration;
            NotificationInstallation notification = GetNotificationInstallation();

            // Mock the PushClient and capture the Installation that we send to NH for later verification
            Installation installation = null;
            var pushClientMock = new Mock<PushClient>(config);
            pushClientMock.Setup(p => p.CreateOrUpdateInstallationAsync(It.IsAny<Installation>()))
                .Returns<Installation>((inst) =>
                {
                    installation = inst;
                    return Task.FromResult(0);
                });
            pushClientMock.Setup(p => p.GetRegistrationsByTagAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(this.CreateCollectionQueryResult<RegistrationDescription>(Enumerable.Empty<RegistrationDescription>())));
            config.SetPushClient(pushClientMock.Object);

            // Act
            await controller.PutInstallation(notification.InstallationId, notification);

            // Assert
            Assert.NotNull(installation);
            Assert.Equal(notification.InstallationId, installation.InstallationId);
            Assert.Equal(notification.PushChannel, installation.PushChannel);
            Assert.Equal(1, installation.Tags.Count());
            Assert.Equal("_UserId:my:userid", installation.Tags[0]);
        }

        [Fact]
        public async Task PutInstallations_RegistersUserId_IfAuthenticatedAndTagsExist()
        {
            // Arrange
            NotificationInstallationsController controller = InitializeAuthenticatedController();
            HttpConfiguration config = controller.Configuration;
            NotificationInstallation notification = GetNotificationInstallation();

            // Mock the PushClient and capture the Installation that we send to NH for later verification
            Installation installation = null;
            var pushClientMock = new Mock<PushClient>(config);
            pushClientMock.Setup(p => p.CreateOrUpdateInstallationAsync(It.IsAny<Installation>()))
                .Returns<Installation>((inst) =>
                {
                    installation = inst;
                    return Task.FromResult(0);
                });
            pushClientMock.Setup(p => p.GetRegistrationsByTagAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(() =>
                {
                    RegistrationDescription[] registrations = new RegistrationDescription[]
                    {
                        new WindowsRegistrationDescription("http://someuri", new string[] { "tag1", "tag2", "_UserId:something" })
                    };
                    return Task.FromResult(this.CreateCollectionQueryResult<RegistrationDescription>(registrations));
                });
            config.SetPushClient(pushClientMock.Object);

            // Act
            await controller.PutInstallation(notification.InstallationId, notification);

            // Assert
            Assert.NotNull(installation);
            Assert.Equal(notification.InstallationId, installation.InstallationId);
            Assert.Equal(notification.PushChannel, installation.PushChannel);
            Assert.Equal(3, installation.Tags.Count());

            // verify the existing userid is removed and replaced with current
            Assert.Equal("_UserId:my:userid", installation.Tags[2]);
        }

        private static NotificationInstallationsController InitializeAuthenticatedController()
        {
            string signingKey = "6523e58bc0eec42c31b9635d5e0dfc23b6d119b73e633bf3a5284c79bb4a1ede"; // SHA256 hash of 'secret_key'
            HttpConfiguration config = new HttpConfiguration();
            AppServiceTokenHandler handler = new AppServiceTokenHandler(config);
            string url = "http://localhost";
            Claim[] claims = new Claim[] { new Claim("sub", "my:userid") };

            // Create a token the same way as App Service Authentication
            JwtSecurityToken token = AppServiceLoginHandler.CreateToken(claims, signingKey, url, url, TimeSpan.FromDays(10));

            // Validate that token and parse it into a ClaimsPrincipal the same way as App Service Authentication
            ClaimsPrincipal user = null;
            string[] validIssAud = new[] { url };
            handler.TryValidateLoginToken(token.RawData, signingKey, validIssAud, validIssAud, out user);

            NotificationInstallationsController controller = new NotificationInstallationsController();
            controller.Configuration = config;
            controller.Request = new HttpRequestMessage();
            controller.User = user;

            return controller;
        }

        private static NotificationInstallation GetNotificationInstallation()
        {
            return new NotificationInstallation
            {
                InstallationId = Guid.NewGuid().ToString(),
                PushChannel = Guid.NewGuid().ToString(),
                Platform = "wns"
            };
        }

        private static NotificationSecondaryTile MakeTestNotificationSecondaryTile()
        {
            NotificationSecondaryTile tile = new NotificationSecondaryTile
            {
                PushChannel = "someChannel",
                Tags = new List<string>
                {
                    "tag1",
                    "tag2"
                },
                Templates = new Dictionary<string, NotificationTemplate>
                {
                    {
                        "templateName", new NotificationTemplate
                        {
                            Body = "someBody",
                            Headers = new Dictionary<string, string>
                            {
                                { "header1", "value1" }
                            },
                            Tags = new List<string>
                            {
                                "tagA",
                                "tagB"
                            }
                        }
                    }
                },
            };

            return tile;
        }

        private static NotificationTemplate MakeTestNotificationTemplate()
        {
            NotificationTemplate template = new NotificationTemplate()
            {
                Body = "someString",
                Tags = new List<string>
                {
                    "tag1",
                    "tag2"
                },
                Headers = new Dictionary<string, string>
                {
                    {
                        "headerName",
                        "headerValue"
                    }
                }
            };

            return template;
        }

        // CollectionQueryResult is internal so we need to use reflection to make one for mocking purposes.
        private CollectionQueryResult<T> CreateCollectionQueryResult<T>(IEnumerable<T> items) where T : EntityDescription
        {
            var constructor = typeof(CollectionQueryResult<T>)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single();
            return constructor.Invoke(new object[] { items, null }) as CollectionQueryResult<T>;
        }

        public class NotificationInstallationsControllerMock : NotificationInstallationsController
        {
            public NotificationInstallationsControllerMock()
                : base()
            {
            }
        }
    }
}