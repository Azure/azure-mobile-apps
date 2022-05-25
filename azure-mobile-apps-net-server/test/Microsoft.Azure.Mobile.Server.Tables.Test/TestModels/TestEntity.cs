// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Azure.Mobile.Server.TestModels
{
    public class TestEntity : EntityData
    {
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int", Justification = "Fine for this test class.")]
        public int IntValue { get; set; }

        public string StringValue { get; set; }

        public bool BooleanValue { get; set; }
    }

    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "These classes are variants of each other.")]
    public class TestEntitySimple
    {
        public string Id { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int", Justification = "Fine for this test class.")]
        public int IntValue { get; set; }

        public string StringValue { get; set; }

        public bool BooleanValue { get; set; }
    }

    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "These classes are variants of each other.")]
    public class TestInvalidEntity
    {
        public string Id { get; set; }

        public string StringValue { get; set; }

        public string UnknownProperty { get; set; }
    }
}
