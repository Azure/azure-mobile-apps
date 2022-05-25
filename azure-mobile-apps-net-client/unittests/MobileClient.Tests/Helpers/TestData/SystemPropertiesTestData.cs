// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;

namespace MobileClient.Tests.Helpers
{
    public static class SystemPropertiesTestData
    {
        public static MobileServiceSystemProperties[] SystemProperties = new MobileServiceSystemProperties[] {
            MobileServiceSystemProperties.None,
            MobileServiceSystemProperties.All,
            MobileServiceSystemProperties.CreatedAt | MobileServiceSystemProperties.UpdatedAt | MobileServiceSystemProperties.Version, MobileServiceSystemProperties.Deleted,
            MobileServiceSystemProperties.CreatedAt | MobileServiceSystemProperties.UpdatedAt | MobileServiceSystemProperties.Version,
            MobileServiceSystemProperties.CreatedAt | MobileServiceSystemProperties.UpdatedAt,
            MobileServiceSystemProperties.UpdatedAt | MobileServiceSystemProperties.Version,
            MobileServiceSystemProperties.Version | MobileServiceSystemProperties.CreatedAt,
            MobileServiceSystemProperties.CreatedAt,
            MobileServiceSystemProperties.UpdatedAt,
            MobileServiceSystemProperties.Version,
            MobileServiceSystemProperties.Deleted
        };

        public static string[] ValidSystemProperties = new string[] {
            "createdAt",
            "updatedAt",
            "version",
            "deleted",
            "CreatedAt",
        };

        public static string[] NonSystemProperties = new string[] {
            "someProperty",
            "__createdAt",
            "__updatedAt",
            "__version",
            "__deleted",
            "_createdAt",
            "_updatedAt",
            "_version",
            "X__createdAt"
        };
    }
}