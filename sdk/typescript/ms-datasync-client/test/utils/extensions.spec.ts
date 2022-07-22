// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import * as Extensions from '../../src/utils/extensions';

const emptyFunction = () => { /* Do nothing */ }; // eslint-disable-line @typescript-eslint/no-empty-function

describe('src/utils/extensions', () => {
    describe('#isDate', () => {
        it('returns true for a date', () => { assert.isTrue(Extensions.isDate(new Date()), 'date'); });

        it('return false for null', () => { assert.isFalse(Extensions.isDate(null), 'null'); });
        it('return false for undefined', () => { assert.isFalse(Extensions.isDate(undefined), 'undefined'); });
        it('return false for number', () => { assert.isFalse(Extensions.isDate(12), 'number'); });
        it('return false for string', () => { assert.isFalse(Extensions.isDate('test'), 'string'); });
        it('return false for bool', () => { assert.isFalse(Extensions.isDate(true), 'bool'); });
        it('return false for function', () => { assert.isFalse(Extensions.isDate(emptyFunction), 'function'); });
        it('return false for object', () => { assert.isFalse(Extensions.isDate({}), 'obj'); });
    });

    describe('#isInteger', () => {
        it('returns true for hex', () => { assert.isTrue(Extensions.isInteger(0x12)); });
        it('returns true for int', () => { assert.isTrue(Extensions.isInteger(11)); });
        it('returns true for zero', () => { assert.isTrue(Extensions.isInteger(0)); });
        it('returns true for fp zero', () => { assert.isTrue(Extensions.isInteger(0.0)); });
        it('returns true for -fp zero', () => { assert.isTrue(Extensions.isInteger(-0.0)); });
        it('returns true for fp int', () => { assert.isTrue(Extensions.isInteger(11.0)); });
        it('returns true for -fp int', () => { assert.isTrue(Extensions.isInteger(-11.0)); });
        it('returns true for -int', () => { assert.isTrue(Extensions.isInteger(-11)); });

        it('return false for null', () => { assert.isFalse(Extensions.isInteger(null), 'null'); });
        it('return false for undefined', () => { assert.isFalse(Extensions.isInteger(undefined), 'undefined'); });
        it('return false for string', () => { assert.isFalse(Extensions.isInteger('a'), 'string'); });
        it('return false for object', () => { assert.isFalse(Extensions.isInteger({}), 'object'); });
        it('return false for array', () => { assert.isFalse(Extensions.isInteger([]), 'array'); });
        it('return false for function', () => { assert.isFalse(Extensions.isInteger(emptyFunction), 'function'); });
        it('return false for bool', () => { assert.isFalse(Extensions.isInteger(false), 'bool'); });
        it('return false for int as string', () => { assert.isFalse(Extensions.isInteger('1'), 'integer as a string'); });
    });

    describe('#isLocalNetwork', () => {
        it('returns true for localhost', () => { assert.isTrue(Extensions.isLocalNetwork('localhost')); });
        it('returns true for .local domain', () => { assert.isTrue(Extensions.isLocalNetwork('myhost.local')); });
        it('returns true for IPv4 loopback', () => { assert.isTrue(Extensions.isLocalNetwork('127.0.0.1')); });
        it('returns true for IPv6 loopback', () => { assert.isTrue(Extensions.isLocalNetwork('::1')); });
        it('returns true for 10.x private net', () => { assert.isTrue(Extensions.isLocalNetwork('10.100.8.1')); });
        it('returns true for 172.16.x private net', () => { assert.isTrue(Extensions.isLocalNetwork('172.17.200.50')); });
        it('returns true for 192.168.x private net', () => { assert.isTrue(Extensions.isLocalNetwork('192.168.0.2')); });

        it('returns false for custom domain', () => { assert.isFalse(Extensions.isLocalNetwork('mydomain.com'), 'mydomain.com'); });
        it('returns false for custom domain', () => { assert.isFalse(Extensions.isLocalNetwork('ds.azurewebsites.net'), 'ds.azurewebsites.net'); });
        it('returns false for custom domain', () => { assert.isFalse(Extensions.isLocalNetwork('4.4.4.4'), 'Non-local IPv4'); });
        it('returns false for custom domain', () => { assert.isFalse(Extensions.isLocalNetwork('2001:0db8:85a3:0000:0000:8a2e:0370:7334'), 'Non-local IPv6'); });
    })

    describe('#isNull', () => {
        it('returns true for null', () => { assert.isTrue(Extensions.isNull(null), 'null should be null.'); });
        it('returns true for undefined', () => { assert.isTrue(Extensions.isNull(undefined), 'undefined should be null.'); });
       
        it('return false for empty string', () => { assert.isFalse(Extensions.isNull(''), 'empty string is not null.'); });
        it('return false for zero', () => { assert.isFalse(Extensions.isNull(0), '0 is not null.'); });
        it('return false for non-empty string', () => { assert.isFalse(Extensions.isNull('Foo'), "'Foo' is not null."); });
    });

    describe('#isNullOrEmpty', () => {
        it('returns true for null', () => { assert.isTrue(Extensions.isNullOrEmpty(null), 'null should be null or empty.'); });
        it('returns true for undefined', () => { assert.isTrue(Extensions.isNullOrEmpty(undefined), 'undefined should be null or empty.'); });
        it('returns true for empty string', () => { assert.isTrue(Extensions.isNullOrEmpty(''), 'empty string should be null or empty.'); });
        it('returns true for empty array', () => { assert.isTrue(Extensions.isNullOrEmpty([]), 'empty array should be null or empty.'); });

        it('return false for int', () => { assert.isFalse(Extensions.isNullOrEmpty(0), '0 is not null or empty.'); });
        it('return false for non-empty string', () => { assert.isFalse(Extensions.isNullOrEmpty('Foo'), "'Foo' is not null empty."); });
        it('return false for non-empty array', () => { assert.isFalse(Extensions.isNullOrEmpty(['Foo']), "['Foo'] is not null empty."); });
        it('return false for object', () => { assert.isFalse(Extensions.isNullOrEmpty({ }), '{ } is not null or empty.'); });
    });

    describe('#isNullOrWhiteSpace', () => {
        it('returns true for null', () => { assert.isTrue(Extensions.isNullOrWhiteSpace(null), 'null should be null or whitespace'); });
        it('returns true for undefined', () => { assert.isTrue(Extensions.isNullOrWhiteSpace(undefined), 'undefined should be null or whitespace'); });
        it('returns true for empty string', () => { assert.isTrue(Extensions.isNullOrWhiteSpace(''), 'empty string should be null or whitespace'); });
        it('returns true for whitespace string', () => { assert.isTrue(Extensions.isNullOrWhiteSpace('   '), 'whitespace should be null or whitespace'); });
       
        it('returns false for non-empty string', () => { assert.isFalse(Extensions.isNullOrWhiteSpace('something'), 'something is not null or whitespace'); });
        it('returns false for number', () => { assert.isFalse(Extensions.isNullOrWhiteSpace(9), 'number is not null or whitespace'); });
    });

    describe('#isNullOrZero', () => {
        it('returns true for null', () => { assert.isTrue(Extensions.isNullOrZero(null), 'null should be null or empty.'); });
        it('returns true for undefined', () => { assert.isTrue(Extensions.isNullOrZero(undefined), 'undefined should be null or empty.'); });
        it('returns true for empty string', () => { assert.isTrue(Extensions.isNullOrZero(''), 'empty string should be null or empty.'); });
        it('returns true for zero', () => { assert.isTrue(Extensions.isNullOrZero(0), 'zero should be null or empty.'); });

        it('return false for empty array', () => { assert.isFalse(Extensions.isNullOrZero([]), 'empty array is not null or empty.'); });
        it('return false for string', () => { assert.isFalse(Extensions.isNullOrZero('Foo'), "'Foo' is not null empty."); });
        it('return false for array', () => { assert.isFalse(Extensions.isNullOrZero(['Foo']), "['Foo'] is not null empty."); });
        it('return false for object', () => { assert.isFalse(Extensions.isNullOrZero({ }), 'empty object is not null or empty.'); });
    });

    describe('#isNumber', () => {
        it('returns true for int', () => { assert.isTrue(Extensions.isNumber(12), 'int'); });
        it('returns true for fp', () => { assert.isTrue(Extensions.isNumber(12.5), 'float'); });

        it('return false for null', () => { assert.isFalse(Extensions.isNumber(null), 'null'); });
        it('return false for undefined', () => { assert.isFalse(Extensions.isNumber(undefined), 'undefined'); });
        it('return false for string', () => { assert.isFalse(Extensions.isNumber('test'), 'string'); });
        it('return false for bool', () => { assert.isFalse(Extensions.isNumber(true), 'bool'); });
        it('return false for function', () => { assert.isFalse(Extensions.isNumber(emptyFunction), 'function'); });
        it('return false for date', () => { assert.isFalse(Extensions.isNumber(new Date()), 'date'); });
        it('return false for object', () => { assert.isFalse(Extensions.isNumber({}), 'obj'); });
    });

    describe('#isValidId', () => {
        it('returns true for a string', () => { assert.isTrue(Extensions.isValidId('id'), 'id is a valid id'); });
        it('returns true for numeric string', () => { assert.isTrue(Extensions.isValidId('12.0'), 'id can be a string number'); });
        it('returns true for bool string', () => { assert.isTrue(Extensions.isValidId('true'), 'id can be a string respresentation of a boolean'); });
        it('returns true for lc guid', () => { assert.isTrue(Extensions.isValidId('aa4da0b5-308c-4877-a5d2-03f274632636'), 'id can contain a guid'); });
        it('returns true for uc guid', () => { assert.isTrue(Extensions.isValidId('69C8BE62-A09F-4638-9A9C-6B448E9ED4E7'), 'id can contain another guid'); });
        it('returns true for bracketed guid', () => { assert.isTrue(Extensions.isValidId('{EC26F57E-1E65-4A90-B949-0661159D0546}'), 'id can contain brackets and guids'); });
        it('returns true for russian ID', () => { assert.isTrue(Extensions.isValidId('id with Russian Где моя машина'), 'id can contain other language characters'); });

        it('returns false for numeric ID', () => { assert.isFalse(Extensions.isValidId(10), '10 is an invalid id'); });
        it('returns false for null ID', () => { assert.isFalse(Extensions.isValidId(null), 'null is an invalid id'); });
        it('returns false for undefined ID', () => { assert.isFalse(Extensions.isValidId(undefined), 'undefined is an invalid id'); });
        it('returns false for empty string', () => { assert.isFalse(Extensions.isValidId(''), 'empty string is an invalid id'); });
        it('returns false for array', () => { assert.isFalse(Extensions.isValidId([]), 'empty array can not be an id'); });
        it('returns false for object', () => { assert.isFalse(Extensions.isValidId({ }), '{ } is an invalid id'); });
        it('returns false for zero', () => { assert.isFalse(Extensions.isValidId(0), '0 is an invalid id'); });
        it('returns false for too long ID', () => { assert.isFalse(Extensions.isValidId(new Array(257).join('A')), 'length of 256 is invalid'); });
        it('returns false for ID containing plus', () => { assert.isFalse(Extensions.isValidId('a+b'), 'id can not contain a +'); });      
        it('returns false for ID containing quotes', () => { assert.isFalse(Extensions.isValidId('a"b'), 'id can not contain a "'); });
        it('returns false for ID containing slash', () => { assert.isFalse(Extensions.isValidId('a/b'), 'id can not contain a /'); });
        it('returns false for ID containing question mark', () => { assert.isFalse(Extensions.isValidId('a?b'), 'id can not contain a ?'); });
        it('returns false for ID containing backslash', () => { assert.isFalse(Extensions.isValidId('a\\b'), 'id can not contain a \\'); });
        it('returns false for ID containing backtick', () => { assert.isFalse(Extensions.isValidId('a`b'), 'id can not contain a `'); });
        it('returns false for ID containing dot', () => { assert.isFalse(Extensions.isValidId('.'), 'id can not be .'); });
        it('returns false for ID containing double-dot', () => { assert.isFalse(Extensions.isValidId('..'), 'id can not be ..'); });
        it('returns false for ID containing nul char', () => { assert.isFalse(Extensions.isValidId('A\u0000C'), 'id can not contain control character u0000'); });
        it('returns false for ID containing control char', () => { assert.isFalse(Extensions.isValidId('A__\u0008C'), 'id can not contain control character u0008'); });
    });

    describe('#isValidEndpoint', () => {
        it('returns true for https domain', () => { assert.isTrue(Extensions.isValidEndpoint(new URL('https://ds.azurewebsites.net')), 'regular https website'); });
        it('returns true for http localhost', () => { assert.isTrue(Extensions.isValidEndpoint(new URL('http://localhost')), 'http localhost'); });
        it('returns true for http local domain', () => { assert.isTrue(Extensions.isValidEndpoint(new URL('http://localhost.local')), 'http local domain'); });
        it('returns true for IPv4 10.x private net', () => { assert.isTrue(Extensions.isValidEndpoint(new URL('http://10.8.8.1')), 'http IPv4 private net'); });
        it('returns true for IPv4 172.16.x private net', () => { assert.isTrue(Extensions.isValidEndpoint(new URL('http://172.16.75.80')), 'http IPv4 private net'); });
        it('returns true for IPv4 192.168.x private net', () => { assert.isTrue(Extensions.isValidEndpoint(new URL('http://192.168.50.2')), 'http IPv4 private net'); });
        it('returns true for https IPv4', () => { assert.isTrue(Extensions.isValidEndpoint(new URL('https://4.4.4.4')), 'http IPv4'); });
        it('returns true for http IPv6 localhost', () => { assert.isTrue(Extensions.isValidEndpoint(new URL('http://[::1]')), 'http IPv6 localhost'); });
        it('returns true for http IPv4 localhost', () => { assert.isTrue(Extensions.isValidEndpoint(new URL('http://127.0.0.1')), 'http localhost'); });

        it('returns false for http domain', () => { assert.isFalse(Extensions.isValidEndpoint(new URL('http://ds.azurewebistes.net')), 'regular http website'); });
        it('returns false for http IPv4', () => { assert.isFalse(Extensions.isValidEndpoint(new URL('http://8.8.8.8')), 'http IPv4 website'); });
        it('returns false for http IPv6', () => { assert.isFalse(Extensions.isValidEndpoint(new URL('http://[2001:0db8:85a3:0000:0000:8a2e:0370:7334]')), 'http IPv6 website'); });
    });
});