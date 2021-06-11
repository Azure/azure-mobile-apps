// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

module.exports = {
    isMissingTable: function (err) {
        return err.message.indexOf('no such table') > -1;
    },
    isMissingColumn: function (err) {
        return err.message.indexOf('no column named') > -1 || err.message.indexOf('no such column') > -1;
    },
    isUniqueViolation: function (err) {
        return err.message.indexOf('UNIQUE constraint failed') > -1;
    },
    isInvalidDataType: function (err) {
        return err.message.indexOf('datatype mismatch') > -1;
    }
}
