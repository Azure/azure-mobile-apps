// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import * as Validate from '../../src/utils/validate';

describe('src/utils/validate', () => {
    describe('#isDate', () => {
        it('throws when null', () => {assert.throws(() => { Validate.isDate(null); }); });
        it('throws when an empty string', () => {assert.throws(() => { Validate.isDate(''); }); });
        it('throws when a string', () => {assert.throws(() => { Validate.isDate('foo'); }); });
        it('throws when a number', () => {assert.throws(() => { Validate.isDate(123123); }); });
        it('throws when a string that looks like a date', () => {assert.throws(() => { Validate.isDate('1/1/12'); }); });

        it('does not throw when the default date', () => {assert.doesNotThrow(() => { Validate.isDate(new Date()); }); });
        it('does not throw when a date', () => {assert.doesNotThrow(() => { Validate.isDate(new Date('04/28/22')); }); });
    });

    describe('#isInteger', () => {
        it('throws when null', () => { assert.throws(() => { Validate.isInteger(null); }); });
        it('throws when empty string', () => { assert.throws(() => { Validate.isInteger(''); }); });
        it('throws when string', () => { assert.throws(() => { Validate.isInteger('foo'); }); });
        it('throws when numeric string', () => { assert.throws(() => { Validate.isInteger('1'); }); });
        it('throws when 1.5', () => { assert.throws(() => { Validate.isInteger(1.5); }); });
        it('throws when 0.001', () => { assert.throws(() => { Validate.isInteger(0.001); }); });
        it('throws when NaN', () => { assert.throws(() => { Validate.isInteger(NaN); }); });

        it('does not throw when zero', () => { assert.doesNotThrow(() => { Validate.isInteger(0); }); });
        it('does not throw when 1', () => { assert.doesNotThrow(() => { Validate.isInteger(1); }); });
        it('does not throw when -1', () => { assert.doesNotThrow(() => { Validate.isInteger(-1); }); });
        it('does not throw when 10', () => { assert.doesNotThrow(() => { Validate.isInteger(10); }); });
    });

    describe('#notNull', () => {
        it('throws when null', () => { assert.throws(() => { Validate.notNull(null); }, TypeError); });
        it('throws when undefined', () => { assert.throws(() => { Validate.notNull(undefined); }, TypeError); });

        it('does not throw when empty string', () => { assert.doesNotThrow(() => { Validate.notNull(''); }); });
        it('does not throw when zero', () => { assert.doesNotThrow(() => { Validate.notNull(0); }); });
        it('does not throw when string', () => { assert.doesNotThrow(() => { Validate.notNull('foo'); }); });
        it('does not throw when array', () => { assert.doesNotThrow(() => { Validate.notNull([]); }); });
        it('does not throw when object', () => { assert.doesNotThrow(() => { Validate.notNull({}); }); });
    });

    describe('#notNullOrEmpty', () => {
        it('throws when null', () => { assert.throws(() => { Validate.notNullOrEmpty(null); }, TypeError); });
        it('throws when undefined', () => { assert.throws(() => { Validate.notNullOrEmpty(undefined); }, TypeError); });
        it('throws when empty string', () => { assert.throws(() => { Validate.notNullOrEmpty(''); }, TypeError); });
        it('throws when empty array', () => { assert.throws(() => { Validate.notNullOrEmpty([]); }, TypeError); });

        it('does not throw when zero', () => { assert.doesNotThrow(() => { Validate.notNullOrEmpty(0); }); });
        it('does not throw when string', () => { assert.doesNotThrow(() => { Validate.notNullOrEmpty('foo'); }); });
        it('does not throw when object', () => { assert.doesNotThrow(() => { Validate.notNullOrEmpty({}); }); });
    });

    describe('#notNullOrZero', () => {
        it('throws when null', () => { assert.throws(() => { Validate.notNullOrZero(null); }, TypeError); });
        it('throws when undefined', () => { assert.throws(() => { Validate.notNullOrZero(undefined); }, TypeError); });
        it('throws when empty string', () => { assert.throws(() => { Validate.notNullOrZero(''); }, TypeError); });
        it('throws when zero', () => { assert.throws(() => { Validate.notNullOrZero(0); }, TypeError); });

        it('does not throw when empty array', () => { assert.doesNotThrow(() => { Validate.notNullOrZero([]); }); });
        it('does not throw when string', () => { assert.doesNotThrow(() => { Validate.notNullOrZero('foo'); }); });
        it('does not throw when object', () => { assert.doesNotThrow(() => { Validate.notNullOrZero({}); }); });
    });

    describe('#isNumber', () => {
        it('throws when null', () => { assert.throws(() => { Validate.isNumber(null); }, TypeError); });
        it('throws when empty string', () => { assert.throws(() => { Validate.isNumber(''); }, TypeError); });
        it('throws when string', () => { assert.throws(() => { Validate.isNumber('foo'); }, TypeError); });
        it('throws when numeric string', () => { assert.throws(() => { Validate.isNumber('1'); }, TypeError); });

        it('does not throw when zero', () => { assert.doesNotThrow(() => { Validate.isNumber(0); }); });
        it('does not throw when 1', () => { assert.doesNotThrow(() => { Validate.isNumber(1); }); });
        it('does not throw when -1', () => { assert.doesNotThrow(() => { Validate.isNumber(-1); }); });
        it('does not throw when 1.5', () => { assert.doesNotThrow(() => { Validate.isNumber(1.5); }); });
        it('does not throw when 10', () => { assert.doesNotThrow(() => { Validate.isNumber(10); }); });
        it('does not throw when 0.001', () => { assert.doesNotThrow(() => { Validate.isNumber(0.001); }); });
        it('does not throw when NaN (which is a number)', () => { assert.doesNotThrow(() => { Validate.isNumber(NaN); }); });
    });

    describe('#isValidEndpoint', () => {
        it('throws when http domain', () => { assert.throws(() => { Validate.isValidEndpoint(new URL('http://ds.endpoint.com')); }); });
        it('does not throw when https domain', () => { assert.doesNotThrow(() => { Validate.isValidEndpoint(new URL('https://ds.endpoint.com')); }); });
        it('throws when http localhost', () => { assert.doesNotThrow(() => { Validate.isValidEndpoint(new URL('http://localhost')); }); });
    });

    describe('#isValidId', () => {
        it('throws when null', () => { assert.throws(() => { Validate.isValidId(null); }, TypeError); });
        it('throws when undefined', () => { assert.throws(() => { Validate.isValidId(undefined); }, TypeError); });
        it('throws when empty string', () => { assert.throws(() => { Validate.isValidId(''); }, TypeError); });
        it('throws when array', () => { assert.throws(() => { Validate.isValidId([]); }, TypeError); });
        it('throws when zero', () => { assert.throws(() => { Validate.isValidId(0); }, TypeError); });
        it('throws when 1', () => { assert.throws(() => { Validate.isValidId(1); }, TypeError); });
        it('throws when 654', () => { assert.throws(() => { Validate.isValidId(654); }, TypeError); });
        it('throws when boolean', () => { assert.throws(() => { Validate.isValidId(true); }, TypeError); });

        it('passes otherwise', () => {
            assert.doesNotThrow(() => { Validate.isValidId('id'); });
            assert.doesNotThrow(() => { Validate.isValidId('12.0'); });
            assert.doesNotThrow(() => { Validate.isValidId('true'); });
            assert.doesNotThrow(() => { Validate.isValidId('false'); });
            assert.doesNotThrow(() => { Validate.isValidId('aa4da0b5-308c-4877-a5d2-03f274632636'); });
            assert.doesNotThrow(() => { Validate.isValidId('69C8BE62-A09F-4638-9A9C-6B448E9ED4E7'); });
            assert.doesNotThrow(() => { Validate.isValidId('{EC26F57E-1E65-4A90-B949-0661159D0546}'); });
            assert.doesNotThrow(() => { Validate.isValidId('id with Russian Где моя машина'); });
        });
    });
});
