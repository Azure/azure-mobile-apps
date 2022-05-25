// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class CompositeTableKeyTests
    {
        public static TheoryDataCollection<string, string[]> ParseValidKeyValues
        {
            get
            {
                return new TheoryDataCollection<string, string[]>
                {
                    { string.Empty, new string[] { } },
                    { "'k1'", new string[] { "k1" } },
                    { "'k1','k2'", new string[] { "k1", "k2" } },
                    { "'안녕하세요','세계'", new string[] { "안녕하세요", "세계" } },
                    { "'💩','🌠'", new string[] { "💩", "🌠" } },
                    { "':',';'", new string[] { ":", ";" } },
                    { "'k,1'", new string[] { "k,1" } },
                    { "'k,,1'", new string[] { "k,,1" } },
                    { "'',''", new string[] { string.Empty, string.Empty } },
                    { "','", new string[] { "," } },
                    { "',,,'", new string[] { ",,," } },
                    { "'o'brian','don''t','''''", new string[] { "o'brian", "don''t", "'''" } },
                };
            }
        }

        public static TheoryDataCollection<string> ParseInvalidKeyValues
        {
            get
            {
                return new TheoryDataCollection<string>
                {
                    { "k1" },
                    { "k1,k2" },
                    { "'k1',k2" },
                    { "💩,🌠" },
                    { "'" },
                    { "'안녕하세요세계" },
                    { "'💩" },
                    { "'k1" },
                    { ",,," },
                    { "',,," },
                    { "k1,',,," },
                    { "," },
                    { ",,," },
                    { "k1'" },
                    { "k'1" },
                    { "'k1', 'k2'" },
                    { "'" }
                };
            }
        }

        public static TheoryDataCollection<string[], string> GenerateKeys
        {
            get
            {
                return new TheoryDataCollection<string[], string>
                {
                    { new string[] { }, string.Empty },
                    { new string[] { string.Empty }, "''" },
                    { new string[] { "a", "b", "c" }, "'a','b','c'" },
                    { new string[] { "," }, "','" },
                    { new string[] { "'" }, "'''" },
                    { new string[] { "'''" }, "'''''" },
                };
            }
        }

        public static TheoryDataCollection<string[]> KeysRoundtrip
        {
            get
            {
                return new TheoryDataCollection<string[]>
                {
                    new string[] { },
                    new string[] { string.Empty },
                    new string[] { "a", "b", "c" },
                    new string[] { "," },
                    new string[] { "'" },
                    new string[] { "'''" },
                    new string[] { "k1" },
                    new string[] { "k1" },
                    new string[] { "k1", "k2" },
                    new string[] { "안녕하세요", "세계" },
                    new string[] { "💩", "🌠" },
                    new string[] { ":", ";" },
                    new string[] { "k1", "k2" },
                    new string[] { "k1", "k2" },
                    new string[] { "k1", " 'k2'" },
                    new string[] { "k", "1" },
                    new string[] { "k,1" },
                    new string[] { "k,,1" },
                    new string[] { "k'1" },
                    new string[] { "k1'" },
                    new string[] { string.Empty, string.Empty },
                    new string[] { "," },
                    new string[] { ",,," },
                };
            }
        }

        [Theory]
        [MemberData("ParseValidKeyValues")]
        public void Parse_SucceedsOnValidKeys(string key, string[] expectedSubkeys)
        {
            // Act
            CompositeTableKey tableKey = CompositeTableKey.Parse(key);

            // Assert
            Assert.Equal(expectedSubkeys, tableKey.Segments);
        }

        [Theory]
        [MemberData("ParseInvalidKeyValues")]
        public void Parse_ThrowsOnInvalidKeys(string key)
        {
            Assert.Throws<ArgumentException>(() => CompositeTableKey.Parse(key));
        }

        [Theory]
        [MemberData("ParseValidKeyValues")]
        public void TryParse_SucceedsOnValidKeys(string key, string[] expectedSegments)
        {
            // Act
            CompositeTableKey tableKey;
            bool result = CompositeTableKey.TryParse(key, out tableKey);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedSegments, tableKey.Segments);
        }

        [Theory]
        [MemberData("ParseInvalidKeyValues")]
        public void TryParse_FailsOnInvalidKeys(string key)
        {
            // Act
            CompositeTableKey tableKey;
            bool result = CompositeTableKey.TryParse(key, out tableKey);

            // Assert
            Assert.False(result);
            Assert.Null(tableKey);
        }

        [Theory]
        [MemberData("GenerateKeys")]
        public void ToString_GeneratesCorrectTableKey(string[] segments, string expectedKey)
        {
            // Arrange
            CompositeTableKey tableKey = new CompositeTableKey(segments);

            // Act
            string actualTableKey = tableKey.ToString();

            // Assert
            Assert.Equal(expectedKey, actualTableKey);
        }

        [Theory]
        [MemberData("KeysRoundtrip")]
        public void ToStringAndParse_Roundtrips(string[] expectedSegments)
        {
            // Arrange
            CompositeTableKey tableKey = new CompositeTableKey(expectedSegments);

            // Act
            string actualTableKey = tableKey.ToString();
            IEnumerable<string> actualSubkeys = CompositeTableKey.Parse(actualTableKey).Segments;

            // Assert
            Assert.Equal(expectedSegments, actualSubkeys);
        }
    }
}
