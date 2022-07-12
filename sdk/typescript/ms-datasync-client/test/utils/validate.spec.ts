// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import * as Validate from '../../src/utils/validate';

describe('src/utils/validate', () => {
    describe('#isDate', () => {
        it('throws when required', () => {
            assert.throws(() => { Validate.isDate(null); });
            assert.throws(() => { Validate.isDate(''); });
            assert.throws(() => { Validate.isDate('foo'); });
            assert.throws(() => { Validate.isDate(123123); });
            assert.throws(() => { Validate.isDate('1/1/12'); });
        });
        it('passes otherwise', () => {
            assert.doesNotThrow(() => { Validate.isDate(new Date()); });
            assert.doesNotThrow(() => { Validate.isDate(new Date('04/28/22')); });
        });
    });

    describe('#isInteger', () => {
        it('throws when required', () => {
            assert.throws(() => { Validate.isInteger(null); });
            assert.throws(() => { Validate.isInteger(''); });
            assert.throws(() => { Validate.isInteger('foo'); });
            assert.throws(() => { Validate.isInteger('1'); });
            assert.throws(() => { Validate.isInteger(1.5); });
            assert.throws(() => { Validate.isInteger(0.001); });
            assert.throws(() => { Validate.isInteger(NaN); });
        });
        it('passes otherwise', () => {
            assert.doesNotThrow(() => { Validate.isInteger(0); });
            assert.doesNotThrow(() => { Validate.isInteger(1); });
            assert.doesNotThrow(() => { Validate.isInteger(-1); });
            assert.doesNotThrow(() => { Validate.isInteger(10); });
        });
    });

    describe('#notNull', () => {
        it('throws when required', () => {
            assert.throws(() => { Validate.notNull(null); }, TypeError);
            assert.throws(() => { Validate.notNull(undefined); }, TypeError);
        });
        it('passes otherwise', () => {
            assert.doesNotThrow(() => { Validate.notNull(''); });
            assert.doesNotThrow(() => { Validate.notNull(0); });
            assert.doesNotThrow(() => { Validate.notNull('foo'); });
            assert.doesNotThrow(() => { Validate.notNull([]); });
            assert.doesNotThrow(() => { Validate.notNull({}); });
        });
    });

    describe('#notNullOrEmpty', () => {
        it('throws when required', () => {
            assert.throws(() => { Validate.notNullOrEmpty(null); });
            assert.throws(() => { Validate.notNullOrEmpty(undefined); });
            assert.throws(() => { Validate.notNullOrEmpty(''); });
            assert.throws(() => { Validate.notNullOrEmpty([]); });
        });
        it('passes otherwise', () => {
            assert.doesNotThrow(() => { Validate.notNullOrEmpty(0); });
            assert.doesNotThrow(() => { Validate.notNullOrEmpty('foo'); });
            assert.doesNotThrow(() => { Validate.notNullOrEmpty({}); });
        });
    });

    describe('#notNullOrZero', () => {
        it('throws when required', () => {
            assert.throws(() => { Validate.notNullOrZero(null); });
            assert.throws(() => { Validate.notNullOrZero(undefined); });
            assert.throws(() => { Validate.notNullOrZero(''); });
            assert.throws(() => { Validate.notNullOrZero(0); });
        });
        it('passes otherwise', () => {
            assert.doesNotThrow(() => { Validate.notNullOrZero([]); });
            assert.doesNotThrow(() => { Validate.notNullOrZero('foo'); });
            assert.doesNotThrow(() => { Validate.notNullOrZero({}); });
        });
    });

    describe('#isNumber', () => {
        it('throws when required', () => {
            assert.throws(() => { Validate.isNumber(null); });
            assert.throws(() => { Validate.isNumber(''); });
            assert.throws(() => { Validate.isNumber('foo'); });
            assert.throws(() => { Validate.isNumber('1'); });
        });
        it('passes otherwise', () => {
            assert.doesNotThrow(() => { Validate.isNumber(0); });
            assert.doesNotThrow(() => { Validate.isNumber(1); });
            assert.doesNotThrow(() => { Validate.isNumber(-1); });
            assert.doesNotThrow(() => { Validate.isNumber(1.5); });
            assert.doesNotThrow(() => { Validate.isNumber(10); });
            assert.doesNotThrow(() => { Validate.isNumber(0.001); });
            assert.doesNotThrow(() => { Validate.isNumber(NaN); });
        });
    });

    describe('#isValidId', () => {
        it('throws when required', () => {
            assert.throws(() => { Validate.isValidId(null); });
            assert.throws(() => { Validate.isValidId(undefined); });
            assert.throws(() => { Validate.isValidId(''); });
            assert.throws(() => { Validate.isValidId([]); });
            assert.throws(() => { Validate.isValidId(0); });
            assert.throws(() => { Validate.isValidId(1); });
            assert.throws(() => { Validate.isValidId(654); });
        });
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
