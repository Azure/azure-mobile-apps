// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


namespace MobileClient.Tests.Helpers
{
    public class TypeWithConstructor
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public TypeWithConstructor(string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}