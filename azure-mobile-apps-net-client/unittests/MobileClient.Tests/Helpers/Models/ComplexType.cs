// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace MobileClient.Tests.Helpers
{

    public class ComplexType
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public MissingIdType Child { get; set; }
    }
}