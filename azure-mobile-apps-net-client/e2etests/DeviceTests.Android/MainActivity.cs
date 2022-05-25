// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using DeviceTests.Shared.Tests;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnitTests.HeadlessRunner;
using Xunit.Runners.UI;

namespace DeviceTests.Android
{
    [Activity(
        Name = "com.microsoft.azure.devicetests.MainActivity",
        Label = "@string/app_name",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : RunnerActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            Xamarin.Essentials.Platform.Init(this, bundle);

            var hostIp = Intent.Extras?.GetString("HOST_IP", null);
            var hostPort = Intent.Extras?.GetInt("HOST_PORT", 63559) ?? 63559;

            if (!string.IsNullOrEmpty(hostIp))
            {
                // Run the headless test runner for CI
                Task.Run(() =>
                {
                    return UnitTests.HeadlessRunner.Tests.RunAsync(new TestOptions
                    {
                        Assemblies = new List<Assembly> { typeof(Shared_Tests).Assembly },
                        NetworkLogHost = hostIp,
                        NetworkLogPort = hostPort,
                        Format = TestResultsFormat.XunitV2,
                    });
                });
            }

            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());
            AddTestAssembly(typeof(Shared_Tests).Assembly);
            AddExecutionAssembly(typeof(Shared_Tests).Assembly);

            base.OnCreate(bundle);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}