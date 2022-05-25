// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    public class QueryResultTests
    {
        private readonly List<int> results;
        private readonly long count;

        private QueryResult queryResult;

        public QueryResultTests()
        {
            this.count = Int64.MaxValue;
            this.results = new List<int>() { 1, 2, 3 };
            this.queryResult = new QueryResult(this.results, this.count);
        }

        [Fact]
        public void Results_Roundtrips()
        {
            List<int> roundtrips = new List<int>();
            PropertyAssert.Roundtrips(this.queryResult, r => r.Results, PropertySetter.NullThrows, defaultValue: this.results, roundtripValue: roundtrips);
        }

        [Fact]
        public void Count_Roundtrips()
        {
            long roundtrips = 32L * 1024 * 1024 * 1024;
            PropertyAssert.Roundtrips(this.queryResult, r => r.Count, PropertySetter.NullRoundtrips, defaultValue: this.count, roundtripValue: roundtrips,
                minLegalValue: 0, illegalLowerValue: -1);
        }

        [Fact]
        public void Serialization_IsConsistent()
        {
            SerializationAssert.VerifySerialization(this.queryResult, "{\"results\":[1,2,3],\"count\":9223372036854775807}");
        }
    }
}
