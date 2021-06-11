// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var expect = require('chai').expect,
    duration = require('../../src/utilities/duration');

describe('azure-mobile-apps.utilities.duration', function () {
    describe('subtract', function () {
        var subtract = duration.subtract;

        it('subtracts single values correctly', function () {
            var date = new Date(2010, 5, 5, 5, 5, 5, 5);
            expect(subtract(date, { milliseconds: 3 })).to.deep.equal(new Date(2010, 5, 5, 5, 5, 5, 2));
            expect(subtract(date, { seconds: 3 })).to.deep.equal(new Date(2010, 5, 5, 5, 5, 2, 5));
            expect(subtract(date, { minutes: 3 })).to.deep.equal(new Date(2010, 5, 5, 5, 2, 5, 5));
            expect(subtract(date, { hours: 3 })).to.deep.equal(new Date(2010, 5, 5, 2, 5, 5, 5));
            expect(subtract(date, { days: 3 })).to.deep.equal(new Date(2010, 5, 2, 5, 5, 5, 5));
            expect(subtract(date, { weeks: 3 })).to.deep.equal(new Date(2010, 4, 15, 5, 5, 5, 5));
            expect(subtract(date, { months: 3 })).to.deep.equal(new Date(2010, 2, 5, 5, 5, 5, 5));
            expect(subtract(date, { years: 3 })).to.deep.equal(new Date(2007, 5, 5, 5, 5, 5, 5));
        });

        it('subtracts multiple values correctly', function () {
            var date = new Date(2010, 5, 5, 5, 5, 5, 5);
            expect(subtract(date, { milliseconds: 3, seconds: 3, minutes: 3 })).to.deep.equal(new Date(2010, 5, 5, 5, 2, 2, 2));
            expect(subtract(date, { hours: 3, days: 3, months:3, years: 3 })).to.deep.equal(new Date(2007, 2, 2, 2, 5, 5, 5));
            expect(subtract(date, { seconds: 63, minutes: 3 })).to.deep.equal(new Date(2010, 5, 5, 5, 1, 2, 5));
        });

        it('handles overlapping dates', function () {
            var date = new Date(2010, 5, 5, 5, 5, 5, 5);
            expect(subtract(date, { milliseconds: 6 })).to.deep.equal(new Date(2010, 5, 5, 5, 5, 4, 999));
            expect(subtract(date, { milliseconds: 6, minutes: 6, months: 6 })).to.deep.equal(new Date(2009, 11, 5, 4, 59, 4, 999));
        });
    })
});