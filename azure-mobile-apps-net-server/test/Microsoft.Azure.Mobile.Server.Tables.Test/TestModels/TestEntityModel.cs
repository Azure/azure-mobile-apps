// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Azure.Mobile.Server.TestModels
{
    public class TestEntityModel : EntityData
    {
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int", Justification = "Fine for this test class.")]
        public int IntValue { get; set; }

        public string StringValue { get; set; }

        public bool BooleanValue { get; set; }
    }
}
