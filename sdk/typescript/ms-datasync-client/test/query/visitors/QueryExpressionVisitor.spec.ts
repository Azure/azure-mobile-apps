// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import { QueryExpressionVisitor } from '../../../src/query/visitors';

describe('src/query/visitors/QueryExpressionVisitor', () => {
    describe('#constructor', () => {
        it('can be created', () => { 
            const n = new QueryExpressionVisitor();
            assert.isDefined(n);
        });
    });
});