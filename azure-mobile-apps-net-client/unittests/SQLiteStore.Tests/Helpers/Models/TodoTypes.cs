// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace SQLiteStore.Tests.Helpers
{
    // Repurpose the random empty table
    [DataTable("test_table")]
    public class ToDo
    {
        public long Id { get; set; }

        [JsonProperty(PropertyName = "col1")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "col5")]
        public bool Complete { get; set; }
    }

    [DataTable("stringId_test_table")]
    public class ToDoWithStringId
    {
        public string Id { get; set; }

        public string String { get; set; }
    }

    [DataTable("test_table")]
    public class ToDoWithStringIdAgainstIntIdTable
    {
        public string Id { get; set; }

        public string String { get; set; }
    }

    [DataTable("test_table")]
    public class ToDoWithIntId
    {
        public long Id { get; set; }

        public string String { get; set; }
    }

    [DataTable("stringId_test_table")]
    public class ToDoWithSystemPropertiesType
    {
        public string Id { get; set; }

        public string String { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }

        [Version]
        public String Version { get; set; }

        [Deleted]
        public bool Deleted { get; set; }

        public ToDoWithSystemPropertiesType()
        {
        }

        public ToDoWithSystemPropertiesType(string id)
        {
            this.Id = id;
        }
    }

    public class TypeWithArray
    {
        public string Id { get; set; }

        [JsonProperty("values")]
        public List<string> Values { get; set; }

    }
}
