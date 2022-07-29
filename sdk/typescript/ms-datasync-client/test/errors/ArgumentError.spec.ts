// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from 'chai';
import { ArgumentError } from '../../src/errors';

describe('errors/ArgumentError', () => {
    describe('#ctor', () => {
        it('can be thrown', () => {
            expect(() => { throw new ArgumentError('test', 'foo'); }).to.throw;
        });
    });

    describe('#argumentName', () => {
        it('sets argumentName', () => {
            const err = new ArgumentError('test', 'foo');
            expect(err.argumentName).to.equal('foo');
        });
    });

    describe('#message', () => {
        it('sets message', () => {
            const err = new ArgumentError('test', 'foo');
            expect(err.message).to.equal('test');
        });
    });
});