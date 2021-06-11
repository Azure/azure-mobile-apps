// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file sqliteSerializer unit tests
 */

var Validate = require('../../../../src/Utilities/Validate'),
    Platform = require('../../../../src/Platform'),
    sqliteSerializer = require('../../../../src/Platform/cordova/sqliteSerializer'),
        ColumnType = require('../../../../src/sync/ColumnType');

$testGroup('sqliteSerializer tests').tests(
    
    $test('Ensure unit tests are up to date')
    .check(function () {

        // If this test fails, it means the column type enum has changed.
        // Add / update UTs to handle the changes and only then fix this test.
        $assert.areEqual(ColumnType, {
            Object: "object",
            Array: "array",
            Integer: "integer",
            Int: "int",
            Float: "float",
            Real: "real",
            String: "string",
            Text: "text",
            Boolean: "boolean",
            Bool: "bool",
            Date: "date"
        });
    }),

    $test('Verify ColumnType to SQLite type conversion')
    .check(function () {
        $assert.areEqual(sqliteSerializer.getSqliteType(ColumnType.Object), 'TEXT');
        $assert.areEqual(sqliteSerializer.getSqliteType(ColumnType.Array), 'TEXT');
        $assert.areEqual(sqliteSerializer.getSqliteType(ColumnType.String), 'TEXT');
        $assert.areEqual(sqliteSerializer.getSqliteType(ColumnType.Text), 'TEXT');

        $assert.areEqual(sqliteSerializer.getSqliteType(ColumnType.Integer), 'INTEGER');
        $assert.areEqual(sqliteSerializer.getSqliteType(ColumnType.Int), 'INTEGER');
        $assert.areEqual(sqliteSerializer.getSqliteType(ColumnType.Boolean), 'INTEGER');
        $assert.areEqual(sqliteSerializer.getSqliteType(ColumnType.Bool), 'INTEGER');

        $assert.areEqual(sqliteSerializer.getSqliteType(ColumnType.Real), 'REAL');
        $assert.areEqual(sqliteSerializer.getSqliteType(ColumnType.Float), 'REAL');

        $assert.areEqual(sqliteSerializer.getSqliteType(ColumnType.Date), 'INTEGER');

        $assertThrows(function () { sqliteSerializer.getSqliteType('notsupported'); });
        $assertThrows(function () { sqliteSerializer.getSqliteType(5); });
        $assertThrows(function () { sqliteSerializer.getSqliteType([]); });
        $assertThrows(function () { sqliteSerializer.getSqliteType(null); });
        $assertThrows(function () { sqliteSerializer.getSqliteType(undefined); });
    }),

    $test('Roundtripping of an object not containing an ID property')
    .check(function () {
        var value = { a: 1 };
        var columnDefinitions = { a: ColumnType.Integer };
        var serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
        $assert.areEqual(serializedValue, value);
        $assert.areEqual(sqliteSerializer.deserialize(serializedValue, columnDefinitions), value);
    }),

    $test('Empty object roundtripping')
    .check(function () {
        var value = {};
        var columnDefinitions = {};
        var serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
        $assert.areEqual(serializedValue, value);
        $assert.areEqual(sqliteSerializer.deserialize(serializedValue, columnDefinitions), value);
    }),

    $test('Roundtripping of an object containing an ID property')
    .check(function () {
        var value = { id: 1, val: '2' };
        var columnDefinitions = { id: ColumnType.Integer, val: ColumnType.String };
        var serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
        $assert.areEqual(serializedValue, value);
        $assert.areEqual(sqliteSerializer.deserialize(serializedValue, columnDefinitions), value);
    }),

    $test('Serialize object when columns are missing from table definition')
    .check(function () {
        var serializedValue = sqliteSerializer.serialize({
            a: 1,
            undefinedColumn: false
        }, {
            a: ColumnType.Integer
        });
        $assert.areEqual(serializedValue, { a: 1 });
    }),

    $test('Deserialize an object when columns are missing from table definition')
    .check(function () {
        var value = {
            object: { a: 1, b: 'str', c: [1, 2] },
            array: [1, 2, { a: 1 }],
            string: 'somestring',
            text: 'sometext',
            integer: 5,
            int: 6,
            bool: true,
            boolean: false,
            real: 1.5,
            float: 2.2,
            date: new Date(2001, 1, 1)
        };
        var deserializedValue = sqliteSerializer.deserialize(value, { /* all columns missing from definition */ });
        $assert.areEqual(deserializedValue, value);
    }),

    $test('Serialize an object when column definition is null')
    .check(function () {
        $assertThrows(function () {
            sqliteSerializer.serialize({ a: 1 }, null);
        });
    }),

    $test('Serialize an object when column definition is undefined')
    .check(function () {
        $assertThrows(function () {
            sqliteSerializer.serialize({ a: 1 });
        });
    }),

    $test('Serialize a value of type boolean')
    .check(function () {
        $assert.areEqual(sqliteSerializer.serializeValue(false), 0);
        $assert.areEqual(sqliteSerializer.serializeValue(true), 1);
    }),

    $test('Serialize a value of type integer') // JS has no integer type, but sql has an integer type
    .check(function () {
        $assert.areEqual(sqliteSerializer.serializeValue(-1), -1);
        $assert.areEqual(sqliteSerializer.serializeValue(0), 0);
        $assert.areEqual(sqliteSerializer.serializeValue(10), 10);
    }),

    $test('Serialize a value of type float') // JS has no float type, but sql has a float type
    .check(function () {
        $assert.areEqual(sqliteSerializer.serializeValue(-1.0), -1);
        $assert.areEqual(sqliteSerializer.serializeValue(-1.1), -1.1);
        $assert.areEqual(sqliteSerializer.serializeValue(0.0), 0);
        $assert.areEqual(sqliteSerializer.serializeValue(10.0), 10);
        $assert.areEqual(sqliteSerializer.serializeValue(10.1), 10.1);
    }),

    $test('Serialize a value of type string')
    .check(function () {
        $assert.areEqual(sqliteSerializer.serializeValue(''), '');
        $assert.areEqual(sqliteSerializer.serializeValue('abc'), 'abc');
    }),

    $test('Serialize a value of type date')
    .check(function () {
        var value = new Date(2011, 10, 11, 12, 13, 14);
        $assert.areEqual(sqliteSerializer.serializeValue(value), value.getTime());
    }),

    $test('Serialize a value of type object')
    .check(function () {
        var value = {a: 1, b: '2'};
        $assert.areEqual(sqliteSerializer.serializeValue(value), JSON.stringify(value));
    }),

    $test('Serialize a value of type array')
    .check(function () {
        var value = [1, {a: 2}];
        $assert.areEqual(sqliteSerializer.serializeValue(value), JSON.stringify(value));
    }),

    $test('Serialize a null')
    .check(function () {
        $assert.areEqual(sqliteSerializer.serializeValue(null), null);
    }),

    $test('Serialize an undefined value')
    .check(function () {
        $assert.areEqual(sqliteSerializer.serializeValue(undefined), null);
    }),

    $test('Deserialize an object when column definition is null')
    .check(function () {
        $assertThrows(function () {
            sqliteSerializer.deserialize({ a: 1 }, null);
        });
    }),

    $test('Deserialize an object when column definition is undefined')
    .check(function () {
        $assertThrows(function () {
            sqliteSerializer.deserialize({ a: 1 } /*, undefined column definition */);
        });
    }),

    $test('Serialize property of type object into columns of different types')
    .check(function () {
        var value = { val: {} },
            columnDefinitions = {},
            serialize = function() {
            sqliteSerializer.serialize(value, columnDefinitions);
        };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            switch (ColumnType[c]) {
                // Serialization should work only for these column types
                case ColumnType.Object:
                case ColumnType.String:
                case ColumnType.Text:
                    var serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, { val: JSON.stringify(value.val) });
                    break;
                // Serializing as any other type should fail
                default:
                    $assertThrows(serialize);
                    break;
            }
        }
    }),

    $test('Serialize property of type array into columns of different types')
    .check(function () {
        var value = { val: [1, 2] },
            columnDefinitions = {},
            serialize = function () {
                sqliteSerializer.serialize(value, columnDefinitions);
            };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            switch (ColumnType[c]) {
                // Serialization should work only for these column types
                case ColumnType.Object:
                case ColumnType.Array:
                case ColumnType.String:
                case ColumnType.Text:
                    var serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, { val: JSON.stringify(value.val) });
                    break;
                // Serializing as any other type should fail
                default:
                    $assertThrows(serialize);
                    break;
            }
        }
    }),

    $test('Serialize property of type string into columns of different types')
    .check(function () {
        var value = { val: 'somestring' },
            columnDefinitions = {},
            serialize = function () {
                sqliteSerializer.serialize(value, columnDefinitions);
            };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            switch (ColumnType[c]) {
                // Serialization should work only for these column types
                case ColumnType.String:
                case ColumnType.Text:
                    var serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, value);
                    break;
                // Serializing as any other type should fail
                default:
                    $assertThrows(serialize);
                    break;
            }
        }
    }),

    $test('Serialize property of type string and integer value into columns of different types')
    .check(function () {
        var value = { val: '5' },
            columnDefinitions = {},
            serialize = function () {
                sqliteSerializer.serialize(value, columnDefinitions);
            };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            switch (ColumnType[c]) {
                // Serialization should work only for these column types
                case ColumnType.String:
                case ColumnType.Text:
                    var serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, value);
                    break;
                    // Serializing as any other type should fail
                default:
                    $assertThrows(serialize);
                    break;
            }
        }
    }),

    $test('Serialize property with integer value into columns of different types')
    .check(function () {
        var value = { val: 51 },
            columnDefinitions = {},
            serialize = function () {
                sqliteSerializer.serialize(value, columnDefinitions);
            };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            var serializedValue;
            switch (ColumnType[c]) {
                case ColumnType.Integer:
                case ColumnType.Int:
                case ColumnType.Float:
                case ColumnType.Real:
                case ColumnType.Boolean:
                case ColumnType.Bool:
                    serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, value);
                    break;
                case ColumnType.String:
                case ColumnType.Text:
                    serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, { val: '51' });
                    break;
                // Serializing as any other type should fail
                default:
                    $assertThrows(serialize);
                    break;
            }
        }
    }),

    $test('Serialize property with a boolean true value into columns of different types')
    .check(function () {
        var value = { val: true },
            columnDefinitions = {},
            serialize = function () {
                sqliteSerializer.serialize(value, columnDefinitions);
            };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            var serializedValue;
            switch (ColumnType[c]) {
                case ColumnType.Integer:
                case ColumnType.Int:
                    serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, { val: 1 });
                    break;
                case ColumnType.String:
                case ColumnType.Text:
                    serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, { val: 'true' });
                    break;
                case ColumnType.Boolean:
                case ColumnType.Bool:
                    serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, { val: 1 });
                    break;
                    // Serializing as any other type should fail
                default:
                    $assertThrows(serialize);
                    break;
            }
        }
    }),

    $test('Serialize property with a boolean false value into columns of different types')
    .check(function () {
        var value = { val: false },
            columnDefinitions = {},
            serialize = function () {
                sqliteSerializer.serialize(value, columnDefinitions);
            };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            var serializedValue;
            switch (ColumnType[c]) {
                case ColumnType.Integer:
                case ColumnType.Int:
                    serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, { val: 0 });
                    break;
                case ColumnType.String:
                case ColumnType.Text:
                    serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, { val: 'false' });
                    break;
                case ColumnType.Boolean:
                case ColumnType.Bool:
                    serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, { val: 0 });
                    break;
                // Serializing as any other type should fail
                default:
                    $assertThrows(serialize);
                    break;
            }
        }
    }),

    $test('Serialize property with a float value into columns of different types')
    .check(function () {
        var value = { val: -5.55 },
            columnDefinitions = {},
            serialize = function () {
                sqliteSerializer.serialize(value, columnDefinitions);
            };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            var serializedValue;
            switch (ColumnType[c]) {
                case ColumnType.Float:
                case ColumnType.Real:
                    serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, { val: -5.55 });
                    break;
                case ColumnType.String:
                case ColumnType.Text:
                    serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, { val: '-5.55' });
                    break;
                // Serializing as any other type should fail
                default:
                    $assertThrows(serialize);
                    break;
            }
        }
    }),

    $test('Serialize property with a date value into columns of different types')
    .check(function () {
        var value = { val: new Date(2011, 10, 11, 12, 13, 14) },
            columnDefinitions = {},
            serialize = function () {
                sqliteSerializer.serialize(value, columnDefinitions);
            };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            var serializedValue;
            switch (ColumnType[c]) {
                case ColumnType.Date:
                    serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, {val: value.val.getTime()});
                    break;
                case ColumnType.String:
                case ColumnType.Text:
                    serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
                    $assert.areEqual(serializedValue, { val: '\"2011-11-11T20:13:14.000Z\"' });
                    break;
                // Serializing as any other type should fail
                default:
                    $assertThrows(serialize);
                    break;
            }
        }
    }),

    $test('Serialize property with null value into columns of different types')
    .check(function () {
        var value = { val: null },
            columnDefinitions = {};

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            var serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
            $assert.areEqual(serializedValue, value);
        }
    }),

    $test('Serialize property with undefined value into columns of different types')
    .check(function () {
        var value = { val: null },
            columnDefinitions = {};

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            var serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
            $assert.areEqual(serializedValue, { val: null });
        }
    }),

    $test('Attempting to serialize to an unsupported column should fail')
    .check(function () {
        var value = {},
            columnDefinitions = {val: 'someunsupportedtype'},
            serialize = function () {
                sqliteSerializer.serialize(value, columnDefinitions);
            };

        // object
        value.val = { a: 1 };
        $assertThrows(serialize);

        // array
        value.val = [1, 2];
        $assertThrows(serialize);

        // integer
        value.val = 5;
        $assertThrows(serialize);

        // float
        value.val = -5.5;
        $assertThrows(serialize);

        // string
        value.val = 'somestring';
        $assertThrows(serialize);

        // bool
        value.val = true;
        $assertThrows(serialize);
    }),

    $test('Serialize null object')
    .check(function () {
        var columnDefinitions = {},
            serialize = function () {
                sqliteSerializer.serialize(null, columnDefinitions);
            };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            // Serializing as any type should fail
            $assertThrows(serialize);
        }
    }),

    $test('Serialize undefined object')
    .check(function () {
        var columnDefinitions = {},
            serialize = function () {
                sqliteSerializer.serialize(undefined, columnDefinitions);
            };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            // Serializing as any type should fail
            $assertThrows(serialize);
        }
    }),

    $test('Serialize property when column definitions defined it with a different case')
    .check(function () {
        $assert.areEqual(sqliteSerializer.serialize({iD: 1, valUE: 2}, {Id: 'integer', Value: 'integer'}),
                                                     {iD: 1, valUE: 2});
    }),

    $test('Deserialize property of type object into columns of different types')
    .check(function () {
        var value = { val: { a: 1 } },
            columnDefinitions = {},
            deserialize = function () {
                sqliteSerializer.deserialize(value, columnDefinitions);
            };

        var deserializedValue;
        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            // Deserializing to any type should fail            
            $assertThrows(deserialize);
        }
    }),

    $test('Deserialize property of type array into columns of different types')
    .check(function () {
        var value = { val: [1, 2] },
            columnDefinitions = {},
            deserialize = function () {
                sqliteSerializer.deserialize(value, columnDefinitions);
            };

        var deserializedValue;
        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            // Deserializing to any type should fail            
            $assertThrows(deserialize);
        }
    }),

    $test('Deserialize property of type string into columns of different types')
    .check(function () {
        var value = { val: 'somestring' },
            columnDefinitions = {},
            deserialize = function () {
                sqliteSerializer.deserialize(value, columnDefinitions);
            };

        var deserializedValue;
        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            switch (ColumnType[c]) {
                case ColumnType.String:
                case ColumnType.Text:
                    deserializedValue = sqliteSerializer.deserialize(value, columnDefinitions);
                    $assert.areEqual(deserializedValue, value);
                    break;
                    // Deserializing to any other type should fail
                default:
                    $assertThrows(deserialize);
                    break;
            }
        }
    }),

    $test('Deserialize property of type string and integer value into columns of different types')
    .check(function () {
        var value = { val: '51' },
            columnDefinitions = {},
            deserialize = function () {
                sqliteSerializer.deserialize(value, columnDefinitions);
            };

        var deserializedValue;
        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            switch (ColumnType[c]) {
                case ColumnType.String:
                case ColumnType.Text:
                    deserializedValue = sqliteSerializer.deserialize(value, columnDefinitions);
                    $assert.areEqual(deserializedValue, value);
                    break;
                // Deserializing to any other type should fail
                default:
                    $assertThrows(deserialize);
                    break;
            }
        }
    }),

    $test('Deserialize property of type integer with a non-zero value into columns of different types')
    .check(function () {
        var value = { val: 51 },
            columnDefinitions = {},
            deserialize = function () {
                sqliteSerializer.deserialize(value, columnDefinitions);
            };

        var deserializedValue;
        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            switch (ColumnType[c]) {
                case ColumnType.Integer:
                case ColumnType.Int:
                case ColumnType.Float:
                case ColumnType.Real:
                    deserializedValue = sqliteSerializer.deserialize(value, columnDefinitions);
                    $assert.areEqual(deserializedValue, value);
                    break;
                case ColumnType.Boolean:
                case ColumnType.Bool:
                    deserializedValue = sqliteSerializer.deserialize(value, columnDefinitions);
                    $assert.areEqual(deserializedValue, { val: true });
                    break;
                case ColumnType.Date:
                    deserializedValue = sqliteSerializer.deserialize(value, columnDefinitions);
                    $assert.isNotNull(deserializedValue);
                    $assert.isNotNull(deserializedValue.val);
                    Validate.isDate(deserializedValue.val);
                    var v = deserializedValue.val.toISOString();
                    $assert.areEqual(v, "1970-01-01T00:00:00.051Z");
                    break;
                // Deserializing to any other type should fail
                default:
                    $assertThrows(deserialize);
                    break;
            }
        }
    }),

    $test('Deserialize property of type integer with value zero into columns of different types')
    .check(function () {
        var value = { val: 0 },
            columnDefinitions = {},
            deserialize = function () {
                sqliteSerializer.deserialize(value, columnDefinitions);
            };

        var deserializedValue;
        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            switch (ColumnType[c]) {
                case ColumnType.Integer:
                case ColumnType.Int:
                case ColumnType.Float:
                case ColumnType.Real:
                    deserializedValue = sqliteSerializer.deserialize(value, columnDefinitions);
                    $assert.areEqual(deserializedValue, value);
                    break;
                case ColumnType.Boolean:
                case ColumnType.Bool:
                    deserializedValue = sqliteSerializer.deserialize(value, columnDefinitions);
                    $assert.areEqual(deserializedValue, { val: false });
                    break;
                case ColumnType.Date:
                    deserializedValue = sqliteSerializer.deserialize(value, columnDefinitions);
                    $assert.isNotNull(deserializedValue);
                    $assert.isNotNull(deserializedValue.val);
                    Validate.isDate(deserializedValue.val);
                    var v = deserializedValue.val.toISOString();
                    $assert.areEqual(v, "1970-01-01T00:00:00.000Z");
                    break;
                // Deserializing to any other type should fail
                default:
                    $assertThrows(deserialize);
                    break;
            }
        }
    }),

    $test('Deserialize property with a boolean true value into columns of different types')
    .check(function () {
        var value = { val: true },
            columnDefinitions = {},
            deserialize = function () {
                sqliteSerializer.deserialize(value, columnDefinitions);
            };

        var deserializedValue;
        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            // Deserializing to any type should fail
            $assertThrows(deserialize);
        }
    }),

    $test('Deserialize property with a boolean false value into columns of different types')
    .check(function () {
        var value = { val: false },
            columnDefinitions = {},
            deserialize = function () {
                sqliteSerializer.deserialize(value, columnDefinitions);
            };

        var deserializedValue;
        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            // Deserializing to any type should fail
            $assertThrows(deserialize);
        }
    }),

    $test('Deserialize property of type float into columns of different types')
    .check(function () {
        var value = { val: -1.5 },
            columnDefinitions = {},
            deserialize = function () {
                sqliteSerializer.deserialize(value, columnDefinitions);
            };

        var deserializedValue;
        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            switch (ColumnType[c]) {
                case ColumnType.Float:
                case ColumnType.Real:
                    deserializedValue = sqliteSerializer.deserialize(value, columnDefinitions);
                    $assert.areEqual(deserializedValue, value);
                    break;
                // Deserializing to any other type should fail
                default:
                    $assertThrows(deserialize);
                    break;
            }
        }
    }),

    $test('Deserialize property of type date into columns of different types')
    .check(function () {
        var value = { val: new Date(2011, 10, 11, 12, 13, 14) },
            columnDefinitions = {},
            deserialize = function () {
                sqliteSerializer.deserialize(value, columnDefinitions);
            };

        var deserializedValue;
        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            // Deserializing to any type should fail
            $assertThrows(deserialize);
        }
    }),

    $test('Deserialize property with null value into columns of different types')
    .check(function () {
        var value = { val: null },
            columnDefinitions = {};

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            $assert.areEqual(sqliteSerializer.deserialize(value, columnDefinitions), value);
        }
    }),

    $test('Deserialize property with undefined value into columns of different types')
    .check(function () {
        var value = { val: undefined },
            columnDefinitions = {};

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            $assert.areEqual(sqliteSerializer.deserialize(value, columnDefinitions), {val: null});
        }
    }),

    $test('Deserialize property when columnDefinitions has a different case')
    .check(function () {
        var value = { val: 1 },
            columnDefinitions = {VAL: 'integer'};

        $assert.areEqual(sqliteSerializer.deserialize(value, columnDefinitions), {VAL: 1});
    }),

    $test('Deserialize Attempting to deserialize to an unsupported column should fail')
    .check(function () {
        var value = {},
            columnDefinitions = { val: 'someunsupportedtype' },
            deserialize = function () {
                sqliteSerializer.deserialize(value, columnDefinitions);
            };

        // object
        value.val = { a: 1 };
        $assertThrows(deserialize);

        // array
        value.val = [1, 2];
        $assertThrows(deserialize);

        // integer
        value.val = 5;
        $assertThrows(deserialize);

        // float
        value.val = -5.5;
        $assertThrows(deserialize);

        // string
        value.val = 'somestring';
        $assertThrows(deserialize);

        // bool
        value.val = true;
        $assertThrows(deserialize);
    }),
    
    $test('Deserialize a null object')
    .check(function () {
        var value = {},
            columnDefinitions = {},
            deserialize = function () {
                sqliteSerializer.deserialize(null, columnDefinitions);
            };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            // Deserializing to any type should fail
            $assertThrows(deserialize);
        }
    }),


    $test('Deserialize an undefined object')
    .check(function () {
        var value = {},
            columnDefinitions = {},
            deserialize = function () {
                sqliteSerializer.deserialize(undefined, columnDefinitions);
            };

        for (var c in ColumnType) {

            columnDefinitions.val = ColumnType[c];

            // Deserializing to any type should fail
            $assertThrows(deserialize);
        }
    }),


    $test('Roundtripping of properties of all types should be lossless')
    .check(function () {
        var value = {
            object: { a: 1, b: 'str', c: [1, 2] },
            array: [1, 2, { a: 1 }],
            string: 'somestring',
            text: 'sometext',
            integer: 5,
            int: 6,
            bool: true,
            boolean: false,
            real: 1.5,
            float: 2.2,
            date: new Date(2001, 11, 12, 13, 14, 59)
        };
        var columnDefinitions = {
            object: ColumnType.Object,
            array: ColumnType.Array,
            string: ColumnType.String,
            text: ColumnType.Text,
            integer: ColumnType.Integer,
            int: ColumnType.Int,
            boolean: ColumnType.Boolean,
            bool: ColumnType.Bool,
            real: ColumnType.Real,
            float: ColumnType.Float,
            date: ColumnType.Date
        };
        var serializedValue = sqliteSerializer.serialize(value, columnDefinitions);
        $assert.areEqual(serializedValue, {
            "object": "{\"a\":1,\"b\":\"str\",\"c\":[1,2]}",
            "array": "[1,2,{\"a\":1}]",
            "string": value.string,
            "text": value.text,
            "integer": value.integer,
            "int": value.int,
            "boolean": 0,
            "bool": 1,
            "real": value.real,
            "float": value.float,
            "date": 1008191699000
        });
        $assert.areEqual(sqliteSerializer.deserialize(serializedValue, columnDefinitions), value);
    })
);
