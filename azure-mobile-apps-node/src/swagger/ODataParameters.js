// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
module.exports = [
    {
        name: "$filter",
        description: "OData filter clause",
        required: false,
        type: "string",
        in: "query"
    }, {
        name: "$orderby",
        description: "OData order by clause",
        required: false,
        type: "string",
        in: "query"
    }, {
        name: "$skip",
        description: "OData skip clause",
        required: false,
        type: "integer",
        in: "query"
    }, {
        name: "$top",
        description: "OData top clause",
        required: false,
        type: "integer",
        in: "query"
    }, {
        name: "$select",
        description: "OData select clause",
        required: false,
        type: "string",
        in: "query"
    }, {
        name: "$inlinecount",
        description: "OData inline count clause",
        required: false,
        type: "string",
        in: "query"
    }
]
