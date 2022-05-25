// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Data.Entity;
using System.Data.SqlClient;

namespace TestUtilities
{
    public static class TestHelper
    {
        public const string UnitTestDatabaseConnectionName = "MS_TableConnectionString";

        public static void ResetTestDatabase(string nameOrConnectionString = UnitTestDatabaseConnectionName)
        {
            // Close all connections to DB from this app domain first
            SqlConnection.ClearAllPools();

            CrossDomainTestHelper helper = CreateIsolatedHelperInstance();

            helper.ResetTestDatabase(nameOrConnectionString);
        }

        /// <summary>
        /// Some test initialization is required to be done in a separate app domain. For example,
        /// EntityFramework's Database.Delete static which we use to clean up test DB schema cannot
        /// be executed in the same domain as our other runtime EF initialization. So this method
        /// creates a separate app domain and loads a test helper instance in it.
        /// </summary>
        private static CrossDomainTestHelper CreateIsolatedHelperInstance()
        {
            AppDomainSetup domainSetup = new AppDomainSetup()
            {
                ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                ApplicationName = AppDomain.CurrentDomain.SetupInformation.ApplicationName,
                LoaderOptimization = LoaderOptimization.MultiDomainHost
            };

            AppDomain childDomain = AppDomain.CreateDomain("Test Isolation Domain", null, domainSetup);

            // create the proxy in a new app domain
            CrossDomainTestHelper helper = (CrossDomainTestHelper)childDomain.CreateInstanceAndUnwrap(
                typeof(CrossDomainTestHelper).Assembly.FullName, typeof(CrossDomainTestHelper).FullName);

            return helper;
        }

        private class CrossDomainTestHelper : MarshalByRefObject
        {
            public void ResetTestDatabase(string nameOrConnectionString)
            {
                Database.Delete(nameOrConnectionString);
            }
        }
    }
}
