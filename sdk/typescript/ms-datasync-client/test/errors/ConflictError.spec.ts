// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from 'chai';
import * as errors from '../../src/errors';

describe('errors/ConflictError', () => {
    it('#isConflictError', () => {
        it('is true for a ConflictError', () => {
            const err = new errors.ConflictError('Conflict');
            expect(errors.isConflictError(err)).to.be.true;
        });

        it('is true for name == ConflictError', () => {
            const err = new Error('Conflict');
            err.name = 'ConflictError';

            expect(errors.isConflictError(err)).to.be.true;
        });

        it('is false for other errors', () => {
            const err = new errors.ArgumentError('Argument', 'err');
            expect(errors.isConflictError(err)).to.be.false;

            const err2 = new Error('Conflict');
            err2.name = 'SomeOtherError';
            expect(errors.isConflictError(err2)).to.be.false;
        });

        it('is false for non-errors', () => {
            const err = {};
            expect(errors.isConflictError(err)).to.be.false;
        });
    });
});