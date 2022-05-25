using DeviceTests.Shared.Tests;
using Foundation;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UIKit;
using UnitTests.HeadlessRunner;

namespace DeviceTests.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register(nameof(AppDelegate))]
    public class AppDelegate : Xunit.Runner.RunnerAppDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // Invoke the headless test runner if a config was specified
            var testCfg = System.IO.File.ReadAllText("tests.cfg")?.Split(':');
            if (testCfg != null && testCfg.Length > 1)
            {
                var ip = testCfg[0];
                if (int.TryParse(testCfg[1], out var port))
                {
                    // Run the headless test runner for CI
                    Task.Run(() =>
                    {
                        return Tests.RunAsync(new TestOptions
                        {
                            Assemblies = new List<Assembly> { typeof(Shared_Tests).Assembly },
                            NetworkLogHost = ip,
                            NetworkLogPort = port,
                            Format = TestResultsFormat.XunitV2,
                        });
                    });
                }
            }

            // We need this to ensure the execution assembly is part of the app bundle
            AddExecutionAssembly(typeof(Shared_Tests).Assembly);

            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());

            AddTestAssembly(typeof(Shared_Tests).Assembly);

            // otherwise you need to ensure that the test assemblies will
            // become part of the app bundle
            //    AddTestAssembly(typeof(PortableTests).Assembly);

            return base.FinishedLaunching(app, options);
        }
    }
}