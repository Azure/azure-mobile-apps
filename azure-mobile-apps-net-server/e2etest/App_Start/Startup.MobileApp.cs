// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using AutoMapper;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Owin.Extensions;
using Owin;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;
using ZumoE2EServerApp.Utils;

namespace ZumoE2EServerApp
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.EnableSystemDiagnosticsTracing();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            config.MapHttpAttributeRoutes();

#pragma warning disable 618
            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .AddPushNotifications()
                .ApplyTo(config);
#pragma warning restore 618

            var cors = new EnableCorsAttribute("http://localhost:1076", "*", "*");
            config.EnableCors(cors);

            IMobileAppSettingsProvider settingsProvider = config.GetMobileAppSettingsProvider();
            MobileAppSettingsDictionary settings = settingsProvider.GetMobileAppSettings();
            IDictionary environmentVariables = Environment.GetEnvironmentVariables();
            foreach (var conKey in settings.Connections.Keys.ToArray())
            {
                var envKey = environmentVariables.Keys.OfType<string>().FirstOrDefault(p => p == conKey);
                if (!string.IsNullOrEmpty(envKey))
                {
                    settings.Connections[conKey].ConnectionString = (string)environmentVariables[envKey];
                }
            }

            foreach (var setKey in settings.Keys.ToArray())
            {
                var envKey = environmentVariables.Keys.OfType<string>().FirstOrDefault(p => p == setKey);
                if (!string.IsNullOrEmpty(envKey))
                {
                    settings[setKey] = (string)environmentVariables[envKey];
                }
            }

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<IntIdRoundTripTableItem, IntIdRoundTripTableItemDto>()
                   .ForMember(dto => dto.Id, map => map.MapFrom(db => MySqlFuncs.LTRIM(MySqlFuncs.StringConvert(db.Id))));
                cfg.CreateMap<IntIdRoundTripTableItemDto, IntIdRoundTripTableItem>()
                   .ForMember(db => db.Id, map => map.MapFrom(dto => MySqlFuncs.LongParse(dto.Id)));

                cfg.CreateMap<IntIdMovie, IntIdMovieDto>()
                   .ForMember(dto => dto.Id, map => map.MapFrom(db => MySqlFuncs.LTRIM(MySqlFuncs.StringConvert(db.Id))));
                cfg.CreateMap<IntIdMovieDto, IntIdMovie>()
                   .ForMember(db => db.Id, map => map.MapFrom(dto => MySqlFuncs.LongParse(dto.Id)));
            });

            Database.SetInitializer(new DbInitializer());

            // Uncomment for local debugging
            // app.UseAppServiceAuthentication(config, AppServiceAuthenticationMode.LocalOnly);
            app.UseWebApi(config);
            app.UseStageMarker(PipelineStage.MapHandler);
        }

        private class DbInitializer : CreateDatabaseIfNotExists<SDKClientTestContext>
        {
            protected override void Seed(SDKClientTestContext context)
            {
                foreach (var movie in TestMovies.GetTestMovies())
                {
                    context.Set<Movie>().Add(movie);
                }
                foreach (var movie in TestMovies.TestIntIdMovies)
                {
                    context.Set<IntIdMovie>().Add(movie);
                }

                base.Seed(context);
            }
        }
    }
}