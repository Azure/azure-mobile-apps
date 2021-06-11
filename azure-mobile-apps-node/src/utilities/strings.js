// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module azure-mobile-apps/src/utilities/strings
@description Provides utility functions for manipulating strings
*/

// Regex to validate string ids to ensure that it does not include any characters which can be used within a URI
var stringIdValidatorRegex = /([\u0000-\u001F]|[\u007F-\u009F]|["\+\?\\\/\`]|^\.{1,2}$)/;

// Match YYYY-MM-DDTHH:MM:SS.sssZ, with the millisecond (.sss) part optional
// Note: we only support a subset of ISO 8601
var iso8601Regex = /^(\d{4})-(\d{2})-(\d{2})T(\d{2})\:(\d{2})\:(\d{2})(\.(\d{3}))?Z$/;

// Match MS Date format "\/Date(1336003790912-0700)\/"
var msDateRegex = /^\/Date\((-?)(\d+)(([+\-])(\d{2})(\d{2})?)?\)\/$/;

var strings = module.exports = {
    isLetter: function (ch) {
        return (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
    },

    isDigit: function (ch) {
        return ch >= '0' && ch <= '9';
    },

    isValidStringId: function (id) {
        return !stringIdValidatorRegex.test(id);
    },

    // remove starting and finishing quotes and remove quote escaping from the middle of a string
    getVersionFromEtag: function (etag) {
        return etag && etag.replace(/^"|\\(?=")|"$/g, '');
    },

    getEtagFromVersion: function (version) {
        return version && '"' + version.replace(/\"/g, '\\"') + '"';
    },

    convertDate: function (value) {
        var date = strings.parseISODate(value);
        if (date) {
            return date;
        }

        date = strings.parseMsDate(value);
        if (date) {
            return date;
        }

        return null;
    },

    // attempt to parse the value as an ISO 8601 date (e.g. 2012-05-03T00:06:00.638Z)
    parseISODate: function (value) {
        if (iso8601Regex.test(value)) {
            return strings.parseDateTimeOffset(value);
        }

        return null;
    },

    // parse a date and convert to UTC
    parseDateTimeOffset: function (value) {
        var ms = Date.parse(value);
        if (!isNaN(ms)) {
            return new Date(ms);
        }
        return null;
    },

    // attempt to parse the value as an MS date (e.g. "\/Date(1336003790912-0700)\/")
    parseMsDate: function (value) {
        var match = msDateRegex.exec(value);
        if (match) {
            // Get the ms and offset
            var milliseconds = parseInt(match[2], 10);
            var offsetMinutes = 0;
            if (match[5]) {
                var hours = parseInt(match[5], 10);
                var minutes = parseInt(match[6] || '0', 10);
                offsetMinutes = (hours * 60) + minutes;
            }

            // Handle negation
            if (match[1] === '-') {
                milliseconds = -milliseconds;
            }
            if (match[4] === '-') {
                offsetMinutes = -offsetMinutes;
            }

            var date = new Date();
            date.setTime(milliseconds + offsetMinutes * 60000);
            return date;
        }
        return null;
    },

    parseBoolean: function (bool) {
        if (bool === undefined || bool === null || typeof bool !== 'string') {
            return undefined;
        } else if (bool.toLowerCase() === 'true') {
            return true;
        } else if (bool.toLowerCase() === 'false') {
            return false;
        } else {
            return undefined;
        }
    }
}
