// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * Valid column types to be used for defining table schema.
 * These types will be mapped to an equivalent SQLite type - TEXT / INT / REAL - while performing SQLite operations.
 */
module.exports = {
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
};

//TODO: Numeric / Number data type
