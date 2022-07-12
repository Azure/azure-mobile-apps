// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import * as Extensions from '../../src/utils/extensions';

const emptyFunction = () => { /* Do nothing */ }; // eslint-disable-line @typescript-eslint/no-empty-function

describe('src/utils/extensions', () => {
    describe('#isDate', () => {
        it('returns true for checked values', () => {
            assert.isTrue(Extensions.isDate(new Date()), 'date');
        });
        it('return false for non-checked values', () => {
            assert.isFalse(Extensions.isDate(null), 'null');
            assert.isFalse(Extensions.isDate(undefined), 'undefined');
            assert.isFalse(Extensions.isDate(12), 'number');
            assert.isFalse(Extensions.isDate('test'), 'string');
            assert.isFalse(Extensions.isDate(true), 'bool');
            assert.isFalse(Extensions.isDate(emptyFunction), 'function');
            assert.isFalse(Extensions.isDate({}), 'obj');
        });
    });

    describe('#isInteger', () => {
        it('returns true for checked values', () => {
            assert.isTrue(Extensions.isInteger(0x12), 'hex');
            assert.isTrue(Extensions.isInteger(11), 'int');
            assert.isTrue(Extensions.isInteger(0), 'zero');
            assert.isTrue(Extensions.isInteger(0.0), 'zero');
            assert.isTrue(Extensions.isInteger(-0.0), 'zero');
            assert.isTrue(Extensions.isInteger(11.0), 'int');
            assert.isTrue(Extensions.isInteger(-11.0), 'negative int');
            assert.isTrue(Extensions.isInteger(11), 'negative int');
        });
        it('return false for non-checked values', () => {
            assert.isFalse(Extensions.isInteger(null), 'null');
            assert.isFalse(Extensions.isInteger(undefined), 'undefined');
            assert.isFalse(Extensions.isInteger('a'), 'string');
            assert.isFalse(Extensions.isInteger({}), 'object');
            assert.isFalse(Extensions.isInteger([]), 'array');
            assert.isFalse(Extensions.isInteger(emptyFunction), 'function');
            assert.isFalse(Extensions.isInteger(false), 'bool');
            assert.isFalse(Extensions.isInteger('1'), 'integer as a string');
        });
    });

    describe('#isNull', () => {
        it('returns true for checked values', () => {
            assert.isTrue(Extensions.isNull(null), 'null should be null.');
            assert.isTrue(Extensions.isNull(undefined), 'undefined should be null.');
        });
        it('return false for non-checked values', () => {
            assert.isFalse(Extensions.isNull(''), 'empty string is not null.');
            assert.isFalse(Extensions.isNull(0), '0 is not null.');
            assert.isFalse(Extensions.isNull('Foo'), "'Foo' is not null.");
        });
    });

    describe('#isNullOrEmpty', () => {
        it('returns true for checked values', () => {
            assert.isTrue(Extensions.isNullOrEmpty(null), 'null should be null or empty.');
            assert.isTrue(Extensions.isNullOrEmpty(undefined), 'undefined should be null or empty.');
            assert.isTrue(Extensions.isNullOrEmpty(''), 'empty string should be null or empty.');
            assert.isTrue(Extensions.isNullOrEmpty([]), 'empty array should be null or empty.');
        });
        it('return false for non-checked values', () => {
            assert.isFalse(Extensions.isNullOrEmpty(0), '0 is not null or empty.');
            assert.isFalse(Extensions.isNullOrEmpty('Foo'), "'Foo' is not null empty.");
            assert.isFalse(Extensions.isNullOrEmpty(['Foo']), "['Foo'] is not null empty.");
            assert.isFalse(Extensions.isNullOrEmpty({ }), '{ } is not null or empty.');
        });
    });

    describe('#isNullOrZero', () => {
        it('returns true for checked values', () => {
            assert.isTrue(Extensions.isNullOrZero(null), 'null should be null or empty.');
            assert.isTrue(Extensions.isNullOrZero(undefined), 'undefined should be null or empty.');
            assert.isTrue(Extensions.isNullOrZero(''), 'empty string should be null or empty.');
            assert.isTrue(Extensions.isNullOrZero(0), 'zero should be null or empty.');
        });
        it('return false for non-checked values', () => {
            assert.isFalse(Extensions.isNullOrZero([]), 'empty array is not null or empty.');
            assert.isFalse(Extensions.isNullOrZero('Foo'), "'Foo' is not null empty.");
            assert.isFalse(Extensions.isNullOrZero(['Foo']), "['Foo'] is not null empty.");
            assert.isFalse(Extensions.isNullOrZero({ }), 'empty object is not null or empty.');
        });
    });

    describe('#isNumber', () => {
        it('returns true for checked values', () => {
            assert.isTrue(Extensions.isNumber(12), 'int');
            assert.isTrue(Extensions.isNumber(12.5), 'float');
        });
        it('return false for non-checked values', () => {
            assert.isFalse(Extensions.isNumber(null), 'null');
            assert.isFalse(Extensions.isNumber(undefined), 'undefined');
            assert.isFalse(Extensions.isNumber('test'), 'string');
            assert.isFalse(Extensions.isNumber(true), 'bool');
            assert.isFalse(Extensions.isNumber(emptyFunction), 'function');
            assert.isFalse(Extensions.isNumber(new Date()), 'date');
            assert.isFalse(Extensions.isNumber({}), 'obj');
        });
    });

    describe('#isValidId', () => {
        it('returns true for checked values', () => {
            assert.isTrue(Extensions.isValidId('id'), 'id is a valid id');
            assert.isTrue(Extensions.isValidId('12.0'), 'id can be a string number');
            assert.isTrue(Extensions.isValidId('true'), 'id can be a string respresentation of a boolean');
            assert.isTrue(Extensions.isValidId('false'), 'id can be a string respresentation of a boolean (false)');
            assert.isTrue(Extensions.isValidId('aa4da0b5-308c-4877-a5d2-03f274632636'), 'id can contain a guid');
            assert.isTrue(Extensions.isValidId('69C8BE62-A09F-4638-9A9C-6B448E9ED4E7'), 'id can contain another guid');
            assert.isTrue(Extensions.isValidId('{EC26F57E-1E65-4A90-B949-0661159D0546}'), 'id can contain brackets and guids');
            assert.isTrue(Extensions.isValidId('id with Russian Где моя машина'), 'id can contain other language characters');
        });
        it('return false for non-checked values', () => {
            assert.isFalse(Extensions.isValidId(10), '10 is an invalid id');
            assert.isFalse(Extensions.isValidId(null), 'null is an invalid id');
            assert.isFalse(Extensions.isValidId(undefined), 'undefined is an invalid id');
            assert.isFalse(Extensions.isValidId(''), 'empty string is an invalid id');
            assert.isFalse(Extensions.isValidId([]), 'empty array can not be an id');
            assert.isFalse(Extensions.isValidId({ }), '{ } is an invalid id');
            assert.isFalse(Extensions.isValidId(0), '0 is an invalid id');
            assert.isFalse(Extensions.isValidId(new Array(257).join('A')), 'length of 256 is invalid');
            assert.isFalse(Extensions.isValidId('a+b'), 'id can not contain a +');        
            assert.isFalse(Extensions.isValidId('a"b'), 'id can not contain a "');
            assert.isFalse(Extensions.isValidId('a/b'), 'id can not contain a /');
            assert.isFalse(Extensions.isValidId('a?b'), 'id can not contain a ?');
            assert.isFalse(Extensions.isValidId('a\\b'), 'id can not contain a \\');
            assert.isFalse(Extensions.isValidId('a`b'), 'id can not contain a `');
            assert.isFalse(Extensions.isValidId('.'), 'id can not be .');
            assert.isFalse(Extensions.isValidId('..'), 'id can not be ..');
            assert.isFalse(Extensions.isValidId('A\u0000C'), 'id can not contain control character u0000');
            assert.isFalse(Extensions.isValidId('A__\u0008C'), 'id can not contain control character u0008');
        });
    });
});