// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

module.exports = {
    subtract: function (date, duration) {
        var result = new Date(date.getTime());

        result.setMilliseconds(result.getMilliseconds() - (duration.milliseconds || 0));
        result.setSeconds(result.getSeconds() - (duration.seconds || 0));
        result.setMinutes(result.getMinutes() - (duration.minutes || 0));
        result.setHours(result.getHours() - (duration.hours || 0));
        result.setDate(result.getDate() - (duration.days || 0));
        result.setDate(result.getDate() - ((duration.weeks * 7) || 0));
        result.setMonth(result.getMonth() - (duration.months || 0));
        result.setFullYear(result.getFullYear() - (duration.years || 0));

        return result;
    }
};