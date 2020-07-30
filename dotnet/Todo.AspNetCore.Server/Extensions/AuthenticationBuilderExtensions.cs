using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Todo.AspNetCore.Server.Extensions
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddCookieAuthentication(this IServiceCollection services)
        {
            return services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie();
        }

        /// <summary>
        /// Configures Facebook Authentication
        /// </summary>
        /// <param name="builder">The Authentication Builder</param>
        /// <param name="config">The Facebook Configuration</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddFacebookWithConfig(this AuthenticationBuilder builder, IConfigurationSection config)
        {
            if (config != null)
            {
                var appId = config["AppId"];
                var appSecret = config["AppSecret"];

                if (appId != null && !appId.StartsWith("-") && appSecret != null && !appSecret.StartsWith("-"))
                {
                    return builder.AddFacebook(config =>
                    {
                        config.AppId = appId;
                        config.AppSecret = appSecret;
                        config.SaveTokens = true;
                    });
                }
            }
            return builder;
        }

        /// <summary>
        /// Configures Google Authentication
        /// </summary>
        /// <param name="builder">The Authentication Builder</param>
        /// <param name="config">The Google Configuration</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddGoogleWithConfig(this AuthenticationBuilder builder, IConfigurationSection config)
        {
            if (config != null)
            {
                var clientId = config["ClientId"];
                var clientSecret = config["ClientSecret"];

                if (clientId != null && !clientId.StartsWith("-") && clientSecret != null && !clientSecret.StartsWith("-"))
                {
                    return builder.AddGoogle(config =>
                    {
                        config.ClientId = clientId;
                        config.ClientSecret = clientSecret;
                        config.SaveTokens = true;
                    });
                }
            }
            return builder;
        }

        /// <summary>
        /// Configures MSA Authentication
        /// </summary>
        /// <param name="builder">The Authentication Builder</param>
        /// <param name="config">The MSA Configuration</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddMicrosoftAccountWithConfig(this AuthenticationBuilder builder, IConfigurationSection config)
        {
            if (config != null)
            {
                var clientId = config["ClientId"];
                var clientSecret = config["ClientSecret"];

                if (clientId != null && !clientId.StartsWith("-") && clientSecret != null && !clientSecret.StartsWith("-"))
                {
                    return builder.AddMicrosoftAccount(config =>
                    {
                        config.ClientId = clientId;
                        config.ClientSecret = clientSecret;
                        config.SaveTokens = true;
                    });
                }
            }
            return builder;
        }

        /// <summary>
        /// Configures Apple Authentication
        /// </summary>
        /// <param name="builder">The Authentication Builder</param>
        /// <param name="config">The Apple Configuration</param>
        /// <param name="env">The web hosting environment</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddAppleWithConfig(this AuthenticationBuilder builder, IConfigurationSection config, IWebHostEnvironment env)
        {
            if (config != null)
            {
                var clientId = config["ClientId"];
                var keyId = config["KeyId"];
                var teamId = config["TeamId"];

                if (clientId != null && !clientId.StartsWith("-"))
                {
                    return builder.AddApple(config =>
                    {
                        config.ClientId = clientId;
                        config.KeyId = keyId;
                        config.TeamId = teamId;
                        config.UsePrivateKey(key => env.ContentRootFileProvider.GetFileInfo($"AuthKey_{key}.p8"));
                        config.SaveTokens = true;
                    });
                }
            }
            return builder;
        }
    }
}
