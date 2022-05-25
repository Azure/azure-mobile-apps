// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using MobileClient.Tests.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace MobileClient.Tests.Table.Serialization
{
    public class MobileServiceSerializer_Test
    {
        static readonly string MinDateTimeSerializedToJson;

        static MobileServiceSerializer_Test()
        {
            var settings = new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'"
            };
            MinDateTimeSerializedToJson = JsonConvert.SerializeObject(default(DateTime).ToUniversalTime(), settings);
        }

        MobileServiceSerializer DefaultSerializer { get; } = new MobileServiceSerializer();

        [Fact]
        public void GetId()
        {
            List<Tuple<object, object>> testCases = new List<Tuple<object, object>>() {
                new Tuple<object, object>(new BoolType() { Bool = true }, 0),
                new Tuple<object, object>(new BoolType() { Bool = false, Id = 15 }, 15),
                new Tuple<object, object>(JToken.Parse("{\"id\":0}"), 0L),
                new Tuple<object, object>(JToken.Parse("{\"id\":25}"), 25L),
                new Tuple<object, object>(JToken.Parse("{\"id\":0.0}"), 0.0),
                new Tuple<object, object>(JToken.Parse("{\"id\":25.0}"), 25.0),
                new Tuple<object, object>(JToken.Parse("{}"), null),
                new Tuple<object, object>(JToken.Parse("{\"id\":null}"), null),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(BoolType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                var actual = DefaultSerializer.GetId(input, allowDefault: true);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void GetIdNegative()
        {
            List<object> testCases = new List<object>()
            {
                "a string",
                new MissingIdType(),
                new PocoType[1],
                JToken.Parse("25"),
                JToken.Parse("[25]")
            };

            foreach (var testCase in testCases)
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(testCase.GetType());
                    DefaultSerializer.GetId(testCase);
                });
            }
        }

        [Fact]
        public void HasDefaultId()
        {
            List<Tuple<object, bool>> testCases = new List<Tuple<object, bool>>() {
                new Tuple<object, bool>(new BoolType() { Bool = true }, true),
                new Tuple<object, bool>(new BoolType() { Bool = false, Id = 15 }, false),
                new Tuple<object, bool>(JToken.Parse("{\"id\":0}"), true),
                new Tuple<object, bool>(JToken.Parse("{\"id\":25}"), false),
                new Tuple<object, bool>(JToken.Parse("{\"id\":0.0}"), true),
                new Tuple<object, bool>(JToken.Parse("{\"id\":25.0}"), false),
                new Tuple<object, bool>(JToken.Parse("{}"), true),
                new Tuple<object, bool>(JToken.Parse("{\"id\":null}"), true),
            };

            foreach (var testCase in testCases)
            {
                // Need to ensure that the type is registered as a table to force the id property check
                DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(testCase.Item1.GetType());

                var input = testCase.Item1;
                var expected = testCase.Item2;

                var id = DefaultSerializer.GetId(input, allowDefault: true);
                var actual = MobileServiceSerializer.IsDefaultId(id);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void HasDefaultIdNegative()
        {
            List<object> testCases = new List<object>()
            {
                "a string",
                new MissingIdType(),
                new PocoType[1],
                JToken.Parse("25"),
                JToken.Parse("[25]")
            };

            foreach (var testCase in testCases)
            {
                Assert.Throws<InvalidOperationException>(() => 
                {
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(testCase.GetType());
                    var id = DefaultSerializer.GetId(testCase);
                    MobileServiceSerializer.IsDefaultId(id);
                });
            }
        }

        [Fact]
        public void SetIdToDefault()
        {
            List<Tuple<object, object>> testCases = new List<Tuple<object, object>>() {
                new Tuple<object, object>(new BoolType() { Bool = true, Id = 0 }, 0),
                new Tuple<object, object>(new BoolType() { Bool = false, Id = 15 }, 0),
            };

            foreach (var testCase in testCases)
            {
                // Need to ensure that the type is registered as a table to force the id property check
                DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(testCase.Item1.GetType());

                var input = testCase.Item1;
                var expected = testCase.Item2;

                DefaultSerializer.SetIdToDefault(input);
                var actual = DefaultSerializer.GetId(input, allowDefault: true);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void IgnoreNullValuesCustomSerialization()
        {
            MobileServiceSerializer serializer = new MobileServiceSerializer();
            serializer.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            // Need to ensure that the type is registered as a table to force the id property check
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoType));

            PocoType pocoType = new PocoType();

            string actual = serializer.Serialize(pocoType).ToString();
            Assert.Equal("{}", actual);
        }

        [Fact]
        public void IndentedFormattingCustomSerialization()
        {
            MobileServiceSerializer serializer = new MobileServiceSerializer();
            serializer.SerializerSettings.Formatting = Formatting.Indented;

            // Need to ensure that the type is registered as a table to force the id property check
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoType));

            PocoType pocoType = new PocoType();

            string actual = serializer.Serialize(pocoType).ToString();
            Assert.Equal("{" + Environment.NewLine + "  \"PublicField\": null," + Environment.NewLine + "  \"PublicProperty\": null" + Environment.NewLine + "}", actual);
        }

        [Fact]
        public void CamelCaseCustomSerialization()
        {
            MobileServiceSerializer serializer = new MobileServiceSerializer();
            serializer.SerializerSettings.CamelCasePropertyNames = true;

            // Need to ensure that the type is registered as a table to force the id property check
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoType));

            PocoType pocoType = new PocoType();

            string actual = serializer.Serialize(pocoType).ToString(Formatting.None);
            Assert.Equal("{\"publicField\":null,\"publicProperty\":null}", actual);
        }

        [Fact]
        public void BoolSerialization()
        {
            List<Tuple<BoolType, string>> testCases = new List<Tuple<BoolType, string>>() {
                new Tuple<BoolType, string>(new BoolType() { Bool = true }, "{\"Bool\":true}"),
                new Tuple<BoolType, string>(new BoolType() { Bool = false }, "{\"Bool\":false}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(BoolType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void BoolDeserialization()
        {
            List<Tuple<BoolType, string>> testCases = new List<Tuple<BoolType, string>>() {
                new Tuple<BoolType, string>(new BoolType() { Bool = true }, "{\"Bool\":true}"),
                new Tuple<BoolType, string>(new BoolType() { Bool = true }, "{\"Bool\":\"true\"}"),
                new Tuple<BoolType, string>(new BoolType() { Bool = true }, "{\"Bool\":1}"),
                new Tuple<BoolType, string>(new BoolType() { Bool = false }, "{}"),
                new Tuple<BoolType, string>(new BoolType() { Bool = false }, "{\"Bool\":false}"),
                new Tuple<BoolType, string>(new BoolType() { Bool = false }, "{\"Bool\":\"false\"}"),
                new Tuple<BoolType, string>(new BoolType() { Bool = false }, "{\"Bool\":0}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(BoolType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                BoolType actual = new BoolType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.Bool, expected.Bool);

                actual = new BoolType();
                actual.Bool = false;
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.Bool, expected.Bool);

                JArray json = JToken.Parse("[" + input + "]") as JArray;
                actual = DefaultSerializer.Deserialize<BoolType>(json).FirstOrDefault();

                Assert.Equal(actual.Bool, expected.Bool);

                actual = (BoolType)DefaultSerializer.Deserialize<BoolType>(input);

                Assert.Equal(actual.Bool, expected.Bool);
            }
        }

        [Fact]
        public void BoolDeserializationNegative()
        {
            List<string> testCases = new List<string>() {
                "{\"Bool\":\"I can't be parsed\"}"
            };


            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase);

                BoolType actual = new BoolType();
                Exception actualError = Assert.ThrowsAny<Exception>(() =>
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(BoolType));

                    DefaultSerializer.Deserialize(input, actual);
                });
                Assert.Equal("Error converting value \"I can't be parsed\" to type 'System.Boolean'. Path 'Bool', line 2, position 29.", actualError.Message);
            }
        }

        [Fact]
        public void ByteSerialization()
        {
            List<Tuple<ByteType, string>> testCases = new List<Tuple<ByteType, string>>() {
                new Tuple<ByteType, string>(new ByteType() { Byte = 0 }, "{\"Byte\":0}"),
                new Tuple<ByteType, string>(new ByteType() { Byte = 1 }, "{\"Byte\":1}"),
                new Tuple<ByteType, string>(new ByteType() { Byte = Byte.MaxValue }, "{\"Byte\":255}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(ByteType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void ByteDeserialization()
        {
            List<Tuple<ByteType, string>> testCases = new List<Tuple<ByteType, string>>() {
                new Tuple<ByteType, string>(new ByteType() { Byte = 0 }, "{}"),
                new Tuple<ByteType, string>(new ByteType() { Byte = 0 }, "{\"Byte\":null}"),
                new Tuple<ByteType, string>(new ByteType() { Byte = 0 }, "{\"Byte\":0}"),
                new Tuple<ByteType, string>(new ByteType() { Byte = 1 }, "{\"Byte\":1}"),
                new Tuple<ByteType, string>(new ByteType() { Byte = 0 }, "{\"Byte\":false}"),
                new Tuple<ByteType, string>(new ByteType() { Byte = 1 }, "{\"Byte\":true}"),
                new Tuple<ByteType, string>(new ByteType() { Byte = Byte.MaxValue }, "{\"Byte\":255}"),
                new Tuple<ByteType, string>(new ByteType() { Byte = Byte.MaxValue }, "{\"Byte\":\"255\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(ByteType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                ByteType actual = new ByteType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.Byte, expected.Byte);

                if (testCase.Item2 != "{}")
                {
                    actual = new ByteType();
                    actual.Byte = 15;
                    DefaultSerializer.Deserialize(input, actual);
                }

                Assert.Equal(actual.Byte, expected.Byte);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<ByteType>(json).FirstOrDefault();

                Assert.Equal(actual.Byte, expected.Byte);

                actual = (ByteType)DefaultSerializer.Deserialize<ByteType>(input);

                Assert.Equal(actual.Byte, expected.Byte);
            }
        }

        [Fact]
        public void ByteDeserializationNegative()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {
                new Tuple<string, string>("{\"Byte\":256}", "Error converting value 256 to type 'System.Byte'. Path 'Byte', line 2, position 13."),
                new Tuple<string, string>("{\"Byte\":\"256\"}","Error converting value \"256\" to type 'System.Byte'. Path 'Byte', line 2, position 15."),
                new Tuple<string, string>("{\"Byte\":-1}","Error converting value -1 to type 'System.Byte'. Path 'Byte', line 2, position 12."),
            };

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item1);
                var expected = testCase.Item2;

                ByteType actual = new ByteType();
                Exception actualError = null;
                try
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(ByteType));

                    DefaultSerializer.Deserialize(input, actual);
                }
                catch (Exception e)
                {
                    actualError = e;
                }

                Assert.Equal(expected, actualError.Message);
            }
        }

        [Fact]
        public void SByteSerialization()
        {
            List<Tuple<SByteType, string>> testCases = new List<Tuple<SByteType, string>>() {
                new Tuple<SByteType, string>(new SByteType() { SByte = 0 }, "{\"SByte\":0}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = 1 }, "{\"SByte\":1}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = -1 }, "{\"SByte\":-1}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = SByte.MaxValue }, "{\"SByte\":127}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = SByte.MinValue }, "{\"SByte\":-128}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(SByteType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void SByteDeserialization()
        {
            List<Tuple<SByteType, string>> testCases = new List<Tuple<SByteType, string>>() {
                new Tuple<SByteType, string>(new SByteType() { SByte = 0 }, "{}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = 0 }, "{\"SByte\":null}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = 0 }, "{\"SByte\":0}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = 1 }, "{\"SByte\":1}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = 0 }, "{\"SByte\":false}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = 1 }, "{\"SByte\":true}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = -1 }, "{\"SByte\":-1}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = SByte.MaxValue }, "{\"SByte\":127}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = SByte.MinValue }, "{\"SByte\":-128}"),
                new Tuple<SByteType, string>(new SByteType() { SByte = SByte.MinValue }, "{\"SByte\":\"-128\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(SByteType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                SByteType actual = new SByteType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.SByte, expected.SByte);

                if (testCase.Item2 != "{}")
                {
                    actual = new SByteType();
                    actual.SByte = 12;
                    DefaultSerializer.Deserialize(input, actual);
                }

                Assert.Equal(actual.SByte, expected.SByte);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<SByteType>(json).FirstOrDefault();

                Assert.Equal(actual.SByte, expected.SByte);

                actual = (SByteType)DefaultSerializer.Deserialize<SByteType>(input);

                Assert.Equal(actual.SByte, expected.SByte);
            }
        }

        [Fact]
        public void SByteDeserializationNegative()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {
                new Tuple<string, string>("{\"SByte\":128}", "Error converting value 128 to type 'System.SByte'. Path 'SByte', line 1, position 12."),
                new Tuple<string, string>("{\"SByte\":\"128\"}","Error converting value \"128\" to type 'System.SByte'. Path 'SByte', line 1, position 14."),
                new Tuple<string, string>("{\"SByte\":-129}","Error converting value -129 to type 'System.SByte'. Path 'SByte', line 1, position 13."),
            };

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                SByteType actual = new SByteType();
                Exception actualError = Assert.ThrowsAny<Exception>(() =>
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(SByteType));

                    DefaultSerializer.Deserialize(input, actual);
                });
                Assert.Equal(expected, actualError.Message);
            }
        }

        [Fact]
        public void UShortSerialization()
        {
            List<Tuple<UShortType, string>> testCases = new List<Tuple<UShortType, string>>() {
                new Tuple<UShortType, string>(new UShortType() { UShort = 0 }, "{\"UShort\":0}"),
                new Tuple<UShortType, string>(new UShortType() { UShort = 1 }, "{\"UShort\":1}"),
                new Tuple<UShortType, string>(new UShortType() { UShort = UInt16.MaxValue }, "{\"UShort\":65535}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(UShortType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void UShortDeserialization()
        {
            List<Tuple<UShortType, string>> testCases = new List<Tuple<UShortType, string>>() {
                new Tuple<UShortType, string>(new UShortType() { UShort = 0 }, "{}"),
                new Tuple<UShortType, string>(new UShortType() { UShort = 0 }, "{\"UShort\":null}"),
                new Tuple<UShortType, string>(new UShortType() { UShort = 0 }, "{\"UShort\":0}"),
                new Tuple<UShortType, string>(new UShortType() { UShort = 1 }, "{\"UShort\":1}"),
                new Tuple<UShortType, string>(new UShortType() { UShort = 0 }, "{\"UShort\":false}"),
                new Tuple<UShortType, string>(new UShortType() { UShort = 1 }, "{\"UShort\":true}"),
                new Tuple<UShortType, string>(new UShortType() { UShort = 19 }, "{\"UShort\":\"19\"}"),
                new Tuple<UShortType, string>(new UShortType() { UShort = UInt16.MaxValue }, "{\"UShort\":65535}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(UShortType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                UShortType actual = new UShortType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.UShort, expected.UShort);

                if (testCase.Item2 != "{}")
                {
                    actual = new UShortType();
                    actual.UShort = 10;
                    DefaultSerializer.Deserialize(input, actual);
                }

                Assert.Equal(actual.UShort, expected.UShort);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<UShortType>(json).FirstOrDefault();

                Assert.Equal(actual.UShort, expected.UShort);

                actual = (UShortType)DefaultSerializer.Deserialize<UShortType>(input);

                Assert.Equal(actual.UShort, expected.UShort);
            }
        }

        [Fact]
        public void UShortDeserializationNegative()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {
                new Tuple<string, string>("{\"UShort\":65536}", "Error converting value 65536 to type 'System.UInt16'. Path 'UShort', line 1, position 15."),
                new Tuple<string, string>("{\"UShort\":\"65536\"}","Error converting value \"65536\" to type 'System.UInt16'. Path 'UShort', line 1, position 17."),
                new Tuple<string, string>("{\"UShort\":-1}","Error converting value -1 to type 'System.UInt16'. Path 'UShort', line 1, position 12."),
            };

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                UShortType actual = new UShortType();
                Exception actualError = Assert.ThrowsAny<Exception>(() =>
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(UShortType));

                    DefaultSerializer.Deserialize(input, actual);
                });
                Assert.Equal(expected, actualError.Message);
            }
        }

        [Fact]
        public void IntSerialization()
        {
            List<Tuple<IntType, string>> testCases = new List<Tuple<IntType, string>>() {
                new Tuple<IntType, string>(new IntType() { Id = 0 }, "{}"),
                new Tuple<IntType, string>(new IntType() { Id = -1 }, "{\"id\":-1}"),
                new Tuple<IntType, string>(new IntType() { Id = 1 }, "{\"id\":1}"),
                new Tuple<IntType, string>(new IntType() { Id = int.MaxValue }, "{\"id\":2147483647}"),
                new Tuple<IntType, string>(new IntType() { Id = int.MinValue }, "{\"id\":-2147483648}")
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(IntType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void IntDeserialization()
        {
            List<Tuple<IntType, string>> testCases = new List<Tuple<IntType, string>>() {
                new Tuple<IntType, string>(new IntType() { Id = 0 }, "{}"),
                new Tuple<IntType, string>(new IntType() { Id = 0 }, "{\"id\":null}"),
                new Tuple<IntType, string>(new IntType() { Id = 0 }, "{\"id\":0}"),
                new Tuple<IntType, string>(new IntType() { Id = 0 }, "{\"id\":false}"),
                new Tuple<IntType, string>(new IntType() { Id = 1 }, "{\"id\":true}"),
                new Tuple<IntType, string>(new IntType() { Id = -1 }, "{\"id\":-1}"),
                new Tuple<IntType, string>(new IntType() { Id = 1 }, "{\"id\":1}"),
                new Tuple<IntType, string>(new IntType() { Id = int.MaxValue }, "{\"id\":2147483647}"),
                new Tuple<IntType, string>(new IntType() { Id = int.MinValue }, "{\"id\":-2147483648}")
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(IntType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                IntType actual = new IntType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.Id, expected.Id);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<IntType>(json).FirstOrDefault();

                Assert.Equal(actual.Id, expected.Id);

                actual = (IntType)DefaultSerializer.Deserialize<IntType>(input);

                Assert.Equal(actual.Id, expected.Id);
            }
        }

        [Fact]
        public void IntDeserializationNegative()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {
                new Tuple<string, string>("{\"id\":2147483648}", "Error converting value 2147483648 to type 'System.Int32'. Path 'id', line 1, position 16."),
                new Tuple<string, string>("{\"id\":-2147483649}", "Error converting value -2147483649 to type 'System.Int32'. Path 'id', line 1, position 17."),
            };

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                IntType actual = new IntType();
                Exception actualError = Assert.ThrowsAny<Exception>(() =>
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(IntType));

                    DefaultSerializer.Deserialize(input, actual);
                });
                Assert.Equal(expected, actualError.Message);
            }
        }

        [Fact]
        public void UIntSerialization()
        {
            List<Tuple<UIntType, string>> testCases = new List<Tuple<UIntType, string>>() {
                new Tuple<UIntType, string>(new UIntType() { UInt = 0 }, "{\"UInt\":0}"),
                new Tuple<UIntType, string>(new UIntType() { UInt = 1 }, "{\"UInt\":1}"),
                new Tuple<UIntType, string>(new UIntType() { UInt = UInt32.MaxValue }, "{\"UInt\":4294967295}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(UIntType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void UIntDeserialization()
        {
            List<Tuple<UIntType, string>> testCases = new List<Tuple<UIntType, string>>() {
                new Tuple<UIntType, string>(new UIntType() { UInt = 0 }, "{}"),
                new Tuple<UIntType, string>(new UIntType() { UInt = 0 }, "{\"UInt\":null}"),
                new Tuple<UIntType, string>(new UIntType() { UInt = 0 }, "{\"UInt\":0}"),
                new Tuple<UIntType, string>(new UIntType() { UInt = 1 }, "{\"UInt\":1}"),
                new Tuple<UIntType, string>(new UIntType() { UInt = 0 }, "{\"UInt\":false}"),
                new Tuple<UIntType, string>(new UIntType() { UInt = 1 }, "{\"UInt\":true}"),
                new Tuple<UIntType, string>(new UIntType() { UInt = 150 }, "{\"UInt\":\"150\"}"),
                new Tuple<UIntType, string>(new UIntType() { UInt = UInt32.MaxValue }, "{\"UInt\":4294967295}"),
                new Tuple<UIntType, string>(new UIntType() { UInt = UInt32.MaxValue }, "{\"UInt\":\"4294967295\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(UIntType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                UIntType actual = new UIntType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.UInt, expected.UInt);

                if (testCase.Item2 != "{}")
                {
                    actual = new UIntType();
                    actual.UInt = 12;
                    DefaultSerializer.Deserialize(input, actual);
                }

                Assert.Equal(actual.UInt, expected.UInt);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<UIntType>(json).FirstOrDefault();

                Assert.Equal(actual.UInt, expected.UInt);

                actual = (UIntType)DefaultSerializer.Deserialize<UIntType>(input);

                Assert.Equal(actual.UInt, expected.UInt);
            }
        }

        [Fact]
        public void UIntDeserializationNegative()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {
                new Tuple<string, string>("{\"UInt\":4294967296}", "Error converting value 4294967296 to type 'System.UInt32'. Path 'UInt', line 1, position 18."),
                new Tuple<string, string>("{\"UInt\":\"4294967296\"}", "Error converting value \"4294967296\" to type 'System.UInt32'. Path 'UInt', line 1, position 20."),
                new Tuple<string, string>("{\"UInt\":-1}", "Error converting value -1 to type 'System.UInt32'. Path 'UInt', line 1, position 10."),
            };

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                UIntType actual = new UIntType();
                Exception actualError = Assert.ThrowsAny<Exception>(() =>
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(UIntType));

                    DefaultSerializer.Deserialize(input, actual);
                });
                Assert.Equal(expected, actualError.Message);
            }
        }

        [Fact]
        public void LongSerialization()
        {
            long twoToTheFifthyThird = 0x20000000000000; // 2^53
            List<Tuple<LongType, string>> testCases = new List<Tuple<LongType, string>>() {
                new Tuple<LongType, string>(new LongType() { Long = 0 }, "{\"Long\":0}"),
                new Tuple<LongType, string>(new LongType() { Long = -1 }, "{\"Long\":-1}"),
                new Tuple<LongType, string>(new LongType() { Long = 1 }, "{\"Long\":1}"),
                new Tuple<LongType, string>(new LongType() { Long = twoToTheFifthyThird }, "{\"Long\":9007199254740992}"), // All integers <= 2^53 will fit in a double; this should be our upper limit
                new Tuple<LongType, string>(new LongType() { Long = -twoToTheFifthyThird }, "{\"Long\":-9007199254740992}"), // All integers >= -2^53 will fit in a double; this should be our lower limit
                new Tuple<LongType, string>(new LongType() { Long = twoToTheFifthyThird - 1 }, "{\"Long\":9007199254740991}"),
                new Tuple<LongType, string>(new LongType() { Long = -twoToTheFifthyThird + 1 }, "{\"Long\":-9007199254740991}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(LongType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void LongSerializationNegative()
        {
            long twoToTheFifthyThird = 0x20000000000000; // 2^53
            List<Tuple<LongType, string>> testCases = new List<Tuple<LongType, string>>() {
                new Tuple<LongType, string>(new LongType() { Long = twoToTheFifthyThird + 1 },"The value 9007199254740993 for member Long is outside the valid range for numeric columns."), // 2^53 + 1
                new Tuple<LongType, string>(new LongType() { Long = -twoToTheFifthyThird - 1 },"The value -9007199254740993 for member Long is outside the valid range for numeric columns."), // -2^53 - 1
                new Tuple<LongType, string>(new LongType() { Long = long.MaxValue },"The value 9223372036854775807 for member Long is outside the valid range for numeric columns."), // long.MaxValue
                new Tuple<LongType, string>(new LongType() { Long = long.MinValue }, "The value -9223372036854775808 for member Long is outside the valid range for numeric columns.")
            };

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;
                Exception actualError = null;
                try
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(LongType));

                    DefaultSerializer.Serialize(input);
                }
                catch (Exception e)
                {
                    actualError = e;
                }

                Assert.Equal(expected, actualError.Message);
            }
        }

        [Fact]
        public void LongDeserialization()
        {
            long twoToTheFifthyThird = 0x20000000000000; // 2^53
            List<Tuple<LongType, string>> testCases = new List<Tuple<LongType, string>>() {
                new Tuple<LongType, string>(new LongType() { Long = 0 }, "{}"),
                new Tuple<LongType, string>(new LongType() { Long = 0 }, "{\"Long\":null}"),
                new Tuple<LongType, string>(new LongType() { Long = 0 }, "{\"Long\":false}"),
                new Tuple<LongType, string>(new LongType() { Long = 1 }, "{\"Long\":true}"),
                new Tuple<LongType, string>(new LongType() { Long = -1 }, "{\"Long\":-1}"),
                new Tuple<LongType, string>(new LongType() { Long = 1 }, "{\"Long\":1}"),
                new Tuple<LongType, string>(new LongType() { Long = -55 }, "{\"Long\":\"-55\"}"),
                new Tuple<LongType, string>(new LongType() { Long = twoToTheFifthyThird }, "{\"Long\":9007199254740992}"),
                new Tuple<LongType, string>(new LongType() { Long = -twoToTheFifthyThird }, "{\"Long\":-9007199254740992}"),
                new Tuple<LongType, string>(new LongType() { Long = long.MinValue }, "{\"Long\":-9223372036854775808}"),
                new Tuple<LongType, string>(new LongType() { Long = long.MaxValue }, "{\"Long\":9223372036854775807}"),
                new Tuple<LongType, string>(new LongType() { Long = long.MinValue }, "{\"Long\":-9.2233720368547758E+18}"), // There is a loss of precision here, but we'll accept it when deserializing.
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(LongType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                LongType actual = new LongType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.Long, expected.Long);

                if (testCase.Item2 != "{}")
                {
                    actual = new LongType();
                    actual.Long = 62;
                    DefaultSerializer.Deserialize(input, actual);
                }

                Assert.Equal(actual.Long, expected.Long);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<LongType>(json).FirstOrDefault();

                Assert.Equal(actual.Long, expected.Long);

                actual = (LongType)DefaultSerializer.Deserialize<LongType>(input);

                Assert.Equal(actual.Long, expected.Long);
            }
        }

        [Fact]
        public void LongDeserializationNegative_1()
        {
            var input = "{\"Long\":9.2233720368547758E+18}";
            var actual = new LongType();
            Assert.Throws<JsonSerializationException>(() =>
            {
                var serializer = DefaultSerializer;
                serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(LongType));
                serializer.Deserialize(input, actual);
            });
        }

        [Fact]
        public void LongDeserializationNegative_2()
        {
            var input = "{\"Long\":9223372036854775808}";
            var actual = new LongType();
            Assert.Throws<JsonSerializationException>(() =>
            {
                var serializer = DefaultSerializer;
                serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(LongType));
                serializer.Deserialize(input, actual);
            });
        }

        [Fact]
        public void ULongSerialization()
        {
            ulong twoToTheFifthyThird = 0x20000000000000; // 2^53
            List<Tuple<ULongType, string>> testCases = new List<Tuple<ULongType, string>>() {
                new Tuple<ULongType, string>(new ULongType() { ULong = 0 }, "{\"ULong\":0}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = 1 }, "{\"ULong\":1}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = twoToTheFifthyThird - 1 },  "{\"ULong\":9007199254740991}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = twoToTheFifthyThird },  "{\"ULong\":9007199254740992}"), // All integers <= 2^53 will fit in a double; this should be our upper limit
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(ULongType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void ULongSerializationNegative()
        {
            ulong twoToTheFifthyThird = 0x20000000000000; // 2^53
            List<Tuple<ULongType, string>> testCases = new List<Tuple<ULongType, string>>() {
                new Tuple<ULongType, string>(new ULongType() { ULong = twoToTheFifthyThird + 1 },"The value 9007199254740993 for member ULong is outside the valid range for numeric columns."), // 2^53 + 1
                new Tuple<ULongType, string>(new ULongType() { ULong = ulong.MaxValue },"The value 18446744073709551615 for member ULong is outside the valid range for numeric columns.") // ulong.MaxValue
            };

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;
                Exception actualError = Assert.ThrowsAny<Exception>(() =>
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(ULongType));

                    DefaultSerializer.Serialize(input);
                });
                Assert.Equal(expected, actualError.Message);
            }
        }

        [Fact]
        public void ULongDeserialization()
        {
            ulong twoToTheFifthyThird = 0x20000000000000; // 2^53
            List<Tuple<ULongType, string>> testCases = new List<Tuple<ULongType, string>>() {
                new Tuple<ULongType, string>(new ULongType() { ULong = 0 }, "{}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = 0 }, "{\"ULong\":null}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = 0 }, "{\"ULong\":0}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = 1 }, "{\"ULong\":1}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = 0 }, "{\"ULong\":false}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = 1 }, "{\"ULong\":true}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = 15 }, "{\"ULong\":\"15\"}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = ulong.MaxValue }, "{\"ULong\":\"18446744073709551615\"}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = twoToTheFifthyThird - 1 },  "{\"ULong\":9007199254740991}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = twoToTheFifthyThird + 1 },  "{\"ULong\":9007199254740993}"),
                new Tuple<ULongType, string>(new ULongType() { ULong = twoToTheFifthyThird },  "{\"ULong\":9007199254740992}"), // All integers <= 2^53 will fit in a double; this should be our upper limit
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(ULongType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                ULongType actual = new ULongType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.ULong, expected.ULong);

                if (testCase.Item2 != "{}")
                {
                    actual = new ULongType();
                    actual.ULong = 19;
                    DefaultSerializer.Deserialize(input, actual);
                }

                Assert.Equal(actual.ULong, expected.ULong);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<ULongType>(json).FirstOrDefault();

                Assert.Equal(actual.ULong, expected.ULong);

                actual = (ULongType)DefaultSerializer.Deserialize<ULongType>(input);

                Assert.Equal(actual.ULong, expected.ULong);
            }
        }

        [Fact]
        public void ULongDeserializationNegative()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {
                new Tuple<string, string>("{\"ULong\":18446744073709551616}","Error converting value 18446744073709551616 to type 'System.UInt64'. Path 'ULong', line 1, position 29."), // ulong.MaxValue + 1
                new Tuple<string, string>("{\"ULong\":-1}","Error converting value -1 to type 'System.UInt64'. Path 'ULong', line 1, position 11."),
            };

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                ULongType actual = new ULongType();
                Exception actualError = Assert.ThrowsAny<Exception>(() =>
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(ULongType));

                    DefaultSerializer.Deserialize(input, actual);
                });
                Assert.Equal(expected, actualError.Message);
            }
        }

        [Fact]
        public void FloatSerialization_1()
        {
            var input = new FloatType() { Float = 0 };
            var expected = "{\"Id\":0,\"Float\":0.0}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FloatSerialization_2()
        {
            var input = new FloatType() { Float = -1 };
            var expected = "{\"Id\":0,\"Float\":-1.0}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FloatSerialization_3()
        {
            var input = new FloatType() { Float = 1 };
            var expected = "{\"Id\":0,\"Float\":1.0}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FloatSerialization_4()
        {
            var input = new FloatType() { Float = float.Epsilon };
            var expected = "{\"Id\":0,\"Float\":1E-45}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FloatSerialization_5()
        {
            var input = new FloatType() { Float = -float.Epsilon };
            var expected = "{\"Id\":0,\"Float\":-1E-45}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FloatSerialization_6()
        {
            var input = new FloatType() { Float = float.MaxValue };
            var expected = "{\"Id\":0,\"Float\":3.4028235E+38}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FloatSerialization_7()
        {
            var input = new FloatType() { Float = float.MinValue };
            var expected = "{\"Id\":0,\"Float\":-3.4028235E+38}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FloatDeserialization_1()
        {
            var expected = new FloatType() { Float = 0 };
            var input = JToken.Parse("{}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_2()
        {
            var expected = new FloatType() { Float = 0 };
            var input = JToken.Parse("{\"Float\":null}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_3()
        {
            var expected = new FloatType() { Float = 0 };
            var input = JToken.Parse("{\"Float\":0}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_4()
        {
            var expected = new FloatType() { Float = 0 };
            var input = JToken.Parse("{\"Float\":false}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_5()
        {
            var expected = new FloatType() { Float = 1 };
            var input = JToken.Parse("{\"Float\":true}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_6()
        {
            var expected = new FloatType() { Float = 5.8f };
            var input = JToken.Parse("{\"Float\":\"5.8\"}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_7()
        {
            var expected = new FloatType() { Float = -1 };
            var input = JToken.Parse("{\"Float\":-1}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_8()
        {
            var expected = new FloatType() { Float = 1 };
            var input = JToken.Parse("{\"Float\":1}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_9()
        {
            var expected = new FloatType() { Float = 0.0F };
            var input = JToken.Parse("{\"Float\":4.94065645841247E-325}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_10()
        {
            var expected = new FloatType() { Float = 0.0F };
            var input = JToken.Parse("{\"Float\":-4.94065645841247E-325}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_11()
        {
            var expected = new FloatType() { Float = float.Epsilon };
            var input = JToken.Parse("{\"Float\":1.401298E-45}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_12()
        {
            var expected = new FloatType() { Float = -float.Epsilon };
            var input = JToken.Parse("{\"Float\":-1.401298E-45}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_13()
        {
            var expected = new FloatType() { Float = float.Epsilon };
            var input = JToken.Parse("{\"Float\":1.4012984643248171E-45}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_14()
        {
            var expected = new FloatType() { Float = -float.Epsilon };
            var input = JToken.Parse("{\"Float\":-1.4012984643248171E-45}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_17()
        {
            var expected = new FloatType() { Float = float.MaxValue };
            var input = JToken.Parse("{\"Float\":3.40282347E+38}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_18()
        {
            var expected = new FloatType() { Float = float.MinValue };
            var input = JToken.Parse("{\"Float\":-3.40282347E+38}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_19()
        {
            var expected = new FloatType() { Float = float.MaxValue };
            var input = JToken.Parse("{\"Float\":3.4028234663852886E+38}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_20()
        {
            var expected = new FloatType() { Float = float.MinValue };
            var input = JToken.Parse("{\"Float\":-3.4028234663852886E+38}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_21()
        {
            var expected = new FloatType() { Float = float.MinValue };
            var input = JToken.Parse("{\"Float\":\"-3.4028234663852886E+38\"}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_22()
        {
            var expected = new FloatType() { Float = float.MaxValue };
            var input = JToken.Parse("{\"Float\":3.40282346638528865E+38}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);
        }

        [Fact]
        public void FloatDeserialization_23()
        {
            var expected = new FloatType() { Float = float.MinValue };
            var input = JToken.Parse("{\"Float\":-3.40282346638528865E+38}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.Equal(expected.Float, actual.Float);

        }

        [Fact]
        public void FloatDeserialization_24()
        {
            var expected = new FloatType() { Float = float.PositiveInfinity };
            var input = JToken.Parse("{\"Float\":3.4028234663852887E+39}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.True(float.IsPositiveInfinity(actual.Float));
        }

        [Fact]
        public void FloatDeserialization_25()
        {
            var expected = new FloatType() { Float = float.NegativeInfinity };
            var input = JToken.Parse("{\"Float\":-3.4028234663852887E+39}");
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            FloatType actual = new FloatType();
            serializer.Deserialize(input, actual);
            Assert.True(float.IsNegativeInfinity(actual.Float));
        }

        [Fact]
        public void FloatDeserializationNegative_1()
        {
            var input = "{\"Float\":1.7976931348623157E+309}"; // Larger double.MaxValue
            var actual = new FloatType();
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            serializer.Deserialize(input, actual);
            Assert.Equal(float.PositiveInfinity, actual.Float);
        }

        [Fact]
        public void FloatDeserializationNegative_2()
        {
            var input = "{\"Float\":-1.7976931348623157E+309}"; // Larger double.MinValue
            var actual = new FloatType();
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));
            serializer.Deserialize(input, actual);
            Assert.Equal(float.NegativeInfinity, actual.Float);
        }

        [Fact]
        public void DoubleSerialization_1()
        {
            var input = new DoubleType() { Double = 0 };
            var expected = "{\"Id\":0,\"Double\":0.0}";
            string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DoubleSerialization_2()
        {
            var input = new DoubleType() { Double = -1 };
            var expected = "{\"Id\":0,\"Double\":-1.0}";
            string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DoubleSerialization_3()
        {
            var input = new DoubleType() { Double = 1 };
            var expected = "{\"Id\":0,\"Double\":1.0}";
            string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DoubleSerialization_4()
        {
            var input = new DoubleType() { Double = Double.Epsilon };
            var expected = "{\"Id\":0,\"Double\":5E-324}";
            string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DoubleSerialization_5()
        {
            var input = new DoubleType() { Double = -Double.Epsilon };
            var expected = "{\"Id\":0,\"Double\":-5E-324}";
            string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DoubleSerialization_6()
        {
            var input = new DoubleType() { Double = Double.MaxValue };
            var expected = "{\"Id\":0,\"Double\":1.7976931348623157E+308}";
            string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DoubleSerialization_7()
        {
            var input = new DoubleType() { Double = Double.MinValue };
            var expected = "{\"Id\":0,\"Double\":-1.7976931348623157E+308}";
            string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0, "{}")]
        [InlineData(0, "{\"Double\":null}")]
        [InlineData(0, "{\"Double\":false}")]
        [InlineData(1, "{\"Double\":true}")]
        [InlineData(5.5, "{\"Double\":\"5.5\"}")]
        [InlineData(0, "{\"Double\":0}")]
        [InlineData(-1, "{\"Double\":-1}")]
        [InlineData(1, "{\"Double\":1}")]
        [InlineData(0, "{\"Double\":4.94065645841247E-325}")]
        [InlineData(0, "{\"Double\":-4.94065645841247E-325}")]
        [InlineData(double.Epsilon, "{\"Double\":4.94065645841247E-324}")]
        [InlineData(-double.Epsilon, "{\"Double\":-4.94065645841247E-324}")]
        [InlineData(double.MaxValue, "{\"Double\":1.7976931348623157E+308}")]
        [InlineData(double.MinValue, "{\"Double\":-1.7976931348623157E+308}")]
        [InlineData(double.MaxValue, "{\"Double\":1.79769313486231575E+308}")] // We're ok with lossing precision here; note the extra 5 digit at the end
        [InlineData(double.MinValue, "{\"Double\":-1.79769313486231575E+308}")]// We're ok with lossing precision here; note the extra 5 digit at the end
        public void DoubleDeserialization(double dval, string inputString)
        {
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DoubleType));
            var input = JToken.Parse(inputString);
            var expected = new DoubleType { Double = dval };

            DoubleType actual = new DoubleType();
            DefaultSerializer.Deserialize(input, actual);

            if (double.IsPositiveInfinity(expected.Double))
            {
                Assert.True(double.IsPositiveInfinity(actual.Double));
            }
            else if (double.IsNegativeInfinity(expected.Double))
            {
                Assert.True(double.IsNegativeInfinity(actual.Double));
            }
            else
            {
                Assert.Equal(actual.Double, expected.Double);
            }

            if (inputString != "{}")
            {
                actual = new DoubleType();
                actual.Double = 34.6;
                DefaultSerializer.Deserialize(input, actual);

                if (double.IsPositiveInfinity(expected.Double))
                {
                    Assert.True(double.IsPositiveInfinity(actual.Double));
                }
                else if (double.IsNegativeInfinity(expected.Double))
                {
                    Assert.True(double.IsNegativeInfinity(actual.Double));
                }
                else
                {
                    Assert.Equal(actual.Double, expected.Double);
                }
            }

            JArray json = JToken.Parse("[" + inputString + "]") as JArray;
            actual = DefaultSerializer.Deserialize<DoubleType>(json).FirstOrDefault();

            if (double.IsPositiveInfinity(expected.Double))
            {
                Assert.True(double.IsPositiveInfinity(actual.Double));
            }
            else if (double.IsNegativeInfinity(expected.Double))
            {
                Assert.True(double.IsNegativeInfinity(actual.Double));
            }
            else
            {
                Assert.Equal(actual.Double, expected.Double);
            }

            actual = DefaultSerializer.Deserialize<DoubleType>(input);

            if (double.IsPositiveInfinity(expected.Double))
            {
                Assert.True(double.IsPositiveInfinity(actual.Double));
            }
            else if (double.IsNegativeInfinity(expected.Double))
            {
                Assert.True(double.IsNegativeInfinity(actual.Double));
            }
            else
            {
                Assert.Equal(actual.Double, expected.Double);
            }
        }

        [Fact]
        public void DoubleDeserializationNegative_1()
        {
            var input = "{\"Double\":5.7976931348623157E+325}";
            var actual = new DoubleType();
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DoubleType));
            serializer.Deserialize(input, actual);
            Assert.Equal(Double.PositiveInfinity, actual.Double);
        }

        [Fact]
        public void DoubleDeserializationNegative_2()
        {
            var input = "{\"Double\":-5.7976931348623157E+325}";
            var actual = new DoubleType();
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DoubleType));
            serializer.Deserialize(input, actual);
            Assert.Equal(Double.NegativeInfinity, actual.Double);
        }

        [Fact]
        public void DecimalSerialization()
        {
            decimal twoToTheFifthyThird = 0x20000000000000; // 2^53
            List<Tuple<DecimalType, string>> testCases = new List<Tuple<DecimalType, string>>() {
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 0 }, "{\"Decimal\":0.0}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -1 }, "{\"Decimal\":-1.0}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 1 }, "{\"Decimal\":1.0}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = twoToTheFifthyThird }, "{\"Decimal\":9007199254740992.0}"), // All integers <= 2^53 will fit in a double; this should be our upper limit
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -twoToTheFifthyThird }, "{\"Decimal\":-9007199254740992.0}"), // All integers >= -2^53 will fit in a double; this should be our lower limit
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = twoToTheFifthyThird - 1 }, "{\"Decimal\":9007199254740991.0}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -twoToTheFifthyThird + 1 }, "{\"Decimal\":-9007199254740991.0}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 9007199.25474099m }, "{\"Decimal\":9007199.25474099}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -99999999999999.9m }, "{\"Decimal\":-99999999999999.9}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -9999999999999.99m }, "{\"Decimal\":-9999999999999.99}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -999999999999.999m }, "{\"Decimal\":-999999999999.999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -99999999999.9999m }, "{\"Decimal\":-99999999999.9999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -9999999999.99999m }, "{\"Decimal\":-9999999999.99999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -999999999.999999m }, "{\"Decimal\":-999999999.999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -99999999.9999999m }, "{\"Decimal\":-99999999.9999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -9999999.99999999m }, "{\"Decimal\":-9999999.99999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -999999.999999999m }, "{\"Decimal\":-999999.999999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -99999.9999999999m }, "{\"Decimal\":-99999.9999999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -9999.99999999999m }, "{\"Decimal\":-9999.99999999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -999.999999999999m }, "{\"Decimal\":-999.999999999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -99.9999999999999m }, "{\"Decimal\":-99.9999999999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -9.99999999999999m }, "{\"Decimal\":-9.99999999999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 99999999999999.9m }, "{\"Decimal\":99999999999999.9}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 9999999999999.99m }, "{\"Decimal\":9999999999999.99}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 999999999999.999m }, "{\"Decimal\":999999999999.999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 99999999999.9999m }, "{\"Decimal\":99999999999.9999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 9999999999.99999m }, "{\"Decimal\":9999999999.99999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 999999999.999999m }, "{\"Decimal\":999999999.999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 99999999.9999999m }, "{\"Decimal\":99999999.9999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 9999999.99999999m }, "{\"Decimal\":9999999.99999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 999999.999999999m }, "{\"Decimal\":999999.999999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 99999.9999999999m }, "{\"Decimal\":99999.9999999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 9999.99999999999m }, "{\"Decimal\":9999.99999999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 999.999999999999m }, "{\"Decimal\":999.999999999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 99.9999999999999m }, "{\"Decimal\":99.9999999999999}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 9.99999999999999m }, "{\"Decimal\":9.99999999999999}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DecimalType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DecimalSerializationNegative()
        {
            decimal twoToTheFifthyThird = 9007199254740992.0m; // 2^53
            string errorString = "The value {0} for member Decimal is outside the valid range for numeric columns.";
            List<Tuple<DecimalType, string>> testCases = new List<Tuple<DecimalType, string>>() {
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = twoToTheFifthyThird + 1 }, string.Format(errorString, "9007199254740993.0")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -twoToTheFifthyThird - 1 }, string.Format(errorString, "-9007199254740993.0")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = decimal.MaxValue }, string.Format(errorString, "79228162514264337593543950335")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = decimal.MinValue }, string.Format(errorString, "-79228162514264337593543950335")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 90071992.547409920m }, string.Format(errorString, "90071992.547409920")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -90071992.547409920m }, string.Format(errorString, "-90071992.547409920")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -100000000000000.1m }, string.Format(errorString, "-100000000000000.1")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -10000000000000.01m }, string.Format(errorString, "-10000000000000.01")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -1000000000000.001m }, string.Format(errorString, "-1000000000000.001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -100000000000.0001m }, string.Format(errorString, "-100000000000.0001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -10000000000.00001m }, string.Format(errorString, "-10000000000.00001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -1000000000.000001m }, string.Format(errorString, "-1000000000.000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -100000000.0000001m }, string.Format(errorString, "-100000000.0000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -10000000.00000001m }, string.Format(errorString, "-10000000.00000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -1000000.000000001m }, string.Format(errorString, "-1000000.000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -100000.0000000001m }, string.Format(errorString, "-100000.0000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -10000.00000000001m }, string.Format(errorString, "-10000.00000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -1000.000000000001m }, string.Format(errorString, "-1000.000000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -100.0000000000001m }, string.Format(errorString, "-100.0000000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -10.00000000000001m }, string.Format(errorString, "-10.00000000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -1.000000000000001m }, string.Format(errorString, "-1.000000000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 100000000000000.1m }, string.Format(errorString, "100000000000000.1")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 10000000000000.01m }, string.Format(errorString, "10000000000000.01")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 1000000000000.001m }, string.Format(errorString, "1000000000000.001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 100000000000.0001m }, string.Format(errorString, "100000000000.0001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 10000000000.00001m }, string.Format(errorString, "10000000000.00001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 1000000000.000001m }, string.Format(errorString, "1000000000.000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 100000000.0000001m }, string.Format(errorString, "100000000.0000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 10000000.00000001m }, string.Format(errorString, "10000000.00000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 1000000.000000001m }, string.Format(errorString, "1000000.000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 100000.0000000001m }, string.Format(errorString, "100000.0000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 10000.00000000001m }, string.Format(errorString, "10000.00000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 1000.000000000001m }, string.Format(errorString, "1000.000000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 100.0000000000001m }, string.Format(errorString, "100.0000000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 10.00000000000001m }, string.Format(errorString, "10.00000000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 1.000000000000001m }, string.Format(errorString, "1.000000000000001")),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 10000000000000000m }, string.Format(errorString, "10000000000000000")),
            };

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;
                Exception actualError = null;
                try
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DecimalType));

                    DefaultSerializer.Serialize(input);
                }
                catch (Exception e)
                {
                    actualError = e;
                }

                Assert.Equal(expected, actualError.Message);
            }
        }

        [Fact]
        public void DecimalDeserialization()
        {
            decimal twoToTheFifthyThird = 0x20000000000000; // 2^53
            List<Tuple<DecimalType, string>> testCases = new List<Tuple<DecimalType, string>>() {
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 0 }, "{\"Decimal\":0}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -1 }, "{\"Decimal\":-1}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 1 }, "{\"Decimal\":1}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 1 }, "{\"Decimal\":true}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 0 }, "{\"Decimal\":false}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = twoToTheFifthyThird }, "{\"Decimal\":9007199254740992}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -twoToTheFifthyThird }, "{\"Decimal\":-9007199254740992}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = twoToTheFifthyThird - 1 }, "{\"Decimal\":9007199254740991}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -twoToTheFifthyThird + 1 }, "{\"Decimal\":-9007199254740991}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = twoToTheFifthyThird + 1 }, "{\"Decimal\":9007199254740993}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = -twoToTheFifthyThird - 1 }, "{\"Decimal\":-9007199254740993}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 0 }, "{\"Decimal\":null}"),
                new Tuple<DecimalType, string>(new DecimalType() { Decimal = 5.00M }, "{\"Decimal\":\"5.00\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DecimalType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                DecimalType actual = new DecimalType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.Decimal, expected.Decimal);

                if (testCase.Item2 != "{}")
                {
                    actual = new DecimalType();
                    actual.Decimal = 34.5M;
                    DefaultSerializer.Deserialize(input, actual);
                }

                Assert.Equal(actual.Decimal, expected.Decimal);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<DecimalType>(json).FirstOrDefault();

                Assert.Equal(actual.Decimal, expected.Decimal);

                actual = (DecimalType)DefaultSerializer.Deserialize<DecimalType>(input);

                Assert.Equal(actual.Decimal, expected.Decimal);
            }
        }

        [Theory]
        [InlineData("{\"Decimal\":-7.9228162514264338E+28}")]
        [InlineData("{\"Decimal\":7.9228162514264338E+28}")]
        public void DecimalDeserializationNegative(string input)
        {
            Assert.Throws<JsonSerializationException>(() =>
            {
                var actual = new DecimalType();
                var serializer = DefaultSerializer;
                serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DecimalType));
                serializer.Deserialize(input, actual);
            });
        }

        [Fact]
        public void StringSerialization()
        {
            List<Tuple<StringType, string>> testCases = new List<Tuple<StringType, string>>() {
                new Tuple<StringType, string>(new StringType() { String = null }, "{\"String\":null}"),
                new Tuple<StringType, string>(new StringType() { String = "" }, "{\"String\":\"\"}"),
                new Tuple<StringType, string>(new StringType() { String = " " }, "{\"String\":\" \"}"),
                new Tuple<StringType, string>(new StringType() { String = "\n" }, "{\"String\":\"\\n\"}"),
                new Tuple<StringType, string>(new StringType() { String = "\t" }, "{\"String\":\"\\t\"}"),
                new Tuple<StringType, string>(new StringType() { String = "hello" }, "{\"String\":\"hello\"}"),
                new Tuple<StringType, string>(new StringType() { String = "\"hello\"" }, "{\"String\":\"\\\"hello\\\"\"}"),
                new Tuple<StringType, string>(new StringType() { String = new string('*', 1025) }, "{\"String\":\"*****************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************\"}"),
                new Tuple<StringType, string>(new StringType() { String = "ÃÇßÑᾆΏ" }, "{\"String\":\"ÃÇßÑᾆΏ\"}"),
                new Tuple<StringType, string>(new StringType() { String = "'hello'" }, "{\"String\":\"'hello'\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(StringType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void StringDeserialization()
        {
            List<Tuple<StringType, string>> testCases = new List<Tuple<StringType, string>>() {
                new Tuple<StringType, string>(new StringType() { String = null }, "{}"),
                new Tuple<StringType, string>(new StringType() { String = null }, "{\"String\":null}"),
                new Tuple<StringType, string>(new StringType() { String = "" }, "{\"String\":\"\"}"),
                new Tuple<StringType, string>(new StringType() { String = " " }, "{\"String\":\" \"}"),
                new Tuple<StringType, string>(new StringType() { String = "\n" }, "{\"String\":\"\\n\"}"),
                new Tuple<StringType, string>(new StringType() { String = "\t" }, "{\"String\":\"\\t\"}"),
                new Tuple<StringType, string>(new StringType() { String = "\n" }, "{\"String\":\"\n\"}"),
                new Tuple<StringType, string>(new StringType() { String = "\t" }, "{\"String\":\"\t\"}"),
                new Tuple<StringType, string>(new StringType() { String = "hello" }, "{\"String\":\"hello\"}"),
                new Tuple<StringType, string>(new StringType() { String = "\"hello\"" }, "{\"String\":\"\\\"hello\\\"\"}"),
                new Tuple<StringType, string>(new StringType() { String = new string('*', 1025) }, "{\"String\":\"*****************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************\"}"),
                new Tuple<StringType, string>(new StringType() { String = "ÃÇßÑᾆΏ" }, "{\"String\":\"ÃÇßÑᾆΏ\"}"),
                new Tuple<StringType, string>(new StringType() { String = "'hello'" }, "{\"String\":\"'hello'\"}"),
                // Known issue: https://github.com/JamesNK/Newtonsoft.Json/issues/1019
                //new Tuple<StringType, string>(new StringType() { String = "True" }, "{\"String\":true}"),
                new Tuple<StringType, string>(new StringType() { String = "5" }, "{\"String\":5}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(StringType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                StringType actual = new StringType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.String, expected.String);

                if (testCase.Item2 != "{}")
                {
                    actual = new StringType();
                    actual.String = "xyz";
                    DefaultSerializer.Deserialize(input, actual);
                }

                Assert.Equal(actual.String, expected.String);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<StringType>(json).FirstOrDefault();

                Assert.Equal(actual.String, expected.String);

                actual = (StringType)DefaultSerializer.Deserialize<StringType>(input);

                Assert.Equal(actual.String, expected.String);
            }
        }

        [Fact]
        public void CharSerialization()
        {
            List<Tuple<CharType, string>> testCases = new List<Tuple<CharType, string>>() {
                new Tuple<CharType, string>(new CharType() { Char = (char)0 }, "{\"Char\":\"\\u0000\"}"),
                new Tuple<CharType, string>(new CharType() { Char = (char)1 }, "{\"Char\":\"\\u0001\"}"),
                new Tuple<CharType, string>(new CharType() { Char = ' ' }, "{\"Char\":\" \"}"),
                new Tuple<CharType, string>(new CharType() { Char = '\n' }, "{\"Char\":\"\\n\"}"),
                new Tuple<CharType, string>(new CharType() { Char = '\t' }, "{\"Char\":\"\\t\"}"),
                new Tuple<CharType, string>(new CharType() { Char = '?' }, "{\"Char\":\"?\"}"),
                new Tuple<CharType, string>(new CharType() { Char = '\u1000' }, "{\"Char\":\"က\"}"),
                new Tuple<CharType, string>(new CharType() { Char = 'Ã' }, "{\"Char\":\"Ã\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(CharType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void CharDeserialization()
        {
            List<Tuple<CharType, string>> testCases = new List<Tuple<CharType, string>>() {
                new Tuple<CharType, string>(new CharType() { Char = (char)0 }, "{}"),
                new Tuple<CharType, string>(new CharType() { Char = (char)0 }, "{\"Char\":null}"),
                new Tuple<CharType, string>(new CharType() { Char = (char)0 }, "{\"Char\":\"\\u0000\"}"),
                new Tuple<CharType, string>(new CharType() { Char = (char)1 }, "{\"Char\":\"\\u0001\"}"),
                new Tuple<CharType, string>(new CharType() { Char = (char)5 }, "{\"Char\":5}"),
                new Tuple<CharType, string>(new CharType() { Char = ' ' }, "{\"Char\":\" \"}"),
                new Tuple<CharType, string>(new CharType() { Char = '\n' }, "{\"Char\":\"\\n\"}"),
                new Tuple<CharType, string>(new CharType() { Char = '\t' }, "{\"Char\":\"\\t\"}"),
                new Tuple<CharType, string>(new CharType() { Char = '\n' }, "{\"Char\":\"\n\"}"),
                new Tuple<CharType, string>(new CharType() { Char = '\t' }, "{\"Char\":\"\t\"}"),
                new Tuple<CharType, string>(new CharType() { Char = '?' }, "{\"Char\":\"?\"}"),
                new Tuple<CharType, string>(new CharType() { Char = '\u1000' }, "{\"Char\":\"က\"}"),
                new Tuple<CharType, string>(new CharType() { Char = 'Ã' }, "{\"Char\":\"Ã\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(CharType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                CharType actual = new CharType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.Char, expected.Char);

                if (testCase.Item2 != "{}")
                {
                    actual = new CharType();
                    actual.Char = 'a';
                    DefaultSerializer.Deserialize(input, actual);
                }

                Assert.Equal(actual.Char, expected.Char);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<CharType>(json).FirstOrDefault();

                Assert.Equal(actual.Char, expected.Char);

                actual = (CharType)DefaultSerializer.Deserialize<CharType>(input);

                Assert.Equal(actual.Char, expected.Char);
            }
        }

        [Fact]
        public void CharDeserializationNegative()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {

                new Tuple<string, string>("{\"Char\":\"I'm not a char\"}", "Error converting value \"I'm not a char\" to type 'System.Char'. Path 'Char', line 1, position 24."),
                new Tuple<string, string>("{\"Char\":true}","Error converting value True to type 'System.Char'. Path 'Char', line 1, position 12.")
            };

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expectedError = testCase.Item2;

                CharType actual = new CharType();
                Exception actualError = Assert.ThrowsAny<Exception>(() =>
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(CharType));

                    DefaultSerializer.Deserialize(input, actual);
                });
                Assert.Equal(expectedError, actualError.Message);
            }
        }

        [Fact]
        public void UriSerialization()
        {
            List<Tuple<UriType, string>> testCases = new List<Tuple<UriType, string>>() {
                new Tuple<UriType, string>(new UriType() { Uri = null }, "{\"Uri\":null}"),
                new Tuple<UriType, string>(new UriType() { Uri = new Uri("http://someHost/") }, "{\"Uri\":\"http://someHost/\"}"),
                new Tuple<UriType, string>(new UriType() { Uri = new Uri("ftp://127.0.0.1/") }, "{\"Uri\":\"ftp://127.0.0.1/\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(UriType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void UriDeserialization()
        {
            List<Tuple<UriType, string>> testCases = new List<Tuple<UriType, string>>() {
                new Tuple<UriType, string>(new UriType() { Uri = null }, "{}"),
                new Tuple<UriType, string>(new UriType() { Uri = null }, "{\"Uri\":null}"),
                new Tuple<UriType, string>(new UriType() { Uri = new Uri("I'm not a URI", UriKind.Relative) }, "{\"Uri\":\"I'm not a URI\"}"),
                new Tuple<UriType, string>(new UriType() { Uri = new Uri("\t", UriKind.Relative) }, "{\"Uri\":\"\t\"}"),
                new Tuple<UriType, string>(new UriType() { Uri = new Uri("ftp://127.0.0.1/") }, "{\"Uri\":\"ftp://127.0.0.1/\"}"),
                new Tuple<UriType, string>(new UriType() { Uri = new Uri("http://somehost/") }, "{\"Uri\":\"http://someHost\"}"),
                new Tuple<UriType, string>(new UriType() { Uri = new Uri("ftp://127.0.0.1/") }, "{\"Uri\":\"ftp://127.0.0.1/\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(UriType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                UriType actual = new UriType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.Uri, expected.Uri);

                if (testCase.Item2 != "{}")
                {
                    actual = new UriType();
                    actual.Uri = new Uri("http://xyz.com");
                    DefaultSerializer.Deserialize(input, actual);
                }

                Assert.Equal(actual.Uri, expected.Uri);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<UriType>(json).FirstOrDefault();

                Assert.Equal(actual.Uri, expected.Uri);

                actual = (UriType)DefaultSerializer.Deserialize<UriType>(input);

                Assert.Equal(actual.Uri, expected.Uri);
            }
        }

        [Fact]
        public void UriDeserializationNegative()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {
                new Tuple<string, string>("{\"Uri\":5}", "Error converting value 5 to type 'System.Uri'. Path 'Uri', line 1, position 8."),
                new Tuple<string, string>("{\"Uri\":true}", "Error converting value True to type 'System.Uri'. Path 'Uri', line 1, position 11."),
            };

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expectedError = testCase.Item2;

                UriType actual = new UriType();
                Exception actualError = Assert.ThrowsAny<Exception>(() =>
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(UriType));

                    DefaultSerializer.Deserialize(input, actual);
                });
                Assert.Equal(expectedError, actualError.Message);
            }
        }

        private static string ExpectedDateTimeSerialization(DateTime dt)
        {
            DateTime utc = dt.ToUniversalTime();
            return "\"" + utc.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK", CultureInfo.InvariantCulture) + "\"";
        }

        private static string ExpectedDateTimeOffsetSerialization(DateTimeOffset dto)
        {
            DateTime utc = dto.ToUniversalTime().UtcDateTime;
            return "\"" + utc.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK", CultureInfo.InvariantCulture) + "\"";
        }

        [Fact]
        public void DateTimeSerialization()
        {
            List<DateTime> dates = new List<DateTime>
            {
                new DateTime(1999, 12, 31, 23, 59, 59),
                new DateTime(2005, 3, 14, 12, 34, 16, DateTimeKind.Local),
                new DateTime(2005, 4, 14, 12, 34, 16, DateTimeKind.Unspecified),
                new DateTime(2005, 3, 14, 12, 34, 16, DateTimeKind.Local),
                new DateTime(2005, 5, 14, 12, 34, 16, DateTimeKind.Utc),
                new DateTime(2012, 2, 29, 12, 0, 0, DateTimeKind.Utc)
            };
            List<Tuple<DateTimeType, string>> testCases = new List<Tuple<DateTimeType, string>>
            {
                new Tuple<DateTimeType, string>(new DateTimeType() { DateTime = new DateTime() }, "{\"DateTime\":" + MinDateTimeSerializedToJson + "}"),
            };
            foreach (var date in dates)
            {
                testCases.Add(new Tuple<DateTimeType, string>(
                    new DateTimeType { DateTime = date },
                    "{\"DateTime\":" + ExpectedDateTimeSerialization(date) + "}"));
            }

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DateTimeDeserialization()
        {
            List<Tuple<DateTimeType, string>> testCases = new List<Tuple<DateTimeType, string>>()
            {
                new Tuple<DateTimeType, string>(new DateTimeType() { DateTime = new DateTime() }, "{}"),
                new Tuple<DateTimeType, string>(new DateTimeType() { DateTime = new DateTime() }, "{\"DateTime\":null}"),
                new Tuple<DateTimeType, string>(new DateTimeType() { DateTime = new DateTime() }, "{\"DateTime\":" + MinDateTimeSerializedToJson + "}")
            };

            List<DateTime> dates = new List<DateTime>
            {
                new DateTime(1999, 12, 31, 23, 59, 59),
                new DateTime(2005, 3, 14, 12, 34, 16, DateTimeKind.Local).ToLocalTime(),
                new DateTime(2005, 4, 14, 12, 34, 16, DateTimeKind.Unspecified),
                new DateTime(2005, 3, 14, 12, 34, 16, DateTimeKind.Local),
                new DateTime(2005, 5, 14, 12, 34, 16, DateTimeKind.Utc).ToLocalTime(),
                new DateTime(2012, 2, 29, 12, 0, 0, DateTimeKind.Utc).ToLocalTime() // leap day
            };

            foreach (var date in dates)
            {
                testCases.Add(new Tuple<DateTimeType, string>(
                    new DateTimeType { DateTime = date },
                    "{\"DateTime\":" + ExpectedDateTimeSerialization(date) + "}"));
            }

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                DateTimeType actual = new DateTimeType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.DateTime.ToUniversalTime(), expected.DateTime.ToUniversalTime());

                if (testCase.Item2 != "{}")
                {
                    actual = new DateTimeType();
                    actual.DateTime = DateTime.Now;
                    DefaultSerializer.Deserialize(input, actual);
                }

                Assert.Equal(actual.DateTime.ToUniversalTime(), expected.DateTime.ToUniversalTime());

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<DateTimeType>(json).FirstOrDefault();

                Assert.Equal(actual.DateTime.ToUniversalTime(), expected.DateTime.ToUniversalTime());

                actual = (DateTimeType)DefaultSerializer.Deserialize<DateTimeType>(input);

                Assert.Equal(actual.DateTime.ToUniversalTime(), expected.DateTime.ToUniversalTime());
            }
        }

        [Fact]
        public void DateTimeDeserializationNegative_InvalidString()
        {
            Assert.Throws<FormatException>(() =>
            {
                var input = "{\"DateTime\":\"I'm not a Date\"}";
                var actual = new DateTimeType();
                var serializer = DefaultSerializer;
                serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeType));
                serializer.Deserialize(input, actual);
            });
        }

        [Fact]
        public void DateTimeDeserializationNegative_Boolean()
        {
            Assert.Throws<JsonSerializationException>(() =>
            {
                var input = "{\"DateTime\":true}";
                var actual = new DateTimeType();
                var serializer = DefaultSerializer;
                serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeType));
                serializer.Deserialize(input, actual);
            });
        }

        [Fact]
        public void DateTimeDeserializationNegative_Number()
        {
            Assert.Throws<JsonSerializationException>(() =>
            {
                var input = "{\"DateTime\":5}";
                var actual = new DateTimeType();
                var serializer = DefaultSerializer;
                serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeType));
                serializer.Deserialize(input, actual);
            });
        }

        [Fact]
        public void DateTimeDeserializationNegative_String()
        {
            Assert.Throws<FormatException>(() =>
            {
                var input = "{\"DateTime\":\"\t\"}";
                var actual = new DateTimeType();
                var serializer = DefaultSerializer;
                serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeType));
                serializer.Deserialize(input, actual);
            });
        }

        [Fact]
        public void DateTimeOffsetSerialization()
        {
            List<Tuple<DateTimeOffsetType, string>> testCases = new List<Tuple<DateTimeOffsetType, string>>();

            List<DateTimeOffset> dates = new List<DateTimeOffset>
            {
                new DateTimeOffset(),
                new DateTimeOffset(new DateTime(1999, 12, 31, 23, 59, 59)),
                new DateTimeOffset(new DateTime(2005, 3, 14, 12, 34, 16, DateTimeKind.Local)),
                new DateTimeOffset(new DateTime(2005, 4, 14, 12, 34, 16, DateTimeKind.Unspecified)),
                new DateTimeOffset(new DateTime(2005, 3, 14, 12, 34, 16, DateTimeKind.Local)),
                new DateTimeOffset(new DateTime(2005, 5, 14, 12, 34, 16, DateTimeKind.Utc)),
                new DateTimeOffset(new DateTime(2012, 2, 29, 12, 0, 0, DateTimeKind.Utc)), // leap day
            };

            foreach (var date in dates)
            {
                testCases.Add(new Tuple<DateTimeOffsetType, string>(
                    new DateTimeOffsetType { DateTimeOffset = date },
                    "{\"DateTimeOffset\":" + ExpectedDateTimeOffsetSerialization(date) + "}"));
            }

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeOffsetType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DateTimeOffsetDeserialization()
        {
            List<Tuple<DateTimeOffsetType, string>> testCases = new List<Tuple<DateTimeOffsetType, string>>()
            {
                new Tuple<DateTimeOffsetType, string>(new DateTimeOffsetType() { DateTimeOffset = new DateTimeOffset() }, "{}"),
                new Tuple<DateTimeOffsetType, string>(new DateTimeOffsetType() { DateTimeOffset = new DateTimeOffset() }, "{\"DateTimeOffset\":null}"),
            };

            List<DateTimeOffset> dates = new List<DateTimeOffset>
            {
                new DateTimeOffset(new DateTime(1999, 12, 31, 23, 59, 59)).ToLocalTime(),
                new DateTimeOffset(new DateTime(2005, 3, 14, 12, 34, 16, DateTimeKind.Local)).ToLocalTime(),
                new DateTimeOffset(new DateTime(2005, 4, 14, 12, 34, 16, DateTimeKind.Unspecified)).ToLocalTime(),
                new DateTimeOffset(new DateTime(2005, 3, 14, 12, 34, 16, DateTimeKind.Local)).ToLocalTime(),
                new DateTimeOffset(new DateTime(2005, 5, 14, 12, 34, 16, DateTimeKind.Utc)).ToLocalTime(),
                new DateTimeOffset(new DateTime(2012, 2, 29, 12, 0, 0, DateTimeKind.Utc)).ToLocalTime(), // leap day
            };

            foreach (var date in dates)
            {
                testCases.Add(new Tuple<DateTimeOffsetType, string>(
                    new DateTimeOffsetType { DateTimeOffset = date },
                    "{\"DateTimeOffset\":" + ExpectedDateTimeOffsetSerialization(date) + "}"));
            }

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeOffsetType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                DateTimeOffsetType actual = new DateTimeOffsetType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.DateTimeOffset.ToLocalTime(), expected.DateTimeOffset.ToLocalTime());
                Assert.Equal(actual.DateTimeOffset.ToUniversalTime(), expected.DateTimeOffset.ToUniversalTime());

                if (testCase.Item2 != "{}")
                {
                    actual = new DateTimeOffsetType();
                    actual.DateTimeOffset = DateTimeOffset.Now;
                    DefaultSerializer.Deserialize(input, actual);
                }

                Assert.Equal(actual.DateTimeOffset.ToUniversalTime(), expected.DateTimeOffset.ToUniversalTime());

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<DateTimeOffsetType>(json).FirstOrDefault();

                Assert.Equal(actual.DateTimeOffset.ToUniversalTime(), expected.DateTimeOffset.ToUniversalTime());

                actual = (DateTimeOffsetType)DefaultSerializer.Deserialize<DateTimeOffsetType>(input);

                Assert.Equal(actual.DateTimeOffset.ToUniversalTime(), expected.DateTimeOffset.ToUniversalTime());
            }
        }

        [Fact]
        public void DateTimeOffsetDeserializationNegative_InvalidString()
        {
            var input = "{\"DateTime\":\"I'm not a Date\"}";
            var actual = new DateTimeOffsetType();
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeOffsetType));
            serializer.Deserialize(input, actual);
            Assert.Equal(DateTimeOffset.MinValue, actual.DateTimeOffset);
        }

        [Fact]
        public void DateTimeOffsetDeserializationNegative_Boolean()
        {
            var input = "{\"DateTime\":true}";
            var actual = new DateTimeOffsetType();
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeOffsetType));
            serializer.Deserialize(input, actual);
            Assert.Equal(DateTimeOffset.MinValue, actual.DateTimeOffset);
        }

        [Fact]
        public void DateTimeOffsetDeserializationNegative_Number()
        {
            var input = "{\"DateTime\":5}";
            var actual = new DateTimeOffsetType();
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeOffsetType));
            serializer.Deserialize(input, actual);
            Assert.Equal(DateTimeOffset.MinValue, actual.DateTimeOffset);
        }

        [Fact]
        public void DateTimeOffsetDeserializationNegative_String()
        {
            var input = "{\"DateTime\":\"\t\"}";
            var actual = new DateTimeOffsetType();
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeOffsetType));
            serializer.Deserialize(input, actual);
            Assert.Equal(DateTimeOffset.MinValue, actual.DateTimeOffset);
        }

        [Fact]
        public void NullableSerialization_1()
        {
            var input = new NullableType() { Nullable = null };
            var expected = "{\"Id\":0,\"Nullable\":null}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullableSerialization_2()
        {
            var input = new NullableType() { Nullable = 0 };
            var expected = "{\"Id\":0,\"Nullable\":0.0}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullableSerialization_3()
        {
            var input = new NullableType() { Nullable = -1 };
            var expected = "{\"Id\":0,\"Nullable\":-1.0}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullableSerialization_4()
        {
            var input = new NullableType() { Nullable = 1 };
            var expected = "{\"Id\":0,\"Nullable\":1.0}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullableSerialization_5()
        {
            var input = new NullableType() { Nullable = Double.Epsilon };
            var expected = "{\"Id\":0,\"Nullable\":5E-324}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullableSerialization_6()
        {
            var input = new NullableType() { Nullable = -Double.Epsilon };
            var expected = "{\"Id\":0,\"Nullable\":-5E-324}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullableSerialization_7()
        {
            var input = new NullableType() { Nullable = Double.MaxValue };
            var expected = "{\"Id\":0,\"Nullable\":1.7976931348623157E+308}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullableSerialization_8()
        {
            var input = new NullableType() { Nullable = Double.MinValue };
            var expected = "{\"Id\":0,\"Nullable\":-1.7976931348623157E+308}";
            var actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullableDeserialization()
        {
            List<Tuple<NullableType, string>> testCases = new List<Tuple<NullableType, string>>() {
                new Tuple<NullableType, string>(new NullableType() { Nullable = null }, "{\"Nullable\":null}"),
                new Tuple<NullableType, string>(new NullableType() { Nullable = null }, "{}"),
                new Tuple<NullableType, string>(new NullableType() { Nullable = 0 }, "{\"Nullable\":0}"),
                new Tuple<NullableType, string>(new NullableType() { Nullable = 0 }, "{\"Nullable\":4.94065645841247E-325}"),
                new Tuple<NullableType, string>(new NullableType() { Nullable = 0 }, "{\"Nullable\":4.94065645841247E-325}"),
                new Tuple<NullableType, string>(new NullableType() { Nullable = -1 }, "{\"Nullable\":-1}"),
                new Tuple<NullableType, string>(new NullableType() { Nullable = 1 }, "{\"Nullable\":1}"),
                new Tuple<NullableType, string>(new NullableType() { Nullable = double.Epsilon }, "{\"Nullable\":4.94065645841247E-324}"),
                new Tuple<NullableType, string>(new NullableType() { Nullable = -double.Epsilon }, "{\"Nullable\":-4.94065645841247E-324}"),
                new Tuple<NullableType, string>(new NullableType() { Nullable = double.MaxValue }, "{\"Nullable\":1.7976931348623157E+308}"),
                new Tuple<NullableType, string>(new NullableType() { Nullable = double.MinValue }, "{\"Nullable\":-1.7976931348623157E+308}"),
                new Tuple<NullableType, string>(new NullableType() { Nullable = double.MaxValue }, "{\"Nullable\":1.79769313486231575E+308}"),  // We're ok with lossing precision here; note the extra 5 digit at the end
                new Tuple<NullableType, string>(new NullableType() { Nullable = double.MinValue }, "{\"Nullable\":-1.79769313486231575E+308}"), // We're ok with lossing precision here; note the extra 5 digit at the end
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(NullableType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                NullableType actual = new NullableType();
                DefaultSerializer.Deserialize(input, actual);

                if (actual.Nullable.HasValue)
                {
                    if (double.IsPositiveInfinity(expected.Nullable.Value))
                    {
                        Assert.True(double.IsPositiveInfinity(actual.Nullable.Value));
                    }
                    else if (double.IsNegativeInfinity(expected.Nullable.Value))
                    {
                        Assert.True(double.IsNegativeInfinity(actual.Nullable.Value));
                    }
                    else
                    {
                        Assert.Equal(actual.Nullable.Value, expected.Nullable.Value);
                    }
                }

                if (testCase.Item2 != "{}")
                {
                    actual = new NullableType();
                    actual.Nullable = 34.6;
                    DefaultSerializer.Deserialize(input, actual);

                    if (actual.Nullable.HasValue)
                    {
                        if (double.IsPositiveInfinity(expected.Nullable.Value))
                        {
                            Assert.True(double.IsPositiveInfinity(actual.Nullable.Value));
                        }
                        else if (double.IsNegativeInfinity(expected.Nullable.Value))
                        {
                            Assert.True(double.IsNegativeInfinity(actual.Nullable.Value));
                        }
                        else
                        {
                            Assert.Equal(actual.Nullable.Value, expected.Nullable.Value);
                        }
                    }
                }

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<NullableType>(json).FirstOrDefault();

                if (actual.Nullable.HasValue)
                {
                    if (double.IsPositiveInfinity(expected.Nullable.Value))
                    {
                        Assert.True(double.IsPositiveInfinity(actual.Nullable.Value));
                    }
                    else if (double.IsNegativeInfinity(expected.Nullable.Value))
                    {
                        Assert.True(double.IsNegativeInfinity(actual.Nullable.Value));
                    }
                    else
                    {
                        Assert.Equal(actual.Nullable.Value, expected.Nullable.Value);
                    }
                }

                actual = DefaultSerializer.Deserialize<NullableType>(input);

                if (actual.Nullable.HasValue)
                {
                    if (double.IsPositiveInfinity(expected.Nullable.Value))
                    {
                        Assert.True(double.IsPositiveInfinity(actual.Nullable.Value));
                    }
                    else if (double.IsNegativeInfinity(expected.Nullable.Value))
                    {
                        Assert.True(double.IsNegativeInfinity(actual.Nullable.Value));
                    }
                    else
                    {
                        Assert.Equal(actual.Nullable.Value, expected.Nullable.Value);
                    }
                }
            }
        }

        [Fact]
        public void NullableDeserializationNegative_1()
        {
            var input = "{\"Nullable\":1.7976931348623157E+309}";
            var actual = new NullableType();
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(NullableType));
            serializer.Deserialize(input, actual);
            Assert.Equal(Double.PositiveInfinity, actual.Nullable);
        }

        [Fact]
        public void NullableDeserializationNegative_2()
        {
            var input = "{\"Nullable\":-1.7976931348623157E+309}";
            var actual = new NullableType();
            var serializer = DefaultSerializer;
            serializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(NullableType));
            serializer.Deserialize(input, actual);
            Assert.Equal(Double.NegativeInfinity, actual.Nullable);
        }

        [Fact]
        public void PocoSerialization()
        {
            List<Tuple<PocoType, string>> testCases = new List<Tuple<PocoType, string>>() {
                new Tuple<PocoType, string>(new PocoType(), "{\"PublicField\":null,\"PublicProperty\":null}"),
                new Tuple<PocoType, string>(new PocoType("_XYZ"), "{\"PublicField\":\"PublicField_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);


                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PocoDeserialization()
        {
            List<Tuple<PocoType, string>> testCases = new List<Tuple<PocoType, string>>() {
                new Tuple<PocoType, string>(new PocoType(), "{\"PublicProperty\":null,\"PublicField\":null}"),
                new Tuple<PocoType, string>(new PocoType("_XYZ", onlySetSerializableMembers: true), "{\"PublicProperty\":\"PublicProperty_XYZ\",\"PublicField\":\"PublicField_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                PocoType actual = new PocoType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                actual = new PocoType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<PocoType>(json).FirstOrDefault();

                Assert.Equal(expected, actual);

                actual = (PocoType)DefaultSerializer.Deserialize<PocoType>(input);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PocoPopulation()
        {
            List<Tuple<PocoType, string>> testCases = new List<Tuple<PocoType, string>>() {
                new Tuple<PocoType, string>(new PocoType(), "{\"PublicProperty\":null,\"PublicField\":null}"),
                new Tuple<PocoType, string>(new PocoType("_XYZ", onlySetSerializableMembers: true), "{\"PublicProperty\":\"PublicProperty_XYZ\",\"PublicField\":\"PublicField_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                PocoType actual = new PocoType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DataContractSerialization()
        {
            List<Tuple<DataContractType, string>> testCases = new List<Tuple<DataContractType, string>>() {
                new Tuple<DataContractType, string>(new DataContractType(), "{\"PublicFieldDataMember\":null,\"InternalFieldDataMember\":null,\"PublicField\":null,\"InternalField\":null,\"PublicPropertyDataMember\":null,\"InternalPropertyDataMember\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateFieldDataMember\":null,\"PrivateField\":null,\"PrivatePropertyDataMember\":null,\"PrivateProperty\":null}"),
                new Tuple<DataContractType, string>(new DataContractType("_XYZ"), "{\"PublicFieldDataMember\":\"PublicFieldNamedDataMember_XYZ\",\"InternalFieldDataMember\":\"InternalFieldNamedDataMember_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PublicPropertyDataMember\":\"PublicPropertyNamedDataMember_XYZ\",\"InternalPropertyDataMember\":\"InternalPropertyNamedDataMember_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PrivateFieldDataMember\":\"PrivateFieldNamedDataMember_XYZ\",\"PrivateField\":\"PrivateField_XYZ\",\"PrivatePropertyDataMember\":\"PrivatePropertyNamedDataMember_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\"}")
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DataContractType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DataContractDeserialization()
        {
            List<Tuple<DataContractType, string>> testCases = new List<Tuple<DataContractType, string>>() {
                new Tuple<DataContractType, string>(new DataContractType(), "{\"PublicPropertyDataMember\":null,\"InternalPropertyDataMember\":null,\"PrivatePropertyDataMember\":null,\"PublicPropertyGetOnlyDataMember\":\"PublicPropertyGetOnlyNamedDataMember\",\"InternalPropertyGetOnlyDataMember\":\"InternalPropertyGetOnlyNamedDataMember\",\"PrivatePropertyGetOnlyDataMember\":\"PrivatePropertyGetOnlyNamedDataMember\",\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateProperty\":null,\"PublicPropertyGetOnly\":\"PublicPropertyGetOnly\",\"InternalPropertyGetOnly\":\"InternalPropertyGetOnly\",\"PrivatePropertyGetOnly\":\"PrivatePropertyGetOnly\",\"PublicFieldDataMember\":null,\"InternalFieldDataMember\":null,\"PrivateFieldDataMember\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateField\":null}"),
                new Tuple<DataContractType, string>(new DataContractType("_XYZ", onlySetSerializableMembers: true), "{\"PublicPropertyDataMember\":\"PublicPropertyNamedDataMember_XYZ\",\"InternalPropertyDataMember\":\"InternalPropertyNamedDataMember_XYZ\",\"PrivatePropertyDataMember\":\"PrivatePropertyNamedDataMember_XYZ\",\"PublicPropertyGetOnlyDataMember\":\"PublicPropertyGetOnlyNamedDataMember\",\"InternalPropertyGetOnlyDataMember\":\"InternalPropertyGetOnlyNamedDataMember\",\"PrivatePropertyGetOnlyDataMember\":\"PrivatePropertyGetOnlyNamedDataMember\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\",\"PublicPropertyGetOnly\":\"PublicPropertyGetOnly\",\"InternalPropertyGetOnly\":\"InternalPropertyGetOnly\",\"PrivatePropertyGetOnly\":\"PrivatePropertyGetOnly\",\"PublicFieldDataMember\":\"PublicFieldNamedDataMember_XYZ\",\"InternalFieldDataMember\":\"InternalFieldNamedDataMember_XYZ\",\"PrivateFieldDataMember\":\"PrivateFieldNamedDataMember_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PrivateField\":\"PrivateField_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DataContractType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                DataContractType actual = new DataContractType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                actual = new DataContractType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<DataContractType>(json).FirstOrDefault();

                Assert.Equal(expected, actual);

                actual = DefaultSerializer.Deserialize<DataContractType>(input);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DataContractPopulation()
        {
            List<Tuple<DataContractType, string>> testCases = new List<Tuple<DataContractType, string>>() {
                new Tuple<DataContractType, string>(new DataContractType(), "{\"PublicPropertyDataMember\":null,\"InternalPropertyDataMember\":null,\"PrivatePropertyDataMember\":null,\"PublicPropertyGetOnlyDataMember\":\"PublicPropertyGetOnlyNamedDataMember\",\"InternalPropertyGetOnlyDataMember\":\"InternalPropertyGetOnlyNamedDataMember\",\"PrivatePropertyGetOnlyDataMember\":\"PrivatePropertyGetOnlyNamedDataMember\",\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateProperty\":null,\"PublicPropertyGetOnly\":\"PublicPropertyGetOnly\",\"InternalPropertyGetOnly\":\"InternalPropertyGetOnly\",\"PrivatePropertyGetOnly\":\"PrivatePropertyGetOnly\",\"PublicFieldDataMember\":null,\"InternalFieldDataMember\":null,\"PrivateFieldDataMember\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateField\":null}"),
                new Tuple<DataContractType, string>(new DataContractType("_XYZ", onlySetSerializableMembers: true), "{\"PublicPropertyDataMember\":\"PublicPropertyNamedDataMember_XYZ\",\"InternalPropertyDataMember\":\"InternalPropertyNamedDataMember_XYZ\",\"PrivatePropertyDataMember\":\"PrivatePropertyNamedDataMember_XYZ\",\"PublicPropertyGetOnlyDataMember\":\"PublicPropertyGetOnlyNamedDataMember\",\"InternalPropertyGetOnlyDataMember\":\"InternalPropertyGetOnlyNamedDataMember\",\"PrivatePropertyGetOnlyDataMember\":\"PrivatePropertyGetOnlyNamedDataMember\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\",\"PublicPropertyGetOnly\":\"PublicPropertyGetOnly\",\"InternalPropertyGetOnly\":\"InternalPropertyGetOnly\",\"PrivatePropertyGetOnly\":\"PrivatePropertyGetOnly\",\"PublicFieldDataMember\":\"PublicFieldNamedDataMember_XYZ\",\"InternalFieldDataMember\":\"InternalFieldNamedDataMember_XYZ\",\"PrivateFieldDataMember\":\"PrivateFieldNamedDataMember_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PrivateField\":\"PrivateField_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DataContractType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                DataContractType actual = new DataContractType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void JsonPropertySerialization()
        {
            List<Tuple<JsonPropertyType, string>> testCases = new List<Tuple<JsonPropertyType, string>>() {
                new Tuple<JsonPropertyType, string>(new JsonPropertyType(), "{\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PublicField\":null,\"InternalField\":null,\"PublicFieldSansAttribute\":null,\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PublicPropertySansAttribute\":null,\"PrivateFieldJsonProperty\":null,\"PrivateField\":null,\"PrivatePropertyJsonProperty\":null,\"PrivateProperty\":null}"),
                new Tuple<JsonPropertyType, string>(new JsonPropertyType("_XYZ"), "{\"PublicFieldJsonProperty\":\"PublicFieldNamedJsonProperty_XYZ\",\"InternalFieldJsonProperty\":\"InternalFieldNamedJsonProperty_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PublicFieldSansAttribute\":\"PublicFieldSansAttribute_XYZ\",\"PublicPropertyJsonProperty\":\"PublicPropertyNamedJsonProperty_XYZ\",\"InternalPropertyJsonProperty\":\"InternalPropertyNamedJsonProperty_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PublicPropertySansAttribute\":\"PublicPropertySansAttribute_XYZ\",\"PrivateFieldJsonProperty\":\"PrivateFieldNamedJsonProperty_XYZ\",\"PrivateField\":\"PrivateField_XYZ\",\"PrivatePropertyJsonProperty\":\"PrivatePropertyNamedJsonProperty_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(JsonPropertyType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void JsonPropertyDeserialization()
        {
            List<Tuple<JsonPropertyType, string>> testCases = new List<Tuple<JsonPropertyType, string>>() {
                new Tuple<JsonPropertyType, string>(new JsonPropertyType(), "{\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PrivateFieldJsonProperty\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateField\":null,\"PublicFieldSansAttribute\":null,\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PrivatePropertyJsonProperty\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateProperty\":null,\"PublicPropertySansAttribute\":null}"),
                new Tuple<JsonPropertyType, string>(new JsonPropertyType("_XYZ",  onlySetSerializableMembers: true), "{\"PublicFieldJsonProperty\":\"PublicFieldNamedJsonProperty_XYZ\",\"InternalFieldJsonProperty\":\"InternalFieldNamedJsonProperty_XYZ\",\"PrivateFieldJsonProperty\":\"PrivateFieldNamedJsonProperty_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PrivateField\":\"PrivateField_XYZ\",\"PublicFieldSansAttribute\":\"PublicFieldSansAttribute_XYZ\",\"PublicPropertyJsonProperty\":\"PublicPropertyNamedJsonProperty_XYZ\",\"InternalPropertyJsonProperty\":\"InternalPropertyNamedJsonProperty_XYZ\",\"PrivatePropertyJsonProperty\":\"PrivatePropertyNamedJsonProperty_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\",\"PublicPropertySansAttribute\":\"PublicPropertySansAttribute_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(JsonPropertyType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                JsonPropertyType actual = new JsonPropertyType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                actual = new JsonPropertyType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<JsonPropertyType>(json).FirstOrDefault();

                Assert.Equal(expected, actual);

                actual = DefaultSerializer.Deserialize<JsonPropertyType>(input);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void JsonPropertyPopulation()
        {
            List<Tuple<JsonPropertyType, string>> testCases = new List<Tuple<JsonPropertyType, string>>() {
                new Tuple<JsonPropertyType, string>(new JsonPropertyType(), "{\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PrivateFieldJsonProperty\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateField\":null,\"PublicFieldSansAttribute\":null,\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PrivatePropertyJsonProperty\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateProperty\":null,\"PublicPropertySansAttribute\":null}"),
                new Tuple<JsonPropertyType, string>(new JsonPropertyType("_XYZ",  onlySetSerializableMembers: true), "{\"PublicFieldJsonProperty\":\"PublicFieldNamedJsonProperty_XYZ\",\"InternalFieldJsonProperty\":\"InternalFieldNamedJsonProperty_XYZ\",\"PrivateFieldJsonProperty\":\"PrivateFieldNamedJsonProperty_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PrivateField\":\"PrivateField_XYZ\",\"PublicFieldSansAttribute\":\"PublicFieldSansAttribute_XYZ\",\"PublicPropertyJsonProperty\":\"PublicPropertyNamedJsonProperty_XYZ\",\"InternalPropertyJsonProperty\":\"InternalPropertyNamedJsonProperty_XYZ\",\"PrivatePropertyJsonProperty\":\"PrivatePropertyNamedJsonProperty_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\",\"PublicPropertySansAttribute\":\"PublicPropertySansAttribute_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(JsonPropertyType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                JsonPropertyType actual = new JsonPropertyType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PocoDerivedPocoSerialization()
        {
            List<Tuple<PocoDerivedPocoType, string>> testCases = new List<Tuple<PocoDerivedPocoType, string>>() {
                new Tuple<PocoDerivedPocoType, string>(new PocoDerivedPocoType(), "{\"DerivedPublicField\":null,\"PublicField\":null,\"DerivedPublicProperty\":null,\"PublicProperty\":null}"),
                new Tuple<PocoDerivedPocoType, string>(new PocoDerivedPocoType("_XYZ"), "{\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoDerivedPocoType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PocoDerivedPocoDeserialization()
        {
            List<Tuple<PocoDerivedPocoType, string>> testCases = new List<Tuple<PocoDerivedPocoType, string>>() {
                new Tuple<PocoDerivedPocoType, string>(new PocoDerivedPocoType(), "{\"DerivedPublicProperty\":null,\"PublicProperty\":null,\"DerivedPublicField\":null,\"PublicField\":null}"),
                new Tuple<PocoDerivedPocoType, string>(new PocoDerivedPocoType("_XYZ", onlySetSerializableMembers: true), "{\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"PublicField\":\"PublicField_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoDerivedPocoType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                PocoDerivedPocoType actual = new PocoDerivedPocoType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                actual = new PocoDerivedPocoType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<PocoDerivedPocoType>(json).FirstOrDefault();

                Assert.Equal(expected, actual);

                actual = DefaultSerializer.Deserialize<PocoDerivedPocoType>(input);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PocoDerivedPocoPopulation()
        {
            List<Tuple<PocoDerivedPocoType, string>> testCases = new List<Tuple<PocoDerivedPocoType, string>>() {
                new Tuple<PocoDerivedPocoType, string>(new PocoDerivedPocoType(), "{\"DerivedPublicProperty\":null,\"PublicProperty\":null,\"DerivedPublicField\":null,\"PublicField\":null}"),
                new Tuple<PocoDerivedPocoType, string>(new PocoDerivedPocoType("_XYZ", onlySetSerializableMembers: true), "{\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"PublicField\":\"PublicField_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoDerivedPocoType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                PocoDerivedPocoType actual = new PocoDerivedPocoType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PocoDerivedDataContractSerialization()
        {
            List<Tuple<PocoDerivedDataContractType, string>> testCases = new List<Tuple<PocoDerivedDataContractType, string>>() {
                new Tuple<PocoDerivedDataContractType, string>(new PocoDerivedDataContractType(), "{\"PublicFieldDataMember\":null,\"InternalFieldDataMember\":null,\"PrivateFieldDataMember\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicPropertyDataMember\":null,\"InternalPropertyDataMember\":null,\"PrivatePropertyDataMember\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null}"),
                new Tuple<PocoDerivedDataContractType, string>(new PocoDerivedDataContractType("_XYZ"), "{\"PublicFieldDataMember\":\"PublicFieldNamedDataMember_XYZ\",\"InternalFieldDataMember\":\"InternalFieldNamedDataMember_XYZ\",\"PrivateFieldDataMember\":\"PrivateFieldNamedDataMember_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicPropertyDataMember\":\"PublicPropertyNamedDataMember_XYZ\",\"InternalPropertyDataMember\":\"InternalPropertyNamedDataMember_XYZ\",\"PrivatePropertyDataMember\":\"PrivatePropertyNamedDataMember_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoDerivedDataContractType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PocoDerivedDataContractDeserialization()
        {
            List<Tuple<PocoDerivedDataContractType, string>> testCases = new List<Tuple<PocoDerivedDataContractType, string>>() {
                new Tuple<PocoDerivedDataContractType, string>(new PocoDerivedDataContractType(), "{\"PublicPropertyDataMember\":null,\"InternalPropertyDataMember\":null,\"PrivatePropertyDataMember\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicFieldDataMember\":null,\"InternalFieldDataMember\":null,\"PrivateFieldDataMember\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null}"),
                new Tuple<PocoDerivedDataContractType, string>(new PocoDerivedDataContractType("_XYZ", onlySetSerializableMembers:true), "{\"PublicPropertyDataMember\":\"PublicPropertyNamedDataMember_XYZ\",\"InternalPropertyDataMember\":\"InternalPropertyNamedDataMember_XYZ\",\"PrivatePropertyDataMember\":\"PrivatePropertyNamedDataMember_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicFieldDataMember\":\"PublicFieldNamedDataMember_XYZ\",\"InternalFieldDataMember\":\"InternalFieldNamedDataMember_XYZ\",\"PrivateFieldDataMember\":\"PrivateFieldNamedDataMember_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoDerivedDataContractType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                PocoDerivedDataContractType actual = new PocoDerivedDataContractType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                actual = new PocoDerivedDataContractType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<PocoDerivedDataContractType>(json).FirstOrDefault();

                Assert.Equal(expected, actual);

                actual = DefaultSerializer.Deserialize<PocoDerivedDataContractType>(input);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PocoDerivedDataContractPopulation()
        {
            List<Tuple<PocoDerivedDataContractType, string>> testCases = new List<Tuple<PocoDerivedDataContractType, string>>() {
                new Tuple<PocoDerivedDataContractType, string>(new PocoDerivedDataContractType(), "{\"PublicPropertyDataMember\":null,\"InternalPropertyDataMember\":null,\"PrivatePropertyDataMember\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicFieldDataMember\":null,\"InternalFieldDataMember\":null,\"PrivateFieldDataMember\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null}"),
                new Tuple<PocoDerivedDataContractType, string>(new PocoDerivedDataContractType("_XYZ", onlySetSerializableMembers:true), "{\"PublicPropertyDataMember\":\"PublicPropertyNamedDataMember_XYZ\",\"InternalPropertyDataMember\":\"InternalPropertyNamedDataMember_XYZ\",\"PrivatePropertyDataMember\":\"PrivatePropertyNamedDataMember_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicFieldDataMember\":\"PublicFieldNamedDataMember_XYZ\",\"InternalFieldDataMember\":\"InternalFieldNamedDataMember_XYZ\",\"PrivateFieldDataMember\":\"PrivateFieldNamedDataMember_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoDerivedDataContractType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                PocoDerivedDataContractType actual = new PocoDerivedDataContractType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PocoDerivedJsonPropertySerialization()
        {
            List<Tuple<PocoDerivedJsonPropertyType, string>> testCases = new List<Tuple<PocoDerivedJsonPropertyType, string>>() {
                new Tuple<PocoDerivedJsonPropertyType, string>(new PocoDerivedJsonPropertyType(), "{\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PrivateFieldJsonProperty\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicFieldSansAttribute\":null,\"PublicField\":null,\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PrivatePropertyJsonProperty\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicPropertySansAttribute\":null,\"PublicProperty\":null}"),
                new Tuple<PocoDerivedJsonPropertyType, string>(new PocoDerivedJsonPropertyType("_XYZ"), "{\"PublicFieldJsonProperty\":\"PublicFieldNamedJsonProperty_XYZ\",\"InternalFieldJsonProperty\":\"InternalFieldNamedJsonProperty_XYZ\",\"PrivateFieldJsonProperty\":\"PrivateFieldNamedJsonProperty_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicFieldSansAttribute\":\"PublicFieldSansAttribute_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"PublicPropertyJsonProperty\":\"PublicPropertyNamedJsonProperty_XYZ\",\"InternalPropertyJsonProperty\":\"InternalPropertyNamedJsonProperty_XYZ\",\"PrivatePropertyJsonProperty\":\"PrivatePropertyNamedJsonProperty_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicPropertySansAttribute\":\"PublicPropertySansAttribute_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoDerivedJsonPropertyType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PocoDerivedJsonPropertyDeserialization()
        {
            List<Tuple<PocoDerivedJsonPropertyType, string>> testCases = new List<Tuple<PocoDerivedJsonPropertyType, string>>() {
                new Tuple<PocoDerivedJsonPropertyType, string>(new PocoDerivedJsonPropertyType(), "{\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PrivatePropertyJsonProperty\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PrivateFieldJsonProperty\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicPropertySansAttribute\":null,\"PublicProperty\":null,\"PublicFieldSansAttribute\":null,\"PublicField\":null}"),
                new Tuple<PocoDerivedJsonPropertyType, string>(new PocoDerivedJsonPropertyType("_XYZ", onlySetSerializableMembers: true), "{\"PublicPropertyJsonProperty\":\"PublicPropertyNamedJsonProperty_XYZ\",\"InternalPropertyJsonProperty\":\"InternalPropertyNamedJsonProperty_XYZ\",\"PrivatePropertyJsonProperty\":\"PrivatePropertyNamedJsonProperty_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicFieldJsonProperty\":\"PublicFieldNamedJsonProperty_XYZ\",\"InternalFieldJsonProperty\":\"InternalFieldNamedJsonProperty_XYZ\",\"PrivateFieldJsonProperty\":\"PrivateFieldNamedJsonProperty_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicPropertySansAttribute\":\"PublicPropertySansAttribute_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"PublicFieldSansAttribute\":\"PublicFieldSansAttribute_XYZ\",\"PublicField\":\"PublicField_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoDerivedJsonPropertyType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                PocoDerivedJsonPropertyType actual = new PocoDerivedJsonPropertyType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                actual = new PocoDerivedJsonPropertyType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<PocoDerivedJsonPropertyType>(json).FirstOrDefault();

                Assert.Equal(expected, actual);

                actual = DefaultSerializer.Deserialize<PocoDerivedJsonPropertyType>(input);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void PocoDerivedJsonPropertyPopulation()
        {
            List<Tuple<PocoDerivedJsonPropertyType, string>> testCases = new List<Tuple<PocoDerivedJsonPropertyType, string>>() {
                new Tuple<PocoDerivedJsonPropertyType, string>(new PocoDerivedJsonPropertyType(), "{\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PrivatePropertyJsonProperty\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PrivateFieldJsonProperty\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicPropertySansAttribute\":null,\"PublicProperty\":null,\"PublicFieldSansAttribute\":null,\"PublicField\":null}"),
                new Tuple<PocoDerivedJsonPropertyType, string>(new PocoDerivedJsonPropertyType("_XYZ", onlySetSerializableMembers: true), "{\"PublicPropertyJsonProperty\":\"PublicPropertyNamedJsonProperty_XYZ\",\"InternalPropertyJsonProperty\":\"InternalPropertyNamedJsonProperty_XYZ\",\"PrivatePropertyJsonProperty\":\"PrivatePropertyNamedJsonProperty_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicFieldJsonProperty\":\"PublicFieldNamedJsonProperty_XYZ\",\"InternalFieldJsonProperty\":\"InternalFieldNamedJsonProperty_XYZ\",\"PrivateFieldJsonProperty\":\"PrivateFieldNamedJsonProperty_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicPropertySansAttribute\":\"PublicPropertySansAttribute_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"PublicFieldSansAttribute\":\"PublicFieldSansAttribute_XYZ\",\"PublicField\":\"PublicField_XYZ\"}" ),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(PocoDerivedJsonPropertyType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                PocoDerivedJsonPropertyType actual = new PocoDerivedJsonPropertyType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DataContractDerivedDataContractSerialization()
        {
            List<Tuple<DataContractDerivedDataContractType, string>> testCases = new List<Tuple<DataContractDerivedDataContractType, string>>() {
                new Tuple<DataContractDerivedDataContractType, string>(new DataContractDerivedDataContractType(), "{\"DerivedPublicFieldDataMember\":null,\"DerivedInternalFieldDataMember\":null,\"DerivedPrivateFieldDataMember\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicFieldDataMember\":null,\"InternalFieldDataMember\":null,\"PublicField\":null,\"InternalField\":null,\"DerivedPublicPropertyDataMember\":null,\"DerivedInternalPropertyDataMember\":null,\"DerivedPrivatePropertyDataMember\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicPropertyDataMember\":null,\"InternalPropertyDataMember\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateFieldDataMember\":null,\"PrivateField\":null,\"PrivatePropertyDataMember\":null,\"PrivateProperty\":null}"),
                new Tuple<DataContractDerivedDataContractType, string>(new DataContractDerivedDataContractType("_XYZ"), "{\"DerivedPublicFieldDataMember\":\"DerivedPublicFieldNamedDataMember_XYZ\",\"DerivedInternalFieldDataMember\":\"DerivedInternalFieldNamedDataMember_XYZ\",\"DerivedPrivateFieldDataMember\":\"DerivedPrivateFieldNamedDataMember_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicFieldDataMember\":\"PublicFieldNamedDataMember_XYZ\",\"InternalFieldDataMember\":\"InternalFieldNamedDataMember_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"DerivedPublicPropertyDataMember\":\"DerivedPublicPropertyNamedDataMember_XYZ\",\"DerivedInternalPropertyDataMember\":\"DerivedInternalPropertyNamedDataMember_XYZ\",\"DerivedPrivatePropertyDataMember\":\"DerivedPrivatePropertyNamedDataMember_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicPropertyDataMember\":\"PublicPropertyNamedDataMember_XYZ\",\"InternalPropertyDataMember\":\"InternalPropertyNamedDataMember_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PrivateFieldDataMember\":\"PrivateFieldNamedDataMember_XYZ\",\"PrivateField\":\"PrivateField_XYZ\",\"PrivatePropertyDataMember\":\"PrivatePropertyNamedDataMember_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\"}")
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DataContractDerivedDataContractType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DataContractDerivedDataContractDeserialization()
        {
            List<Tuple<DataContractDerivedDataContractType, string>> testCases = new List<Tuple<DataContractDerivedDataContractType, string>>() {
                new Tuple<DataContractDerivedDataContractType, string>(new DataContractDerivedDataContractType(), "{\"DerivedPublicPropertyDataMember\":null,\"DerivedInternalPropertyDataMember\":null,\"DerivedPrivatePropertyDataMember\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicPropertyDataMember\":null,\"InternalPropertyDataMember\":null,\"PrivatePropertyDataMember\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateProperty\":null,\"DerivedPublicFieldDataMember\":null,\"DerivedInternalFieldDataMember\":null,\"DerivedPrivateFieldDataMember\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicFieldDataMember\":null,\"InternalFieldDataMember\":null,\"PrivateFieldDataMember\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateField\":null}"),
                new Tuple<DataContractDerivedDataContractType, string>(new DataContractDerivedDataContractType("_XYZ", onlySetSerializableMembers: true), "{\"DerivedPublicPropertyDataMember\":\"DerivedPublicPropertyNamedDataMember_XYZ\",\"DerivedInternalPropertyDataMember\":\"DerivedInternalPropertyNamedDataMember_XYZ\",\"DerivedPrivatePropertyDataMember\":\"DerivedPrivatePropertyNamedDataMember_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicPropertyDataMember\":\"PublicPropertyNamedDataMember_XYZ\",\"InternalPropertyDataMember\":\"InternalPropertyNamedDataMember_XYZ\",\"PrivatePropertyDataMember\":\"PrivatePropertyNamedDataMember_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\",\"DerivedPublicFieldDataMember\":\"DerivedPublicFieldNamedDataMember_XYZ\",\"DerivedInternalFieldDataMember\":\"DerivedInternalFieldNamedDataMember_XYZ\",\"DerivedPrivateFieldDataMember\":\"DerivedPrivateFieldNamedDataMember_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicFieldDataMember\":\"PublicFieldNamedDataMember_XYZ\",\"InternalFieldDataMember\":\"InternalFieldNamedDataMember_XYZ\",\"PrivateFieldDataMember\":\"PrivateFieldNamedDataMember_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PrivateField\":\"PrivateField_XYZ\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DataContractDerivedDataContractType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                DataContractDerivedDataContractType actual = new DataContractDerivedDataContractType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                actual = new DataContractDerivedDataContractType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<DataContractDerivedDataContractType>(json).FirstOrDefault();

                Assert.Equal(expected, actual);

                actual = DefaultSerializer.Deserialize<DataContractDerivedDataContractType>(input);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DataContractDerivedDataContractPopulation()
        {
            List<Tuple<DataContractDerivedDataContractType, string>> testCases = new List<Tuple<DataContractDerivedDataContractType, string>>() {
                new Tuple<DataContractDerivedDataContractType, string>(new DataContractDerivedDataContractType(), "{\"DerivedPublicPropertyDataMember\":null,\"DerivedInternalPropertyDataMember\":null,\"DerivedPrivatePropertyDataMember\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicPropertyDataMember\":null,\"InternalPropertyDataMember\":null,\"PrivatePropertyDataMember\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateProperty\":null,\"DerivedPublicFieldDataMember\":null,\"DerivedInternalFieldDataMember\":null,\"DerivedPrivateFieldDataMember\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicFieldDataMember\":null,\"InternalFieldDataMember\":null,\"PrivateFieldDataMember\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateField\":null}"),
                new Tuple<DataContractDerivedDataContractType, string>(new DataContractDerivedDataContractType("_XYZ", onlySetSerializableMembers: true), "{\"DerivedPublicPropertyDataMember\":\"DerivedPublicPropertyNamedDataMember_XYZ\",\"DerivedInternalPropertyDataMember\":\"DerivedInternalPropertyNamedDataMember_XYZ\",\"DerivedPrivatePropertyDataMember\":\"DerivedPrivatePropertyNamedDataMember_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicPropertyDataMember\":\"PublicPropertyNamedDataMember_XYZ\",\"InternalPropertyDataMember\":\"InternalPropertyNamedDataMember_XYZ\",\"PrivatePropertyDataMember\":\"PrivatePropertyNamedDataMember_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\",\"DerivedPublicFieldDataMember\":\"DerivedPublicFieldNamedDataMember_XYZ\",\"DerivedInternalFieldDataMember\":\"DerivedInternalFieldNamedDataMember_XYZ\",\"DerivedPrivateFieldDataMember\":\"DerivedPrivateFieldNamedDataMember_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicFieldDataMember\":\"PublicFieldNamedDataMember_XYZ\",\"InternalFieldDataMember\":\"InternalFieldNamedDataMember_XYZ\",\"PrivateFieldDataMember\":\"PrivateFieldNamedDataMember_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PrivateField\":\"PrivateField_XYZ\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DataContractDerivedDataContractType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                DataContractDerivedDataContractType actual = new DataContractDerivedDataContractType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DataContractDerivedJsonPropertySerialization()
        {
            List<Tuple<DataContractDerivedJsonPropertyType, string>> testCases = new List<Tuple<DataContractDerivedJsonPropertyType, string>>() {
                new Tuple<DataContractDerivedJsonPropertyType, string>(new DataContractDerivedJsonPropertyType(), "{\"DerivedPublicFieldJsonProperty\":null,\"DerivedInternalFieldJsonProperty\":null,\"DerivedPrivateFieldJsonProperty\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicFieldDataMember\":null,\"InternalFieldDataMember\":null,\"PublicField\":null,\"InternalField\":null,\"DerivedPublicPropertyJsonProperty\":null,\"DerivedInternalPropertyJsonProperty\":null,\"DerivedPrivatePropertyJsonProperty\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicPropertyDataMember\":null,\"InternalPropertyDataMember\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateFieldDataMember\":null,\"PrivateField\":null,\"PrivatePropertyDataMember\":null,\"PrivateProperty\":null}"),
                new Tuple<DataContractDerivedJsonPropertyType, string>(new DataContractDerivedJsonPropertyType("_XYZ"), "{\"DerivedPublicFieldJsonProperty\":\"DerivedPublicFieldNamedJsonProperty_XYZ\",\"DerivedInternalFieldJsonProperty\":\"DerivedInternalFieldNamedJsonProperty_XYZ\",\"DerivedPrivateFieldJsonProperty\":\"DerivedPrivateFieldNamedJsonProperty_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicFieldDataMember\":\"PublicFieldNamedDataMember_XYZ\",\"InternalFieldDataMember\":\"InternalFieldNamedDataMember_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"DerivedPublicPropertyJsonProperty\":\"DerivedPublicPropertyNamedJsonProperty_XYZ\",\"DerivedInternalPropertyJsonProperty\":\"DerivedInternalPropertyNamedJsonProperty_XYZ\",\"DerivedPrivatePropertyJsonProperty\":\"DerivedPrivatePropertyNamedJsonProperty_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicPropertyDataMember\":\"PublicPropertyNamedDataMember_XYZ\",\"InternalPropertyDataMember\":\"InternalPropertyNamedDataMember_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PrivateFieldDataMember\":\"PrivateFieldNamedDataMember_XYZ\",\"PrivateField\":\"PrivateField_XYZ\",\"PrivatePropertyDataMember\":\"PrivatePropertyNamedDataMember_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\"}")
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DataContractDerivedJsonPropertyType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DataContractDerivedJsonPropertyDeserialization()
        {
            List<Tuple<DataContractDerivedJsonPropertyType, string>> testCases = new List<Tuple<DataContractDerivedJsonPropertyType, string>>() {
                new Tuple<DataContractDerivedJsonPropertyType, string>(new DataContractDerivedJsonPropertyType(), "{\"DerivedPublicFieldJsonProperty\":null,\"DerivedInternalFieldJsonProperty\":null,\"DerivedPrivateFieldJsonProperty\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicFieldDataMember\":null,\"InternalFieldDataMember\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateFieldDataMember\":null,\"PrivateField\":null,\"DerivedPublicPropertyJsonProperty\":null,\"DerivedInternalPropertyJsonProperty\":null,\"DerivedPrivatePropertyJsonProperty\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicPropertyDataMember\":null,\"InternalPropertyDataMember\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivatePropertyDataMember\":null,\"PrivateProperty\":null}"),
                new Tuple<DataContractDerivedJsonPropertyType, string>(new DataContractDerivedJsonPropertyType("_XYZ", onlySetSerializableMembers: true), "{\"DerivedPublicFieldJsonProperty\":\"DerivedPublicFieldNamedJsonProperty_XYZ\",\"DerivedInternalFieldJsonProperty\":\"DerivedInternalFieldNamedJsonProperty_XYZ\",\"DerivedPrivateFieldJsonProperty\":\"DerivedPrivateFieldNamedJsonProperty_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicFieldDataMember\":\"PublicFieldNamedDataMember_XYZ\",\"InternalFieldDataMember\":\"InternalFieldNamedDataMember_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PrivateFieldDataMember\":\"PrivateFieldNamedDataMember_XYZ\",\"PrivateField\":\"PrivateField_XYZ\",\"DerivedPublicPropertyJsonProperty\":\"DerivedPublicPropertyNamedJsonProperty_XYZ\",\"DerivedInternalPropertyJsonProperty\":\"DerivedInternalPropertyNamedJsonProperty_XYZ\",\"DerivedPrivatePropertyJsonProperty\":\"DerivedPrivatePropertyNamedJsonProperty_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicPropertyDataMember\":\"PublicPropertyNamedDataMember_XYZ\",\"InternalPropertyDataMember\":\"InternalPropertyNamedDataMember_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PrivatePropertyDataMember\":\"PrivatePropertyNamedDataMember_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DataContractDerivedJsonPropertyType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                DataContractDerivedJsonPropertyType actual = new DataContractDerivedJsonPropertyType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                actual = new DataContractDerivedJsonPropertyType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<DataContractDerivedJsonPropertyType>(json).FirstOrDefault();

                Assert.Equal(expected, actual);

                actual = DefaultSerializer.Deserialize<DataContractDerivedJsonPropertyType>(input);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DataContractDerivedJsonPropertyPopulation()
        {
            List<Tuple<DataContractDerivedJsonPropertyType, string>> testCases = new List<Tuple<DataContractDerivedJsonPropertyType, string>>() {
                new Tuple<DataContractDerivedJsonPropertyType, string>(new DataContractDerivedJsonPropertyType(), "{\"DerivedPublicFieldJsonProperty\":null,\"DerivedInternalFieldJsonProperty\":null,\"DerivedPrivateFieldJsonProperty\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicFieldDataMember\":null,\"InternalFieldDataMember\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateFieldDataMember\":null,\"PrivateField\":null,\"DerivedPublicPropertyJsonProperty\":null,\"DerivedInternalPropertyJsonProperty\":null,\"DerivedPrivatePropertyJsonProperty\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicPropertyDataMember\":null,\"InternalPropertyDataMember\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivatePropertyDataMember\":null,\"PrivateProperty\":null}"),
                new Tuple<DataContractDerivedJsonPropertyType, string>(new DataContractDerivedJsonPropertyType("_XYZ", onlySetSerializableMembers: true), "{\"DerivedPublicFieldJsonProperty\":\"DerivedPublicFieldNamedJsonProperty_XYZ\",\"DerivedInternalFieldJsonProperty\":\"DerivedInternalFieldNamedJsonProperty_XYZ\",\"DerivedPrivateFieldJsonProperty\":\"DerivedPrivateFieldNamedJsonProperty_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicFieldDataMember\":\"PublicFieldNamedDataMember_XYZ\",\"InternalFieldDataMember\":\"InternalFieldNamedDataMember_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PrivateFieldDataMember\":\"PrivateFieldNamedDataMember_XYZ\",\"PrivateField\":\"PrivateField_XYZ\",\"DerivedPublicPropertyJsonProperty\":\"DerivedPublicPropertyNamedJsonProperty_XYZ\",\"DerivedInternalPropertyJsonProperty\":\"DerivedInternalPropertyNamedJsonProperty_XYZ\",\"DerivedPrivatePropertyJsonProperty\":\"DerivedPrivatePropertyNamedJsonProperty_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicPropertyDataMember\":\"PublicPropertyNamedDataMember_XYZ\",\"InternalPropertyDataMember\":\"InternalPropertyNamedDataMember_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PrivatePropertyDataMember\":\"PrivatePropertyNamedDataMember_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DataContractDerivedJsonPropertyType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                DataContractDerivedJsonPropertyType actual = new DataContractDerivedJsonPropertyType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void JsonPropertyDerivedJsonPropertySerialization()
        {
            List<Tuple<JsonPropertyDerivedJsonPropertyType, string>> testCases = new List<Tuple<JsonPropertyDerivedJsonPropertyType, string>>() {
                new Tuple<JsonPropertyDerivedJsonPropertyType, string>(new JsonPropertyDerivedJsonPropertyType(), "{\"DerivedPublicFieldJsonProperty\":null,\"DerivedInternalFieldJsonProperty\":null,\"DerivedPrivateFieldJsonProperty\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"DerivedPublicFieldSansAttribute\":null,\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PublicField\":null,\"InternalField\":null,\"PublicFieldSansAttribute\":null,\"DerivedPublicPropertyJsonProperty\":null,\"DerivedInternalPropertyJsonProperty\":null,\"DerivedPrivatePropertyJsonProperty\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"DerivedPublicPropertySansAttribute\":null,\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PublicPropertySansAttribute\":null,\"PrivateFieldJsonProperty\":null,\"PrivateField\":null,\"PrivatePropertyJsonProperty\":null,\"PrivateProperty\":null}") ,
                new Tuple<JsonPropertyDerivedJsonPropertyType, string>(new JsonPropertyDerivedJsonPropertyType("_XYZ"), "{\"DerivedPublicFieldJsonProperty\":\"DerivedPublicFieldNamedJsonProperty_XYZ\",\"DerivedInternalFieldJsonProperty\":\"DerivedInternalFieldNamedJsonProperty_XYZ\",\"DerivedPrivateFieldJsonProperty\":\"DerivedPrivateFieldNamedJsonProperty_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"DerivedPublicFieldSansAttribute\":\"DerivedPublicFieldSansAttribute_XYZ\",\"PublicFieldJsonProperty\":\"PublicFieldNamedJsonProperty_XYZ\",\"InternalFieldJsonProperty\":\"InternalFieldNamedJsonProperty_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PublicFieldSansAttribute\":\"PublicFieldSansAttribute_XYZ\",\"DerivedPublicPropertyJsonProperty\":\"DerivedPublicPropertyNamedJsonProperty_XYZ\",\"DerivedInternalPropertyJsonProperty\":\"DerivedInternalPropertyNamedJsonProperty_XYZ\",\"DerivedPrivatePropertyJsonProperty\":\"DerivedPrivatePropertyNamedJsonProperty_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"DerivedPublicPropertySansAttribute\":\"DerivedPublicPropertySansAttribute_XYZ\",\"PublicPropertyJsonProperty\":\"PublicPropertyNamedJsonProperty_XYZ\",\"InternalPropertyJsonProperty\":\"InternalPropertyNamedJsonProperty_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PublicPropertySansAttribute\":\"PublicPropertySansAttribute_XYZ\",\"PrivateFieldJsonProperty\":\"PrivateFieldNamedJsonProperty_XYZ\",\"PrivateField\":\"PrivateField_XYZ\",\"PrivatePropertyJsonProperty\":\"PrivatePropertyNamedJsonProperty_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\"}")
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(JsonPropertyDerivedJsonPropertyType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void JsonPropertyDerivedJsonPropertyDeserialization()
        {
            List<Tuple<JsonPropertyDerivedJsonPropertyType, string>> testCases = new List<Tuple<JsonPropertyDerivedJsonPropertyType, string>>() {
                new Tuple<JsonPropertyDerivedJsonPropertyType, string>(new JsonPropertyDerivedJsonPropertyType(), "{\"DerivedPublicPropertyJsonProperty\":null,\"DerivedInternalPropertyJsonProperty\":null,\"DerivedPrivatePropertyJsonProperty\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PrivatePropertyJsonProperty\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateProperty\":null,\"DerivedPublicFieldJsonProperty\":null,\"DerivedInternalFieldJsonProperty\":null,\"DerivedPrivateFieldJsonProperty\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PrivateFieldJsonProperty\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateField\":null,\"DerivedPublicPropertySansAttribute\":null,\"PublicPropertySansAttribute\":null,\"DerivedPublicFieldSansAttribute\":null,\"PublicFieldSansAttribute\":null}"),
                new Tuple<JsonPropertyDerivedJsonPropertyType, string>(new JsonPropertyDerivedJsonPropertyType("_XYZ", onlySetSerializableMembers: true), "{\"DerivedPublicPropertyJsonProperty\":\"DerivedPublicPropertyNamedJsonProperty_XYZ\",\"DerivedInternalPropertyJsonProperty\":\"DerivedInternalPropertyNamedJsonProperty_XYZ\",\"DerivedPrivatePropertyJsonProperty\":\"DerivedPrivatePropertyNamedJsonProperty_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicPropertyJsonProperty\":\"PublicPropertyNamedJsonProperty_XYZ\",\"InternalPropertyJsonProperty\":\"InternalPropertyNamedJsonProperty_XYZ\",\"PrivatePropertyJsonProperty\":null,\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PrivateProperty\":null,\"DerivedPublicFieldJsonProperty\":\"DerivedPublicFieldNamedJsonProperty_XYZ\",\"DerivedInternalFieldJsonProperty\":\"DerivedInternalFieldNamedJsonProperty_XYZ\",\"DerivedPrivateFieldJsonProperty\":\"DerivedPrivateFieldNamedJsonProperty_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicFieldJsonProperty\":\"PublicFieldNamedJsonProperty_XYZ\",\"InternalFieldJsonProperty\":\"InternalFieldNamedJsonProperty_XYZ\",\"PrivateFieldJsonProperty\":null,\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PrivateField\":null,\"DerivedPublicPropertySansAttribute\":\"DerivedPublicPropertySansAttribute_XYZ\",\"PublicPropertySansAttribute\":\"PublicPropertySansAttribute_XYZ\",\"DerivedPublicFieldSansAttribute\":\"DerivedPublicFieldSansAttribute_XYZ\",\"PublicFieldSansAttribute\":\"PublicFieldSansAttribute_XYZ\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(JsonPropertyDerivedJsonPropertyType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                JsonPropertyDerivedJsonPropertyType actual = new JsonPropertyDerivedJsonPropertyType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                actual = new JsonPropertyDerivedJsonPropertyType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<JsonPropertyDerivedJsonPropertyType>(json).FirstOrDefault();

                Assert.Equal(expected, actual);

                actual = DefaultSerializer.Deserialize<JsonPropertyDerivedJsonPropertyType>(input);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void JsonPropertyDerivedJsonPropertyPopulation()
        {
            List<Tuple<JsonPropertyDerivedJsonPropertyType, string>> testCases = new List<Tuple<JsonPropertyDerivedJsonPropertyType, string>>() {
                new Tuple<JsonPropertyDerivedJsonPropertyType, string>(new JsonPropertyDerivedJsonPropertyType(), "{\"DerivedPublicPropertyJsonProperty\":null,\"DerivedInternalPropertyJsonProperty\":null,\"DerivedPrivatePropertyJsonProperty\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PrivatePropertyJsonProperty\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateProperty\":null,\"DerivedPublicFieldJsonProperty\":null,\"DerivedInternalFieldJsonProperty\":null,\"DerivedPrivateFieldJsonProperty\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PrivateFieldJsonProperty\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateField\":null,\"DerivedPublicPropertySansAttribute\":null,\"PublicPropertySansAttribute\":null,\"DerivedPublicFieldSansAttribute\":null,\"PublicFieldSansAttribute\":null}"),
                new Tuple<JsonPropertyDerivedJsonPropertyType, string>(new JsonPropertyDerivedJsonPropertyType("_XYZ", onlySetSerializableMembers: true), "{\"DerivedPublicFieldJsonProperty\":\"DerivedPublicFieldNamedJsonProperty_XYZ\",\"DerivedInternalFieldJsonProperty\":\"DerivedInternalFieldNamedJsonProperty_XYZ\",\"DerivedPrivateFieldJsonProperty\":\"DerivedPrivateFieldNamedJsonProperty_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"DerivedPublicFieldSansAttribute\":\"DerivedPublicFieldSansAttribute_XYZ\",\"PublicFieldJsonProperty\":\"PublicFieldNamedJsonProperty_XYZ\",\"InternalFieldJsonProperty\":\"InternalFieldNamedJsonProperty_XYZ\",\"PublicField\":\"PublicField_XYZ\",\"InternalField\":\"InternalField_XYZ\",\"PublicFieldSansAttribute\":\"PublicFieldSansAttribute_XYZ\",\"DerivedPublicPropertyJsonProperty\":\"DerivedPublicPropertyNamedJsonProperty_XYZ\",\"DerivedInternalPropertyJsonProperty\":\"DerivedInternalPropertyNamedJsonProperty_XYZ\",\"DerivedPrivatePropertyJsonProperty\":\"DerivedPrivatePropertyNamedJsonProperty_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"DerivedPublicPropertySansAttribute\":\"DerivedPublicPropertySansAttribute_XYZ\",\"PublicPropertyJsonProperty\":\"PublicPropertyNamedJsonProperty_XYZ\",\"InternalPropertyJsonProperty\":\"InternalPropertyNamedJsonProperty_XYZ\",\"PublicProperty\":\"PublicProperty_XYZ\",\"InternalProperty\":\"InternalProperty_XYZ\",\"PublicPropertySansAttribute\":\"PublicPropertySansAttribute_XYZ\",\"PrivateFieldJsonProperty\":\"PrivateFieldNamedJsonProperty_XYZ\",\"PrivateField\":\"PrivateField_XYZ\",\"PrivatePropertyJsonProperty\":\"PrivatePropertyNamedJsonProperty_XYZ\",\"PrivateProperty\":\"PrivateProperty_XYZ\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(JsonPropertyDerivedJsonPropertyType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                JsonPropertyDerivedJsonPropertyType actual = new JsonPropertyDerivedJsonPropertyType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void JsonPropertyDerivedDataContractSerialization()
        {
            // Verifies that given a object it serializes into the expected string representation
            List<Tuple<JsonPropertyDerivedDataContractType, string>> testCases = new List<Tuple<JsonPropertyDerivedDataContractType, string>>() {
                new Tuple<JsonPropertyDerivedDataContractType, string>(new JsonPropertyDerivedDataContractType(), "{\"DerivedPublicFieldDataMember\":null,\"DerivedInternalFieldDataMember\":null,\"DerivedPrivateFieldDataMember\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PublicField\":null,\"InternalField\":null,\"DerivedPublicPropertyDataMember\":null,\"DerivedInternalPropertyDataMember\":null,\"DerivedPrivatePropertyDataMember\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateFieldJsonProperty\":null,\"PrivateField\":null,\"PrivatePropertyJsonProperty\":null,\"PrivateProperty\":null}"),
                new Tuple<JsonPropertyDerivedDataContractType, string>(new JsonPropertyDerivedDataContractType("_XYZ"), "{\"DerivedPublicFieldDataMember\":\"DerivedPublicFieldNamedDataMember_XYZ\",\"DerivedInternalFieldDataMember\":\"DerivedInternalFieldNamedDataMember_XYZ\",\"DerivedPrivateFieldDataMember\":\"DerivedPrivateFieldNamedDataMember_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PublicField\":null,\"InternalField\":null,\"DerivedPublicPropertyDataMember\":\"DerivedPublicPropertyNamedDataMember_XYZ\",\"DerivedInternalPropertyDataMember\":\"DerivedInternalPropertyNamedDataMember_XYZ\",\"DerivedPrivatePropertyDataMember\":\"DerivedPrivatePropertyNamedDataMember_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateFieldJsonProperty\":null,\"PrivateField\":null,\"PrivatePropertyJsonProperty\":null,\"PrivateProperty\":null}")
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(JsonPropertyDerivedDataContractType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void JsonPropertyDerivedDataContractDeserialization()
        {
            // Verifies that given a serialized json object, it can be deserialized into the expected object
            // It also checks that deserializing an object into a different one will correctly override the
            // properties of the original
            List<Tuple<JsonPropertyDerivedDataContractType, string>> testCases = new List<Tuple<JsonPropertyDerivedDataContractType, string>>() {
                new Tuple<JsonPropertyDerivedDataContractType, string>(new JsonPropertyDerivedDataContractType(), "{\"DerivedPublicFieldDataMember\":null,\"DerivedInternalFieldDataMember\":null,\"DerivedPrivateFieldDataMember\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateFieldJsonProperty\":null,\"PrivateField\":null,\"DerivedPublicPropertyDataMember\":null,\"DerivedInternalPropertyDataMember\":null,\"DerivedPrivatePropertyDataMember\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivatePropertyJsonProperty\":null,\"PrivateProperty\":null}"),
                new Tuple<JsonPropertyDerivedDataContractType, string>(new JsonPropertyDerivedDataContractType("_XYZ", onlySetSerializableMembers: true), "{\"DerivedPublicFieldDataMember\":\"DerivedPublicFieldNamedDataMember_XYZ\",\"DerivedInternalFieldDataMember\":\"DerivedInternalFieldNamedDataMember_XYZ\",\"DerivedPrivateFieldDataMember\":\"DerivedPrivateFieldNamedDataMember_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateFieldJsonProperty\":null,\"PrivateField\":null,\"DerivedPublicPropertyDataMember\":\"DerivedPublicPropertyNamedDataMember_XYZ\",\"DerivedInternalPropertyDataMember\":\"DerivedInternalPropertyNamedDataMember_XYZ\",\"DerivedPrivatePropertyDataMember\":\"DerivedPrivatePropertyNamedDataMember_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivatePropertyJsonProperty\":null,\"PrivateProperty\":null}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(JsonPropertyDerivedDataContractType));

            foreach (var testCase in testCases)
            {
                var input = JToken.Parse(testCase.Item2);
                var expected = testCase.Item1;

                JsonPropertyDerivedDataContractType actual = new JsonPropertyDerivedDataContractType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                actual = new JsonPropertyDerivedDataContractType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);

                JArray json = JToken.Parse("[" + testCase.Item2 + "]") as JArray;
                actual = DefaultSerializer.Deserialize<JsonPropertyDerivedDataContractType>(json).FirstOrDefault();

                Assert.Equal(expected, actual);

                actual = DefaultSerializer.Deserialize<JsonPropertyDerivedDataContractType>(input);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void JsonPropertyDerivedDataContractPopulation()
        {
            // Verifies that given a serialized json object, it can be deserialized into the expected object
            List<Tuple<JsonPropertyDerivedDataContractType, string>> testCases = new List<Tuple<JsonPropertyDerivedDataContractType, string>>() {
                new Tuple<JsonPropertyDerivedDataContractType, string>(new JsonPropertyDerivedDataContractType(), "{\"DerivedPublicFieldDataMember\":null,\"DerivedInternalFieldDataMember\":null,\"DerivedPrivateFieldDataMember\":null,\"DerivedPublicField\":null,\"DerivedInternalField\":null,\"DerivedPrivateField\":null,\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PublicField\":null,\"InternalField\":null,\"PrivateFieldJsonProperty\":null,\"PrivateField\":null,\"DerivedPublicPropertyDataMember\":null,\"DerivedInternalPropertyDataMember\":null,\"DerivedPrivatePropertyDataMember\":null,\"DerivedPublicProperty\":null,\"DerivedInternalProperty\":null,\"DerivedPrivateProperty\":null,\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivatePropertyJsonProperty\":null,\"PrivateProperty\":null}"),
                new Tuple<JsonPropertyDerivedDataContractType, string>(new JsonPropertyDerivedDataContractType("_XYZ", onlySetSerializableMembers: true), "{\"DerivedPublicFieldDataMember\":\"DerivedPublicFieldNamedDataMember_XYZ\",\"DerivedInternalFieldDataMember\":\"DerivedInternalFieldNamedDataMember_XYZ\",\"DerivedPrivateFieldDataMember\":\"DerivedPrivateFieldNamedDataMember_XYZ\",\"DerivedPublicField\":\"DerivedPublicField_XYZ\",\"DerivedInternalField\":\"DerivedInternalField_XYZ\",\"DerivedPrivateField\":\"DerivedPrivateField_XYZ\",\"PublicFieldJsonProperty\":null,\"InternalFieldJsonProperty\":null,\"PublicField\":null,\"InternalField\":null,\"DerivedPublicPropertyDataMember\":\"DerivedPublicPropertyNamedDataMember_XYZ\",\"DerivedInternalPropertyDataMember\":\"DerivedInternalPropertyNamedDataMember_XYZ\",\"DerivedPrivatePropertyDataMember\":\"DerivedPrivatePropertyNamedDataMember_XYZ\",\"DerivedPublicProperty\":\"DerivedPublicProperty_XYZ\",\"DerivedInternalProperty\":\"DerivedInternalProperty_XYZ\",\"DerivedPrivateProperty\":\"DerivedPrivateProperty_XYZ\",\"PublicPropertyJsonProperty\":null,\"InternalPropertyJsonProperty\":null,\"PublicProperty\":null,\"InternalProperty\":null,\"PrivateFieldJsonProperty\":null,\"PrivateField\":null,\"PrivatePropertyJsonProperty\":null,\"PrivateProperty\":null}")
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(JsonPropertyDerivedDataContractType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                JsonPropertyDerivedDataContractType actual = new JsonPropertyDerivedDataContractType("_ABC", onlySetSerializableMembers: true);
                DefaultSerializer.Deserialize(input, actual);

                string test = DefaultSerializer.Serialize(expected).ToString(Formatting.None);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void SimpleTreeSerialization()
        {
            List<Tuple<SimpleTreeType, string>> testCases = new List<Tuple<SimpleTreeType, string>>() {
                new Tuple<SimpleTreeType, string>(new SimpleTreeType(), "{\"Name\":null,\"Children\":[]}"),
                new Tuple<SimpleTreeType, string>(new SimpleTreeType(setValues:true), "{\"id\":5,\"Name\":\"Root\",\"Children\":[{\"id\":6,\"Name\":\"Child1\",\"Children\":[]},{\"id\":7,\"Name\":\"Child2\",\"Children\":[]}]}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(SimpleTreeType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void SimpleTreeDeserialization()
        {
            List<Tuple<SimpleTreeType, string>> testCases = new List<Tuple<SimpleTreeType, string>>() {
                new Tuple<SimpleTreeType, string>(new SimpleTreeType(), "{\"Name\":null,\"Children\":[]}"),
                new Tuple<SimpleTreeType, string>(new SimpleTreeType(setValues:true), "{\"id\":5,\"name\":\"Root\",\"children\":[{\"id\":6,\"name\":\"Child1\",\"children\":[]},{\"id\":7,\"name\":\"Child2\",\"children\":[]}]}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(SimpleTreeType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                SimpleTreeType actual = new SimpleTreeType(setValues: input != "{\"Name\":null,\"Children\":[]}");
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void SimpleTreePopulation()
        {
            List<Tuple<SimpleTreeType, string>> testCases = new List<Tuple<SimpleTreeType, string>>() {
                new Tuple<SimpleTreeType, string>(new SimpleTreeType(), "{\"Name\":null,\"Children\":[]}"),
                new Tuple<SimpleTreeType, string>(new SimpleTreeType(setValues:true), "{\"id\":5,\"name\":\"Root\",\"children\":[{\"id\":6,\"name\":\"Child1\",\"children\":[]},{\"id\":7,\"name\":\"Child2\",\"children\":[]}]}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(SimpleTreeType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                SimpleTreeType actual = new SimpleTreeType();
                actual.Name = "Not the original name";
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DataContractDerivedPocoSerialization()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                // Need to ensure that the type is registered as a table to force the id property check
                DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DataContractDerivedPocoType));
                DefaultSerializer.Serialize(new DataContractDerivedPocoType());
            });
        }

        [Fact]
        public void DataMemberSerialization()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                DefaultSerializer.Serialize(new DataMemberType());
            });
        }

        [Fact]
        public void DuplicateKeySerialization()
        {
            Assert.Throws<JsonSerializationException>(() =>
            {
                DefaultSerializer.Serialize(new DuplicateKeyType());
            });
        }

        [Fact]
        public void DerivedDuplicateKeySerialization()
        {
            string expected = "{\"PublicField\":null,\"PublicProperty\":\"OtherThanPublicProperty\"}";
            var instance = new DerivedDuplicateKeyType();
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(instance.GetType());
            instance.OtherThanPublicProperty = "OtherThanPublicProperty";
            instance.PublicProperty = "PublicProperty";
            string actual = DefaultSerializer.Serialize(instance).ToString(Formatting.None);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IdTypeSerialization()
        {
            List<Tuple<object, string>> testCases = new List<Tuple<object, string>>() {
                new Tuple<object, string>(new idType() { id = 9 }, "{\"id\":9}"),
                new Tuple<object, string>(new IDType() { ID = 10 }, "{\"id\":10}"),
            };

            foreach (var testCase in testCases)
            {
                // Need to ensure that the type is registered as a table to force the id property check
                DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(testCase.Item1.GetType());

                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void IdTypeSerializationNegative()
        {
            List<object> testCases = new List<object>()
            {
                new iDType() { iD = 8 },
                new DataContractMissingIdType() { id = 8 },
                new IgnoreDataMemberMissingIdType() { id = 8 },
                new JsonIgnoreMissingIdType() { id = 8 },
                new MulitpleIdType() { Id = 7, id = 8 }
            };

            foreach (var testCase in testCases)
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    // Need to ensure that the type is registered as a table to force the id property check
                    DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(testCase.GetType());
                    DefaultSerializer.Serialize(testCase);
                });
            }
        }

        [Fact]
        public void InterfacePropertyTypeSerialization()
        {
            List<Tuple<object, string>> testCases = new List<Tuple<object, string>>() {
                new Tuple<object, string>(new InterfacePropertyType() { Id = 5, Lookup = new Dictionary<string,string>() { { "x", "y"}} }, "{\"id\":5,\"Lookup\":{\"x\":\"y\"}}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(InterfacePropertyType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void InterfacePropertyTypeDeserialization()
        {
            List<Tuple<object, string>> testCases = new List<Tuple<object, string>>() {
                new Tuple<object, string>(new InterfacePropertyType() { Id = 5, Lookup = new Dictionary<string,string>() { { "x", "y"}} }, "{\"id\":5,\"Lookup\":{\"x\":\"y\"}}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(InterfacePropertyType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                var actual = new InterfacePropertyType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void EnumSerialization()
        {
            List<Tuple<EnumType, string>> testCases = new List<Tuple<EnumType, string>>() {
                new Tuple<EnumType, string>(new EnumType() { Enum1 = Enum1.Enum1Value1 }, "{\"Enum1\":\"Enum1Value1\",\"Enum2\":\"Enum2Value1\",\"Enum3\":0,\"Enum4\":\"Enum4Value1\",\"Enum5\":\"Enum5Value1\",\"Enum6\":0}"),
                new Tuple<EnumType, string>(new EnumType() { Enum1 = Enum1.Enum1Value2 }, "{\"Enum1\":\"Enum1Value2\",\"Enum2\":\"Enum2Value1\",\"Enum3\":0,\"Enum4\":\"Enum4Value1\",\"Enum5\":\"Enum5Value1\",\"Enum6\":0}"),
                new Tuple<EnumType, string>(new EnumType() { Enum3 = Enum3.Enum3Value2 | Enum3.Enum3Value1 }, "{\"Enum1\":\"Enum1Value1\",\"Enum2\":\"Enum2Value1\",\"Enum3\":\"Enum3Value1, Enum3Value2\",\"Enum4\":\"Enum4Value1\",\"Enum5\":\"Enum5Value1\",\"Enum6\":0}"),
                new Tuple<EnumType, string>(new EnumType() { Enum1 = (Enum1)1000 }, "{\"Enum1\":1000,\"Enum2\":\"Enum2Value1\",\"Enum3\":0,\"Enum4\":\"Enum4Value1\",\"Enum5\":\"Enum5Value1\",\"Enum6\":0}"),
                new Tuple<EnumType, string>(new EnumType() { Enum1 = (Enum1)(-1000) }, "{\"Enum1\":-1000,\"Enum2\":\"Enum2Value1\",\"Enum3\":0,\"Enum4\":\"Enum4Value1\",\"Enum5\":\"Enum5Value1\",\"Enum6\":0}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(EnumType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void EnumDeserialization()
        {
            List<Tuple<EnumType, string>> testCases = new List<Tuple<EnumType, string>>() {
                new Tuple<EnumType, string>(new EnumType() { Enum1 = Enum1.Enum1Value1 }, "{\"Enum1\":'Enum1Value1'}"),
                new Tuple<EnumType, string>(new EnumType() { Enum1 = Enum1.Enum1Value2 }, "{\"Enum1\":'Enum1Value2'}"),
                new Tuple<EnumType, string>(new EnumType() { Enum3 = Enum3.Enum3Value2 | Enum3.Enum3Value1 }, "{\"Enum3\":'Enum3Value2,Enum3Value1'}"),
                new Tuple<EnumType, string>(new EnumType() { Enum3 = (Enum3)1000 }, "{\"Enum3\":1000}"),
                new Tuple<EnumType, string>(new EnumType() { Enum1 = (Enum1)(-1000) }, "{\"Enum1\":-1000}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(EnumType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                EnumType actual = new EnumType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.Enum1, expected.Enum1);
            }
        }

        [Fact]
        public void ComplexTypeSerialization()
        {
            // This test checks that child type does not have to have an id
            List<Tuple<object, string>> testCases = new List<Tuple<object, string>>() {
                new Tuple<object, string>(new ComplexType() { Id = 5, Name = "Some Name", Child = new MissingIdType() { NotAnId = 4 }}, "{\"id\":5,\"Name\":\"Some Name\",\"Child\":{\"NotAnId\":4}}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(ComplexType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void CustomConverterOnPropertyTestSerialization()
        {
            List<Tuple<ConverterType, string>> testCases = new List<Tuple<ConverterType, string>>
            {
                new Tuple<ConverterType, string>(
                    new ConverterType { Number = 12 },
                    "{\"Number\":null}"),
                new Tuple<ConverterType, string>(
                    new ConverterType { Number = 0 },
                    "{\"Number\":null}"),
            };

            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(ConverterType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData("{\"Number\":\"14\"}")]
        [InlineData("{\"Number\":\"12\"}")]
        public void CustomConverterOnPropertyTestDeserialization(string testCase)
        {
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(ConverterType));
            var input = JToken.Parse(testCase);

            ConverterType actual = new ConverterType();
            DefaultSerializer.Deserialize(input, actual);

            Assert.Equal(0, actual.Number);

            actual = new ConverterType();
            actual.Number = 10;
            DefaultSerializer.Deserialize(input, actual);

            Assert.Equal(10, actual.Number);

            JArray json = JToken.Parse("[" + testCase + "]") as JArray;
            actual = DefaultSerializer.Deserialize<ConverterType>(json).FirstOrDefault();

            Assert.Equal(0, actual.Number);

            actual = (ConverterType)DefaultSerializer.Deserialize<ConverterType>(input);

            Assert.Equal(0, actual.Number);
        }

        [Fact]
        public void CreatedAtTypeSerialization()
        {
            List<Tuple<CreatedAtType, string>> testCases = new List<Tuple<CreatedAtType, string>>
            {
                new Tuple<CreatedAtType, string>(
                    new CreatedAtType { CreatedAt = new DateTime(2012, 1, 5, 12, 0, 0, DateTimeKind.Utc) },
                    "{\"createdAt\":\"2012-01-05T12:00:00.000Z\"}"),
                new Tuple<CreatedAtType, string>(
                    new CreatedAtType { CreatedAt = default(DateTime) },
                    "{\"createdAt\":" + MinDateTimeSerializedToJson + "}"),
            };

            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(CreatedAtType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void CreatedAtTypeDeserialization()
        {
            List<Tuple<CreatedAtType, string>> testCases = new List<Tuple<CreatedAtType, string>>() {
                new Tuple<CreatedAtType, string>(new CreatedAtType { CreatedAt = default(DateTime) }, "{\"createdAt\":" + MinDateTimeSerializedToJson + "}"),
                new Tuple<CreatedAtType, string>(new CreatedAtType { CreatedAt = new DateTime(2012, 1, 5, 12, 0, 0, DateTimeKind.Utc) }, "{\"createdAt\":\"2012-01-05T12:00:00.000Z\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(CreatedAtType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                CreatedAtType actual = new CreatedAtType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.CreatedAt.ToUniversalTime(), expected.CreatedAt.ToUniversalTime());
            }
        }

        [Fact]
        public void UpdatedAtTypeSerialization()
        {
            List<Tuple<UpdatedAtType, string>> testCases = new List<Tuple<UpdatedAtType, string>>
            {
                new Tuple<UpdatedAtType, string>(
                    new UpdatedAtType { UpdatedAt = new DateTime(2012, 1, 5, 12, 0, 0, DateTimeKind.Utc) },
                    "{\"updatedAt\":\"2012-01-05T12:00:00.000Z\"}"),
                new Tuple<UpdatedAtType, string>(
                    new UpdatedAtType { UpdatedAt = default(DateTime) },
                    "{\"updatedAt\":" + MinDateTimeSerializedToJson + "}"),
            };

            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(UpdatedAtType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void UpdatedAtTypeDeserialization()
        {
            List<Tuple<UpdatedAtType, string>> testCases = new List<Tuple<UpdatedAtType, string>>() {
                new Tuple<UpdatedAtType, string>(new UpdatedAtType { UpdatedAt = default(DateTime) }, "{\"updatedAt\":" + MinDateTimeSerializedToJson + "}"),
                new Tuple<UpdatedAtType, string>(new UpdatedAtType { UpdatedAt = new DateTime(2012, 1, 5, 12, 0, 0, DateTimeKind.Utc) }, "{\"updatedAt\":\"2012-01-05T12:00:00.000Z\"}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(UpdatedAtType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                UpdatedAtType actual = new UpdatedAtType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.UpdatedAt.ToUniversalTime(), expected.UpdatedAt.ToUniversalTime());
            }
        }

        [Fact]
        public void Deserialize_DoesNotTransformException_WhenIdTypeDoesNotMismatch()
        {
            Assert.Throws<JsonSerializationException>(() => DefaultSerializer.Deserialize<LongIdType>(new JValue(true)));
        }

        [Fact]
        public void Deserialize_TransoformsException_WhenIdTypeMismatches()
        {
            Assert.Throws<JsonSerializationException>(() =>
            {
                DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(LongIdType));
                var token = new JObject() { { "id", "asdf" } };
                DefaultSerializer.Deserialize<LongIdType>(token);
            });
        }

        [Fact]
        public void VersionTypeSerialization()
        {
            List<Tuple<VersionType, string>> testCases = new List<Tuple<VersionType, string>>
            {
                new Tuple<VersionType, string>(new VersionType { Version = "0x0004F" }, "{\"version\":\"0x0004F\"}"),
                new Tuple<VersionType, string>(new VersionType { Version = null }, "{\"version\":null}"),
            };

            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(VersionType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void VersionTypeDeserialization()
        {
            List<Tuple<VersionType, string>> testCases = new List<Tuple<VersionType, string>>() {
                new Tuple<VersionType, string>(new VersionType { Version = "0x0004F" }, "{\"version\":\"0x0004F\"}"),
                new Tuple<VersionType, string>(new VersionType { Version = null }, "{\"version\":null}"),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(VersionType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                VersionType actual = new VersionType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.Version, expected.Version);
            }
        }

        [Fact]
        public void AllSystemPropertiesTypeSerialization()
        {
            List<Tuple<AllSystemPropertiesType, string>> testCases = new List<Tuple<AllSystemPropertiesType, string>>
            {
                new Tuple<AllSystemPropertiesType, string>(new AllSystemPropertiesType { UpdatedAt = new DateTime(2012, 1, 5, 12, 0, 0, DateTimeKind.Utc),
                                                                                         CreatedAt = new DateTime(2012, 1, 5, 12, 0, 0, DateTimeKind.Utc),
                                                                                         Version = "0x0004F" },
                                                                                         "{\"createdAt\":\"2012-01-05T12:00:00.000Z\",\"updatedAt\":\"2012-01-05T12:00:00.000Z\",\"version\":\"0x0004F\"}"),
                new Tuple<AllSystemPropertiesType, string>(
                    new AllSystemPropertiesType { Version = null },
                    "{\"createdAt\":MIN_DATE,\"updatedAt\":MIN_DATE,\"version\":null}".Replace("MIN_DATE", MinDateTimeSerializedToJson)
                ),
            };

            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(AllSystemPropertiesType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                string actual = DefaultSerializer.Serialize(input).ToString(Formatting.None);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void AllSystemPropertiesTypeDeserialization()
        {
            List<Tuple<AllSystemPropertiesType, string>> testCases = new List<Tuple<AllSystemPropertiesType, string>>
            {
                new Tuple<AllSystemPropertiesType, string>(new AllSystemPropertiesType { UpdatedAt = new DateTime(2012, 1, 5, 12, 0, 0, DateTimeKind.Utc),
                                                                                         CreatedAt = new DateTime(2012, 1, 5, 12, 0, 0, DateTimeKind.Utc),
                                                                                         Version = "0x0004F" },
                                                                                         "{\"createdAt\":\"2012-01-05T12:00:00.000Z\",\"updatedAt\":\"2012-01-05T12:00:00.000Z\",\"version\":\"0x0004F\"}"),
                new Tuple<AllSystemPropertiesType, string>(
                    new AllSystemPropertiesType { Version = null },
                    "{\"createdAt\":MIN_DATE,\"updatedAt\":MIN_DATE,\"version\":null}".Replace("MIN_DATE", MinDateTimeSerializedToJson)),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(AllSystemPropertiesType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item2;
                var expected = testCase.Item1;

                AllSystemPropertiesType actual = new AllSystemPropertiesType();
                DefaultSerializer.Deserialize(input, actual);

                Assert.Equal(actual.CreatedAt.ToUniversalTime(), expected.CreatedAt.ToUniversalTime());
                Assert.Equal(actual.UpdatedAt.ToUniversalTime(), expected.UpdatedAt.ToUniversalTime());
                Assert.Equal(actual.Version, expected.Version);
            }
        }
    }
}
