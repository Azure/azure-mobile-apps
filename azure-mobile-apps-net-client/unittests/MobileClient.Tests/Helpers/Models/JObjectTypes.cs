// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Newtonsoft.Json.Linq;

namespace MobileClient.Tests.Helpers
{
    public static class JObjectTypes
    {
        public static JObject GetObjectWithAllTypes()
        {
            return new JObject()
            {
                { "Object", new JObject() },
                { "Array", new JArray() },
                { "Integer", 0L },
                { "Float", 0f },
                { "String", String.Empty },
                { "Boolean", false },
                { "Date", DateTime.MinValue },
                { "Bytes", new byte[0] },
                { "Guid", Guid.Empty },
                { "TimeSpan", TimeSpan.Zero }
            };
        }
    }
}