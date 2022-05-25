// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;

namespace SQLiteStore.Tests
{
    public class SqlHelper_Test
    {
        [Fact]
        public void GetStoreCastType_Throws_WhenTypeIsNotSupported()
        {
            var types = new[] { typeof(SqlHelper_Test), typeof(DateTimeOffset) };

            foreach (Type type in types)
            {
                var ex = Assert.Throws<NotSupportedException>(() => SqlHelpers.GetStoreCastType(type));
                Assert.Equal("Value of type '" + type.Name + "' is not supported.", ex.Message);
            }
        }

        [Fact]
        public void GetStoreCastType_ReturnsCorrectType()
        {
            var data = new Dictionary<Type, string>()
            {
                { typeof(bool), SqlColumnType.Numeric },
                { typeof(DateTime), SqlColumnType.Numeric },
                { typeof(decimal), SqlColumnType.Numeric },
                { typeof(int), SqlColumnType.Integer },
                { typeof(uint), SqlColumnType.Integer },
                { typeof(long), SqlColumnType.Integer },
                { typeof(ulong), SqlColumnType.Integer },
                { typeof(short), SqlColumnType.Integer },
                { typeof(ushort), SqlColumnType.Integer },
                { typeof(byte), SqlColumnType.Integer },
                { typeof(sbyte), SqlColumnType.Integer },
                { typeof(float), SqlColumnType.Real },
                { typeof(double), SqlColumnType.Real },
                { typeof(string), SqlColumnType.Text },
                { typeof(Guid), SqlColumnType.Text },
                { typeof(byte[]), SqlColumnType.Text },
                { typeof(Uri), SqlColumnType.Text },
                { typeof(TimeSpan), SqlColumnType.Text }
            };

            foreach (var item in data)
            {
                Assert.Equal(SqlHelpers.GetStoreCastType(item.Key), item.Value);
            }
        }

        [Fact]
        public void GetStoreType_ReturnsCorrectType()
        {
            var data = new Dictionary<JTokenType, string>()
            {
                { JTokenType.Boolean, SqlColumnType.Boolean },
                { JTokenType.Integer, SqlColumnType.Integer },
                { JTokenType.Date, SqlColumnType.DateTime },
                { JTokenType.Float, SqlColumnType.Float },
                { JTokenType.String, SqlColumnType.Text },
                { JTokenType.Guid, SqlColumnType.Guid },
                { JTokenType.Array, SqlColumnType.Json },
                { JTokenType.Object, SqlColumnType.Json },
                { JTokenType.Bytes, SqlColumnType.Blob },
                { JTokenType.Uri, SqlColumnType.Uri },
                { JTokenType.TimeSpan, SqlColumnType.TimeSpan },
            };

            foreach (var item in data)
            {
                Assert.Equal(SqlHelpers.GetStoreType(item.Key, allowNull: false), item.Value);
            }
        }

        [Fact]
        public void GetStoreType_Throws_OnUnsupportedTypes()
        {
            var items = new[] { JTokenType.Comment, JTokenType.Constructor, JTokenType.None, JTokenType.Property, JTokenType.Raw, JTokenType.Undefined, JTokenType.Null };

            foreach (var type in items)
            {
                var ex = Assert.Throws<NotSupportedException>(() => SqlHelpers.GetStoreType(type, allowNull: false));
                Assert.Equal(ex.Message, String.Format("Property of type '{0}' is not supported.", type));
            }
        }

        [Fact]
        public void SerializeValue_LosesPrecision_WhenValueIsDate()
        {
            var original = new DateTime(635338107839470268);
            var serialized = (double)SqlHelpers.SerializeValue(new JValue(original), SqlColumnType.DateTime, JTokenType.Date);
            Assert.Equal(1398213983.9470000, serialized);
        }

        [Fact]
        public void ParseReal_LosesPrecision_WhenValueIsDate()
        {
            var date = (DateTime)SqlHelpers.DeserializeValue(1398213983.9470267, SqlColumnType.Real, JTokenType.Date);
            Assert.Equal(635338107839470000, date.Ticks);
        }
    }
}
